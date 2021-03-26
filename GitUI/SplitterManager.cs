﻿using System;
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
            _splitters.Add(new SplitterData(
                splitter,
                settingName,
                DpiUtil.DpiX,
                defaultDistance));
        }

        public void RestoreSplitters()
        {
            // TODO: Disabled as splitters have become unstable. Refer to #8745 for more details.
            ////foreach (var splitter in _splitters)
            ////{
            ////    splitter.RestoreFromSettings(_settings);
            ////}
        }

        public void SaveSplitters()
        {
            // TODO: Disabled as splitters have become unstable. Refer to #8745 for more details.
            ////foreach (var splitter in _splitters)
            ////{
            ////    splitter.SaveToSettings(_settings);
            ////}
        }

        private sealed class SplitterData
        {
            private readonly SplitContainer _splitter;
            private readonly string _settingName;
            private readonly int _dpi;
            private readonly int? _defaultDistance;

            public SplitterData(SplitContainer splitter, string settingName, int dpi, int? defaultDistance)
            {
                _splitter = splitter;
                _settingName = settingName;
                _dpi = dpi;
                _defaultDistance = defaultDistance;
            }

            private int SplitterSize => _splitter.Orientation == Orientation.Horizontal ? _splitter.Height : _splitter.Width;
            private string SizeSettingsKey => _settingName + "_Size";
            private string DpiSettingsKey => _settingName + "_Dpi";
            private string DistanceSettingsKey => _settingName + "_Distance";
            private string FontSizeSettingsKey => _settingName + "_FontSize";
            private string Panel1CollapsedSettingsKey => _settingName + "_Panel1Collapsed";

            public void RestoreFromSettings(ISettingsSource settings)
            {
                _splitter.BeginInit();
                _splitter.SuspendLayout();

                int prevDpi = settings.GetValue(DpiSettingsKey, DpiUtil.DpiX);
                int prevSize = settings.GetValue(SizeSettingsKey, 0);
                int prevDistance = settings.GetValue(DistanceSettingsKey, 0);

                if (prevSize > 0 && prevDistance > 0)
                {
                    var fixedPanel = _splitter.FixedPanel;
                    var splitterWidth = _splitter.SplitterWidth;
                    if (SplitterSize == prevSize && _dpi == prevDpi)
                    {
                        SetSplitterDistance(fixedPanel == FixedPanel.Panel2 ? prevDistance + splitterWidth : prevDistance);
                    }
                    else
                    {
                        switch (fixedPanel)
                        {
                            case FixedPanel.None:
                                // At this point, the property "SplitterSize" has its original value from design time,
                                // i.e. the actual size after opening the window is unknown yet. The calculation below
                                // determines the resulting splitter distance by the ratio of both sides of the splitter.
                                SetSplitterDistance((float)SplitterSize * prevDistance / prevSize);
                                break;
                            case FixedPanel.Panel1:
                                SetSplitterDistance(DpiUtil.Scale(prevDistance, prevDpi));
                                break;
                            case FixedPanel.Panel2:
                                int panel2PrevSize = DpiUtil.Scale(prevSize, prevDpi) - DpiUtil.Scale(prevDistance, prevDpi);
                                const int paddingOffset = 2; // Refer to FormCommit.ctor+WorkaroundPaddingIncreaseBug
                                SetSplitterDistance(SplitterSize - panel2PrevSize - paddingOffset + splitterWidth);
                                break;
                        }
                    }
                }

                _splitter.Panel1Collapsed = settings.GetValue(Panel1CollapsedSettingsKey, defaultValue: false);

                _splitter.ResumeLayout();
                _splitter.EndInit();
            }

            public void SaveToSettings(ISettingsSource settings)
            {
                settings.SetValue(DpiSettingsKey, _dpi);
                settings.SetValue(SizeSettingsKey, SplitterSize);
                settings.SetValue(DistanceSettingsKey, _splitter.SplitterDistance);
                settings.SetValue(FontSizeSettingsKey, _splitter.Font.Size);
                settings.SetValue(Panel1CollapsedSettingsKey, _splitter.Panel1Collapsed);
            }

            private void SetSplitterDistance(float distance)
            {
                try
                {
                    int intDistance = Convert.ToInt32(distance);

                    if (IsValidSplitterDistance(intDistance))
                    {
                        _splitter.SplitterDistance = intDistance;
                    }
                    else if (_defaultDistance.HasValue && IsValidSplitterDistance(_defaultDistance.Value))
                    {
                        _splitter.SplitterDistance = _defaultDistance.Value;
                    }
                }
                catch
                {
                    // The attempt to set even the default value has failed.
                }

                bool IsValidSplitterDistance(int d)
                {
                    return d > _splitter.Panel1MinSize &&
                           d < SplitterSize - _splitter.Panel2MinSize;
                }
            }
        }
    }
}
