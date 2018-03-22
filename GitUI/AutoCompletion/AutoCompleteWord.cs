using System;
using System.Linq;

namespace GitUI.AutoCompletion
{
    public class AutoCompleteWord : IEquatable<AutoCompleteWord>
    {
        public string Word { get; }
        private readonly string _camelHumps;

        public AutoCompleteWord(string word)
        {
            Word = word;
            _camelHumps = string.Join("", Word.Where(char.IsUpper));
        }

        public bool Matches(string typedWord)
        {
            return Word.StartsWith(typedWord, StringComparison.OrdinalIgnoreCase) || (typedWord.All(char.IsUpper) && _camelHumps.StartsWith(typedWord));
        }

        public bool Equals(AutoCompleteWord other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Word, other.Word);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((AutoCompleteWord)obj);
        }

        public override int GetHashCode()
        {
            return Word != null ? Word.GetHashCode() : 0;
        }
    }
}