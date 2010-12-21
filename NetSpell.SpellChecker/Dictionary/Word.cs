using System;

namespace NetSpell.SpellChecker.Dictionary
{
	/// <summary>
	/// The Word class represents a base word in the dictionary
	/// </summary>
	public class Word  : IComparable
	{
		private string _AffixKeys = "";
		private int _EditDistance = 0;
		private int _height = 0;
		private int _index = 0;
		private string _PhoneticCode = "";
		private string _text = "";

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		public Word()
		{
		}

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		/// <param name="text" type="string">
		///     <para>
		///         The string for the base word
		///     </para>
		/// </param>
		/// <param name="affixKeys" type="string">
		///     <para>
		///         The affix keys that can be applied to this base word
		///     </para>
		/// </param>
		/// <param name="phoneticCode" type="string">
		///     <para>
		///         The phonetic code for this word
		///     </para>
		/// </param>
		public Word(string text, string affixKeys, string phoneticCode)
		{
			_text = text;
			_AffixKeys = affixKeys;
			_PhoneticCode = phoneticCode;
		}

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		/// <param name="text" type="string">
		///     <para>
		///         The string for the base word
		///     </para>
		/// </param>
		/// <param name="affixKeys" type="string">
		///     <para>
		///         The affix keys that can be applied to this base word
		///     </para>
		/// </param>
		public Word(string text, string affixKeys)
		{
			_text = text;
			_AffixKeys = affixKeys;
		}

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		/// <param name="text" type="string">
		///     <para>
		///         The string for the base word
		///     </para>
		/// </param>
		public Word(string text)
		{
			_text = text;
		}

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		/// <param name="text" type="string">
		///     <para>
		///         The string for the word
		///     </para>
		/// </param>
		/// <param name="index" type="int">
		///     <para>
		///         The position index of this word
		///     </para>
		/// </param>
		/// <param name="height" type="int">
		///     <para>
		///         The line height of this word
		///     </para>
		/// </param>
		/// <returns>
		///     A void value...
		/// </returns>
		internal Word(string text, int index, int height)
		{
			_text = text;
			_index = index;
			_height = height;
		}

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		/// <param name="text" type="string">
		///     <para>
		///         The string for the base word
		///     </para>
		/// </param>
		/// <param name="editDistance" type="int">
		///     <para>
		///         The edit distance from the misspelled word
		///     </para>
		/// </param>
		internal Word(string text, int editDistance)
		{
			_text = text;
			_EditDistance = editDistance;
		}

		/// <summary>
		///     Sorts a collection of words by EditDistance
		/// </summary>
		/// <remarks>
		///		The compare sorts in desc order, largest EditDistance first
		/// </remarks>
		public int CompareTo(object obj)
		{
			int result = this.EditDistance.CompareTo(((Word)obj).EditDistance);
			return result; // * -1; // sorts desc order
		}


		/// <summary>
		///     The affix keys that can be applied to this base word
		/// </summary>
		public string AffixKeys
		{
			get {return _AffixKeys;}
			set {_AffixKeys = value;}
		}

		
		/// <summary>
		///     The index position of where this word appears
		/// </summary>
		public int Index
		{
			get { return _index; }
			set { _index = value; }
		}

		/// <summary>
		///     The phonetic code for this word
		/// </summary>
		public string PhoneticCode
		{
			get {return _PhoneticCode;}
			set {_PhoneticCode = value;}
		}

		/// <summary>
		///     The string for the base word
		/// </summary>
		public string Text
		{
			get { return _text; }
			set { _text = value; }
		}	


		/// <summary>
		///     Used for sorting suggestions by its edit distance for 
		///     the misspelled word
		/// </summary>
		internal int EditDistance
		{
			get {return _EditDistance;}
			set {_EditDistance = value;}
		}

		/// <summary>
		///     The line height of this word
		/// </summary>
		internal int Height
		{
			get { return _height; }
			set { _height = value; }
		}

		/// <summary>
		///     Converts the word object to a string
		/// </summary>
		/// <returns>
		///		Returns the Text Property contents
		/// </returns>
		public override string ToString()
		{
			return _text;
		}
	}
}
