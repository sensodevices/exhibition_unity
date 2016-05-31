using System;
using System.Globalization;
using System.Text;

public static class StringExtensions
{
	private const string SpaceSymbol = " ";
	private static readonly string[] SpaceSymbolPattern = {SpaceSymbol};

	public static string SplitByIndexes(this string sourceString, int startIndex, int endIndex)
	{
		return sourceString.Substring(startIndex, endIndex - startIndex);
	}

	/// <summary>
	/// Имплементация проверки на null, string.Empty и равенство пробелу.
	/// </summary>
	/// <param name="sourceString">Строка для проверки</param>
	/// <returns>True - если строка равна null или равна string.Empty или состоит из пробелов.</returns>
	public static bool IsNullOrWhiteSpace(this string sourceString)
	{
		if (string.IsNullOrEmpty(sourceString))
			return true;

		return string.IsNullOrEmpty(sourceString.Trim());
	}

	/// <summary>
	/// Проверят строку на наличие пробелов.
	/// </summary>
	/// <param name="sourceString">Строка для проверки</param>
	/// <returns>True - если строка содержит пробел, иначе - false.</returns>
	public static bool IsContainsSpace(this string sourceString)
	{
		return sourceString.Contains(SpaceSymbol);
	}

	/// <summary>
	/// Разделяет строку по пробелам.
	/// </summary>
	/// <param name="sourceString">Строка для проверки</param>
	/// <returns>Возвращает элементы строки, которые в оригинале были отделены пробелом.</returns>
	public static string[] SplitBySpace(this string sourceString)
	{
		return sourceString.Split(SpaceSymbolPattern, StringSplitOptions.RemoveEmptyEntries);
	}

	private static char[] WordsDelimiters = {' ', ',', '.', '-', '!', '?'};

	public static string ExtractWord(this string sourceString, int wordIndex)
	{
		int wordStartIndex;
		return ExtractWord(sourceString, wordIndex, out wordStartIndex);
	}

	public static string ExtractWord(this string sourceString, int wordIndex, out int wordStartIndex)
	{
		wordStartIndex = 0;
		if (wordIndex == 0 && sourceString.ContainsWordsDelimiter() == false) return sourceString;

		var foundedWordsCounter = 0;
		var iteratingThroughWord = false;
		for (var i = 0; i < sourceString.Length; i++)
		{
			var testChar = sourceString[i];
			if (testChar.IsWordsDelimiter())
			{
				iteratingThroughWord = false;
				continue;
			}

			if (!iteratingThroughWord)
			{
				iteratingThroughWord = true;
				foundedWordsCounter++;
			}

			if (foundedWordsCounter == (wordIndex + 1))
			{
				wordStartIndex = i;
				break;
			}
		}

		if (foundedWordsCounter < (wordIndex + 1))
			throw new InvalidOperationException("Не удается найти слово с указанным индексом - строка содержит меньшее количество слов");

		var wordEndIndex = sourceString.IndexOfWordsDelimiter(wordStartIndex);
		if (wordEndIndex == -1) wordEndIndex = sourceString.Length;

		var word = sourceString.Substring2(wordStartIndex, wordEndIndex);
		return word;
	}

	public static string Substring2(this string sourceString, int startIndex, int endIndex)
	{
		if (startIndex > endIndex) throw new ArgumentException("Начальный индекс не может быть больше конечного");
		return sourceString.Substring(startIndex, endIndex - startIndex);
	}

	private static bool ContainsWordsDelimiter(this string sourceSting)
	{
		foreach (var delimiter in WordsDelimiters)
		{
			if (sourceSting.IndexOf(delimiter) != -1) return true;
		}

		return false;
	}

	private static int IndexOfWordsDelimiter(this string sourceSting, int baseIndex = 0)
	{
		var delimiterFound = false;
		var smallestIndex = int.MaxValue;
		foreach (var delimiter in WordsDelimiters)
		{
			var delimiterIndex = sourceSting.IndexOf(delimiter, baseIndex);
			if (delimiterIndex != -1)
			{
				delimiterFound = true;

				if (delimiterIndex < smallestIndex)
					smallestIndex = delimiterIndex;
			}
		}

		if (delimiterFound == false) return -1;
		return smallestIndex;
	}

	private static bool IsWordsDelimiter(this char sourceChar)
	{
		foreach (var delimiter in WordsDelimiters)
		{
			if (sourceChar == delimiter) return true;
		}

		return false;
	}

	public static int ToInt(this string sourceString, bool isHex = false)
	{
		var numberStyles = NumberStyles.Integer;
		if (isHex) numberStyles = NumberStyles.HexNumber;
		return int.Parse(sourceString, numberStyles);
	}

	public static double ToDouble(this string sourceString)
	{
		return double.Parse(sourceString, CultureInfo.InvariantCulture);
	}

	public static bool IsIncorrectGalaxyNick(this string source)
	{
		if (source.IsNullOrWhiteSpace()) return true;
		return source == "0";
	}

	/*public static string UriDecode(this string source)
	{
		if (source.IsNullOrWhiteSpace()) return source;
		return UrlDecoder.UrlDecode(source, Encoding.UTF8);
	}*/

	public static string TransformToString(this byte[] source)
	{
		return Encoding.UTF8.GetString(source, 0, source.Length);
	}

	public static string FixImageID(this string source)
	{
		if (source.Contains(".png")) return source;
		if (source.EndsWith("_")) return source;

		return source + "_";
	}

	/*public static string FormatAsChatMessageCommand(this string message, int messageID, Identity addressation)
	{
		// [<:adv>] PRIVMSG <count> <target_user_id> :<text>
		var targetID = -1;
		if (addressation != null) targetID = addressation.ID;
		var formatted = string.Format("PRIVMSG {0} {1} :{2}", messageID, targetID, message);
		return formatted;
	}

	public static IInlineElement ToInlineElement(this string source, int smileSize = 24, string[] names = null)
	{
		if (names == null)
			return GalaxyDefaultParagraphParser.Parse(source, smileSize);
		return GalaxyDefaultParagraphParser.Parse(source, smileSize, names);
	}

	public static string FromParagraph(this Paragraph source)
	{
		if (source == null || source.Elements == null) return string.Empty;
		var sb = GlobalStringBuilder.Acquire();
		foreach (var inlineElement in source.Elements)
			sb.Append(inlineElement);
		return GlobalStringBuilder.GetResultAndRelease();
	}*/

	public static int[] ConvertStringsToInts(this string[] source)
	{
		var result = new int[source.Length];
		for (var i = 0; i < source.Length; i++)
		{
			result[i] = source[i].ToInt();
		}
		return result;
	}
}
	