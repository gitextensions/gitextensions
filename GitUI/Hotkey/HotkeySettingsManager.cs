using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.Hotkey
{
  class HotkeySettingsManager<T>
  {
    public void SaveSettings(HotkeyMapping[] hotkeys)
    {
      var settings = CreateSettings(hotkeys);
    }

    private HotkeySettings CreateSettings(HotkeyMapping[] hotkeys)
    {
      return new HotkeySettings()
      {
        Name = typeof(T).FullName,
        Mappings = hotkeys
      };
    }

    public HotkeyMapping[] LoadSettings()
    {
      return null;
    }
  }
}
