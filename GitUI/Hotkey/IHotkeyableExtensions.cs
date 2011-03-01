using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.Hotkey
{
  public static class IHotkeyableExtensions
  {
    public static HotkeySettings CreateSetting(this IHotkeyable hotkeyable)
    {
      return new HotkeySettings() { Name = hotkeyable.Name, Commands = hotkeyable.AvailableCommands.ToArray() };
    }
  }
}
