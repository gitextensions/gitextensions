using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.Hotkey
{
  public interface IHotkeyable
  {
    string Name { get; }
    IEnumerable<HotkeyCommand> AvailableCommands { get; }
  }
}
