using System.Diagnostics;

namespace ResourceManager
{
    /// <summary>Provides translated text.</summary>
    [DebuggerDisplay("{" + nameof(Text) + "}")]
    public class TranslationString
    {
        /// <summary>Creates a new <see cref="TranslationString"/> with the specified <paramref name="text"/>.</summary>
        public TranslationString(string text)
        {
            Text = text;
        }

        /// <summary>Gets the translated text.</summary>
        /// <remarks>Setter is required because this property is set via reflection by the translation engine.</remarks>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string Text { get; private set; }

        /// <summary>Returns <see cref="Text"/> value.</summary>
        public override string ToString()
        {
            return Text;
        }
    }
}
