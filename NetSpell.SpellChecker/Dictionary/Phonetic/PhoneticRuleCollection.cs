using System;

namespace NetSpell.SpellChecker.Dictionary.Phonetic
{
	/// <summary>
	///     A collection that stores 'PhoneticRule' objects.
	/// </summary>
	[Serializable()]
	public class PhoneticRuleCollection : System.Collections.CollectionBase 
	{
    
		/// <summary>
		///     Initializes a new instance of 'PhoneticRuleCollection'.
		/// </summary>
		public PhoneticRuleCollection() 
		{
		}
    
		/// <summary>
		///     Initializes a new instance of 'PhoneticRuleCollection' based on an already existing instance.
		/// </summary>
		/// <param name='value'>
		///     A 'PhoneticRuleCollection' from which the contents is copied
		/// </param>
		public PhoneticRuleCollection(PhoneticRuleCollection value) 
		{
			this.AddRange(value);
		}
    
		/// <summary>
		///     Initializes a new instance of 'PhoneticRuleCollection' with an array of 'PhoneticRule' objects.
		/// </summary>
		/// <param name='value'>
		///     An array of 'PhoneticRule' objects with which to initialize the collection
		/// </param>
		public PhoneticRuleCollection(PhoneticRule[] value) 
		{
			this.AddRange(value);
		}
    
		/// <summary>
		///     Represents the 'PhoneticRule' item at the specified index position.
		/// </summary>
		/// <param name='index'>
		///     The zero-based index of the entry to locate in the collection.
		/// </param>
		/// <value>
		///     The entry at the specified index of the collection.
		/// </value>
		public PhoneticRule this[int index] 
		{
			get 
			{
				return ((PhoneticRule)(List[index]));
			}
			set 
			{
				List[index] = value;
			}
		}
    
		/// <summary>
		///     Adds a 'PhoneticRule' item with the specified value to the 'PhoneticRuleCollection'
		/// </summary>
		/// <param name='value'>
		///     The 'PhoneticRule' to add.
		/// </param>
		/// <returns>
		///     The index at which the new element was inserted.
		/// </returns>
		public int Add(PhoneticRule value) 
		{
			return List.Add(value);
		}
    
		/// <summary>
		///     Copies the elements of an array at the end of this instance of 'PhoneticRuleCollection'.
		/// </summary>
		/// <param name='value'>
		///     An array of 'PhoneticRule' objects to add to the collection.
		/// </param>
		public void AddRange(PhoneticRule[] value) 
		{
			for (int Counter = 0; (Counter < value.Length); Counter = (Counter + 1)) 
			{
				this.Add(value[Counter]);
			}
		}
    
		/// <summary>
		///     Adds the contents of another 'PhoneticRuleCollection' at the end of this instance.
		/// </summary>
		/// <param name='value'>
		///     A 'PhoneticRuleCollection' containing the objects to add to the collection.
		/// </param>
		public void AddRange(PhoneticRuleCollection value) 
		{
			for (int Counter = 0; (Counter < value.Count); Counter = (Counter + 1)) 
			{
				this.Add(value[Counter]);
			}
		}
    
		/// <summary>
		///     Gets a value indicating whether the 'PhoneticRuleCollection' contains the specified value.
		/// </summary>
		/// <param name='value'>
		///     The item to locate.
		/// </param>
		/// <returns>
		///     True if the item exists in the collection; false otherwise.
		/// </returns>
		public bool Contains(PhoneticRule value) 
		{
			return List.Contains(value);
		}
    
		/// <summary>
		///     Copies the 'PhoneticRuleCollection' values to a one-dimensional System.Array
		///     instance starting at the specified array index.
		/// </summary>
		/// <param name='array'>
		///     The one-dimensional System.Array that represents the copy destination.
		/// </param>
		/// <param name='index'>
		///     The index in the array where copying begins.
		/// </param>
		public void CopyTo(PhoneticRule[] array, int index) 
		{
			List.CopyTo(array, index);
		}
    
		/// <summary>
		///     Returns the index of a 'PhoneticRule' object in the collection.
		/// </summary>
		/// <param name='value'>
		///     The 'PhoneticRule' object whose index will be retrieved.
		/// </param>
		/// <returns>
		///     If found, the index of the value; otherwise, -1.
		/// </returns>
		public int IndexOf(PhoneticRule value) 
		{
			return List.IndexOf(value);
		}
    
		/// <summary>
		///     Inserts an existing 'PhoneticRule' into the collection at the specified index.
		/// </summary>
		/// <param name='index'>
		///     The zero-based index where the new item should be inserted.
		/// </param>
		/// <param name='value'>
		///     The item to insert.
		/// </param>
		public void Insert(int index, PhoneticRule value) 
		{
			List.Insert(index, value);
		}
    
		/// <summary>
		///     Returns an enumerator that can be used to iterate through
		///     the 'PhoneticRuleCollection'.
		/// </summary>
		public new PhoneticRuleEnumerator GetEnumerator() 
		{
			return new PhoneticRuleEnumerator(this);
		}
    
		/// <summary>
		///     Removes a specific item from the 'PhoneticRuleCollection'.
		/// </summary>
		/// <param name='value'>
		///     The item to remove from the 'PhoneticRuleCollection'.
		/// </param>
		public void Remove(PhoneticRule value) 
		{
			List.Remove(value);
		}
	}

}
