using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Text;
using GenerateApiDocs;
using GenerateApiDocs.Formatters;

#pragma warning disable IL2070, IL2026, IL2057


if (args.Length < 2)
{
	Console.WriteLine("Usage: dotnet run -- [options] <Assembly.dll|package.nupkg|nugetpackage@1.0.0> [<Assembly.dll|package.nupkg|nugetpackage@1.0.0> ...] <OutputDir>");
	Console.WriteLine("Example: dotnet run -- path/To/MyFile.dll path/to/OtherFile.dll docs/api");
Console.WriteLine("Options:");
Console.WriteLine("  --flatten-namespaces                Flatten namespaces in output paths");
Console.WriteLine("  --include-explicit-implementations  Include explicit interface implementations");
Console.WriteLine("  --move-deprecated-members           Move deprecated properties and methods to their own sections");
Console.WriteLine("  --split-members                     Generate individual pages for constructors, fields, properties, events, and methods");
Console.WriteLine("  --reference <spec>                  Preload extra assemblies or NuGet packages for type resolution");
return 1;
}

var argIndex = 0;
bool flattenNamespaces = false;
bool includeExplicitImplementations = false;
bool moveDeprecatedMembers = false;
bool splitMembers = false;
var referenceSpecs = new List<string>();


while (argIndex < args.Length && args[argIndex].StartsWith("--", StringComparison.Ordinal))
{
	switch (args[argIndex])
	{
		case "--flatten-namespaces":
			flattenNamespaces = true;
			break;
		case "--include-explicit-implementations":
			includeExplicitImplementations = true;
			break;
		case "--move-deprecated-members":
			moveDeprecatedMembers = true;
			break;
		case "--split-members":
			splitMembers = true;
			break;
		case "--reference":
			argIndex++;
			if (argIndex >= args.Length)
			{
				Console.Error.WriteLine("--reference requires a value.");
				return 1;
			}
			referenceSpecs.Add(args[argIndex]);
			break;
		default:
			Console.Error.WriteLine($"Unknown option: {args[argIndex]}");
			return 1;
	}
	argIndex++;
}

if (args.Length - argIndex < 2)
{
	Console.Error.WriteLine("Error: Missing required arguments.");
	return 1;
}

if (splitMembers && moveDeprecatedMembers)
{
	Console.WriteLine("Warning: --move-deprecated-members is ignored when --split-members is enabled.");
	moveDeprecatedMembers = false;
}

var outputRoot = Path.GetFullPath(args[^1]);
var assemblyArgs = args.Skip(argIndex).Take(args.Length - argIndex - 1).ToList();
if (assemblyArgs.Count == 0)
{
	Console.Error.WriteLine("Error: At least one assembly must be specified.");
	return 1;
}

using var assemblyLoader = new AssemblyLoader();

var typeLookup = new Dictionary<string, Type>();
var allXmlMembers = new List<XElement>();
var assemblyPackageMap = new Dictionary<string, NuGetPackageInfo?>(StringComparer.OrdinalIgnoreCase);

foreach (var reference in referenceSpecs)
{
	if (!assemblyLoader.TryAddReference(reference))
		return 1;
}

if (!assemblyLoader.TryLoadAssemblies(assemblyArgs, out var assemblyInputs))
	return 1;

foreach (var asmInput in assemblyInputs)
{
	var asmPath = asmInput.AssemblyPath;
	var xmlPath = asmInput.XmlDocPath;

	var asm = Assembly.LoadFrom(asmPath);
	var asmLocation = asm.Location;
	if (!string.IsNullOrEmpty(asmLocation))
		assemblyPackageMap[asmLocation] = asmInput.Package;

	foreach (var type in asm.GetExportedTypes())
	{
		var key = XmlDocumentation.NormalizeTypeName(type);
		if (!typeLookup.ContainsKey(key))
			typeLookup[key] = type;
	}

	if (!string.IsNullOrEmpty(xmlPath) && File.Exists(xmlPath))
	{
		var xmlDoc = XDocument.Load(xmlPath);
		var membersElement = xmlDoc.Root?.Element("members");
		if (membersElement != null)
		{
			allXmlMembers.AddRange(membersElement.Elements("member"));
		}
	}
}

Directory.CreateDirectory(outputRoot);

var allTypes = typeLookup.Values
	.Where(t => t.IsClass || t.IsInterface || t.IsEnum || t.IsValueType || IsDelegate(t))
	.OrderBy(t => GetTypeSlug(t))
	// .ThenBy(t => t.Name)
	.ToList();

var csharpFormatter = new CSharpLanguageFormatter();
var languageFormatters = new LanguageFormatter[]
{
	csharpFormatter,
	// new CppLanguageFormatter(),
	new VbLanguageFormatter(),
	new FSharpLanguageFormatter(),
	new PythonLanguageFormatter()
};

// Build XML index
var xmlDocs = XmlDocumentation.Build(allXmlMembers);

// Map types to doc IDs for linking between types
var typeIdMap = new Dictionary<string, string>(); // normalized type name -> docId

foreach (var type in allTypes)
{
	var ns = type.Namespace ?? "";
	var nsPath = ns.Replace('.', '/');
	if (flattenNamespaces)
		nsPath = "";
	var typeSlug = GetTypeSlug(type, true);
	var id = string.IsNullOrEmpty(nsPath) ? typeSlug : $"{nsPath}/{typeSlug}";
	var normalizedName = XmlDocumentation.NormalizeTypeName(type);

	typeIdMap[normalizedName] = id;
}

// For sidebar
var sidebarMap = new SortedDictionary<string, List<SidebarEntry>>(StringComparer.Ordinal);
var namespaceSlugMap = new Dictionary<string, string>(StringComparer.Ordinal);

