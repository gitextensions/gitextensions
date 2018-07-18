using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;

namespace GitUI
{
    public sealed class SplitterManager
    {
        private readonly List<SplitterData> _splitters = new List<SplitterData>();
        private readonly ISettingsSource _settings;

        public SplitterManager(ISettingsSource settings)
        {
            _settings = settings;
        }

        public void AddSplitter(SplitContainer splitter, string settingName, int? defaultDistance = null)
        {
            _splitters.Add(new SplitterData
            {
                Splitter = splitter,
                SettingName = settingName,
                DefaultDistance = defaultDistance,
                Dpi = DpiUtil.DpiX
            });
        }

        public void RestoreSplitters()
        {
            foreach (var splitter in _splitters)
            {
                splitter.RestoreFromSettings(_settings);
            }
        }

        public void SaveSplitters()
        {
            foreach (var splitter in _splitters)
            {
                splitter.SaveToSettings(_settings);
            }
        }

        private sealed class SplitterData
        {
            public SplitContainer Splitter;
            public string SettingName;
            public int Dpi;
            public int? DefaultDistance;

            private int SplitterSize => Splitter.Orientation == Orientation.Horizontal ? Splitter.Height : Splitter.Width;
            private string SizeSettingsKey => SettingName + "_Size";
            private string DpiSettingsKey => SettingName + "_Dpi";
            private string DistanceSettingsKey => SettingName + "_Distance";
            private string FontSizeSettingsKey => SettingName + "_FontSize";
            private string Panel1CollapsedSettingsKey => SettingName + "_Panel1Collapsed";

            public void RestoreFromSettings(ISettingsSource settings)
            {
                Splitter.BeginInit();
                Splitter.SuspendLayout();

                int prevDpi = settings.GetInt(DpiSettingsKey) ?? DpiUtil.DpiX;
                int prevSize = settings.GetInt(SizeSettingsKey) ?? 0;
                int prevDistance = settings.GetInt(DistanceSettingsKey) ?? 0;

                if (prevSize > 0 && prevDistance > 0)
                {
                    if (SplitterSize == prevSize && Dpi == prevDpi)
                    {
                        SetSplitterDistance(prevDistance);
                    }
                    else
                    {
                        switch (Splitter.FixedPanel)
                        {
                            case FixedPanel.None:
                                SetSplitterDistance((float)SplitterSize * prevDistance / prevSize);
                                break;
                            case FixedPanel.Panel1:
                                SetSplitterDistance(DpiUtil.Scale(prevDistance, prevDpi));
                                break;
                            case FixedPanel.Panel2:
                                int panel2PrevSize = DpiUtil.Scale(prevSize, prevDpi) - DpiUtil.Scale(prevDistance, prevDpi);
                                SetSplitterDistance(SplitterSize - panel2PrevSize);
                                break;
                        }
                    }
                }

                Splitter.Panel1Collapsed = settings.GetBool(Panel1CollapsedSettingsKey, defaultValue: false);

                Splitter.ResumeLayout();
                Splitter.EndInit();
            }

            public void SaveToSettings(ISettingsSource settings)
            {
                settings.SetInt(DpiSettingsKey, Dpi);
                settings.SetInt(SizeSettingsKey, SplitterSize);
                settings.SetInt(DistanceSettingsKey, Splitter.SplitterDistance);
                settings.SetFloat(FontSizeSettingsKey, Splitter.Font.Size);
                settings.SetBool(Panel1CollapsedSettingsKey, Splitter.Panel1Collapsed);
            }

            private void SetSplitterDistance(float distance)
            {
                try
                {
                    int intDistance = Convert.ToInt32(distance);

                    if (IsValidSplitterDistance(intDistance))
                    {
                        Splitter.SplitterDistance = intDistance;
                    }
                    else if (DefaultDistance.HasValue && IsValidSplitterDistance(DefaultDistance.Value))
                    {
                        Splitter.SplitterDistance = DefaultDistance.Value;
                    }
                }
                catch
                {
                    // The attempt to set even the default value has failed.
                }

                bool IsValidSplitterDistance(int d)
                {
                    return d > Splitter.Panel1MinSize &&
                           d < SplitterSize - Splitter.Panel2MinSize;
                }
            }
        }
    }
}
