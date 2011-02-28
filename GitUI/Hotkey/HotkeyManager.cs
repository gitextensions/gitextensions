using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.Hotkey
{
  public interface IHotkeyable
  {
    //string Name;
    IEnumerable<HotkeyableCommand> AvailableCommands { get; }
  }

  /// <summary>
  /// 
  /// </summary>
  public class HotkeyManager<T> 
    where T : IHotkeyable
  {

    internal void Edit()
    {
      HotkeySettingsManager<T> settingsManager = new HotkeySettingsManager<T>();
      var mappings = settingsManager.LoadSettings();

      using (FormHotkeys form = new FormHotkeys())
      {
        form.ShowDialog();
      }
    }
  }
}
