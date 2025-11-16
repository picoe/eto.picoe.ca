using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using NuGet.Versioning;

sealed class AssemblyLoader : IDisposable
{
	private const string NuGetFlatContainerBaseUrl = "https://api.nuget.org/v3-flatcontainer/";

	private readonly HttpClient httpClient = new();
	private readonly Dictionary<string, string> dependencyAssemblyPaths = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> processedDependencyPackages = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, IReadOnlyList<NuGetVersion>> packageVersionsCache = new(StringComparer.OrdinalIgnoreCase);
	private readonly AssemblyLoadContext loadContext;
	private bool disposed;

	public AssemblyLoader()
	{
		loadContext = AssemblyLoadContext.Default;
		loadContext.Resolving += ResolveDependencyAssembly;
	}

	public void Dispose()
	{
		if (disposed)
			return;

		loadContext.Resolving -= ResolveDependencyAssembly;
		httpClient.Dispose();
		disposed = true;
	}

	public bool TryLoadAssemblies(IEnumerable<string> assemblyArgs, out List<AssemblyInput> assemblyInputs)
	{
		assemblyInputs = new List<AssemblyInput>();
		foreach (var asmArg in assemblyArgs)
		{
			var assemblyInput = ResolveAssemblyInput(asmArg);
			if (assemblyInput == null)
			{
				assemblyInputs.Clear();
				return false;
			}

			assemblyInputs.Add(assemblyInput);
		}

		return true;
	}

	public bool TryAddReference(string referenceSpec)
	{
		var assemblyInput = ResolveAssemblyInput(referenceSpec);
		if (assemblyInput == null)
			return false;

		RegisterDependencyAssembly(assemblyInput.AssemblyPath);
		return true;
	}

	private AssemblyInput? ResolveAssemblyInput(string asmArg)
	{
		if (string.IsNullOrWhiteSpace(asmArg))
		{
			Console.Error.WriteLine("Invalid assembly argument.");
			return null;
		}

		var fullPath = Path.GetFullPath(asmArg);
		if (File.Exists(fullPath))
		{
			var ext = Path.GetExtension(fullPath);
			if (!string.IsNullOrEmpty(ext) && ext.Equals(".nupkg", StringComparison.OrdinalIgnoreCase))
				return CreateAssemblyInputFromPackage(fullPath);
			return CreateAssemblyInputFromAssembly(fullPath);
		}

		if (LooksLikePackageId(asmArg))
			return CreateAssemblyInputFromNuGetPackage(asmArg);

		Console.Error.WriteLine($"Assembly not found: {fullPath}");
		return null;
	}

	private AssemblyInput? CreateAssemblyInputFromAssembly(string asmPath)
	{
		var xmlPath = Path.ChangeExtension(asmPath, ".xml");

		if (!File.Exists(asmPath))
		{
			Console.Error.WriteLine($"Assembly not found: {asmPath}");
			return null;
		}

		if (xmlPath == null || !File.Exists(xmlPath))
		{
			Console.WriteLine($"Warning: XML doc file not found: {xmlPath}");
			xmlPath = null;
		}

		return new AssemblyInput(asmPath, xmlPath);
	}

	private AssemblyInput? CreateAssemblyInputFromNuGetPackage(string packageSpec)
	{
		if (!TryParsePackageSpec(packageSpec, out var packageId, out var versionSpec))
		{
			Console.Error.WriteLine($"Invalid NuGet package specification: {packageSpec}");
			return null;
		}

		var packageVersion = ResolvePackageVersion(packageId, versionSpec);
		if (string.IsNullOrEmpty(packageVersion))
		{
			Console.Error.WriteLine($"Unable to determine a version for package: {packageId}");
			return null;
		}

		var packagePath = DownloadNuGetPackage(packageId, packageVersion);
		if (packagePath == null)
			return null;

		return CreateAssemblyInputFromPackage(packagePath, new NuGetPackageInfo(packageId, packageVersion));
	}

