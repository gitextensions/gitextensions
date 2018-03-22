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
        {
        }

        public HotkeySettings(string name, params HotkeyCommand[] commands)
        {
            Name = name;
            Commands = commands;
        }

        public override bool Equals(object obj)
        {
            return obj is HotkeySettings other && Commands.SequenceEqual(other.Commands);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
