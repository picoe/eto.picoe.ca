using System.Globalization;
using System.Reflection;

namespace GenerateApiDocs.Formatters;

sealed class CSharpLanguageFormatter : LanguageFormatter
{
	public override string DisplayName => "C#";
	public override string LanguageKey => "csharp";

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
				prefix = "ref ";
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
		return $"event {FormatTypeName(eventInfo.EventHandlerType!)} {eventInfo.Name};";
	}

	public override string FormatTypeName(Type type)
	{
		var aliases = new Dictionary<Type, string>
		{
			{ typeof(bool), "bool" },
			{ typeof(byte), "byte" },
			{ typeof(sbyte), "sbyte" },
			{ typeof(char), "char" },
			{ typeof(decimal), "decimal" },
			{ typeof(double), "double" },
			{ typeof(float), "float" },
			{ typeof(int), "int" },
			{ typeof(uint), "uint" },
			{ typeof(long), "long" },
			{ typeof(ulong), "ulong" },
			{ typeof(short), "short" },
			{ typeof(ushort), "ushort" },
			{ typeof(object), "object" },
			{ typeof(string), "string" },
			{ typeof(void), "void" }
		};

		if (aliases.TryGetValue(type, out var alias))
			return alias;

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
			return $"{FormatTypeName(type.DeclaringType!)}.{type.Name}";

		return type.Name.Contains('`') ? type.Name[..type.Name.IndexOf('`')] : type.Name;
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
			float f => f.ToString(CultureInfo.InvariantCulture) + "f",
			double d => d.ToString(CultureInfo.InvariantCulture),
			decimal m => m.ToString(CultureInfo.InvariantCulture) + "m",
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