// Generate one page per type
foreach (var type in allTypes)
{
	var ns = type.Namespace ?? "";
	var nsKey = string.IsNullOrEmpty(ns) ? "(Global)" : ns;
	var nsPath = ns.Replace('.', Path.DirectorySeparatorChar);
	var nsSlugPath = string.IsNullOrEmpty(ns) ? "" : ns.Replace('.', '/');
	if (flattenNamespaces)
	{
		nsPath = "";
	}
	var typeSlug = GetTypeSlug(type, true);
	var fileDir = string.IsNullOrEmpty(nsPath)
		? outputRoot
		: Path.Combine(outputRoot, nsPath);
	Directory.CreateDirectory(fileDir);

	var filePath = Path.Combine(fileDir, $"{typeSlug}.mdx");

	var docId = typeIdMap[XmlDocumentation.NormalizeTypeName(type)];

	if (!sidebarMap.TryGetValue(nsKey, out var list))
	{
		list = new List<SidebarEntry>();
		sidebarMap[nsKey] = list;
	}
	var sidebarEntry = new SidebarEntry(docId, GetDisplayTypeName(type), splitMembers ? new List<SectionEntry>() : null);
	list.Add(sidebarEntry);

	if (!namespaceSlugMap.ContainsKey(nsKey))
	{
		var slug = string.IsNullOrEmpty(nsSlugPath) ? "/global" : "/" + nsSlugPath;
		namespaceSlugMap[nsKey] = slug;
	}

	using var w = new StreamWriter(filePath);

	var context = new TypeOutputContext(splitMembers, typeSlug, fileDir, sidebarEntry, new List<MemberTableEntry>());

	// --- frontmatter ---
	w.WriteLine("---");
	w.WriteLine($"id: {EscapeYaml(typeSlug)}");
	w.WriteLine($"title: {EscapeYaml(GetDisplayTypeName(type))}");
	w.WriteLine($"sidebar_label: {EscapeYaml(GetDisplayTypeName(type))}");
	w.WriteLine("---");
	w.WriteLine();

	WriteTabsImports(w);

	xmlDocs.TryGetType(type, out var typeXml);

	// Title
	w.WriteLine($"# {Helpers.EscapePlainText(GetDisplayTypeFullName(type))}");
	w.WriteLine();
	WriteDeprecationWarning(w, type);

	w.WriteLine("## Definition");

	// Namespace & kind
	w.WriteLine("<div style={{ fontSize: '0.8rem' }}>");
	w.WriteLine($"Namespace: {(string.IsNullOrEmpty(ns) ? "`(global)`" : $"`{ns}`")}<br />");
	w.WriteLine($"Type: `{GetTypeKind(type)}`<br />");

	var packageInfo = GetPackageInfoForAssembly(type.Assembly);
	if (assemblyInputs.Count > 1 || packageInfo != null)
	{
		w.WriteLine($"Assembly: `{type.Assembly.GetName().Name}.dll`<br />");
		if (packageInfo != null)
		{
			var packageDisplay = Helpers.EscapePlainText(FormatPackageDisplay(packageInfo));
			var packageUrl = GetNuGetPackageUrl(packageInfo);
			w.WriteLine($"NuGet package: [{packageDisplay}]({packageUrl})<br />");
		}
	}
	w.WriteLine("</div>");

	w.WriteLine();

	WriteSummarySection(w, typeXml, typeIdMap, includeHeader: false);

	// Base type & interfaces (with links if available)
	WriteBaseAndInterfacesSection(w, type, typeIdMap);

	// Type summary / remarks / example from XML
	WriteRemarksSection(w, typeXml, typeIdMap);
	WriteExampleSection(w, typeXml);

	var deprecatedConstructors = moveDeprecatedMembers ? new List<ConstructorInfo>() : null;
	WriteConstructorsSection(w, type, xmlDocs, typeIdMap, deprecatedConstructors, context);

	// Members
	WriteFieldsSection(w, type, xmlDocs, typeIdMap, context);
	var deprecatedProperties = moveDeprecatedMembers ? new List<PropertyInfo>() : null;
	WritePropertiesSection(w, type, xmlDocs, typeIdMap, deprecatedProperties, context);
	WriteEventsSection(w, type, xmlDocs, typeIdMap, context);
	var deprecatedMethods = moveDeprecatedMembers ? new List<MethodInfo>() : null;
	WriteMethodsSection(w, type, xmlDocs, typeIdMap, deprecatedMethods, context);

	if (includeExplicitImplementations)
		WriteExplicitInterfaceImplementationsSection(w, type);

	if (moveDeprecatedMembers)
	{
		WriteDeprecatedConstructorsSection(w, type, deprecatedConstructors, typeIdMap, xmlDocs);
		WriteDeprecatedPropertiesSection(w, type, deprecatedProperties, typeIdMap, xmlDocs);
		WriteDeprecatedMethodsSection(w, type, deprecatedMethods, typeIdMap, xmlDocs);
	}

	if (splitMembers && context.TableEntries.Count > 0)
	{
		WriteMemberOverview(w, context);
	}
}

WriteApiIndex(outputRoot, sidebarMap, namespaceSlugMap);

WriteSidebar(outputRoot, sidebarMap, namespaceSlugMap);
Console.WriteLine($"Generated {allTypes.Count} type pages from {assemblyInputs.Count} assemblies into: {outputRoot}");
return 0;

// ---------------- XML index ----------------

// ---------------- Type-level helpers ----------------

bool IsDelegate(Type t) => typeof(MulticastDelegate).IsAssignableFrom(t) && t != typeof(MulticastDelegate);