	private AssemblyInput? CreateAssemblyInputFromPackage(string packagePath, NuGetPackageInfo? overridePackageInfo = null, string? preferredFramework = null)
	{
		if (!File.Exists(packagePath))
		{
			Console.Error.WriteLine($"NuGet package not found: {packagePath}");
			return null;
		}

		using var archive = ZipFile.OpenRead(packagePath);

		var nuspec = LoadNuspec(archive);
		if (nuspec == null)
		{
			Console.Error.WriteLine($"Unable to read .nuspec from package: {packagePath}");
			return null;
		}

		var packageInfo = overridePackageInfo ?? ExtractPackageInfo(nuspec) ?? new NuGetPackageInfo(Path.GetFileNameWithoutExtension(packagePath), "");
		var candidates = GetPackageAssemblyCandidates(archive);
		if (candidates.Count == 0)
		{
			Console.Error.WriteLine($"NuGet package does not contain any assemblies under lib/: {packagePath}");
			return null;
		}

		var selection = SelectFrameworkCandidates(candidates, packageInfo.Id, preferredFramework);

		var tempDir = Path.Combine(Path.GetTempPath(), "GenerateApiDocs", Guid.NewGuid().ToString("n"));
		Directory.CreateDirectory(tempDir);

		var extractionResult = ExtractFrameworkAssemblies(archive, selection, tempDir, packageInfo);
		if (string.IsNullOrEmpty(extractionResult.PrimaryAssemblyPath))
		{
			Console.Error.WriteLine($"Unable to extract primary assembly for package: {packageInfo.Id}");
			return null;
		}

		if (string.IsNullOrEmpty(extractionResult.PrimaryXmlPath))
		{
			Console.WriteLine($"Warning: XML doc file not found for {selection.PrimaryCandidate.Entry.FullName} inside package: {packagePath}");
		}

		EnsurePackageDependencies(nuspec, selection.NormalizedFramework, packageInfo);

		return new AssemblyInput(extractionResult.PrimaryAssemblyPath, extractionResult.PrimaryXmlPath, packageInfo);
	}

	private static XDocument? LoadNuspec(ZipArchive archive)
	{
		var nuspecEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
		if (nuspecEntry == null)
			return null;

		using var nuspecStream = nuspecEntry.Open();
		return XDocument.Load(nuspecStream);
	}

	private static NuGetPackageInfo? ExtractPackageInfo(XDocument nuspec)
	{
		var root = nuspec.Root;
		if (root == null)
			return null;

		var ns = root.GetDefaultNamespace();
		var metadata = root.Element(ns + "metadata") ?? root.Element("metadata");
		if (metadata == null)
			return null;

		var idElement = metadata.Element(ns + "id") ?? metadata.Element("id");
		var versionElement = metadata.Element(ns + "version") ?? metadata.Element("version");
		var id = idElement?.Value?.Trim();
		if (string.IsNullOrEmpty(id))
			return null;
		var version = versionElement?.Value?.Trim() ?? "";
		return new NuGetPackageInfo(id, version);
	}

