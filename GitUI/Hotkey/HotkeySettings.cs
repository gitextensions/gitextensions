using System;
using System.Linq;
using System.Xml.Serialization;
using ResourceManager;

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
        {}

        public HotkeySettings(string name, params HotkeyCommand[] commands)
        {
            Name = name;
            Commands = commands;
        }

        public override bool Equals(object obj)
        {
            if (obj is HotkeySettings other)
                return Commands.SequenceEqual(other.Commands);
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
