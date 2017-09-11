using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ResourceManager
{
    [Serializable]
    [DebuggerDisplay("Hotkey: {CommandCode} {Name}")]
    public class HotkeyCommand
    {
        #region Properties

        [XmlAttribute]
        public int CommandCode { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public Keys KeyData { get; set; }

        #endregion

        public HotkeyCommand()
        {
        }
        public HotkeyCommand(int commandCode, string name)
        {
            this.CommandCode = commandCode;
            this.Name = name;
        }

        public static HotkeyCommand[] FromEnum(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<object>().Select(c => new HotkeyCommand((int)c, c.ToString())).ToArray();
        }

        public override bool Equals(object obj)
        {
            HotkeyCommand other = obj as HotkeyCommand;
            if (other == null)
            {
                return false;
            }

            return GetFieldsToCompare().SequenceEqual(other.GetFieldsToCompare());
        }

        private IEnumerable<object> GetFieldsToCompare()
        {
            yield return Name;
            yield return CommandCode;
            yield return KeyData;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
