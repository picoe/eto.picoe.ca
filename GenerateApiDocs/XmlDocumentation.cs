using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace GenerateApiDocs;

class XmlDocumentation
{
	readonly Dictionary<XmlMemberKey, XElement> index;

	XmlDocumentation(Dictionary<XmlMemberKey, XElement> index)
	{
		this.index = index;
	}

	public static XmlDocumentation Build(IEnumerable<XElement> xmlMembers)
	{
		var dict = new Dictionary<XmlMemberKey, XElement>();
		foreach (var member in xmlMembers)
		{
			var nameAttr = member.Attribute("name")?.Value;
			if (string.IsNullOrWhiteSpace(nameAttr) || nameAttr.Length < 3)
				continue;

			if (XmlKeyBuilder.TryParse(nameAttr, out var key))
				dict[key] = member;
		}
		return new XmlDocumentation(dict);
	}

	public bool TryGetType(Type type, out XElement? xml)
		=> index.TryGetValue(XmlKeyBuilder.GetTypeKey(type), out xml);

	public bool TryGetConstructor(Type type, ConstructorInfo ctor, out XElement? xml)
		=> index.TryGetValue(XmlKeyBuilder.GetMemberKey(type, ctor), out xml);

	public bool TryGetMethod(Type type, MethodInfo method, out XElement? xml)
		=> index.TryGetValue(XmlKeyBuilder.GetMemberKey(type, method), out xml);

	public bool TryGetProperty(Type type, PropertyInfo prop, [NotNullWhen(true)] out XElement? xml)
		=> index.TryGetValue(XmlKeyBuilder.GetPropertyKey(type, prop), out xml);

	public bool TryGetField(Type type, FieldInfo field, [NotNullWhen(true)] out XElement? xml)
		=> index.TryGetValue(XmlKeyBuilder.GetFieldKey(type, field), out xml);

	public bool TryGetEvent(Type type, EventInfo ev, [NotNullWhen(true)] out XElement? xml)
		=> index.TryGetValue(XmlKeyBuilder.GetEventKey(type, ev), out xml);

	public string GetSummaryText(XElement? xml, Dictionary<string, string> typeIdMap)
	{
		if (xml == null)
			return "";
		return RenderDocumentation(xml.Element("summary"), typeIdMap).Trim();
	}

	string RenderLinkOrCodeType(Type type, Dictionary<string, string> typeIdMap, bool includeLink = true)
	{
		var normalized = NormalizeTypeName(type);
		if (includeLink && typeIdMap.TryGetValue(normalized, out var id))
		{
			return $"[`{type.Name}`](/api/{id})";
		}
		return $"`{type.Name}`";
	}

	public string RenderDocumentation(XElement? element, Dictionary<string, string> typeIdMap)
	{
		if (element == null) return "";
		return RenderDocumentationNodes(element.Nodes(), typeIdMap).Trim();
	}

	string RenderDocumentationNodes(IEnumerable<XNode> nodes, Dictionary<string, string> typeIdMap)
	{
		var sb = new StringBuilder();
		foreach (var node in nodes)
		{
			sb.Append(RenderDocumentationNode(node, typeIdMap));
		}
		return sb.ToString();
	}

	string RenderDocumentationNode(XNode node, Dictionary<string, string> typeIdMap)
	{
		return node switch
		{
			XText text => Helpers.EscapePlainText(text.Value),
			XElement element => RenderDocumentationElement(element, typeIdMap),
			_ => ""
		};
	}

	string RenderDocumentationElement(XElement element, Dictionary<string, string> typeIdMap)
	{
		var name = element.Name.LocalName.ToLowerInvariant();
		return name switch
		{
			"see" => RenderSeeElement(element, typeIdMap),
			"paramref" => RenderParamRefElement(element),
			"c" => $"`{EscapeInlineCode(element.Value)}`",
			"code" => FormatCodeBlock(element.Value ?? string.Empty),
			"para" => FormatParagraph(element, typeIdMap),
			"br" => "\n\n",
			_ => RenderDocumentationNodes(element.Nodes(), typeIdMap)
		};
	}

