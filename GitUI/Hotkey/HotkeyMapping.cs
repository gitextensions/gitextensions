using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GitUI.Hotkey
{
  [Serializable]
  public class HotkeyMapping
  {
    [XmlAttribute]
    public int Command { get; set; }

    [XmlAttribute]
    public Keys KeyData { get; set; }

    public HotkeyMapping()
    {
    }
    public HotkeyMapping(Keys keyData, int command)
    {
      this.KeyData = keyData;
      this.Command = command;
    }
  }
}
