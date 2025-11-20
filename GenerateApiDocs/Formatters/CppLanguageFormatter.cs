using System.Globalization;
using System.Reflection;

namespace GenerateApiDocs.Formatters;

sealed class CppLanguageFormatter : LanguageFormatter
{
	public override string DisplayName => "C++";
	public override string LanguageKey => "cpp";

	public override string FormatMethodSignature(Type type, MethodBase method)
	{
		bool isCtor = method.IsConstructor;
		string name = isCtor ? FormatTypeName(type) : method.Name;

		if (method is MethodInfo op && IsOperatorMethod(op))
		{
			var ret = FormatTypeName(op.ReturnType);
			var parameters = op.GetParameters();
			return op.Name switch
			{
				"op_Addition" => $"{ret} operator +({FormatTypeName(parameters[0].ParameterType)} left, {FormatTypeName(parameters[1].ParameterType)} right)",
				"op_Subtraction" => $"{ret} operator -({FormatTypeName(parameters[0].ParameterType)} left, {FormatTypeName(parameters[1].ParameterType)} right)",
				"op_Multiply" => $"{ret} operator *({FormatTypeName(parameters[0].ParameterType)} left, {FormatTypeName(parameters[1].ParameterType)} right)",
				"op_Division" => $"{ret} operator /({FormatTypeName(parameters[0].ParameterType)} left, {FormatTypeName(parameters[1].ParameterType)} right)",
				"op_Equality" => $"{ret} operator ==({FormatTypeName(parameters[0].ParameterType)} left, {FormatTypeName(parameters[1].ParameterType)} right)",
				"op_Inequality" => $"{ret} operator !=({FormatTypeName(parameters[0].ParameterType)} left, {FormatTypeName(parameters[1].ParameterType)} right)",
				"op_GreaterThan" => $"{ret} operator >({FormatTypeName(parameters[0].ParameterType)} left, {FormatTypeName(parameters[1].ParameterType)} right)",
				"op_LessThan" => $"{ret} operator <({FormatTypeName(parameters[0].ParameterType)} left, {FormatTypeName(parameters[1].ParameterType)} right)",
				_ => $"{ret} {op.Name}({string.Join(", ", parameters.Select(p => $"{FormatTypeName(p.ParameterType)} {p.Name}"))})"
			};
		}

		if (method is MethodInfo generic && generic.IsGenericMethod)
		{
			var typeArgs = generic.GetGenericArguments();
			name += $"<{string.Join(", ", typeArgs.Select(a => a.Name))}>";
		}

		var paramPieces = method.GetParameters().Select(parameter =>
		{
			var parameterType = parameter.ParameterType;
			string prefix = "";
			if (parameterType.IsByRef)
			{
				parameterType = parameterType.GetElementType()!;
				prefix = "& ";
			}
			var defaultSuffix = FormatDefaultValue(parameter);
			return $"{prefix}{FormatTypeName(parameterType)} {parameter.Name}{defaultSuffix}";
		});

		var accessibility = GetAccessibility(method);
		if (method is MethodInfo mi && !isCtor)
		{
			var retName = FormatTypeName(mi.ReturnType);
			return $"{accessibility} {retName} {name}({string.Join(", ", paramPieces)})";
		}

		return $"{accessibility} {name}({string.Join(", ", paramPieces)})";
	}

	public override string FormatPropertySignature(PropertyInfo property)
	{
		var accessor = property.CanRead && property.GetMethod!.IsPublic && property.CanWrite && property.SetMethod!.IsPublic
			? "{ get; set; }"
			: property.CanRead && property.GetMethod!.IsPublic
				? "{ get; }"
				: property.CanWrite && property.SetMethod!.IsPublic
					? "{ set; }"
					: "{}";

		var staticModifier = (property.GetMethod != null && property.GetMethod.IsStatic) || (property.SetMethod != null && property.SetMethod.IsStatic)
			? "static "
			: "";

		return $"{GetAccessibility(property.GetMethod ?? property.SetMethod!)} {staticModifier}{FormatTypeName(property.PropertyType)} {property.Name} {accessor}";
	}

	public override string FormatFieldDeclaration(FieldInfo field)
	{
		var staticModifier = field.IsStatic ? "static " : string.Empty;
		return $"{staticModifier}{FormatTypeName(field.FieldType)} {field.Name};";
	}

	public override string FormatEventDeclaration(EventInfo eventInfo)
	{
		return $"// Events are represented via delegates\n{FormatTypeName(eventInfo.EventHandlerType!)} {eventInfo.Name};";
	}

	public override string FormatTypeName(Type type)
	{
		if (type.IsByRef) type = type.GetElementType()!;
		if (type.IsArray) return FormatTypeName(type.GetElementType()!) + "[]";

		if (Nullable.GetUnderlyingType(type) is Type underlying)
			return FormatTypeName(underlying) + "?";

		if (type.IsGenericParameter)
			return type.Name;

		if (type.IsGenericType)
		{
			var def = type.GetGenericTypeDefinition();
			var name = def.Name;
			var tick = name.IndexOf('`');
			if (tick >= 0) name = name.Substring(0, tick);

			var args = type.GetGenericArguments().Select(FormatTypeName);
			return $"{name}<{string.Join(", ", args)}>";
		}

		if (type.IsNested)
			return $"{FormatTypeName(type.DeclaringType!)}::{type.Name}";

		return type.FullName switch
        {
			"System.Boolean" => "bool",
			"System.Byte" => "uint8_t",
			"System.SByte" => "int8_t",
			"System.Char" => "char",
			"System.Decimal" => "decimal",
			"System.Double" => "double",
			"System.Single" => "float",
			"System.Int32" => "int",
			"System.UInt32" => "unsigned int",
			"System.Int64" => "long long",
			"System.UInt64" => "unsigned long long",
			"System.Int16" => "short",
			"System.UInt16" => "unsigned short",
			"System.Object" => "object",
			"System.String" => "std::string",
			"System.Void" => "void",
			_ => type.Name.Replace('.', ':')
        };
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
			return "nullptr";
		return value switch
		{
			bool b => b ? "true" : "false",
			string s => $"\"{EscapeString(s)}\"",
			char c => $"'{EscapeChar(c)}'",
			Enum e => $"{FormatTypeName(e.GetType())}::{Enum.GetName(e.GetType(), e)}",
			float f => f.ToString(CultureInfo.InvariantCulture) + "f",
			double d => d.ToString(CultureInfo.InvariantCulture),
			decimal m => m.ToString(CultureInfo.InvariantCulture),
			_ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "nullptr"
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

	static string GetAccessibility(MethodBase method)
	{
		if (method.IsPublic) return "public";
		if (method.IsFamily) return "protected";
		if (method.IsAssembly) return "internal";
		if (method.IsFamilyOrAssembly) return "protected internal";
		if (method.IsPrivate) return "private";
		return "private";
	}
}
