using System.Text;

namespace GenerateApiDocs;

static class Helpers
{
	static string[] MdxSpecialCharacters() => new[] { "\\", "<", ">", "{", "}", "[", "]", "(", ")", "*", "_", "#", "`", "|", "!", "-", "+", ".", "?", "^", "$", "%", "&", "~", "=", ":", ";", "\"", "'" };

	public static string EscapePlainText(string text)
	{
		if (string.IsNullOrEmpty(text))
			return "";
		var sb = new StringBuilder();
		bool previousSpace = false;
		foreach (var ch in text)
		{
			if (char.IsWhiteSpace(ch))
			{
				if (!previousSpace)
				{
					sb.Append(' ');
					previousSpace = true;
				}
			}
			else
			{
				sb.Append(ch);
				previousSpace = false;
			}
		}
		var normalized = sb.ToString();
		foreach (var s in MdxSpecialCharacters())
		{
			normalized = normalized.Replace(s, "\\" + s);
		}
		return normalized;
	}
}