	string RenderSeeElement(XElement element, Dictionary<string, string> typeIdMap)
	{
		var href = element.Attribute("href")?.Value;
		var cref = element.Attribute("cref")?.Value;
		var langword = element.Attribute("langword")?.Value;
		var inner = RenderDocumentationNodes(element.Nodes(), typeIdMap).Trim();
		if (!string.IsNullOrEmpty(href))
		{
			var label = string.IsNullOrEmpty(inner) ? href : inner;
			return $"[{label}]({href})";
		}
		if (!string.IsNullOrEmpty(langword))
		{
			return $"`{EscapeInlineCode(langword)}`";
		}
		if (!string.IsNullOrEmpty(cref))
		{
			var (prefix, target) = SplitCref(cref);
			return prefix switch
			{
				"T" => RenderTypeReference(target, inner, typeIdMap),
				"M" or "P" or "F" or "E" => RenderMemberReference(target, inner, typeIdMap),
				_ => $"`{(string.IsNullOrEmpty(inner) ? target : inner)}`"
			};
		}
		return inner;
	}

	string RenderTypeReference(string target, string inner, Dictionary<string, string> typeIdMap)
	{
		var normalized = NormalizeTypeNameString(target);
		var display = string.IsNullOrEmpty(inner) ? GetShortTypeDisplayName(normalized) : inner;
		if (typeIdMap.TryGetValue(normalized, out var id))
		{
			return $"[{display}](/api/{id})";
		}
		return $"`{display}`";
	}
	
	string RenderMemberReference(string target, string inner, Dictionary<string, string> typeIdMap)
	{
		if (string.IsNullOrEmpty(target))
			return inner;

		var memberTarget = target;
		string paramTarget = string.Empty;
		int paramIdx = memberTarget.IndexOf('(');
		if (paramIdx >= 0)
		{
			paramTarget = memberTarget[(paramIdx + 1)..^1];
			memberTarget = memberTarget[..paramIdx];
			var paramTypes = ParseParameterTypeList(paramTarget)
				.Select(t => GetParameterizedTypeName(t))
				.ToList();
			paramTarget = string.Join("_", paramTypes).ToLowerInvariant();
		}

		var lastDot = memberTarget.LastIndexOf('.');
		if (lastDot < 0)
		{
			var displayOnly = string.IsNullOrEmpty(inner) ? memberTarget : inner;
			return $"`{displayOnly}`";
		}

		var typeName = NormalizeTypeNameString(memberTarget[..lastDot]);
		var memberName = memberTarget[(lastDot + 1)..];
		var display = string.IsNullOrEmpty(inner) ? $"{GetShortTypeDisplayName(typeName)}.{memberName}" : inner;

		if (typeIdMap.TryGetValue(typeName, out var id))
		{
		return $"[{display}](/api/{id}#{GetMemberDisplayName(memberName)?.ToLowerInvariant()}{Helpers.EscapePlainText(paramTarget)})";
		}

		return $"`{display}`";
	}

	static string GetShortTypeDisplayName(string fullName)
	{
		if (string.IsNullOrEmpty(fullName))
			return "";
		var clean = fullName.Replace('+', '.');
		var tick = clean.IndexOf('`');
		if (tick >= 0)
			clean = clean[..tick];
		var lastDot = clean.LastIndexOf('.');
		return lastDot >= 0 ? clean[(lastDot + 1)..] : clean;
	}

	static string GetMemberDisplayName(string cref)
	{
		if (string.IsNullOrEmpty(cref))
			return "";
		var colon = cref.IndexOf(':');
		var clean = colon >= 0 ? cref[(colon + 1)..] : cref;
		clean = clean.Replace('+', '.');
		var lastDot = clean.LastIndexOf('.');
		if (lastDot >= 0 && lastDot < clean.Length - 1)
			return clean[(lastDot + 1)..];
		return clean;
	}

	static string EscapeInlineCode(string code) => (code ?? string.Empty).Replace("`", "\\`").Trim();

