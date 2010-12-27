using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace NetSpell.SpellChecker.Dictionary.Affix
{
	/// <summary>
	///		Rule for expanding base words
	/// </summary>
	public class AffixRule
	{
		private bool _AllowCombine = false;
		private AffixEntryCollection _AffixEntries = new AffixEntryCollection();
		private string _Name = "";

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		public AffixRule()
		{
		}

		/// <summary>
		///     Allow combining prefix and suffix
		/// </summary>
		public bool AllowCombine
		{
			get {return _AllowCombine;}
			set {_AllowCombine = value;}
		}

		/// <summary>
		///     Collection of text entries that make up this rule
		/// </summary>
		public AffixEntryCollection AffixEntries
		{
			get {return _AffixEntries;}
			set {_AffixEntries = value;}
		}

		/// <summary>
		///     Name of the Affix rule
		/// </summary>
		public string Name
		{
			get {return _Name;}
			set {_Name = value;}
		}

	}
}
