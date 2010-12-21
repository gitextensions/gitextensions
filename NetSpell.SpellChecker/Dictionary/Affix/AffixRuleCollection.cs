using System;
using System.Collections;

namespace NetSpell.SpellChecker.Dictionary.Affix
{
	/// <summary>
	///     A dictionary collection that stores 'AffixRule' objects.
	/// </summary>	
	public class AffixRuleCollection : IDictionary, ICollection, IEnumerable, ICloneable
	{
		/// <summary>
		///     Internal Hashtable
		/// </summary>
		protected Hashtable innerHash;
		
		#region "Constructors"
		
		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		public  AffixRuleCollection()
		{
			innerHash = new Hashtable();
		}
		
		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="original" type="AffixRuleCollection">
		///     <para>
		///         A 'AffixRuleCollection' from which the contents is copied
		///     </para>
		/// </param>
		public AffixRuleCollection(AffixRuleCollection original)
		{
			innerHash = new Hashtable (original.innerHash);
		}
		
		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="dictionary" type="System.Collections.IDictionary">
		///     <para>
		///         The IDictionary to copy to a new 'AffixRuleCollection'.
		///     </para>
		/// </param>
		public AffixRuleCollection(IDictionary dictionary)
		{
			innerHash = new Hashtable (dictionary);
		}

		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="capacity" type="int">
		///     <para>
		///         The approximate number of elements that the 'AffixRuleCollection' can initially contain.
		///     </para>
		/// </param>
		public AffixRuleCollection(int capacity)
		{
			innerHash = new Hashtable(capacity);
		}

		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="dictionary" type="System.Collections.IDictionary">
		///     <para>
		///         The IDictionary to copy to a new 'AffixRuleCollection'.
		///     </para>
		/// </param>
		/// <param name="loadFactor" type="float">
		///     <para>
		///         A number in the range from 0.1 through 1.0 indicating the maximum ratio of elements to buckets.
		///     </para>
		/// </param>
		public AffixRuleCollection(IDictionary dictionary, float loadFactor)
		{
			innerHash = new Hashtable(dictionary, loadFactor);
		}

		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="codeProvider" type="System.Collections.IHashCodeProvider">
		///     <para>
		///         The IHashCodeProvider that supplies the hash codes for all keys in the 'AffixRuleCollection'.
		///     </para>
		/// </param>
		/// <param name="comparer" type="System.Collections.IComparer">
		///     <para>
		///         The IComparer to use to determine whether two keys are equal.
		///     </para>
		/// </param>
		public AffixRuleCollection(IHashCodeProvider codeProvider, IComparer comparer)
		{
			innerHash = new Hashtable (codeProvider, comparer);
		}

		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="capacity" type="int">
		///     <para>
		///         The approximate number of elements that the 'AffixRuleCollection' can initially contain.
		///     </para>
		/// </param>
		/// <param name="loadFactor" type="int">
		///     <para>
		///         A number in the range from 0.1 through 1.0 indicating the maximum ratio of elements to buckets.
		///     </para>
		/// </param>
		public AffixRuleCollection(int capacity, int loadFactor)
		{
			innerHash = new Hashtable(capacity, loadFactor);
		}


		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="dictionary" type="System.Collections.IDictionary">
		///     <para>
		///         The IDictionary to copy to a new 'AffixRuleCollection'.
		///     </para>
		/// </param>
		/// <param name="codeProvider" type="System.Collections.IHashCodeProvider">
		///     <para>
		///         The IHashCodeProvider that supplies the hash codes for all keys in the 'AffixRuleCollection'.
		///     </para>
		/// </param>
		/// <param name="comparer" type="System.Collections.IComparer">
		///     <para>
		///         The IComparer to use to determine whether two keys are equal.
		///     </para>
		/// </param>
		public AffixRuleCollection(IDictionary dictionary, IHashCodeProvider codeProvider, IComparer comparer)
		{
			innerHash = new Hashtable (dictionary, codeProvider, comparer);
		}
		
		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="capacity" type="int">
		///     <para>
		///         The approximate number of elements that the 'AffixRuleCollection' can initially contain.
		///     </para>
		/// </param>
		/// <param name="codeProvider" type="System.Collections.IHashCodeProvider">
		///     <para>
		///         The IHashCodeProvider that supplies the hash codes for all keys in the 'AffixRuleCollection'.
		///     </para>
		/// </param>
		/// <param name="comparer" type="System.Collections.IComparer">
		///     <para>
		///         The IComparer to use to determine whether two keys are equal.
		///     </para>
		/// </param>		
		public AffixRuleCollection(int capacity, IHashCodeProvider codeProvider, IComparer comparer)
		{
			innerHash = new Hashtable (capacity, codeProvider, comparer);
		}

		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="dictionary" type="System.Collections.IDictionary">
		///     <para>
		///         The IDictionary to copy to a new 'AffixRuleCollection'.
		///     </para>
		/// </param>
		/// <param name="loadFactor" type="float">
		///     <para>
		///         A number in the range from 0.1 through 1.0 indicating the maximum ratio of elements to buckets.
		///     </para>
		/// </param>
		/// <param name="codeProvider" type="System.Collections.IHashCodeProvider">
		///     <para>
		///         The IHashCodeProvider that supplies the hash codes for all keys in the 'AffixRuleCollection'.
		///     </para>
		/// </param>
		/// <param name="comparer" type="System.Collections.IComparer">
		///     <para>
		///         The IComparer to use to determine whether two keys are equal.
		///     </para>
		/// </param>
		public AffixRuleCollection(IDictionary dictionary, float loadFactor, IHashCodeProvider codeProvider, IComparer comparer)
		{
			innerHash = new Hashtable (dictionary, loadFactor, codeProvider, comparer);
		}

