using System;
using System.Collections.Generic;
using System.IO;

namespace GenerateApiDocs;

class SidebarWriter
{
	public SortedDictionary<string, List<SidebarEntry>> SidebarMap { get; } = new(StringComparer.Ordinal);
	public Dictionary<string, string> NamespaceSlugs { get; } = new(StringComparer.Ordinal);

	public SidebarEntry AddType(string nsKey, string nsSlugPath, string docId, string label, bool splitMembers)
	{
		if (!SidebarMap.TryGetValue(nsKey, out var list))
		{
			list = new List<SidebarEntry>();
			SidebarMap[nsKey] = list;
		}

		var entry = new SidebarEntry(docId, label, splitMembers ? new List<SectionEntry>() : null);
		list.Add(entry);

		if (!NamespaceSlugs.ContainsKey(nsKey))
		{
			var slug = string.IsNullOrEmpty(nsSlugPath) ? "/global" : "/" + nsSlugPath;
			NamespaceSlugs[nsKey] = slug;
		}

		return entry;
	}

	public void WriteSidebar(string outputRoot)
	{
		var sidebarPath = Path.Combine(outputRoot, "sidebars-api.cjs");
		using (var sw = new StreamWriter(sidebarPath))
		{
			sw.WriteLine("module.exports = {");
			sw.WriteLine("  apiSidebar: [");
			sw.WriteLine("    { type: 'doc', id: 'index', label: 'Namespaces' },");
			sw.WriteLine();

			bool firstNs = true;
			foreach (var kvp in SidebarMap)
			{
				if (!firstNs) sw.WriteLine(",");
				firstNs = false;

				var nsLabel = kvp.Key == "(Global)" ? "Global" : kvp.Key;

				sw.WriteLine("    {");
				sw.WriteLine("      type: 'category',");
				sw.WriteLine($"      label: {Helpers.ToJsString(nsLabel)},");
				if (NamespaceSlugs.TryGetValue(kvp.Key, out var slug))
				{
					sw.WriteLine("      link: {");
					sw.WriteLine("        type: 'generated-index',");
					sw.WriteLine($"        slug: {Helpers.ToJsString(slug)},");
					sw.WriteLine($"        title: {Helpers.ToJsString("Namespace: " + nsLabel)},");
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
						sw.WriteLine($"          label: {Helpers.ToJsString(entry.Label)},");
						sw.WriteLine("          link: {");
						sw.WriteLine("            type: 'doc',");
						sw.WriteLine($"            id: {Helpers.ToJsString(entry.DocId)},");
						sw.WriteLine("          },");
						sw.WriteLine("          items: [");
						for (int j = 0; j < entry.Sections.Count; j++)
						{
							var section = entry.Sections[j];
							sw.WriteLine("            {");
							sw.WriteLine("              type: 'category',");
							sw.WriteLine($"              label: {Helpers.ToJsString(section.Label)},");
							sw.WriteLine("              items: [");
							for (int k = 0; k < section.DocIds.Count; k++)
							{
								var childId = section.DocIds[k];
								sw.Write($"                {Helpers.ToJsString(childId)}");
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
						sw.Write($"        {Helpers.ToJsString(entry.DocId)}");
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

	public void WriteApiIndex(string outputRoot)
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

		foreach (var kvp in SidebarMap)
		{
			var nsLabel = kvp.Key == "(Global)" ? "Global" : kvp.Key;
			if (NamespaceSlugs.TryGetValue(kvp.Key, out var slug) && !string.IsNullOrEmpty(slug))
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
}
