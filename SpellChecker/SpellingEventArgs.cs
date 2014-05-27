using System;

namespace SpellChecker
{
	/// <summary>
	///     Class sent to the event handler when the DoubleWord or 
	///     MisspelledWord event occurs
	/// </summary>
	public class SpellingEventArgs : EventArgs 
	{
		private readonly int _textIndex;
		private readonly string _word;
		private readonly int _wordIndex;

		/// <summary>
		///     Constructor used to pass in properties
		/// </summary>
		public SpellingEventArgs(string word, int wordIndex, int textIndex)
		{
			_word = word;
			_wordIndex = wordIndex;
			_textIndex = textIndex;
		}

		/// <summary>
		///     Text index of the WordEvent
		/// </summary>
		public int TextIndex
		{
			get {return _textIndex;}
		}

		/// <summary>
		///     Word that caused the WordEvent
		/// </summary>
		public string Word
		{
			get {return _word;}
		}

		/// <summary>
		///     Word index of the WordEvent
		/// </summary>
		public int WordIndex
		{
			get {return _wordIndex;}
		}

	} // Class SpellingEventArgs
}
