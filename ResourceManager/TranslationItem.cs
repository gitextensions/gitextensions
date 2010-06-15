using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ResourceManager.Translation
{
    public class TranslationItem
    {
        public TranslationItem()
        {

        }
        public TranslationItem(string name, string property, string value)
        {
            this.name = name;
            this.property = property;
            this.value = value;
        }

        private string name;
        [XmlAttribute]
        public string Name 
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string property;
        [XmlAttribute]
        public string Property 
        {
            get
            {
                return property;
            }
            set
            {
                property = value;
            }
        }

        private string value;
        public string Value 
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}