	private static List<NuGetAssemblyCandidate> GetPackageAssemblyCandidates(ZipArchive archive)
	{
		var candidates = new List<NuGetAssemblyCandidate>();
		foreach (var entry in archive.Entries)
		{
			if (!entry.FullName.StartsWith("lib/", StringComparison.OrdinalIgnoreCase))
				continue;
			if (!entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				continue;

			var framework = GetFrameworkFromEntry(entry.FullName);
			if (framework == null)
				continue;

			candidates.Add(new NuGetAssemblyCandidate(entry, framework, NormalizeFramework(framework)));
		}
		return candidates;
	}

	private FrameworkSelection SelectFrameworkCandidates(List<NuGetAssemblyCandidate> candidates, string packageId, string? preferredFramework)
	{
		if (candidates.Count == 0)
			throw new InvalidOperationException("No assembly candidates available.");

		IEnumerable<NuGetAssemblyCandidate> filtered = candidates;
		if (!string.IsNullOrEmpty(preferredFramework))
		{
			var preferredMatches = candidates
				.Where(c => string.Equals(c.NormalizedFramework, preferredFramework, StringComparison.OrdinalIgnoreCase))
				.ToList();
			if (preferredMatches.Count > 0)
				filtered = preferredMatches;
		}

		var ordered = filtered
			.OrderBy(c => GetFrameworkPriority(c.NormalizedFramework))
			.ThenBy(c => c.Entry.FullName, StringComparer.OrdinalIgnoreCase)
			.ToList();

		var selectedFramework = ordered.First().Framework;
		var normalizedFramework = ordered.First().NormalizedFramework;
		var frameworkCandidates = ordered
			.Where(c => string.Equals(c.Framework, selectedFramework, StringComparison.OrdinalIgnoreCase))
			.ToList();

		var primaryCandidate = frameworkCandidates
			.FirstOrDefault(c => string.Equals(Path.GetFileNameWithoutExtension(c.Entry.Name), packageId, StringComparison.OrdinalIgnoreCase))
			?? frameworkCandidates.First();

		return new FrameworkSelection(selectedFramework, normalizedFramework, frameworkCandidates, primaryCandidate);
	}

	private FrameworkExtractionResult ExtractFrameworkAssemblies(ZipArchive archive, FrameworkSelection selection, string destinationDir, NuGetPackageInfo packageInfo, bool logMissingXmlForPrimary = true)
	{
		string? primaryAssemblyPath = null;
		string? primaryXmlPath = null;

		foreach (var candidate in selection.Candidates)
		{
			var assemblyFileName = Path.GetFileName(candidate.Entry.Name);
			var asmPath = Path.Combine(destinationDir, assemblyFileName);
			ExtractArchiveEntry(candidate.Entry, asmPath);
			RegisterDependencyAssembly(asmPath);

			if (candidate.Entry == selection.PrimaryCandidate.Entry)
				primaryAssemblyPath = asmPath;

			var xmlEntry = FindXmlDocEntry(archive, candidate.Entry);
			if (xmlEntry != null)
			{
				var xmlFileName = Path.GetFileName(xmlEntry.Name);
				var xmlPath = Path.Combine(destinationDir, xmlFileName);
				ExtractArchiveEntry(xmlEntry, xmlPath);
				if (candidate.Entry == selection.PrimaryCandidate.Entry)
					primaryXmlPath = xmlPath;
			}
			else if (candidate.Entry == selection.PrimaryCandidate.Entry && logMissingXmlForPrimary)
			{
				Console.WriteLine($"Warning: XML doc file not found for {candidate.Entry.FullName} inside package: {packageInfo.Id}");
			}
		}

		return new FrameworkExtractionResult(primaryAssemblyPath, primaryXmlPath);
	}

	private void EnsurePackageDependencies(XDocument nuspec, string normalizedFramework, NuGetPackageInfo packageInfo)
	{
		foreach (var dependency in GetDependenciesForFramework(nuspec, normalizedFramework))
		{
			if (IsSystemPackageId(dependency.Id))
				continue;

			var version = ResolvePackageVersion(dependency.Id, dependency.VersionSpec);
			if (string.IsNullOrEmpty(version))
			{
				Console.WriteLine($"Warning: Unable to resolve version for dependency {dependency.Id} of {packageInfo.Id}.");
				continue;
			}

			ProcessDependencyPackage(dependency.Id, version, normalizedFramework);
		}
	}

	private void ProcessDependencyPackage(string packageId, string version, string? preferredFramework)
	{
		var packageKey = $"{packageId}@{version}";
		if (!processedDependencyPackages.Add(packageKey))
			return;

		var packagePath = DownloadNuGetPackage(packageId, version);
		if (packagePath == null)
			return;

		using var archive = ZipFile.OpenRead(packagePath);
		var nuspec = LoadNuspec(archive);
		var candidates = GetPackageAssemblyCandidates(archive);
		if (candidates.Count == 0)
			return;

		var selection = SelectFrameworkCandidates(candidates, packageId, preferredFramework);
		var extractionRoot = Path.Combine(Path.GetTempPath(), "GenerateApiDocs", "deps", SanitizeFileName($"{packageId}.{version}"), selection.Framework);
		Directory.CreateDirectory(extractionRoot);
		var packageInfo = new NuGetPackageInfo(packageId, version);
		ExtractFrameworkAssemblies(archive, selection, extractionRoot, packageInfo, logMissingXmlForPrimary: false);

		if (nuspec != null)
		{
			EnsurePackageDependencies(nuspec, selection.NormalizedFramework, packageInfo);
		}
	}

	private static IEnumerable<NuGetDependency> GetDependenciesForFramework(XDocument nuspec, string normalizedFramework)
	{
		var root = nuspec.Root;
		if (root == null)
			yield break;

		var ns = root.GetDefaultNamespace();
		var metadata = root.Element(ns + "metadata") ?? root.Element("metadata");
		if (metadata == null)
			yield break;

		var dependenciesElement = metadata.Element(ns + "dependencies") ?? metadata.Element("dependencies");
		if (dependenciesElement == null)
			yield break;

		XElement? GetGroupElement()
		{
			var groups = dependenciesElement
				.Elements(ns + "group").Concat(dependenciesElement.Elements("group"))
				.ToList();

			if (groups.Count == 0)
				return null;

			if (!string.IsNullOrEmpty(normalizedFramework))
			{
				var match = groups.FirstOrDefault(g =>
				{
					var target = g.Attribute("targetFramework")?.Value;
					if (string.IsNullOrEmpty(target))
						return false;
					return string.Equals(NormalizeFramework(target), normalizedFramework, StringComparison.OrdinalIgnoreCase);
				});
				if (match != null)
					return match;
			}

			var neutral = groups.FirstOrDefault(g => string.IsNullOrEmpty(g.Attribute("targetFramework")?.Value));
			return neutral ?? groups.First();
		}

		var group = GetGroupElement();
		if (group != null)
		{
			foreach (var dep in group.Elements(ns + "dependency").Concat(group.Elements("dependency")))
			{
				var id = dep.Attribute("id")?.Value;
				if (string.IsNullOrEmpty(id))
					continue;
				var version = dep.Attribute("version")?.Value;
				yield return new NuGetDependency(id, version);
			}
			yield break;
		}

		foreach (var dep in dependenciesElement.Elements(ns + "dependency").Concat(dependenciesElement.Elements("dependency")))
		{
			var id = dep.Attribute("id")?.Value;
			if (string.IsNullOrEmpty(id))
				continue;
			var version = dep.Attribute("version")?.Value;
			yield return new NuGetDependency(id, version);
		}
	}

	private static bool IsSystemPackageId(string packageId)
	{
		return packageId.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
			|| packageId.Equals("System", StringComparison.OrdinalIgnoreCase)
			|| packageId.StartsWith("Microsoft.NETCore", StringComparison.OrdinalIgnoreCase)
			|| packageId.StartsWith("Microsoft.Win32", StringComparison.OrdinalIgnoreCase)
			|| packageId.Equals("NETStandard.Library", StringComparison.OrdinalIgnoreCase);
	}

	private static string? GetFrameworkFromEntry(string entryFullName)
	{
		const string prefix = "lib/";
		if (!entryFullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			return null;

		var remainder = entryFullName[prefix.Length..];
		var slashIndex = remainder.IndexOf('/');
		if (slashIndex < 0)
			return null;

		return remainder[..slashIndex];
	}

	private static string NormalizeFramework(string framework)
	{
		if (string.IsNullOrEmpty(framework))
			return framework;

		var delimiterIndex = framework.IndexOfAny(new[] { '-', '+' });
		return delimiterIndex > 0 ? framework[..delimiterIndex] : framework;
	}

	private static int GetFrameworkPriority(string? framework)
	{
		if (string.IsNullOrEmpty(framework))
			return int.MaxValue;

		ReadOnlySpan<string> preferenceOrder = new[]
		{
			"net8.0",
			"net7.0",
			"net6.0",
			"net5.0",
			"netcoreapp3.1",
			"netcoreapp3.0",
			"netcoreapp2.1",
			"netcoreapp2.0",
			"netstandard2.1",
			"netstandard2.0",
			"net48",
			"net472",
			"net471",
			"net47",
			"net462",
			"net461",
			"net46",
			"net452",
			"net451",
			"net45",
			"net40",
			"net35"
		};

		for (var i = 0; i < preferenceOrder.Length; i++)
		{
			if (string.Equals(preferenceOrder[i], framework, StringComparison.OrdinalIgnoreCase))
				return i;
		}

		return preferenceOrder.Length + framework.Length;
	}

	private static ZipArchiveEntry? FindXmlDocEntry(ZipArchive archive, ZipArchiveEntry assemblyEntry)
	{
		string xmlName;
		if (assemblyEntry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
			xmlName = assemblyEntry.FullName[..^3] + "xml";
		else
			xmlName = $"{assemblyEntry.FullName}.xml";

		return archive.Entries.FirstOrDefault(e => string.Equals(e.FullName, xmlName, StringComparison.OrdinalIgnoreCase));
	}

	private static void ExtractArchiveEntry(ZipArchiveEntry entry, string destinationPath)
	{
		using var sourceStream = entry.Open();
		using var destinationStream = File.Create(destinationPath);
		sourceStream.CopyTo(destinationStream);
	}

	private static bool TryParsePackageSpec(string packageSpec, out string packageId, out string? version)
	{
		packageId = packageSpec;
		version = null;
		if (string.IsNullOrWhiteSpace(packageSpec))
			return false;

		var atIndex = packageSpec.IndexOf('@');
		if (atIndex >= 0)
		{
			packageId = packageSpec[..atIndex];
			version = packageSpec[(atIndex + 1)..];
			if (string.IsNullOrWhiteSpace(version))
				version = null;
		}

		packageId = packageId.Trim();
		if (string.IsNullOrEmpty(packageId))
			return false;

		version = version?.Trim();
		return true;
	}

	private string? DownloadNuGetPackage(string packageId, string version)
	{
		try
		{
			var packagesRoot = Path.Combine(Path.GetTempPath(), "GenerateApiDocs", "packages");
			Directory.CreateDirectory(packagesRoot);
			var packageFileName = SanitizeFileName($"{packageId}.{version}.nupkg");
			var destinationPath = Path.Combine(packagesRoot, packageFileName);
			if (File.Exists(destinationPath))
				return destinationPath;

			Console.WriteLine($"Downloading {packageId} {version} from nuget.org...");

			var idLower = packageId.ToLowerInvariant();
			var versionLower = version.ToLowerInvariant();
			var packageUrl = $"{NuGetFlatContainerBaseUrl}{idLower}/{versionLower}/{idLower}.{versionLower}.nupkg";

			using var response = httpClient.GetAsync(packageUrl).GetAwaiter().GetResult();
			if (!response.IsSuccessStatusCode)
			{
				Console.Error.WriteLine($"Failed to download {packageId} {version} (HTTP {(int)response.StatusCode}).");
				return null;
			}

			using var stream = response.Content.ReadAsStream();
			using var fileStream = File.Create(destinationPath);
			stream.CopyTo(fileStream);

			return destinationPath;
		}
		catch (HttpRequestException ex)
		{
			Console.Error.WriteLine($"Error downloading NuGet package {packageId} {version}: {ex.Message}");
			return null;
		}
		catch (IOException ex)
		{
			Console.Error.WriteLine($"File error while downloading NuGet package {packageId} {version}: {ex.Message}");
			return null;
		}
	}

	private string? ResolvePackageVersion(string packageId, string? versionSpec)
	{
		if (string.IsNullOrWhiteSpace(versionSpec))
			return GetPackageVersion(packageId, range: null);

		if (VersionRange.TryParse(versionSpec, out var range))
			return GetPackageVersion(packageId, range);

		return versionSpec;
	}

	private string? GetPackageVersion(string packageId, VersionRange? range)
	{
		var versions = GetAllPackageVersions(packageId);
		if (versions.Count == 0)
			return null;

		IEnumerable<NuGetVersion> filtered = versions;
		if (range != null)
			filtered = versions.Where(range.Satisfies);

		var candidates = filtered.ToList();
		if (candidates.Count == 0)
			return null;

		var stableCandidates = candidates.Where(v => !v.IsPrerelease).ToList();
		var bestList = stableCandidates.Count > 0 ? stableCandidates : candidates;
		var best = bestList.Max();
		return best?.ToNormalizedString();
	}

	private IReadOnlyList<NuGetVersion> GetAllPackageVersions(string packageId)
	{
		if (packageVersionsCache.TryGetValue(packageId, out var cached))
			return cached;

		try
		{
			var idLower = packageId.ToLowerInvariant();
			var indexUrl = $"{NuGetFlatContainerBaseUrl}{idLower}/index.json";
			using var response = httpClient.GetAsync(indexUrl).GetAwaiter().GetResult();
			if (!response.IsSuccessStatusCode)
			{
				Console.Error.WriteLine($"Failed to query versions for {packageId} (HTTP {(int)response.StatusCode}).");
				return Array.Empty<NuGetVersion>();
			}

			using var stream = response.Content.ReadAsStream();
			using var doc = JsonDocument.Parse(stream);
			if (!doc.RootElement.TryGetProperty("versions", out var versionsElement))
			{
				Console.Error.WriteLine($"NuGet version index missing 'versions' array for {packageId}.");
				return Array.Empty<NuGetVersion>();
			}

			var versions = new List<NuGetVersion>();
			foreach (var versionValue in versionsElement.EnumerateArray())
			{
				var versionString = versionValue.GetString();
				if (string.IsNullOrWhiteSpace(versionString))
					continue;
				if (NuGetVersion.TryParse(versionString, out var nugetVersion))
					versions.Add(nugetVersion);
			}

			versions.Sort();
			packageVersionsCache[packageId] = versions;
			return versions;
		}
		catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
		{
			Console.Error.WriteLine($"Error retrieving versions for {packageId}: {ex.Message}");
			return Array.Empty<NuGetVersion>();
		}
	}

	private static bool LooksLikePackageId(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return false;

		if (value.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) >= 0)
			return false;

		if (value.Contains(':'))
			return false;

		if (value.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
			value.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase) ||
			value.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
			value.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		return true;
	}

	private static string SanitizeFileName(string value)
	{
		var invalidChars = Path.GetInvalidFileNameChars();
		var builder = new StringBuilder(value.Length);
		foreach (var ch in value)
		{
			builder.Append(invalidChars.Contains(ch) ? '_' : ch);
		}
		return builder.ToString();
	}

	private void RegisterDependencyAssembly(string assemblyPath)
	{
		try
		{
			var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
			var simpleName = assemblyName.Name;
			if (string.IsNullOrEmpty(simpleName))
				return;

			if (!dependencyAssemblyPaths.ContainsKey(simpleName))
			{
				dependencyAssemblyPaths[simpleName] = assemblyPath;
			}
		}
		catch (Exception ex) when (ex is BadImageFormatException || ex is FileLoadException || ex is IOException)
		{
			// Ignore native or invalid assemblies.
		}
	}

	private Assembly? ResolveDependencyAssembly(AssemblyLoadContext context, AssemblyName assemblyName)
	{
		if (assemblyName.Name == null)
			return null;

		if (dependencyAssemblyPaths.TryGetValue(assemblyName.Name, out var path) && File.Exists(path))
		{
			try
			{
				return context.LoadFromAssemblyPath(path);
			}
			catch (FileLoadException)
			{
				return null;
			}
		}

		return null;
	}
}
