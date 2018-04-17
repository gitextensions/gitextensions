using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class SplitterManager
    {
        private readonly ISettingsSource _settings;
        private readonly List<SplitterData> _splitters = new List<SplitterData>();
        private readonly float _designTimeFontSize;

        public SplitterManager(ISettingsSource settings, float designTimeFontSize = 8.25F)
        {
            _settings = settings;
            _designTimeFontSize = designTimeFontSize;
        }

        public void AddSplitter(SplitContainer splitter, string settingName, int? defaultDistance = null)
        {
            var data = new SplitterData
            {
                Splitter = splitter,
                SettingName = settingName,
                DefaultDistance = defaultDistance,
                DesignTimeFontSize = _designTimeFontSize
            };
            _splitters.Add(data);
        }

        public void RestoreSplitters()
        {
            _splitters.ForEach(s => s.RestoreFromSettings(_settings));
        }

        public void SaveSplitters()
        {
            _splitters.ForEach(s => s.SaveToSettings(_settings));
        }

        private class SplitterData
        {
            public SplitContainer Splitter;
            public string SettingName;
            public int? DefaultDistance;
            public float DesignTimeFontSize;
            private string SizeSettingsKey => SettingName + "_Size";
            private string DistanceSettingsKey => SettingName + "_Distance";
            private string FontSizeSettingsKey => SettingName + "_FontSize";
            private string Panel1CollapsedSettingsKey => SettingName + "_Panel1Collapsed";
            private float? _latestFontSize;

            private int SplitterSize => (Splitter.Orientation == Orientation.Horizontal)
                ? Splitter.Height
                : Splitter.Width;

            public void RestoreFromSettings(ISettingsSource settings)
            {
                _latestFontSize = settings.GetFloat(FontSizeSettingsKey) ?? DesignTimeFontSize;

                int? prevSize = settings.GetInt(SizeSettingsKey);
                int? prevDistance = settings.GetInt(DistanceSettingsKey);

                if (prevSize > 0 && prevDistance > 0)
                {
                    if (SplitterSize == prevSize)
                    {
                        SetSplitterDistance(prevDistance.Value);
                    }
                    else
                    {
                        if (Splitter.FixedPanel == FixedPanel.None)
                        {
                            SetSplitterDistance(1F * SplitterSize * prevDistance.Value / prevSize.Value);
                        }

                        if (Splitter.FixedPanel == FixedPanel.Panel1)
                        {
                            SetSplitterDistance(prevDistance.Value);
                        }

                        if (Splitter.FixedPanel == FixedPanel.Panel2)
                        {
                            int panel2PrevSize = prevSize.Value - prevDistance.Value;
                            SetSplitterDistance(SplitterSize - panel2PrevSize);
                        }
                    }
                }

                AdjustToCurrentFontSize();

                Splitter.Panel1Collapsed = settings.GetBool(Panel1CollapsedSettingsKey, defaultValue: false);
            }

            private void AdjustToCurrentFontSize()
            {
                if (_latestFontSize.Value != 0 && Splitter.Font.Size != 0)
                {
                    if (_latestFontSize.Value != Splitter.Font.Size)
                    {
                        float scaleFactor = CalculateScaleFactor();
                        if (scaleFactor != 0)
                        {
                            if (Splitter.FixedPanel == FixedPanel.Panel1)
                            {
                                SetSplitterDistance(Splitter.SplitterDistance + (Splitter.SplitterDistance * scaleFactor));
                            }

                            if (Splitter.FixedPanel == FixedPanel.Panel2)
                            {
                                int panel2Size = SplitterSize - Splitter.SplitterDistance;
                                SetSplitterDistance(Splitter.SplitterDistance - (panel2Size * scaleFactor));
                            }
                        }
                    }
                }

                _latestFontSize = Splitter.Font.Size;
            }

            private float CalculateScaleFactor()
            {
                return (1F * Splitter.Font.Size / _latestFontSize.Value) - 1;
            }

            public void SaveToSettings(ISettingsSource settings)
            {
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
                    else
                    {
                        // Both the value and default are invalid.
                        // Don't attempt to change the SplitterDistance
                        // Use designtime font size to adjust to the current font size
                        _latestFontSize = DesignTimeFontSize;
                    }
                }
                catch
                {
                    // The attempt to set even the default value has failed.
                }
            }

            /// <summary>
            /// Determine whether a given splitter distance value would be permitted for the Splitter
            /// </summary>
            /// <param name="distance">The potential SplitterDistance to try </param>
            /// <returns>true if it is expected that setting a SplitterDistance of distance would succeed
            /// </returns>
            private bool IsValidSplitterDistance(int distance)
            {
                var limit = SplitterSize;

                return distance > Splitter.Panel1MinSize &&
                       distance < limit - Splitter.Panel2MinSize;
            }
        }
    }
}
