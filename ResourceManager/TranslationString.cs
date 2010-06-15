using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace ResourceManager.Translation
{
    public class TranslationString : Component
    {
        public TranslationString(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}
