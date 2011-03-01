using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GitUI.Hotkey
{
  /// <summary>
  /// Stores all hotkey mappings of one target
  /// </summary>
  [Serializable]
  public class HotkeySettings
  {
    [XmlArray]
    //public HotkeyMapping[] Mappings { get; set; }
    public HotkeyCommand[] Commands { get; set; }

    [XmlAttribute]
    public string Name { get; set; }

    public HotkeySettings()
    {
    }
  }
}
