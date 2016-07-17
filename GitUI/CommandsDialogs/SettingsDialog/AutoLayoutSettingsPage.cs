using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public abstract partial class AutoLayoutSettingsPage : RepoDistSettingsPage
    {
        private readonly IList<string> _autoGenKeywords = new List<string>();
        private bool pageInited = false;
        private TableLayoutPanel settingsLayout;
        private List<ISettingControlBinding> controlBindings = new List<ISettingControlBinding>();

        protected abstract IEnumerable<ISetting> GetSettings();

        protected override string GetCommaSeparatedKeywordList()
        {
            return string.Join(",", _autoGenKeywords);
        }

        public override void OnPageShown()
        {
            if (pageInited)
                return;

            pageInited = true;
            CreateSettingsControls();
            LoadSettings();
        }

        protected override void SettingsToPage()
        {
            foreach (var cb in controlBindings)
            {
                cb.LoadSetting(GetCurrentSettings(), AreEffectiveSettingsSet);
            }
        }

        protected override void PageToSettings()
        {
            foreach (var cb in controlBindings)
            {
                cb.SaveSetting(GetCurrentSettings());
            }
        }

        protected virtual ISettingsSource GetCurrentSettings()
        {
            return CurrentSettings;
        }

        protected virtual TableLayoutPanel GetSettingsLayout()
        {
            if (settingsLayout == null)
            {
                settingsLayout = CreateSettingsLayout();
                if (settingsLayout.Parent == null)
                {
                    this.Controls.Add(settingsLayout);
                }
            }

            return settingsLayout;
        }

        protected virtual TableLayoutPanel CreateSettingsLayout()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel();

            mainLayout.AutoSize = true;
            mainLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mainLayout.ColumnCount = 3;
            mainLayout.ColumnStyles.Add(new ColumnStyle());
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.ColumnStyles.Add(new ColumnStyle());
            mainLayout.Dock = DockStyle.Top;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "settingsLayout";
            mainLayout.RowCount = 0;
            mainLayout.Size = new Size(951, 518);

            return mainLayout;
        }

        private void CreateSettingsControls()
        {
            var tableLayout = GetSettingsLayout();
            var settings = GetSettings();

            int row = -1;
            foreach (var setting in settings)
            {                
                _autoGenKeywords.Add(setting.Caption);
                row++;

                var label =
                    new Label
                    {
                        Text = setting.Caption,
                        AutoSize = true
                    };

                label.Anchor = AnchorStyles.Left;
                tableLayout.Controls.Add(label, 0, row);

                var controlBinding = setting.CreateControlBinding();
                controlBindings.Add(controlBinding);
                var control = controlBinding.GetControl();
                control.Dock = DockStyle.Fill;
                tableLayout.Controls.Add(control, 1, row);
            }
        }

    }

    public class BoolNullableISettingAdapter : GitUIPluginInterfaces.BoolSetting
    {
        public BoolNullableISettingAdapter(string aCaption, BoolNullableSetting setting)
            : base(setting.FullPath, aCaption, setting.DefaultValue.Value)
        { }
    }
}
