using System;

namespace NetSpell.SpellChecker.Dictionary.Affix
{
	/// <summary>
	///     A strongly typed enumerator for 'AffixEntryCollection'
	/// </summary>
	public class AffixEntryEnumerator : object, System.Collections.IEnumerator 
	{
    
		private System.Collections.IEnumerator Base;
    
		private System.Collections.IEnumerable Local;
    
		/// <summary>
		///     Enumerator constructor
		/// </summary>
		public AffixEntryEnumerator(AffixEntryCollection mappings) 
		{
			this.Local = ((System.Collections.IEnumerable)(mappings));
			this.Base = Local.GetEnumerator();
		}
    
		/// <summary>
		///     Gets the current element from the collection (strongly typed)
		/// </summary>
		public AffixEntry Current 
		{
			get 
			{
				return ((AffixEntry)(Base.Current));
			}
		}
    
		/// <summary>
		///     Gets the current element from the collection
		/// </summary>
		object System.Collections.IEnumerator.Current 
		{
			get 
			{
				return Base.Current;
			}
		}
    
		/// <summary>
		///     Advances the enumerator to the next element of the collection
		/// </summary>
		public bool MoveNext() 
		{
			return Base.MoveNext();
		}
    
		/// <summary>
		///     Advances the enumerator to the next element of the collection
		/// </summary>
		bool System.Collections.IEnumerator.MoveNext() 
		{
			return Base.MoveNext();
		}
    
		/// <summary>
		///     Sets the enumerator to the first element in the collection
		/// </summary>
		public void Reset() 
		{
			Base.Reset();
		}
    
		/// <summary>
		///     Sets the enumerator to the first element in the collection
		/// </summary>
		void System.Collections.IEnumerator.Reset() 
		{
			Base.Reset();
		}
	}


}
