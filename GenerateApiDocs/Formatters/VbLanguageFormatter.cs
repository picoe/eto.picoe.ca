using System.Globalization;
using System.Reflection;

namespace GenerateApiDocs.Formatters;

sealed class VbLanguageFormatter : LanguageFormatter
{
	public override string DisplayName => "VB";
	public override string LanguageKey => "vbnet";

	public override string FormatMethodSignature(Type type, MethodBase method)
	{
		bool isCtor = method.IsConstructor;
		string name = isCtor ? FormatTypeName(type) : method.Name;

		var accessibility = GetAccessibility(method);
		var paramPieces = method.GetParameters().Select(parameter =>
		{
			var parameterType = parameter.ParameterType;
			bool byRef = parameterType.IsByRef;
			if (byRef) parameterType = parameterType.GetElementType()!;
			string mode = byRef ? "ByRef " : "ByVal ";
			var defaultSuffix = FormatDefaultValue(parameter);
			return $"{mode}{parameter.Name} As {FormatTypeName(parameterType)}{defaultSuffix}";
		});

		if (method is MethodInfo mi)
		{
			var retType = mi.ReturnType;
			if (retType == typeof(void))
				return $"{accessibility} Sub {name}({string.Join(", ", paramPieces)})";
			else
				return $"{accessibility} Function {name}({string.Join(", ", paramPieces)}) As {FormatTypeName(retType)}";
		}

		return $"{accessibility} Sub {name}({string.Join(", ", paramPieces)})";
	}

	public override string FormatPropertySignature(PropertyInfo property)
	{
		var accessor = property.CanRead && property.GetMethod!.IsPublic && property.CanWrite && property.SetMethod!.IsPublic
			? "Get, Set"
			: property.CanRead && property.GetMethod!.IsPublic
				? "Get"
				: property.CanWrite && property.SetMethod!.IsPublic
					? "Set"
					: "";

		var staticModifier = (property.GetMethod != null && property.GetMethod.IsStatic) || (property.SetMethod != null && property.SetMethod.IsStatic)
			? "Shared "
			: "";

		return $"{GetAccessibility(property.GetMethod ?? property.SetMethod!)} {staticModifier}Property {property.Name} As {FormatTypeName(property.PropertyType)} {accessor}";
	}

	public override string FormatFieldDeclaration(FieldInfo field)
	{
		var staticModifier = field.IsStatic ? "Shared " : string.Empty;
		return $"{staticModifier}Dim {field.Name} As {FormatTypeName(field.FieldType)}";
	}

	public override string FormatEventDeclaration(EventInfo eventInfo)
	{
		return $"Event {eventInfo.Name} As {FormatTypeName(eventInfo.EventHandlerType!)}";
	}

	public override string FormatTypeName(Type type)
	{
		if (type.IsByRef) type = type.GetElementType()!;
		if (type.IsArray) return $"{FormatTypeName(type.GetElementType()!)}()";

		if (Nullable.GetUnderlyingType(type) is Type innerNullable)
			return FormatTypeName(innerNullable) + "?";

		if (!type.IsGenericType)
		{
			return type.FullName switch
			{
				"System.Void" => "Void",
				"System.String" => "String",
				"System.Int32" => "Integer",
				"System.Int64" => "Long",
				"System.Boolean" => "Boolean",
				"System.Object" => "Object",
				"System.Double" => "Double",
				"System.Single" => "Single",
				"System.Decimal" => "Decimal",
				"System.Char" => "Char",
				_ => type.Name
			};
		}

		var def = type.GetGenericTypeDefinition();
		var name = def.Name;
		var tick = name.IndexOf('`');
		if (tick >= 0) name = name.Substring(0, tick);
		var args = type.GetGenericArguments().Select(FormatTypeName);
		return $"{name}(Of {string.Join(", ", args)})";
	}

	string FormatDefaultValue(ParameterInfo parameter)
	{
		if (!TryGetDefaultValue(parameter, out var value))
			return string.Empty;
		return $" = {FormatLiteral(value)}";
	}

	string FormatLiteral(object? value)
	{
		if (value == null)
			return "Nothing";
		return value switch
		{
			bool b => b ? "True" : "False",
			string s => $"\"{s.Replace("\"", "\"\"")}\"",
			char c => $"ChrW({(int)c})",
			Enum e => $"{FormatTypeName(e.GetType())}.{Enum.GetName(e.GetType(), e)}",
			_ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "Nothing"
		};
	}

	static string GetAccessibility(MethodBase method)
	{
		if (method.IsPublic) return "Public";
		if (method.IsFamily) return "Protected";
		if (method.IsAssembly) return "Friend";
		if (method.IsFamilyOrAssembly) return "Protected Friend";
		if (method.IsPrivate) return "Private";
		return "Private";
	}
}
