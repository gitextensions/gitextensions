using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI;

namespace GitUI;

public sealed class SplitterManager
{
    private readonly List<SplitterData> _splitters = [];
    private readonly SettingsSource _settings;

    public SplitterManager(SettingsSource settings)
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
        foreach (SplitterData splitter in _splitters)
        {
            splitter.RestoreFromSettings(_settings);
        }
    }

    /// <summary>
    ///  Restores the distances which have been stored for a larger window than the one the
    ///  splitters had while <see cref="RestoreSplitters"/> ran, and which had to be skipped
    ///  therefore. To be called once the form has reached its final size.
    /// </summary>
    public void RestorePendingSplitters()
    {
        foreach (SplitterData splitter in _splitters)
        {
            if (splitter.IsRestorePending)
            {
                splitter.RestoreFromSettings(_settings);
            }
        }
    }

    public void SaveSplitters()
    {
        foreach (SplitterData splitter in _splitters)
        {
            splitter.SaveToSettings(_settings);
        }
    }

    internal sealed class SplitterData
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
        internal string SizeSettingsKey => _settingName + "_Size";
        internal string DpiSettingsKey => _settingName + "_Dpi";
        internal string DistanceSettingsKey => _settingName + "_Distance";
        internal string FontSizeSettingsKey => _settingName + "_FontSize";
        internal string Panel1CollapsedSettingsKey => _settingName + "_Panel1Collapsed";
        internal string Panel2CollapsedSettingsKey => _settingName + "_Panel2Collapsed";

        /// <summary>
        ///  Gets whether the stored distance did not fit the size the splitter had while it was
        ///  restored, so that it has been kept for <see cref="RestorePendingSplitters"/>.
        /// </summary>
        internal bool IsRestorePending { get; private set; }

        public void RestoreFromSettings(SettingsSource settings)
        {
            IsRestorePending = false;

            _splitter.BeginInit();
            _splitter.SuspendLayout();

            int prevDpi = settings.GetInt(DpiSettingsKey) ?? DpiUtil.DpiX;
            int? prevSizeSetting = settings.GetInt(SizeSettingsKey);
            int prevSize = prevSizeSetting ?? 0;
            int? prevDistanceSetting = settings.GetInt(DistanceSettingsKey);
            int prevDistance = prevDistanceSetting ?? 0;

            if (prevSize > 0 && prevDistance > 0)
            {
                FixedPanel fixedPanel = _splitter.FixedPanel;
                int splitterWidth = _splitter.SplitterWidth;
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
            else if (_defaultDistance.HasValue && !prevSizeSetting.HasValue && !prevDistanceSetting.HasValue)
            {
                SetSplitterDistance(_defaultDistance.Value);
            }

            // Keep the collapsed states as in the designer if not stored yet
            _splitter.Panel1Collapsed = settings.GetBool(Panel1CollapsedSettingsKey, defaultValue: _splitter.Panel1Collapsed);
            _splitter.Panel2Collapsed = settings.GetBool(Panel2CollapsedSettingsKey, defaultValue: _splitter.Panel2Collapsed);

            _splitter.ResumeLayout();
            _splitter.EndInit();
        }

        public void SaveToSettings(SettingsSource settings)
        {
            settings.SetInt(DpiSettingsKey, _dpi);
            settings.SetInt(SizeSettingsKey, SplitterSize);
            settings.SetInt(DistanceSettingsKey, _splitter.SplitterDistance);
            settings.SetFloat(FontSizeSettingsKey, _splitter.Font.Size);
            settings.SetBool(Panel1CollapsedSettingsKey, _splitter.Panel1Collapsed);
            settings.SetBool(Panel2CollapsedSettingsKey, _splitter.Panel2Collapsed);
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
                else
                {
                    // The window is still smaller than the one the distance has been stored for,
                    // because its stored bounds and state are applied only after the form has been
                    // constructed. Keep the distance for RestorePendingSplitters() rather than
                    // dropping it, which used to reset the layout of an enlarged window.
                    IsRestorePending = true;

                    if (_defaultDistance.HasValue && IsValidSplitterDistance(_defaultDistance.Value))
                    {
                        _splitter.SplitterDistance = _defaultDistance.Value;
                    }
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

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly SplitterManager _manager;

        public TestAccessor(SplitterManager manager)
        {
            _manager = manager;
        }

        public List<SplitterData> Splitters => _manager._splitters;
    }
}
