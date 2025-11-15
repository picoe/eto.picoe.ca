using System.Globalization;
using System.Reflection;

namespace GenerateApiDocs.Formatters;

sealed class FSharpLanguageFormatter : LanguageFormatter
{
	public override string DisplayName => "F#";
	public override string LanguageKey => "fsharp";

	public override string FormatMethodSignature(Type type, MethodBase method)
	{
		bool isCtor = method.IsConstructor;
		string name = isCtor ? FormatTypeName(type) : method.Name;

		var paramPieces = method.GetParameters().Select(parameter =>
		{
			var parameterType = parameter.ParameterType;
			if (parameterType.IsByRef)
				parameterType = parameterType.GetElementType()!;
			var defaultSuffix = FormatDefaultValue(parameter);
			return $"{parameter.Name}:{FormatTypeName(parameterType)}{defaultSuffix}";
		});

		string retType = "unit";
		if (method is MethodInfo mi && mi.ReturnType != typeof(void))
			retType = FormatTypeName(mi.ReturnType);

		var accessibility = GetAccessibility(method);
		return $"{accessibility} this.{name} ({string.Join(", ", paramPieces)}) : {retType}";
	}

	public override string FormatPropertySignature(PropertyInfo property)
	{
		var withSection = property.CanRead && property.GetMethod!.IsPublic && property.CanWrite && property.SetMethod!.IsPublic
			? " with get, set"
			: property.CanRead && property.GetMethod!.IsPublic
				? " with get"
				: property.CanWrite && property.SetMethod!.IsPublic
					? " with set"
					: "";

		var staticModifier = property.GetMethod != null && property.GetMethod.IsStatic || property.SetMethod != null && property.SetMethod.IsStatic
			? "static "
			: "";

		return $"{GetAccessibility(property.GetMethod ?? property.SetMethod!)} {staticModifier}{property.Name} : {FormatTypeName(property.PropertyType)}{withSection}";
	}

	public override string FormatFieldDeclaration(FieldInfo field)
	{
		return $"val {field.Name} : {FormatTypeName(field.FieldType)}";
	}

	public override string FormatEventDeclaration(EventInfo eventInfo)
	{
		return $"member this.{eventInfo.Name} : IEvent<{FormatTypeName(eventInfo.EventHandlerType!)}, _>";
	}

	public override string FormatTypeName(Type type)
	{
		if (type.IsByRef) type = type.GetElementType()!;
		if (type.IsArray) return $"{FormatTypeName(type.GetElementType()!)}[]";

		if (Nullable.GetUnderlyingType(type) is Type innerNullable)
			return $"{FormatTypeName(innerNullable)} option";

		if (!type.IsGenericType)
		{
			return type.FullName switch
			{
				"System.Void" => "unit",
				"System.String" => "string",
				"System.Int32" => "int",
				"System.Int64" => "int64",
				"System.Boolean" => "bool",
				"System.Object" => "obj",
				"System.Double" => "float",
				"System.Single" => "single",
				"System.Decimal" => "decimal",
				"System.Char" => "char",
				_ => type.Name.Contains('`') ? type.Name[..type.Name.IndexOf('`')] : type.Name
			};
		}

		var def = type.GetGenericTypeDefinition();
		var name = def.Name;
		var tick = name.IndexOf('`');
		if (tick >= 0) name = name.Substring(0, tick);
		var args = type.GetGenericArguments().Select(FormatTypeName);
		return $"{name}<{string.Join(", ", args)}>";
	}

	static string GetAccessibility(MethodBase method)
	{
		if (method.IsPublic) return "member public";
		if (method.IsFamily) return "member protected";
		if (method.IsAssembly) return "member internal";
		if (method.IsFamilyOrAssembly) return "member protected internal";
		if (method.IsPrivate) return "member private";
		return "member private";
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
			return "null";
		return value switch
		{
			bool b => b ? "true" : "false",
			string s => $"\"{EscapeString(s)}\"",
			char c => $"'{EscapeChar(c)}'",
			Enum e => $"{FormatTypeName(e.GetType())}.{Enum.GetName(e.GetType(), e)}",
			_ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null"
		};
	}

	static string EscapeString(string value)
		=> value.Replace("\\", "\\\\").Replace("\"", "\\\"");

	static string EscapeChar(char c)
	{
		return c switch
		{
			'\\' => "\\\\",
			'\'' => "\\'",
			'\n' => "\\n",
			'\r' => "\\r",
			'\t' => "\\t",
			_ => c.ToString()
		};
	}
}
