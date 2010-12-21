using System;

namespace NetSpell.SpellChecker.Dictionary.Phonetic
{
	/// <summary>
	///		This class hold the settings for a phonetic rule
	/// </summary>
	public class PhoneticRule
	{
		private bool _BeginningOnly;
		private int[] _Condition = new int[256];
		private int _ConditionCount = 0;
		private int _ConsumeCount;
		private bool _EndOnly;
		private int _Priority;
		private bool _ReplaceMode = false;
		private string _ReplaceString;

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		public PhoneticRule()
		{
		}

		/// <summary>
		///     True if this rule should be applied to the beginning only
		/// </summary>
		public bool BeginningOnly
		{
			get {return _BeginningOnly;}
			set {_BeginningOnly = value;}
		}


		/// <summary>
		///     The ascii condition array
		/// </summary>
		public int[] Condition
		{
			get {return _Condition;}
		}

		/// <summary>
		///     The number of conditions
		/// </summary>
		public int ConditionCount
		{
			get {return _ConditionCount;}
			set {_ConditionCount = value;}
		}

		/// <summary>
		///     The number of chars to consume with this rule
		/// </summary>
		public int ConsumeCount
		{
			get {return _ConsumeCount;}
			set {_ConsumeCount = value;}
		}

		/// <summary>
		///     True if this rule should be applied to the end only
		/// </summary>
		public bool EndOnly
		{
			get {return _EndOnly;}
			set {_EndOnly = value;}
		}

		/// <summary>
		///     The priority of this rule
		/// </summary>
		public int Priority
		{
			get {return _Priority;}
			set {_Priority = value;}
		}

		/// <summary>
		///     True if this rule should run in replace mode
		/// </summary>
		public bool ReplaceMode
		{
			get {return _ReplaceMode;}
			set {_ReplaceMode = value;}
		}

		/// <summary>
		///     The string to use when replacing
		/// </summary>
		public string ReplaceString
		{
			get {return _ReplaceString;}
			set {_ReplaceString = value;}
		}

	}
}
