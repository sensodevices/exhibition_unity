using System;

/// <summary>
/// Используется для взаимодействия между клиентом и сервером.
/// 
/// Синтаксис:
///   :префикс имя параметр1 параметр2 ... :постфикс
/// 
/// (префикса, параметров и постфикса может не быть)
/// </summary>
public sealed class GalaCommand
{
	private readonly string source;

	public GalaCommand(string source)
	{
		this.source = source;
		CreatedOn = DateTime.Now;

		//if (source.IsNullOrWhiteSpace()) throw new ArgumentException("Invalid message source", "source");
		if (source.IsNullOrWhiteSpace())
			return;

		ExtractMessagePartsFrom(source);
	}

	public DateTime CreatedOn { get; private set; }
	public string Name { get; private set; }
	public string Postfix { get; private set; }
	public string Prefix { get; private set; }
	public string[] Parameters { get; private set; }

	public override string ToString()
	{
		return source;
	}

	private void ExtractMessagePartsFrom(string source)
	{
		var sourceWithoutPostfix = CutPostfixFromSource(source);
		var messageParts = sourceWithoutPostfix.SplitBySpace();

		int parametersStartIndex;
		ParsePrefixAndName(messageParts, out parametersStartIndex);
		ParseParameters(messageParts, parametersStartIndex);
	}

	private string CutPostfixFromSource(string source)
	{
		const string postfixSignature = " :";
		var postfixIndex = source.IndexOf(postfixSignature);
		if (postfixIndex == -1)
		{
			Postfix = string.Empty;
			return source;
		}

		Postfix = source.SplitByIndexes(postfixIndex + postfixSignature.Length, source.Length);
		return source.SplitByIndexes(0, postfixIndex);
	}

	private void ParsePrefixAndName(string[] messageParts, out int parametersStartIndex)
	{
		var i = 0;
		var firstPart = messageParts[i];
		const string prefixSignature = ":";
		if (firstPart.StartsWith(prefixSignature))
		{
			i++;
			Prefix = firstPart.Substring(prefixSignature.Length);
		}
		else
			Prefix = string.Empty;

		Name = messageParts[i++];
		parametersStartIndex = i;
	}

	private void ParseParameters(string[] messageParts, int parametersStartIndex)
	{
		if (parametersStartIndex == messageParts.Length)
			Parameters = new string[0];
		else
		{
			var parametersCount = messageParts.Length - parametersStartIndex;
			Parameters = new string[parametersCount];

			for (var i = 0; i < parametersCount; i++)
				Parameters[i] = messageParts[i + parametersStartIndex];
		}
	}
}
