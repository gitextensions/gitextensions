// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializableDictionary.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Util.Serialization
{
	using System;
	using System.Collections.Generic;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.Schema;
	using System.Xml.Serialization;

	[Serializable]
	[XmlRoot("dictionary")]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SerializableDictionary{TKey,TValue}"/> class.
		/// This is the default constructor provided for XML serializer.
		/// </summary>
		public SerializableDictionary()
		{
		}

		public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException();
			}

			foreach (var pair in dictionary)
			{
				this.Add(pair.Key, pair.Value);
			}
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			/*if (reader.IsEmptyElement)
			{
				return;
			}*/

			var xElement = XElement.Load(reader.ReadSubtree());
			if (xElement.HasElements)
			{
				foreach (var element in xElement.Elements())
				{
					this.Add((TKey)Convert.ChangeType(element.Name.ToString(), typeof(TKey)), (TValue)Convert.ChangeType(element.Value, typeof(TValue)));
				}
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach (var key in this.Keys)
			{
				writer.WriteStartElement(key.ToString());
				writer.WriteValue(this[key]);
				writer.WriteEndElement();
			}
		}
	}
}