string GetTypeSlug(Type t, bool forNavigation = false, bool shortName = false)
{
	// Console.WriteLine($"GetTypeSlug: {t}, forNavigation={forNavigation}, shortName={shortName}");
	// System.Threading.Thread.Sleep(100);
	var name = flattenNamespaces && !shortName ? t.FullName : t.Name;

	if (name == null)
		return t.Name;
	var baseName = name.Contains('`') ? name[..name.IndexOf('`')] : name;

	// Handle nested types for slug
	if (t.IsNested && !t.IsGenericParameter)
	{
		var parentSlug = GetTypeSlug(t.DeclaringType!, forNavigation, shortName);
		baseName = $"{parentSlug}.{t.Name}";
	}

	// Handle generic types for slug
	if (t.IsGenericType)
	{
		var typeArgs = t.GetGenericArguments();
		if (forNavigation)
			baseName += $"-{typeArgs.Length}";
		else
		{
			var argsPart = string.Join(",", typeArgs.Select(a => GetTypeSlug(a, shortName: true)));
			baseName = $"{baseName}<{argsPart}>";
		}
	}

	return baseName;
}

string GetDisplayTypeName(Type t) => GetTypeSlug(t, shortName: true);

string GetDisplayTypeFullName(Type t) => GetDisplayTypeName(t);

string GetTypeKind(Type t)
{
	if (t.IsEnum) return "enum";
	if (t.IsInterface) return "interface";
	if (t.IsValueType && !t.IsEnum) return "struct";
	if (IsDelegate(t)) return "delegate";
	if (t.IsClass && t.IsAbstract && t.IsSealed) return "static class";
	if (t.IsClass && t.IsAbstract) return "abstract class";
	if (t.IsClass) return "class";
	return "type";
}

void WriteBaseAndInterfacesSection(StreamWriter w, Type type, Dictionary<string, string> typeIdMap)
{
	var baseType = type.BaseType;
	var interfaces = type.GetInterfaces();

	var items = new List<string>();

	if (baseType != null && baseType != typeof(object) && !type.IsInterface && !type.IsValueType)
	{
		w.WriteLine($"Inheritance: {LinkOrCodeType(baseType)}");
		w.WriteLine();
	}

	// find all derived classes that implement this type
	var derivedTypes = allTypes
		.Where(t2 => t2 != type && type.IsAssignableFrom(t2))
		.OrderBy(t => GetDisplayTypeName(t))
		.ToList();

	if (derivedTypes.Count > 0)
	{
		w.WriteLine($"Derived: {string.Join(", ", derivedTypes.Select(dt => LinkOrCodeType(dt)))}");
		w.WriteLine();
	}

	if (interfaces.Length > 0)
	{
		var ifaceStr = string.Join(", ", interfaces.Select(i => LinkOrCodeType(i)));
		w.WriteLine($"Implements: {ifaceStr}");
		w.WriteLine();
	}
}

string LinkOrCodeType(Type t, Func<Type, string>? getName = null)
{
	var norm = XmlDocumentation.NormalizeTypeName(t);
	var display = getName?.Invoke(t) ?? csharpFormatter.FormatTypeName(t);
	if (typeIdMap.TryGetValue(norm, out var id))
	{
		return $"[`{display}`](/api/{id})";
	}

	// if it's a system type, link to the microsoft api docs:
	if (t.Namespace != null && t.Namespace.StartsWith("System"))
	{
		if (t.IsGenericType)
		{
			// for generic types, link to the generic type definition
			t = t.GetGenericTypeDefinition();
		}

		var msdnUrl = $"https://learn.microsoft.com/en-us/dotnet/api/{t.FullName?.ToLower().Replace('+', '.').Replace('`', '-')}";
		return $"[`{display}`]({msdnUrl})";
	}
	return $"`{display}`";
}

// ---------------- Member sections ----------------

void WriteTabsImports(StreamWriter w)
{
	w.WriteLine("import Tabs from '@theme/Tabs';");
	w.WriteLine("import TabItem from '@theme/TabItem';");
	w.WriteLine();
}

void WriteLanguageTabs(StreamWriter w, Func<LanguageFormatter, string> getCode)
{
	w.WriteLine("<Tabs groupId=\"code-lang\">");
	foreach (var formatter in languageFormatters)
	{
		w.WriteLine($"  <TabItem value=\"{formatter.LanguageKey}\" label=\"{formatter.DisplayName}\">");
		w.WriteLine();
		w.WriteLine($"```{formatter.LanguageKey}");
		w.WriteLine(getCode(formatter));
		w.WriteLine("```");
		w.WriteLine();
		w.WriteLine("  </TabItem>");
		w.WriteLine();
	}
	w.WriteLine("</Tabs>");
	w.WriteLine();
}

void WriteDeprecationWarning(StreamWriter w, MemberInfo member)
{
	if (member == null)
		return;

	var obsoleteAttr = GetObsoleteAttribute(member);
	if (obsoleteAttr == null)
		return;

	var admonition = obsoleteAttr.IsError ? "error" : "warning";
	var title = obsoleteAttr.IsError ? "Deprecated (errors)" : "Deprecated";
	var message = string.IsNullOrWhiteSpace(obsoleteAttr.Message)
		? "This member is deprecated and may be removed in future versions."
		: Helpers.EscapePlainText(obsoleteAttr.Message);

	w.WriteLine($":::{admonition} {title}");
	w.WriteLine(message);
	w.WriteLine(":::");
	w.WriteLine();
}

bool IsDeprecated(MemberInfo member) => GetObsoleteAttribute(member) != null;

ObsoleteAttribute? GetObsoleteAttribute(MemberInfo member)
{
	if (member == null)
		return null;
	return member.GetCustomAttribute<ObsoleteAttribute>();
}

