
namespace SpellChecker
{
	/// <summary>
	/// Summary description for ReplaceWordEventArgs.
	/// </summary>
	public class ReplaceWordEventArgs : SpellingEventArgs
	{
		private readonly string _replacementWord;

		/// <summary>
		///     Class sent to the event handler when the ReplacedWord Event is fired
		/// </summary>
		public ReplaceWordEventArgs(string replacementWord, string word, int wordIndex, int textIndex) : base (word, wordIndex, textIndex)
		{
			_replacementWord = replacementWord;
		}

		/// <summary>
		///     The word to use in replacing the misspelled word
		/// </summary>
		public string ReplacementWord
		{
			get {return _replacementWord;}
		}
	}
}
