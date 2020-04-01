using System;

namespace GitExtUtils.GitUI.Theming
{
    public struct ThemeId
    {
        public static ThemeId Default { get; } = new ThemeId(string.Empty, false);

        public string Name { get; }
        public bool IsBuiltin { get; }

        public ThemeId(string name, bool isBuiltin)
        {
            Name = name ?? string.Empty;
            IsBuiltin = isBuiltin;
        }

        public override bool Equals(object obj) =>
            obj is ThemeId other && Equals(other);

        public override int GetHashCode()
        {
            // Name can be null because of default struct constructor
            int nameHashCode = Name == null
                ? 0
                : StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
            return nameHashCode ^ IsBuiltin.GetHashCode();
        }

        public static bool operator ==(ThemeId left, ThemeId right) =>
            left.Equals(right);

        public static bool operator !=(ThemeId left, ThemeId right) =>
            !left.Equals(right);

        private bool Equals(ThemeId other) =>
            string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
            IsBuiltin == other.IsBuiltin;
    }
}