void WriteConstructorsSection(StreamWriter w, Type type, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, List<ConstructorInfo>? deprecatedConstructors, TypeOutputContext context)
{
	var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
		.ToList();
	if (ctors.Count == 0) return;

	var orderedConstructors = ctors
		.OrderBy(c => c.GetParameters().Length)
		.ThenBy(c => GetMethodHeading(type, c))
		.ToList();

	if (context.SplitMembers)
	{
		WriteConstructorDocuments(type, orderedConstructors, xmlDocs, typeIdMap, context);
		return;
	}

	w.WriteLine("## Constructors");
	w.WriteLine();

	if (moveDeprecatedMembers && deprecatedConstructors != null)
	{
		deprecatedConstructors.AddRange(orderedConstructors.Where(IsDeprecated));
	}

	var ctorsToRender = moveDeprecatedMembers
		? orderedConstructors.Where(c => !IsDeprecated(c)).ToList()
		: orderedConstructors;

	if (ctorsToRender.Count == 0)
		return;

	foreach (var ctor in ctorsToRender)
	{
		WriteConstructorBlock(w, type, ctor, xmlDocs, typeIdMap);
	}
}

string GetMethodHeading(Type type, MethodBase method)
{
	var name = method.IsConstructor ? GetDisplayTypeName(type) : method.Name;

	if (method is MethodInfo mi && mi.IsGenericMethod)
	{
		var typeArgs = mi.GetGenericArguments();
		if (typeArgs.Length > 0)
			name += $"<{string.Join(", ", typeArgs.Select(a => a.Name))}>";
	}

	return $"{name}{FormatParameterTypeList(method)}";
}

string FormatParameterTypeList(MethodBase method)
{
	var parameters = method.GetParameters();
	if (parameters.Length == 0)
		return "()";

	return "(" + string.Join(", ", parameters.Select(GetParameterHeadingTypeName)) + ")";
}
string GetParameterHeadingTypeName(ParameterInfo parameter)
{
	var type = parameter.ParameterType;
	string modifier = "";
	if (type.IsByRef)
	{
		type = type.GetElementType()!;
		modifier = parameter.IsOut ? "out " : "ref ";
	}
	else if (parameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Any())
	{
		modifier = "params ";
	}

	return modifier + csharpFormatter.FormatTypeName(type);
}

void WriteFieldsSection(StreamWriter w, Type type, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, TypeOutputContext context)
{
	var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
		.Where(f => !f.IsSpecialName)
		.ToList();
	if (fields.Count == 0) return;

	if (context.SplitMembers)
	{
		WriteFieldDocuments(type, fields, xmlDocs, typeIdMap, context);
		return;
	}

	w.WriteLine("## Fields");
	w.WriteLine();

	foreach (var field in fields.OrderBy(f => f.Name))
	{
		w.WriteLine($"### {field.Name}");
		w.WriteLine();

		WriteFieldContent(w, type, field, xmlDocs, typeIdMap);
	}
}

void WritePropertiesSection(StreamWriter w, Type type, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, List<PropertyInfo>? deprecatedProperties, TypeOutputContext context)
{
	var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
		.ToList();
	if (props.Count == 0) return;

	var orderedProps = props.OrderBy(p => p.Name).ToList();

	if (context.SplitMembers)
	{
		WritePropertyDocuments(type, orderedProps, xmlDocs, typeIdMap, context);
		return;
	}

	if (moveDeprecatedMembers && deprecatedProperties != null)
	{
		deprecatedProperties.AddRange(orderedProps.Where(IsDeprecated));
	}

	var propsToRender = moveDeprecatedMembers
		? orderedProps.Where(p => !IsDeprecated(p)).ToList()
		: orderedProps;

	if (propsToRender.Count == 0)
		return;

	w.WriteLine("## Properties");
	w.WriteLine();

	foreach (var prop in propsToRender)
	{
		WritePropertyBlock(w, type, prop, xmlDocs, typeIdMap);
	}
}

void WritePropertyBlock(StreamWriter w, Type type, PropertyInfo prop, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap)
{
	w.WriteLine($"### {prop.Name}");
	w.WriteLine();
	WritePropertyContent(w, type, prop, xmlDocs, typeIdMap);
}

void WritePropertyContent(StreamWriter w, Type type, PropertyInfo prop, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap)
{
	if (xmlDocs.TryGetProperty(type, prop, out var xml))
		WriteSummarySection(w, xml, typeIdMap, includeHeader: false);
	WriteDeprecationWarning(w, prop);

	WriteLanguageTabs(w, formatter => formatter.FormatPropertySignature(prop));

	w.WriteLine("#### Property Value");
	w.WriteLine();
	w.WriteLine(LinkOrCodeType(prop.PropertyType));
	w.WriteLine();

	WriteRemarksSection(w, xml, typeIdMap);
	WriteExampleSection(w, xml);
}

void WriteConstructorBlock(StreamWriter w, Type type, ConstructorInfo ctor, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap)
{
	w.WriteLine($"### {Helpers.EscapePlainText(GetMethodHeading(type, ctor))}");
	w.WriteLine();
	WriteConstructorContent(w, type, ctor, xmlDocs, typeIdMap);
}

void WriteConstructorContent(StreamWriter w, Type type, ConstructorInfo ctor, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap)
{
	xmlDocs.TryGetConstructor(type, ctor, out var xml);

	WriteSummarySection(w, xml, typeIdMap, includeHeader: false);
	WriteDeprecationWarning(w, ctor);

	WriteLanguageTabs(w, formatter => formatter.FormatMethodSignature(type, ctor));

	WriteParamsSection(w, xml, ctor, typeIdMap);
	WriteRemarksSection(w, xml, typeIdMap);
	WriteExampleSection(w, xml);
}

void WriteSummarySection(StreamWriter w, XElement? xml, Dictionary<string, string> typeIdMap, bool includeHeader = true)
{
	if (xml == null) return;
	var summary = xmlDocs.RenderDocumentation(xml.Element("summary"), typeIdMap);
	if (!string.IsNullOrEmpty(summary))
	{
		if (includeHeader)
			w.WriteLine("#### Summary");
		w.WriteLine(Helpers.EscapePlainText(summary));
		w.WriteLine();
	}
}

