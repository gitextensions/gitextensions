using System;
using System.ComponentModel;
using System.Diagnostics;
using ResourceManager.Xliff;

namespace TranslationApp
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TranslationItemWithCategory : INotifyPropertyChanged, ICloneable
    {
        public TranslationItemWithCategory()
        {
            _item = new TranslationItem();
        }

        public TranslationItemWithCategory(string category, TranslationItem item)
        {
            Category = category;
            _item = item;
        }

        public string Category { get; set; }

        private TranslationItem _item;
        public TranslationItem GetTranslationItem()
        {
            return _item;
        }

        public string Name { get { return _item.Name; } set { _item.Name = value; } }
        public string Property { get { return _item.Property; } set { _item.Property = value; } }
        public string NeutralValue { get { return _item.Source; } set { _item.Source = value; } }
        public string TranslatedValue
        {
            get { return _item.Value; }
            set
            {
                var pc = PropertyChanged;
                if (pc != null)
                {
                    pc(this, new PropertyChangedEventArgs("TranslatedValue"));
                }
                if (value != _item.Value)
                {
                    _item.Value = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSourceEqual(string value)
        {
            if (NeutralValue == null)
                return true;
            bool equal = (value == NeutralValue);
            if (!equal && value.Contains("\n"))
                return value.Replace(Environment.NewLine, "\n") == NeutralValue.Replace(Environment.NewLine, "\n");
            return equal;
        }

        private string DebuggerDisplay
        {
            get
            {
                return string.Format("\"{0}\" - \"{1}\"", Category, NeutralValue);
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public TranslationItemWithCategory Clone()
        {
            return new TranslationItemWithCategory(Category, _item.Clone());
        }
    }
}
