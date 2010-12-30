using System;
using System.Collections;

namespace NetSpell.SpellChecker.Dictionary.Affix
{
	/// <summary>
	///     A strongly typed enumerator for 'AffixRuleCollection'
	/// </summary>
	public class AffixRuleEnumerator : IDictionaryEnumerator
	{
		private IDictionaryEnumerator innerEnumerator;
			
		internal AffixRuleEnumerator (AffixRuleCollection enumerable)
		{
			innerEnumerator = enumerable.InnerHash.GetEnumerator();
		}

		#region Implementation of IDictionaryEnumerator
		
		/// <summary>
		///      gets the key of the current AffixRuleCollection entry.
		/// </summary>
		public string Key
		{
			get
			{
				return (string)innerEnumerator.Key;
			}
		}
		object IDictionaryEnumerator.Key
		{
			get
			{
				return Key;
			}
		}


		/// <summary>
		///     gets the value of the current AffixRuleCollection entry.
		/// </summary>
		public AffixRule Value
		{
			get
			{
				return (AffixRule)innerEnumerator.Value;
			}
		}
		object IDictionaryEnumerator.Value
		{
			get
			{
				return Value;
			}
		}

		/// <summary>
		///      gets both the key and the value of the current AffixRuleCollection entry.
		/// </summary>
		public System.Collections.DictionaryEntry Entry
		{
			get
			{
				return innerEnumerator.Entry;
			}
		}

		#endregion

		#region Implementation of IEnumerator
		
		/// <summary>
		///     Sets the enumerator to the first element in the collection
		/// </summary>
		public void Reset()
		{
			innerEnumerator.Reset();
		}

		/// <summary>
		///     Advances the enumerator to the next element of the collection
		/// </summary>
		public bool MoveNext()
		{
			return innerEnumerator.MoveNext();
		}

		/// <summary>
		///     Gets the current element from the collection
		/// </summary>
		public object Current
		{
			get
			{
				return innerEnumerator.Current;
			}
		}
		#endregion
	}
}
