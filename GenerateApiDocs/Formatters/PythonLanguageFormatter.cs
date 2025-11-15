using System.Globalization;
using System.Reflection;

namespace GenerateApiDocs.Formatters;

sealed class PythonLanguageFormatter : LanguageFormatter
{
	public override string DisplayName => "Python";
	public override string LanguageKey => "python";

	public override string FormatMethodSignature(Type type, MethodBase method)
	{
		bool isCtor = method.IsConstructor;
		string name = isCtor ? type.Name : method.Name;

		var parameters = method.GetParameters();
		var paramPieces = new List<string>();
		if (!method.IsStatic)
			paramPieces.Add("self");

		paramPieces.AddRange(parameters.Select(parameter =>
		{
			var parameterType = parameter.ParameterType;
			if (parameterType.IsByRef) parameterType = parameterType.GetElementType()!;
			var defaultSuffix = FormatDefaultValue(parameter);
			return $"{parameter.Name}: {FormatTypeName(parameterType)}{defaultSuffix}";
		}));

		string ret = "None";
		if (method is MethodInfo mi && mi.ReturnType != typeof(void))
			ret = FormatTypeName(mi.ReturnType);

		return $"def {name}({string.Join(", ", paramPieces)}) -> {ret}: ...";
	}

	public override string FormatPropertySignature(PropertyInfo property)
	{
		return $"{property.Name}: {FormatTypeName(property.PropertyType)}";
	}

	public override string FormatFieldDeclaration(FieldInfo field)
	{
		return $"{field.Name}: {FormatTypeName(field.FieldType)}";
}

	public override string FormatEventDeclaration(EventInfo eventInfo)
	{
		return $"{eventInfo.Name}: {FormatTypeName(eventInfo.EventHandlerType!)}";
	}

	public override string FormatTypeName(Type type)
	{
		if (type.IsByRef) type = type.GetElementType()!;
		if (type.IsArray) return "list[" + FormatTypeName(type.GetElementType()!) + "]";

		if (Nullable.GetUnderlyingType(type) is Type innerNullable)
			return $"Optional[{FormatTypeName(innerNullable)}]";

		if (!type.IsGenericType)
		{
			return type.FullName switch
			{
				"System.Void" => "None",
				"System.String" => "str",
				"System.Int32" => "int",
				"System.Int64" => "int",
				"System.Boolean" => "bool",
				"System.Object" => "object",
				"System.Double" => "float",
				"System.Single" => "float",
				"System.Decimal" => "float",
				"System.Char" => "str",
				_ => type.Name.Contains('`') ? type.Name[..type.Name.IndexOf('`')] : type.Name
			};
		}

		var def = type.GetGenericTypeDefinition();
		var args = type.GetGenericArguments().Select(FormatTypeName);
		var baseName = def.Name.Contains('`') ? def.Name[..def.Name.IndexOf('`')] : def.Name;
		return $"{baseName}[{string.Join(", ", args)}]";
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
			return "None";
		return value switch
		{
			bool b => b ? "True" : "False",
			string s => $"'{EscapeString(s)}'",
			char c => $"'{EscapeChar(c)}'",
			Enum e => $"{FormatTypeName(e.GetType())}.{Enum.GetName(e.GetType(), e)}",
			_ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "None"
		};
	}

	static string EscapeString(string value)
		=> value.Replace("\\", "\\\\").Replace("'", "\\'");

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
