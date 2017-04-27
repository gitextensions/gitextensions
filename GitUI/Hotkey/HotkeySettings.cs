using System;
using System.Xml.Serialization;
using ResourceManager;
using System.Linq;

namespace GitUI.Hotkey
{
    /// <summary>
    /// Stores all hotkey mappings of one target
    /// </summary>
    [Serializable]
    public class HotkeySettings
    {
        [XmlArray]
        public HotkeyCommand[] Commands { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        public HotkeySettings()
        {
        }
        public HotkeySettings(string name, params HotkeyCommand[] commands)
        {
            this.Name = name;
            this.Commands = commands;
        }

        public override bool Equals(object obj)
        {
            HotkeySettings other = obj as HotkeySettings;
            if (other == null)
            {
                return false;
            }

            return Commands.SequenceEqual(other.Commands);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
