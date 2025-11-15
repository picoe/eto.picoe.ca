using System;
using System.Reflection;

namespace GenerateApiDocs;

abstract class LanguageFormatter
{
	public abstract string DisplayName { get; }
	public abstract string LanguageKey { get; }

	public abstract string FormatMethodSignature(Type type, MethodBase method);
	public abstract string FormatPropertySignature(PropertyInfo property);
	public abstract string FormatFieldDeclaration(FieldInfo field);
	public abstract string FormatEventDeclaration(EventInfo eventInfo);
	public abstract string FormatTypeName(Type type);
	internal static bool IsOperatorMethod(MethodInfo method)
	{
		return method.IsSpecialName &&
		   (method.Name.StartsWith("op_", StringComparison.Ordinal) ||
			method.Name.StartsWith("op_Checked", StringComparison.Ordinal));
	}

	protected static bool TryGetDefaultValue(ParameterInfo parameter, out object? defaultValue)
	{
		if (parameter.HasDefaultValue)
		{
			defaultValue = parameter.DefaultValue;
			if (defaultValue == DBNull.Value || defaultValue == Missing.Value)
				defaultValue = null;
			return true;
		}
		defaultValue = null;
		return false;
	}
}