void WriteParamsSection(StreamWriter w, XElement? xml, MethodBase method, Dictionary<string, string> typeIdMap)
{
	if (xml == null) return;
	var paramElems = xml.Elements("param").ToList();
	if (paramElems.Count == 0) return;

	var paramDict = paramElems.ToDictionary(
		p => p.Attribute("name")?.Value ?? "",
		p => xmlDocs.RenderDocumentation(p, typeIdMap));

	w.WriteLine("#### Parameters");
	w.WriteLine();

	foreach (var p in method.GetParameters())
	{
		var name = p.Name ?? "";
		paramDict.TryGetValue(name, out var desc);
		desc ??= "";
		var typeName = LinkOrCodeType(p.ParameterType);
		w.WriteLine($"- `{name}` ({typeName}) – {Helpers.EscapePlainText(desc)}");
	}

	w.WriteLine();
}

void WriteRemarksSection(StreamWriter w, XElement? xml, Dictionary<string, string> typeIdMap)
{
	if (xml == null) return;
	var remarks = xmlDocs.RenderDocumentation(xml.Element("remarks"), typeIdMap);
	if (!string.IsNullOrEmpty(remarks))
	{
		w.WriteLine("#### Remarks");
		w.WriteLine(Helpers.EscapePlainText(remarks));
		w.WriteLine();
	}
}

void WriteReturnsSection(StreamWriter w, XElement? xml, MethodBase method, Dictionary<string, string> typeIdMap)
{
	if (xml == null) return;
	var mi = method as MethodInfo;
	var returnType = mi == null || mi.ReturnType == typeof(void) ? "" : LinkOrCodeType(mi.ReturnType);
	var returns = xmlDocs.RenderDocumentation(xml.Element("returns"), typeIdMap);
	
	if (string.IsNullOrEmpty(returnType) && string.IsNullOrEmpty(returns))
		return;

	w.WriteLine("#### Returns");
	if (!string.IsNullOrEmpty(returnType))
		w.WriteLine($"Type: {returnType}");
	if (!string.IsNullOrEmpty(returns))
	{
		w.WriteLine(Helpers.EscapePlainText(returns));
	}
	w.WriteLine();
}


void WriteExampleSection(StreamWriter w, XElement? xml)
{
	if (xml == null) return;
	var example = xml.Element("example");
	if (example == null) return;
	var text = example.Value?.Trim();
	if (string.IsNullOrEmpty(text)) return;

	w.WriteLine("#### Example");
	w.WriteLine();
	w.WriteLine("```csharp");
	w.WriteLine(text.Trim());
	w.WriteLine("```");
	w.WriteLine();
}



void WriteEventsSection(StreamWriter w, Type type, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, TypeOutputContext context)
{
	var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
		.ToList();
	if (events.Count == 0) return;

	if (context.SplitMembers)
	{
		WriteEventDocuments(type, events, xmlDocs, typeIdMap, context);
		return;
	}

	w.WriteLine("## Events");
	w.WriteLine();

	foreach (var ev in events.OrderBy(e => e.Name))
	{
		w.WriteLine($"### {ev.Name}");
		w.WriteLine();

		WriteEventContent(w, type, ev, xmlDocs, typeIdMap);
	}
}

void WriteMethodsSection(StreamWriter w, Type type, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, List<MethodInfo>? deprecatedMethods, TypeOutputContext context)
{
	var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(m => !(m.IsSpecialName && !LanguageFormatter.IsOperatorMethod(m)))
			.ToList();

	if (methods.Count == 0) return;

	var orderedMethods = methods
		.OrderBy(m => m.Name)
		.ThenBy(m => m.GetParameters().Length)
		.ThenBy(m => GetMethodHeading(type, m))
		.ToList();

	if (context.SplitMembers)
	{
		WriteMethodDocuments(type, orderedMethods, xmlDocs, typeIdMap, context);
		return;
	}

	if (moveDeprecatedMembers && deprecatedMethods != null)
	{
		deprecatedMethods.AddRange(orderedMethods.Where(IsDeprecated));
	}

	var methodsToRender = moveDeprecatedMembers
		? orderedMethods.Where(m => !IsDeprecated(m)).ToList()
		: orderedMethods;

	if (methodsToRender.Count == 0)
		return;

	w.WriteLine("## Methods");
	w.WriteLine();

	foreach (var method in methodsToRender)
	{
		WriteMethodBlock(w, type, method, xmlDocs, typeIdMap);
	}
}

void WriteMethodBlock(StreamWriter w, Type type, MethodInfo method, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap)
{
	w.WriteLine($"### {Helpers.EscapePlainText(GetMethodHeading(type, method))}");
	w.WriteLine();
	WriteMethodContent(w, type, method, xmlDocs, typeIdMap);
}

void WriteMethodContent(StreamWriter w, Type type, MethodInfo method, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap)
{
	xmlDocs.TryGetMethod(type, method, out var xml);

	WriteSummarySection(w, xml, typeIdMap, includeHeader: false);
	WriteDeprecationWarning(w, method);

	WriteLanguageTabs(w, formatter => formatter.FormatMethodSignature(type, method));

	WriteParamsSection(w, xml, method, typeIdMap);
	WriteReturnsSection(w, xml, method, typeIdMap);
	WriteRemarksSection(w, xml, typeIdMap);
	WriteExampleSection(w, xml);
}

void WriteExplicitInterfaceImplementationsSection(StreamWriter w, Type type)
{
	var explicitMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(m => m.IsPrivate && m.IsFinal && m.IsVirtual)
			.Where(m => m.Name.Contains("."))
			.ToList();

	if (explicitMethods.Count == 0)
		return;

	w.WriteLine("## Explicit Interface Implementations");
	w.WriteLine();

	foreach (var m in explicitMethods)
	{
		w.WriteLine("```csharp");
		w.WriteLine(csharpFormatter.FormatMethodSignature(type, m));
		w.WriteLine("```");
		w.WriteLine();
	}
}

