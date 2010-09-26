using System.ComponentModel;

namespace ResourceManager.Translation
{
    public class TranslationString : Component
    {
        public TranslationString(string text)
        {
            Text = text;
        }

        public string Text { get; private set; }
    }
}
