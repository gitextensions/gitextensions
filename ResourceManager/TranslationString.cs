using System.ComponentModel;
using System.Diagnostics;

namespace ResourceManager.Translation
{
    /// <summary>Provides translated text.</summary>
    [DebuggerDisplay("{Text}")]
    public class TranslationString : Component
    {
        /// <summary>Creates a new <see cref="TranslationString"/> with the specified <paramref name="text"/>.</summary>
        public TranslationString(string text)
        {
            Text = text;
        }

        /// <summary>Gets the translated text.</summary>
        public string Text { get; private set; }
        /// <summary>Returns <see cref="Text"/> value.</summary>
        public override string ToString() { return Text; }
    }
}
