using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace GitUI.Hotkey
{
  class HotkeySettingsManager
  {
    #region Serializer
    private static XmlSerializer _Serializer;
    /// <summary>Lazy-loaded Serializer for HotkeySettings[]</summary>
    private static XmlSerializer Serializer
    {
      get
      {
        if (_Serializer == null)
          _Serializer = new XmlSerializer(typeof(HotkeySettings[]), new[] { typeof(HotkeyCommand) });
        return _Serializer;
      }
    }
    #endregion

    public static HotkeyCommand[] LoadHotkeys(string name)
    {
      var settings = LoadSettings().FirstOrDefault(s => s.Name == name);

      return settings != null ? settings.Commands : null;
    }

    public static HotkeySettings[] LoadSettings()
    {
      // Get the default settings
      var defaultSettings = CreateDefaultSettings();      
      var loadedSettings = LoadSerializedSettings();

      // If the default settings and the loaded settings do not match, then get the default settings, as we don't trust the loaded ones
      if (DidDefaultSettingsChange(defaultSettings, loadedSettings))
        return defaultSettings;
      else
        return loadedSettings;
    }

    /// <summary>Serializes and saves the supplied settings</summary>
    public static void SaveSettings(HotkeySettings[] settings)
    {
      try
      {
        StringBuilder strBuilder = new StringBuilder();
        using (StringWriter writer = new StringWriter(strBuilder))
        {
          Serializer.Serialize(writer, settings);
          Properties.Settings.Default.Hotkeys = strBuilder.ToString();
          Properties.Settings.Default.Save();
        }
      }
      catch { }
    }

    private static bool DidDefaultSettingsChange(HotkeySettings[] defaultSettings, HotkeySettings[] loadedSettings)
    {
      if (defaultSettings == null || loadedSettings == null)
        return true;

       if (defaultSettings.Length != loadedSettings.Length)
         return true;

      var defaultCmds = defaultSettings.SelectMany(s => s.Commands).ToArray();
      var loadedCmds = loadedSettings.SelectMany(s => s.Commands).ToArray();

      if (defaultCmds.Length != loadedCmds.Length)
        return true;

      // TODO Add additional checks

      return false;
    }

    private static HotkeySettings[] LoadSerializedSettings()
    {
      HotkeySettings[] settings = null;

      try
      {
        using (StringReader reader = new StringReader(Properties.Settings.Default.Hotkeys))
        {
          settings = Serializer.Deserialize(reader) as HotkeySettings[];
        }
      }
      catch { }

      return settings;
    }

    /// <summary>Asks the IHotkeyables to create their default hotkey settings</summary>
    public static HotkeySettings[] CreateDefaultSettings()
    {
      Func<object, Keys, HotkeyCommand> hk = (en, k) => new HotkeyCommand((int)en, en.ToString()) { KeyData = k };
      return new[]
      {
        // FormCommit
        new HotkeySettings(FormCommit.HotkeySettingsName, 
            hk(FormCommit.Commands.FocusUnstagedFiles, Keys.Control | Keys.D1),
            hk(FormCommit.Commands.FocusSelectedDiff, Keys.Control | Keys.D2),
            hk(FormCommit.Commands.FocusStagedFiles, Keys.Control | Keys.D3),
            hk(FormCommit.Commands.FocusCommitMessage, Keys.Control | Keys.D4),
            hk(FormCommit.Commands.StageSelectedFile, Keys.S),
            hk(FormCommit.Commands.UnStageSelectedFile, Keys.U)),
        new HotkeySettings(FormBrowse.HotkeySettingsName,
            hk(FormBrowse.Commands.GitBash, Keys.Control | Keys.G),
            hk(FormBrowse.Commands.GitGui, Keys.None),
            hk(FormBrowse.Commands.GitGitK, Keys.None),
            hk(FormBrowse.Commands.FocusRevisionGrid, Keys.Control | Keys.D1),
            hk(FormBrowse.Commands.FocusCommitInfo, Keys.Control | Keys.D2),
            hk(FormBrowse.Commands.FocusFileTree, Keys.Control | Keys.D3),
            hk(FormBrowse.Commands.FocusDiff, Keys.Control | Keys.D4),
            hk(FormBrowse.Commands.Commit, Keys.Control | Keys.Space),
            hk(FormBrowse.Commands.AddNotes, Keys.Control | Keys.Shift | Keys.N),
            hk(FormBrowse.Commands.FindFileInSelectedCommit, Keys.Control | Keys.Shift | Keys.F),
            hk(FormBrowse.Commands.SelectCurrentRevision, Keys.Control | Keys.Shift | Keys.C),
            hk(FormBrowse.Commands.CheckoutBranch, Keys.Control | Keys.Decimal),
            hk(FormBrowse.Commands.QuickFetch, Keys.None),
            hk(FormBrowse.Commands.QuickPush, Keys.None)),
        new HotkeySettings(RevisionGrid.HotkeySettingsName,
            hk(RevisionGrid.Commands.RevisionFilter, Keys.Control | Keys.F),
            hk(RevisionGrid.Commands.ToggleRevisionGraph, Keys.None),
            hk(RevisionGrid.Commands.ToggleAuthorDateCommitDate, Keys.None),
            hk(RevisionGrid.Commands.ToggleOrderRevisionsByDate, Keys.None),
            hk(RevisionGrid.Commands.ToggleShowRelativeDate, Keys.None),
            hk(RevisionGrid.Commands.ToggleDrawNonRelativesGray, Keys.None),
            hk(RevisionGrid.Commands.ToggleShowGitNotes, Keys.None))
      };
    }
  }
}
