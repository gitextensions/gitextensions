using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace GitUI.Hotkey
{
  [Serializable]
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
  }
}