void WriteDeprecatedPropertiesSection(StreamWriter w, Type type, List<PropertyInfo>? deprecatedProperties, Dictionary<string, string> typeIdMap, XmlDocumentation xmlDocs)
{
	if (!moveDeprecatedMembers || deprecatedProperties == null || deprecatedProperties.Count == 0)
		return;

	w.WriteLine("## Deprecated Properties");
	w.WriteLine();

	foreach (var prop in deprecatedProperties)
	{
		WritePropertyBlock(w, type, prop, xmlDocs, typeIdMap);
	}
}

void WriteDeprecatedMethodsSection(StreamWriter w, Type type, List<MethodInfo>? deprecatedMethods, Dictionary<string, string> typeIdMap, XmlDocumentation xmlDocs)
{
	if (!moveDeprecatedMembers || deprecatedMethods == null || deprecatedMethods.Count == 0)
		return;

	w.WriteLine("## Deprecated Methods");
	w.WriteLine();

	foreach (var method in deprecatedMethods)
	{
		WriteMethodBlock(w, type, method, xmlDocs, typeIdMap);
	}
}

void WriteDeprecatedConstructorsSection(StreamWriter w, Type type, List<ConstructorInfo>? deprecatedConstructors, Dictionary<string, string> typeIdMap, XmlDocumentation xmlDocs)
{
	if (!moveDeprecatedMembers || deprecatedConstructors == null || deprecatedConstructors.Count == 0)
		return;

	w.WriteLine("## Deprecated Constructors");
	w.WriteLine();

	foreach (var ctor in deprecatedConstructors)
	{
		WriteConstructorBlock(w, type, ctor, xmlDocs, typeIdMap);
	}
}

void WriteConstructorDocuments(Type type, List<ConstructorInfo> constructors, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, TypeOutputContext context)
{
	foreach (var ctor in constructors)
	{
		var memberSlug = GetMemberSlug(type, ctor);
		var heading = GetMethodHeading(type, ctor);
		var title = $"{GetDisplayTypeName(type)}.{heading}";
		xmlDocs.TryGetConstructor(type, ctor, out var xml);
		var summary = xmlDocs.GetSummaryText(xml, typeIdMap);
		WriteMemberDoc(type, context, memberSlug, title, heading, "Constructors", summary, writer =>
		{
			WriteConstructorContent(writer, type, ctor, xmlDocs, typeIdMap);
		});
	}
}

void WriteFieldDocuments(Type type, List<FieldInfo> fields, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, TypeOutputContext context)
{
	foreach (var field in fields.OrderBy(f => f.Name))
	{
		var memberSlug = GetMemberSlug(type, field);
		var title = $"{GetDisplayTypeName(type)}.{field.Name}";
		xmlDocs.TryGetField(type, field, out var xml);
		var summary = xmlDocs.GetSummaryText(xml, typeIdMap);
		WriteMemberDoc(type, context, memberSlug, title, field.Name, "Fields", summary, writer =>
		{
			WriteFieldContent(writer, type, field, xmlDocs, typeIdMap);
		});
	}
}

void WritePropertyDocuments(Type type, List<PropertyInfo> properties, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, TypeOutputContext context)
{
	foreach (var prop in properties)
	{
		var memberSlug = GetMemberSlug(type, prop);
		var title = $"{GetDisplayTypeName(type)}.{prop.Name}";
		xmlDocs.TryGetProperty(type, prop, out var xml);
		var summary = xmlDocs.GetSummaryText(xml, typeIdMap);
		WriteMemberDoc(type, context, memberSlug, title, prop.Name, "Properties", summary, writer =>
		{
			WritePropertyContent(writer, type, prop, xmlDocs, typeIdMap);
		});
	}
}

void WriteEventDocuments(Type type, List<EventInfo> events, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, TypeOutputContext context)
{
	foreach (var ev in events.OrderBy(e => e.Name))
	{
		var memberSlug = GetMemberSlug(type, ev);
		var title = $"{GetDisplayTypeName(type)}.{ev.Name}";
		xmlDocs.TryGetEvent(type, ev, out var xml);
		var summary = xmlDocs.GetSummaryText(xml, typeIdMap);
		WriteMemberDoc(type, context, memberSlug, title, ev.Name, "Events", summary, writer =>
		{
			WriteEventContent(writer, type, ev, xmlDocs, typeIdMap);
		});
	}
}

void WriteMethodDocuments(Type type, List<MethodInfo> methods, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap, TypeOutputContext context)
{
	foreach (var method in methods)
	{
		var memberSlug = GetMemberSlug(type, method);
		var heading = GetMethodHeading(type, method);
		var title = $"{GetDisplayTypeName(type)}.{heading}";
		xmlDocs.TryGetMethod(type, method, out var xml);
		var summary = xmlDocs.GetSummaryText(xml, typeIdMap);
		WriteMemberDoc(type, context, memberSlug, title, heading, "Methods", summary, writer =>
		{
			WriteMethodContent(writer, type, method, xmlDocs, typeIdMap);
		});
	}
}

void WriteMemberDoc(Type type, TypeOutputContext context, string memberSlug, string title, string sidebarLabel, string? sectionLabel, string summary, Action<StreamWriter> writeContent)
{
	var docId = $"{context.TypeSlug}--{memberSlug}";
	var filePath = Path.Combine(context.FileDirectory, $"{docId}.mdx");
	using var w = new StreamWriter(filePath);
	w.WriteLine("---");
	w.WriteLine($"id: {EscapeYaml(docId)}");
	w.WriteLine($"title: {EscapeYaml(title)}");
	w.WriteLine($"sidebar_label: {EscapeYaml(sidebarLabel)}");
	w.WriteLine("---");
	w.WriteLine();
	WriteTabsImports(w);
	w.WriteLine($"# {Helpers.EscapePlainText(title)}");
	w.WriteLine();
	w.WriteLine($"Parent type: [`{GetDisplayTypeName(type)}`](/api/{context.TypeSlug})");
	w.WriteLine();
	writeContent(w);
	if (context.SplitMembers)
	{
		if (!string.IsNullOrEmpty(sectionLabel))
		{
			var section = GetOrCreateSection(context, sectionLabel);
			section.DocIds.Add(docId);
		}
		context.TableEntries.Add(new MemberTableEntry(sectionLabel ?? "Members", sidebarLabel, docId, string.IsNullOrWhiteSpace(summary) ? "No description available." : summary));
	}
}

