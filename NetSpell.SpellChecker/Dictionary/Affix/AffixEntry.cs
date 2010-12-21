using System;
using System.Text.RegularExpressions;

namespace NetSpell.SpellChecker.Dictionary.Affix
{
	/// <summary>
	///		Rule Entry for expanding base words
	/// </summary>
	public class AffixEntry
	{
		private int _ConditionCount;
		private string _AddCharacters = "";
		private int[] _Condition = new int[256] ;
		private string _StripCharacters = "";

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		public AffixEntry()
		{
		}

		/// <summary>
		///     The characters to add to the string
		/// </summary>
		public string AddCharacters
		{
			get {return _AddCharacters;}
			set {_AddCharacters = value;}
		}

		/// <summary>
		///     The condition to be met in order to add characters
		/// </summary>
		public int[] Condition
		{
			get {return _Condition;}
			set {_Condition = value;}
		}


		/// <summary>
		///     The characters to remove before adding characters
		/// </summary>
		public string StripCharacters
		{
			get {return _StripCharacters;}
			set {_StripCharacters = value;}
		}


		/// <summary>
		///     The number of conditions that must be met
		/// </summary>
		public int ConditionCount
		{
			get {return _ConditionCount;}
			set {_ConditionCount = value;}
		}

	}
}
