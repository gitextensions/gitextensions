using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.Hotkey
{
  public class HotkeyableCommand
  {
    public int Command { get; set; }
    public string Name { get; set; }

    public HotkeyableCommand(int command, string name)
    {
      this.Command = command;
      this.Name = name;
    }
  }
}