void WriteMemberOverview(StreamWriter w, TypeOutputContext context)
{
	w.WriteLine("## Member Overview");
	w.WriteLine();
	var groups = context.TableEntries
		.GroupBy(e => e.Section)
		.OrderBy(g => GetSectionOrder(g.Key))
		.ThenBy(g => g.Key, StringComparer.Ordinal);

	foreach (var group in groups)
	{
		w.WriteLine($"### {group.Key}");
		w.WriteLine();
		w.WriteLine("| Member | Description |");
		w.WriteLine("| --- | --- |");
		foreach (var entry in group.OrderBy(e => e.Label, StringComparer.Ordinal))
		{
			w.WriteLine($"| [{Helpers.EscapePlainText(entry.Label)}](/api/{entry.DocId}) | {FormatTableCell(entry.Description)} |");
		}
		w.WriteLine();
	}
}

static void WriteSidebar(string outputRoot, SortedDictionary<string, List<SidebarEntry>> sidebarMap, Dictionary<string, string> namespaceSlugMap)
{
	// Write sidebar
	var sidebarPath = Path.Combine(outputRoot, "sidebars-api.cjs");
	using (var sw = new StreamWriter(sidebarPath))
	{
		sw.WriteLine("module.exports = {");
		sw.WriteLine("  apiSidebar: [");
		sw.WriteLine("    { type: 'doc', id: 'index', label: 'Namespaces' },");
		sw.WriteLine();

		bool firstNs = true;
		foreach (var kvp in sidebarMap)
		{
			if (!firstNs) sw.WriteLine(",");
			firstNs = false;

			var nsLabel = kvp.Key == "(Global)" ? "Global" : kvp.Key;

			sw.WriteLine("    {");
			sw.WriteLine("      type: 'category',");
			sw.WriteLine($"      label: {ToJsString(nsLabel)},");
			if (namespaceSlugMap.TryGetValue(kvp.Key, out var slug))
			{
				sw.WriteLine("      link: {");
				sw.WriteLine("        type: 'generated-index',");
				sw.WriteLine($"        slug: {ToJsString(slug)},");
				sw.WriteLine($"        title: {ToJsString(nsLabel + " Namespace")},");
				sw.WriteLine("      },");
			}
			sw.WriteLine("      items: [");
			for (int i = 0; i < kvp.Value.Count; i++)
			{
				var entry = kvp.Value[i];
				if (entry.Sections is { Count: > 0 })
				{
					sw.WriteLine("        {");
					sw.WriteLine("          type: 'category',");
					sw.WriteLine($"          label: {ToJsString(entry.Label)},");
					sw.WriteLine("          link: {");
					sw.WriteLine("            type: 'doc',");
					sw.WriteLine($"            id: {ToJsString(entry.DocId)},");
					sw.WriteLine("          },");
					sw.WriteLine("          items: [");
					for (int j = 0; j < entry.Sections.Count; j++)
					{
						var section = entry.Sections[j];
						sw.WriteLine("            {");
						sw.WriteLine("              type: 'category',");
						sw.WriteLine($"              label: {ToJsString(section.Label)},");
						sw.WriteLine("              items: [");
						for (int k = 0; k < section.DocIds.Count; k++)
						{
							var childId = section.DocIds[k];
							sw.Write($"                {ToJsString(childId)}");
							if (k < section.DocIds.Count - 1)
								sw.Write(",");
							sw.WriteLine();
						}
						sw.WriteLine("              ],");
						sw.Write("            }");
						if (j < entry.Sections.Count - 1)
							sw.Write(",");
						sw.WriteLine();
					}
					sw.WriteLine("          ],");
					sw.Write("        }");
				}
				else
				{
					sw.Write($"        {ToJsString(entry.DocId)}");
				}

				if (i < kvp.Value.Count - 1)
					sw.Write(",");
				sw.WriteLine();
			}
			sw.WriteLine("      ],");
			sw.WriteLine("    }");
		}

		sw.WriteLine();
		sw.WriteLine("  ],");
		sw.WriteLine("};");
	}
	Console.WriteLine($"Generated sidebar: {sidebarPath}");
}

static void WriteApiIndex(string outputRoot, SortedDictionary<string, List<SidebarEntry>> sidebarMap, Dictionary<string, string> namespaceSlugMap)
{
	var indexPath = Path.Combine(outputRoot, "index.mdx");
	using var w = new StreamWriter(indexPath);

	w.WriteLine("---");
	w.WriteLine("id: index");
	w.WriteLine("title: API Reference");
	w.WriteLine("slug: /");
	w.WriteLine("sidebar_label: Namespaces");
	w.WriteLine("---");
	w.WriteLine();
	w.WriteLine("# API Reference");
	w.WriteLine();
	w.WriteLine("Browse the namespaces below to explore the available types.");
	w.WriteLine();
	w.WriteLine("## Namespaces");
	w.WriteLine();

	foreach (var kvp in sidebarMap)
	{
		var nsLabel = kvp.Key == "(Global)" ? "Global" : kvp.Key;
		if (namespaceSlugMap.TryGetValue(kvp.Key, out var slug) && !string.IsNullOrEmpty(slug))
		{
			w.WriteLine($"- [{nsLabel}](/api{slug})");
		}
		else
		{
			w.WriteLine($"- {nsLabel}");
		}
	}

	w.WriteLine();

	Console.WriteLine($"Generated API index: {indexPath}");
}