	static string FormatParagraph(XElement element, Dictionary<string, string> typeIdMap)
	{
		var content = RenderStaticNodes(element.Nodes(), typeIdMap).Trim();
		if (string.IsNullOrEmpty(content))
			return "\n\n";
		return $"\n\n{content}\n\n";
	}

	static string RenderStaticNodes(IEnumerable<XNode> nodes, Dictionary<string, string> typeIdMap)
	{
		var sb = new StringBuilder();
		foreach (var node in nodes)
		{
			sb.Append(node switch
			{
				XText text => Helpers.EscapePlainText(text.Value),
				XElement element => RenderStaticElement(element, typeIdMap),
				_ => ""
			});
		}
		return sb.ToString();
	}

	static string RenderStaticElement(XElement element, Dictionary<string, string> typeIdMap)
	{
		var name = element.Name.LocalName.ToLowerInvariant();
		return name switch
		{
			"see" => RenderStaticSee(element, typeIdMap),
			_ => RenderStaticNodes(element.Nodes(), typeIdMap)
		};
	}

	static string RenderStaticSee(XElement element, Dictionary<string, string> typeIdMap)
	{
		var inner = RenderStaticNodes(element.Nodes(), typeIdMap).Trim();
		var cref = element.Attribute("cref")?.Value ?? "";
		if (string.IsNullOrEmpty(cref))
			return inner;
		return $"`{inner}`";
	}

	static string RenderParamRefElement(XElement element)
	{
		var name = element.Attribute("name")?.Value;
		if (string.IsNullOrEmpty(name))
			return "";
		return $"`{EscapeInlineCode(name)}`";
	}

	static string FormatCodeBlock(string code)
	{
		var trimmed = code.Trim('\r', '\n');
		if (string.IsNullOrEmpty(trimmed))
			return "";
		return $"\n```\n{trimmed}\n```\n";
	}

	(string prefix, string target) SplitCref(string cref)
	{
		var colon = cref.IndexOf(':');
		if (colon <= 0)
			return ("", cref);
		return (cref[..colon].ToUpperInvariant(), cref[(colon + 1)..]);
	}

	static string GetParameterizedTypeName(string t)
	{
		var type = Type.GetType(t, false);
		if (type == null)
			return t;
		return NormalizeTypeName(type);
	}

	internal static string NormalizeTypeName(Type t)
	{
		var full = t.FullName ?? t.Name;
		return full.Replace('+', '.');
	}

	internal static string NormalizeTypeNameString(string full)
	{
		return full.Replace('+', '.');
	}

	static List<string> ParseParameterTypeList(string paramList)
	{
		var list = new List<string>();
		if (string.IsNullOrWhiteSpace(paramList))
			return list;

		var current = new StringBuilder();
		int depth = 0;
		foreach (var ch in paramList)
		{
			if (ch == '{')
			{
				depth++;
				current.Append(ch);
			}
			else if (ch == '}')
			{
				depth = Math.Max(0, depth - 1);
				current.Append(ch);
			}
			else if (ch == ',' && depth == 0)
			{
				list.Add(NormalizeDocParameterType(current.ToString()));
				current.Clear();
			}
			else
			{
				current.Append(ch);
			}
		}

		if (current.Length > 0)
			list.Add(NormalizeDocParameterType(current.ToString()));

		return list;
	}

	static string NormalizeDocParameterType(string typeName)
	{
		if (string.IsNullOrWhiteSpace(typeName))
			return "";

		var sb = new StringBuilder();
		foreach (var ch in typeName.Trim())
		{
			if (!char.IsWhiteSpace(ch))
				sb.Append(ch);
		}
		return sb.ToString().Replace('+', '.');
	}

	static class XmlKeyBuilder
	{
		public static bool TryParse(string name, out XmlMemberKey key)
		{
			key = default;
			char kind = name[0];
			if (name[1] != ':')
				return false;

			var rest = name.Substring(2);
			if (kind == 'T')
			{
				key = new XmlMemberKey(kind, NormalizeTypeNameString(rest), "", "");
				return true;
			}

			ParseXmlMember(rest, out var typeFull, out var memberName, out var paramList);
			var typeNorm = NormalizeTypeNameString(typeFull);
			var paramSignature = BuildDocParamSignature(paramList);
			key = new XmlMemberKey(kind, typeNorm, memberName, paramSignature);
			return true;
		}

