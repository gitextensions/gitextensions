using System.ComponentModel;
using System.Diagnostics;

namespace ResourceManager.Translation
{
    [DebuggerDisplay("{Text}")]
    public class TranslationString : Component
    {
        public TranslationString(string text)
        {
            Text = text;
        }

        public string Text { get; private set; }
    }
}