		/// <summary>
		///     Initializes a new instance of 'AffixRuleCollection'.
		/// </summary>
		/// <param name="capacity" type="int">
		///     <para>
		///         The approximate number of elements that the 'AffixRuleCollection' can initially contain. 
		///     </para>
		/// </param>
		/// <param name="loadFactor" type="float">
		///     <para>
		///         A number in the range from 0.1 through 1.0 indicating the maximum ratio of elements to buckets.
		///     </para>
		/// </param>
		/// <param name="codeProvider" type="System.Collections.IHashCodeProvider">
		///     <para>
		///         The IHashCodeProvider that supplies the hash codes for all keys in the 'AffixRuleCollection'.
		///     </para>
		/// </param>
		/// <param name="comparer" type="System.Collections.IComparer">
		///     <para>
		///         The IComparer to use to determine whether two keys are equal. 
		///     </para>
		/// </param>
		public AffixRuleCollection(int capacity, float loadFactor, IHashCodeProvider codeProvider, IComparer comparer)
		{
			innerHash = new Hashtable (capacity, loadFactor, codeProvider, comparer);
		}

		
		#endregion

		#region Implementation of IDictionary
		
		/// <summary>
		///     Returns an enumerator that can be used to iterate through the 'AffixRuleCollection'.
		/// </summary>
		public AffixRuleEnumerator GetEnumerator()
		{
			return new AffixRuleEnumerator(this);
		}
        
		System.Collections.IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new AffixRuleEnumerator(this);
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		///     Removes the element with the specified key from the AffixRuleCollection.
		/// </summary>
		/// <param name="key" type="string">
		///     <para>
		///         The key of the element to remove
		///     </para>
		/// </param>
		public void Remove(string key)
		{
			innerHash.Remove (key);
		}
		void IDictionary.Remove(object key)
		{
			Remove ((string)key);
		}

		/// <summary>
		///     Determines whether the AffixRuleCollection contains an element with the specified key.
		/// </summary>
		/// <param name="key" type="string">
		///     <para>
		///         The key to locate in the AffixRuleCollection.
		///     </para>
		/// </param>
		/// <returns>
		///     true if the AffixRuleCollection contains an element with the key; otherwise, false.
		/// </returns>
		public bool Contains(string key)
		{
			return innerHash.Contains(key);
		}
		bool IDictionary.Contains(object key)
		{
			return Contains((string)key);
		}

		/// <summary>
		///     removes all elements from the AffixRuleCollection.
		/// </summary>
		public void Clear()
		{
			innerHash.Clear();		
		}

		/// <summary>
		///     adds an element with the provided key and value to the AffixRuleCollection.
		/// </summary>
		/// <param name="key" type="string">
		///     <para>
		///         The string Object to use as the key of the element to add.
		///     </para>
		/// </param>
		/// <param name="value" type="AffixRule">
		///     <para>
		///         The AffixRule Object to use as the value of the element to add.
		///     </para>
		/// </param>
		public void Add(string key, AffixRule value)
		{
			innerHash.Add (key, value);
		}
		void IDictionary.Add(object key, object value)
		{
			Add ((string)key, (AffixRule)value);
		}