		static void ParseXmlMember(string rest, out string typeName, out string memberName, out string paramList)
		{
			paramList = "";
			string beforeParams = rest;
			int parenIdx = rest.IndexOf('(');
			if (parenIdx >= 0)
			{
				beforeParams = rest.Substring(0, parenIdx);
				paramList = rest.Substring(parenIdx + 1, rest.Length - parenIdx - 2);
			}

			int lastDot = beforeParams.LastIndexOf('.');
			if (lastDot <= 0)
			{
				typeName = beforeParams;
				memberName = "";
				return;
			}

			typeName = beforeParams.Substring(0, lastDot);
			memberName = beforeParams[(lastDot + 1)..];
		}

		static string BuildDocParamSignature(string paramList)
		{
			var types = ParseParameterTypeList(paramList);
			if (types.Count == 0)
				return "";
			return string.Join("|", types);
		}

		public static XmlMemberKey GetTypeKey(Type t)
		{
			return new XmlMemberKey('T', NormalizeTypeName(t), "", "");
		}

		public static XmlMemberKey GetMemberKey(Type type, MethodBase method)
		{
			string memberName = method.IsConstructor ? "#ctor" : method.Name;
			var typeNorm = NormalizeTypeName(type);
			var paramSignature = BuildReflectionParamSignature(method.GetParameters());
			return new XmlMemberKey('M', typeNorm, memberName, paramSignature);
		}

		public static XmlMemberKey GetPropertyKey(Type type, PropertyInfo prop)
		{
			var typeNorm = NormalizeTypeName(type);
			var paramSignature = BuildReflectionParamSignature(prop.GetIndexParameters());
			return new XmlMemberKey('P', typeNorm, prop.Name, paramSignature);
		}

		public static XmlMemberKey GetFieldKey(Type type, FieldInfo field)
			=> new XmlMemberKey('F', NormalizeTypeName(type), field.Name, "");

		public static XmlMemberKey GetEventKey(Type type, EventInfo ev)
			=> new XmlMemberKey('E', NormalizeTypeName(type), ev.Name, "");

		static string BuildReflectionParamSignature(IEnumerable<ParameterInfo> parameters)
		{
			var types = parameters.Select(p => GetXmlDocTypeName(p.ParameterType));
			var list = types.Where(t => !string.IsNullOrEmpty(t)).ToList();
			if (list.Count == 0)
				return "";
			return string.Join("|", list);
		}

		static string GetXmlDocTypeName(Type type)
		{
			if (type.IsByRef)
				return GetXmlDocTypeName(type.GetElementType()!) + "&";
			if (type.IsPointer)
				return GetXmlDocTypeName(type.GetElementType()!) + "*";
			if (type.IsArray)
			{
				var ranksb = new StringBuilder();
				ranksb.Append('[');
				ranksb.Append(new string(',', type.GetArrayRank() - 1));
				ranksb.Append(']');
				return GetXmlDocTypeName(type.GetElementType()!) + ranksb;
			}
			if (type.IsGenericParameter)
				return type.Name;
			if (type.IsGenericType)
			{
				var genericTypeDef = type.GetGenericTypeDefinition();
				var name = (genericTypeDef.FullName ?? genericTypeDef.Name) ?? "";
				var tick = name.IndexOf('`');
				if (tick >= 0)
					name = name[..tick];
				name = name.Replace('+', '.');
				var args = type.GetGenericArguments().Select(GetXmlDocTypeName);
				return $"{name}{{{string.Join(",", args)}}}";
			}
			var fullName = type.FullName ?? type.Name ?? "";
			return fullName.Replace('+', '.');
		}
	}
}

readonly record struct XmlMemberKey(char Kind, string TypeName, string MemberName, string ParamSignature);