string GetMemberSlug(Type type, MemberInfo member)
{
	var slug = member switch
	{
		ConstructorInfo ctor => $"ctor{FormatParameterSlug(ctor.GetParameters())}",
		MethodInfo method => $"{method.Name}{FormatParameterSlug(method.GetParameters())}",
		PropertyInfo prop => $"property-{prop.Name}",
		EventInfo ev => $"event-{ev.Name}",
		FieldInfo field => $"field-{field.Name}",
		_ => $"member-{member.Name}"
	};
	return SanitizeSlug(slug);
}

string FormatParameterSlug(ParameterInfo[] parameters)
{
	if (parameters.Length == 0)
		return "";
	var parts = parameters.Select(p => GetShortTypeNameForSlug(p.ParameterType));
	return "-" + string.Join("-", parts.Select(SanitizeSlug));
}

string GetShortTypeNameForSlug(Type type)
{
	if (type.IsByRef)
		return GetShortTypeNameForSlug(type.GetElementType()!);
	if (type.IsGenericType)
	{
		var name = type.Name;
		var tick = name.IndexOf('`');
		if (tick >= 0) name = name[..tick];
		var args = type.GetGenericArguments().Select(GetShortTypeNameForSlug);
		return $"{name}-{string.Join("-", args)}";
	}
	return type.Name;
}

string SanitizeSlug(string value)
{
	if (string.IsNullOrEmpty(value))
		return "member";
	var sb = new StringBuilder();
	char previous = '\0';
	foreach (var ch in value)
	{
		char c = char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-';
		if (c != '-' || previous != '-')
			sb.Append(c);
		previous = c;
	}
	var result = sb.ToString().Trim('-');
	return string.IsNullOrEmpty(result) ? "member" : result;
}

int GetSectionOrder(string section)
{
	return section switch
	{
		"Constructors" => 0,
		"Fields" => 1,
		"Properties" => 2,
		"Events" => 3,
		"Methods" => 4,
		_ => 5
	};
}

string FormatTableCell(string text)
{
	if (string.IsNullOrWhiteSpace(text))
		return "";
	return text.Replace("|", "\\|").Replace("\n", "<br />");
}

SectionEntry GetOrCreateSection(TypeOutputContext context, string label)
{
	context.Entry.Sections ??= new List<SectionEntry>();
	var section = context.Entry.Sections.FirstOrDefault(s => string.Equals(s.Label, label, StringComparison.Ordinal));
	if (section == null)
	{
		section = new SectionEntry(label, new List<string>());
		context.Entry.Sections.Add(section);
	}
	return section;
}

NuGetPackageInfo? GetPackageInfoForAssembly(Assembly assembly)
{
	if (assembly == null)
		return null;

	var location = assembly.Location;
	if (string.IsNullOrEmpty(location))
		return null;

	return assemblyPackageMap.TryGetValue(location, out var info) ? info : null;
}

string FormatPackageDisplay(NuGetPackageInfo package)
{
	if (string.IsNullOrEmpty(package.Version))
		return package.Id;
	return $"{package.Id} {package.Version}";
}

string GetNuGetPackageUrl(NuGetPackageInfo package)
{
	var idSegment = Uri.EscapeDataString(package.Id);
	if (string.IsNullOrEmpty(package.Version))
		return $"https://www.nuget.org/packages/{idSegment}/";

	var versionSegment = Uri.EscapeDataString(package.Version);
	return $"https://www.nuget.org/packages/{idSegment}/{versionSegment}/";
}

// ---------------- misc helpers ----------------

static string EscapeYaml(string value) => value.Replace("\"", "\\\"");

static string ToJsString(string value)
	=> "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

void WriteFieldContent(StreamWriter w, Type type, FieldInfo field, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap)
{
	WriteLanguageTabs(w, formatter => formatter.FormatFieldDeclaration(field));

	if (xmlDocs.TryGetField(type, field, out var xml))
	{
		WriteSummarySection(w, xml, typeIdMap);
		WriteRemarksSection(w, xml, typeIdMap);
		WriteExampleSection(w, xml);
	}
}

void WriteEventContent(StreamWriter w, Type type, EventInfo ev, XmlDocumentation xmlDocs, Dictionary<string, string> typeIdMap)
{
	WriteLanguageTabs(w, formatter => formatter.FormatEventDeclaration(ev));

	if (xmlDocs.TryGetEvent(type, ev, out var xml))
	{
		WriteSummarySection(w, xml, typeIdMap);
		WriteRemarksSection(w, xml, typeIdMap);
		WriteExampleSection(w, xml);
	}
}

record struct TypeOutputContext(
	bool SplitMembers,
	string TypeSlug,
	string FileDirectory,
	SidebarEntry Entry,
	List<MemberTableEntry> TableEntries);

record class SidebarEntry
{
	public string DocId { get; init; }
	public string Label { get; init; }
	public List<SectionEntry>? Sections { get; set; }

	public SidebarEntry(string docId, string label, List<SectionEntry>? sections = null)
	{
		DocId = docId;
		Label = label;
		Sections = sections;
	}
}

record class SectionEntry(string Label, List<string> DocIds);

record class MemberTableEntry(string Section, string Label, string DocId, string Description);

record class AssemblyInput(string AssemblyPath, string? XmlDocPath, NuGetPackageInfo? Package = null);

record class NuGetAssemblyCandidate(ZipArchiveEntry Entry, string Framework, string NormalizedFramework);

record class NuGetPackageInfo(string Id, string Version);

record struct FrameworkSelection(string Framework, string NormalizedFramework, List<NuGetAssemblyCandidate> Candidates, NuGetAssemblyCandidate PrimaryCandidate);

record struct FrameworkExtractionResult(string? PrimaryAssemblyPath, string? PrimaryXmlPath);

record struct NuGetDependency(string Id, string? VersionSpec);
