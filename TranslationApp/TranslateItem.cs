using ResourceManager.Translation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TranslationApp
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TranslateItem : INotifyPropertyChanged
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Property { get; set; }
        public string NeutralValue { get; set; }
        private string _translatedValue;
        public string TranslatedValue
        {
            get { return _translatedValue; }
            set
            {
                var pc = PropertyChanged;
                if (pc != null)
                {
                    pc(this, new PropertyChangedEventArgs("TranslatedValue"));
                }
                _translatedValue = value;
            }
        }
        public TranslationType Status { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string DebuggerDisplay
        {
            get
            {
                return string.Format("\"{0}\" - \"{1}\"{2}", Category, NeutralValue,
                    Status == TranslationType.Translated ? "" : " " + Status.ToString());
            }
        }
    }
}
