using System;

namespace NetSpell.SpellChecker.Dictionary.Affix
{
	
	/// <summary>
	///     A collection that stores 'AffixEntry' objects.
	/// </summary>
	[Serializable()]
	public class AffixEntryCollection : System.Collections.CollectionBase 
	{
    
		/// <summary>
		///     Initializes a new instance of 'AffixEntryCollection'.
		/// </summary>
		public AffixEntryCollection() 
		{
		}
    
		/// <summary>
		///     Initializes a new instance of 'AffixEntryCollection' based on an already existing instance.
		/// </summary>
		/// <param name='value'>
		///     A 'AffixEntryCollection' from which the contents is copied
		/// </param>
		public AffixEntryCollection(AffixEntryCollection value) 
		{
			this.AddRange(value);
		}
    
		/// <summary>
		///     Initializes a new instance of 'AffixEntryCollection' with an array of 'AffixEntry' objects.
		/// </summary>
		/// <param name='value'>
		///     An array of 'AffixEntry' objects with which to initialize the collection
		/// </param>
		public AffixEntryCollection(AffixEntry[] value) 
		{
			this.AddRange(value);
		}
    
		/// <summary>
		///     Represents the 'AffixEntry' item at the specified index position.
		/// </summary>
		/// <param name='index'>
		///     The zero-based index of the entry to locate in the collection.
		/// </param>
		/// <value>
		///     The entry at the specified index of the collection.
		/// </value>
		public AffixEntry this[int index] 
		{
			get 
			{
				return ((AffixEntry)(List[index]));
			}
			set 
			{
				List[index] = value;
			}
		}
    
		/// <summary>
		///     Adds a 'AffixEntry' item with the specified value to the 'AffixEntryCollection'
		/// </summary>
		/// <param name='value'>
		///     The 'AffixEntry' to add.
		/// </param>
		/// <returns>
		///     The index at which the new element was inserted.
		/// </returns>
		public int Add(AffixEntry value) 
		{
			return List.Add(value);
		}
    
		/// <summary>
		///     Copies the elements of an array at the end of this instance of 'AffixEntryCollection'.
		/// </summary>
		/// <param name='value'>
		///     An array of 'AffixEntry' objects to add to the collection.
		/// </param>
		public void AddRange(AffixEntry[] value) 
		{
			for (int Counter = 0; (Counter < value.Length); Counter = (Counter + 1)) 
			{
				this.Add(value[Counter]);
			}
		}
    
		/// <summary>
		///     Adds the contents of another 'AffixEntryCollection' at the end of this instance.
		/// </summary>
		/// <param name='value'>
		///     A 'AffixEntryCollection' containing the objects to add to the collection.
		/// </param>
		public void AddRange(AffixEntryCollection value) 
		{
			for (int Counter = 0; (Counter < value.Count); Counter = (Counter + 1)) 
			{
				this.Add(value[Counter]);
			}
		}
    
		/// <summary>
		///     Gets a value indicating whether the 'AffixEntryCollection' contains the specified value.
		/// </summary>
		/// <param name='value'>
		///     The item to locate.
		/// </param>
		/// <returns>
		///     True if the item exists in the collection; false otherwise.
		/// </returns>
		public bool Contains(AffixEntry value) 
		{
			return List.Contains(value);
		}
    
		/// <summary>
		///     Copies the 'AffixEntryCollection' values to a one-dimensional System.Array
		///     instance starting at the specified array index.
		/// </summary>
		/// <param name='array'>
		///     The one-dimensional System.Array that represents the copy destination.
		/// </param>
		/// <param name='index'>
		///     The index in the array where copying begins.
		/// </param>
		public void CopyTo(AffixEntry[] array, int index) 
		{
			List.CopyTo(array, index);
		}
    
		/// <summary>
		///     Returns the index of a 'AffixEntry' object in the collection.
		/// </summary>
		/// <param name='value'>
		///     The 'AffixEntry' object whose index will be retrieved.
		/// </param>
		/// <returns>
		///     If found, the index of the value; otherwise, -1.
		/// </returns>
		public int IndexOf(AffixEntry value) 
		{
			return List.IndexOf(value);
		}
    
		/// <summary>
		///     Inserts an existing 'AffixEntry' into the collection at the specified index.
		/// </summary>
		/// <param name='index'>
		///     The zero-based index where the new item should be inserted.
		/// </param>
		/// <param name='value'>
		///     The item to insert.
		/// </param>
		public void Insert(int index, AffixEntry value) 
		{
			List.Insert(index, value);
		}
    
		/// <summary>
		///     Returns an enumerator that can be used to iterate through
		///     the 'AffixEntryCollection'.
		/// </summary>
		public new AffixEntryEnumerator GetEnumerator() 
		{
			return new AffixEntryEnumerator(this);
		}
    
		/// <summary>
		///     Removes a specific item from the 'AffixEntryCollection'.
		/// </summary>
		/// <param name='value'>
		///     The item to remove from the 'AffixEntryCollection'.
		/// </param>
		public void Remove(AffixEntry value) 
		{
			List.Remove(value);
		}
	}

}
