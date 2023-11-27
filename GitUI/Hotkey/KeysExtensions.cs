using System.Globalization;

namespace GitUI.Hotkey
{
    public static class KeysExtensions
    {
        /// <summary>
        /// Strips the modifier from KeyData.
        /// </summary>
        public static Keys GetKeyCode(this Keys keyData)
        {
            return keyData & Keys.KeyCode;
        }

        public static bool IsModifierKey(this Keys key)
        {
            return key == Keys.ShiftKey ||
                   key == Keys.ControlKey ||
                   key == Keys.Alt;
        }

        public static Keys[] GetModifiers(this Keys key)
        {
            // Retrieve the modifiers, mask away the rest
            Keys modifier = key & Keys.Modifiers;

            List<Keys> modifierList = [];

            void AddIfContains(Keys m)
            {
                if (m == (m & modifier))
                {
                    modifierList.Add(m);
                }
            }

            AddIfContains(Keys.Control);
            AddIfContains(Keys.Shift);
            AddIfContains(Keys.Alt);

            return modifierList.ToArray();
        }

        public static string ToText(this Keys key)
        {
            return string.Join(
                "+",
                key.GetModifiers()
                    .Union(new[] { key.GetKeyCode() })
                    .Select(k => k.ToFormattedString())
                    .ToArray());
        }

        public static string? ToFormattedString(this Keys key)
        {
            if (key == Keys.Oemcomma)
            {
                return ",";
            }

            if (key == Keys.Decimal)
            {
                return ".";
            }

            // Get the string representation
            string str = key.ToCultureSpecificString();

            // Strip the leading 'D' if it's a Decimal Key (D1, D2, ...)
            if (str?.Length is 2 && str[0] == 'D')
            {
                str = str[1].ToString();
            }

            return str;
        }

        public static string ToShortcutKeyDisplayString(this Keys key)
        {
            return key.ToText();
        }

        public static string ToShortcutKeyToolTipString(this Keys key)
        {
            return key == Keys.None ? "" : $" ({key.ToShortcutKeyDisplayString()})";
        }

        private static string? ToCultureSpecificString(this Keys key)
        {
            if (key == Keys.None)
            {
                return null;
            }

            // var str = key.ToString(); // OLD: this is culture unspecific
            CultureInfo culture = CultureInfo.CurrentUICulture; // TODO: replace this with the GitExtensions language setting

            // for modifier keys this yields for example "Ctrl+None" thus we have to strip the rest after the +
            return new KeysConverter().ConvertToString(null, culture, key)?.SubstringUntil('+');
        }
    }
}
