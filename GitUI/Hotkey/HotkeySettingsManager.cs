using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.Hotkey
{
  class HotkeySettingsManager
  {
    public HotkeySettings[] LoadSettings()
    {
      // Check if we have saved some 
      return new[]
      {
        new FormCommit().CreateSetting()
      };
    }

    //public void SaveSettings(HotkeySettings settings)
    //{
    //}

    //public HotkeySettings LoadSettings(string name)
    //{
    //  return null;
    //}
  }
}