		/// <summary>
		///     gets a value indicating whether the AffixRuleCollection is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return innerHash.IsReadOnly;
			}
		}

		/// <summary>
		///     Gets or sets the element with the specified key.
		/// </summary>
		/// <value>
		///     <para>
		///         The key of the element to get or set.
		///     </para>
		/// </value>
		public AffixRule this[string key]
		{
			get
			{
				return (AffixRule) innerHash[key];
			}
			set
			{
				innerHash[key] = value;
			}
		}
		object IDictionary.this[object key]
		{
			get
			{
				return this[(string)key];
			}
			set
			{
				this[(string)key] = (AffixRule)value;
			}
		}
        
		/// <summary>
		///     gets an ICollection containing the values in the AffixRuleCollection.
		/// </summary>
		public System.Collections.ICollection Values
		{
			get
			{
				return innerHash.Values;
			}
		}

		/// <summary>
		///     gets an ICollection containing the keys of the AffixRuleCollection.
		/// </summary>
		public System.Collections.ICollection Keys
		{
			get
			{
				return innerHash.Keys;
			}
		}

		/// <summary>
		///     gets a value indicating whether the AffixRuleCollection has a fixed size.
		/// </summary>
		public bool IsFixedSize
		{
			get
			{
				return innerHash.IsFixedSize;
			}
		}
		#endregion

		#region Implementation of ICollection

		/// <summary>
		///     copies the elements of the AffixRuleCollection to an Array, starting at a particular Array index.
		/// </summary>
		/// <param name="array" type="System.Array">
		///     <para>
		///         The one-dimensional Array that is the destination of the elements copied from AffixRuleCollection. The Array must have zero-based indexing. 
		///     </para>
		/// </param>
		/// <param name="index" type="int">
		///     <para>
		///         The zero-based index in array at which copying begins. 
		///     </para>
		/// </param>		
		public void CopyTo(System.Array array, int index)
		{
			innerHash.CopyTo (array, index);
		}

		/// <summary>
		///     Gets a value indicating whether access to the AffixRuleCollection is synchronized (thread-safe).
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return innerHash.IsSynchronized;
			}
		}

		/// <summary>
		///     Gets the number of elements contained in the AffixRuleCollection.
		/// </summary>
		public int Count
		{
			get
			{
				return innerHash.Count;
			}
		}

		/// <summary>
		///     Gets an object that can be used to synchronize access to the AffixRuleCollection.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				return innerHash.SyncRoot;
			}
		}
		#endregion

		#region Implementation of ICloneable
		
		/// <summary>
		///     Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		///     A new object that is a copy of this instance.
		/// </returns>
		public AffixRuleCollection Clone()
		{
			AffixRuleCollection clone = new AffixRuleCollection();
			clone.innerHash = (Hashtable) innerHash.Clone();
			
			return clone;
		}
		object ICloneable.Clone()
		{
			return Clone();
		}
		
		#endregion
		
		#region "HashTable Methods"
		
		/// <summary>
		///     Determines whether the AffixRuleCollection contains a specific key.
		/// </summary>
		/// <param name="key" type="string">
		///     <para>
		///         The key to locate in the AffixRuleCollection.
		///     </para>
		/// </param>
		/// <returns>
		///     true if the AffixRuleCollection contains an element with the specified key; otherwise, false.
		/// </returns>
		public bool ContainsKey (string key)
		{
			return innerHash.ContainsKey(key);
		}
		
		/// <summary>
		///     Determines whether the AffixRuleCollection contains a specific value.
		/// </summary>
		/// <param name="value" type="AffixRule">
		///     <para>
		///         The value to locate in the AffixRuleCollection. The value can be a null reference (Nothing in Visual Basic).
		///     </para>
		/// </param>
		/// <returns>
		///     true if the AffixRuleCollection contains an element with the specified value; otherwise, false.
		/// </returns>
		public bool ContainsValue (AffixRule value)
		{
			return innerHash.ContainsValue(value);
		}
		
		/// <summary>
		///     Returns a synchronized (thread-safe) wrapper for the AffixRuleCollection.
		/// </summary>
		/// <param name="nonSync" type="AffixRuleCollection">
		///     <para>
		///         The AffixRuleCollection to synchronize.
		///     </para>
		/// </param>
		public static AffixRuleCollection Synchronized(AffixRuleCollection nonSync)
		{
			AffixRuleCollection sync = new AffixRuleCollection();
			sync.innerHash = Hashtable.Synchronized(nonSync.innerHash);

			return sync;
		}
		
		#endregion

		internal Hashtable InnerHash
		{
			get
			{
				return innerHash;
			}
		}
	}
	
}
