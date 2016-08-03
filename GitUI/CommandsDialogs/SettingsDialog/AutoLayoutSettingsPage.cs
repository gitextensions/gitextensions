using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public abstract partial class AutoLayoutSettingsPage : RepoDistSettingsPage, SettingsLayout
    {
        internal readonly IList<string> _autoGenKeywords = new List<string>();
        private SettingsLayout settingsLayout;
        internal List<ISettingControlBinding> controlBindings = new List<ISettingControlBinding>();

        protected override string GetCommaSeparatedKeywordList()
        {
            return string.Join(",", _autoGenKeywords);
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

        protected virtual SettingsLayout GetSettingsLayout()
        {
            if (settingsLayout == null)
            {
                settingsLayout = CreateSettingsLayout();
                if (settingsLayout.GetControl().Parent == null)
                {
                    this.Controls.Add(settingsLayout.GetControl());
                }
            }

            return settingsLayout;
        }

        protected virtual SettingsLayout CreateSettingsLayout()
        {
            return new TableSettingsLayout(this, CreateDefaultTableLayoutPanel());
        }

        public static TableLayoutPanel CreateDefaultTableLayoutPanel()
        {
            TableLayoutPanel layout = new TableLayoutPanel();

            layout.AutoSize = true;
            layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layout.ColumnCount = 3;
            layout.ColumnStyles.Add(new ColumnStyle());
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new ColumnStyle());
            layout.Dock = DockStyle.Top;
            layout.Location = new Point(0, 0);
            layout.RowCount = 0;
            layout.Size = new Size(951, 518);

            return layout;
        }

        public void AddSettingControl(ISettingControlBinding controlBinding)
        {
            GetSettingsLayout().AddSettingControl(controlBinding);
        }

        public Control GetControl()
        {
            throw new NotImplementedException();
        }

        public void AddKeyword(string aKeyword)
        {
            _autoGenKeywords.Add(aKeyword);
        }

        public void AddControlBinding(ISettingControlBinding controlBinding)
        {
            controlBindings.Add(controlBinding);
        }

        public void AddSettingsLayout(SettingsLayout aLayout)
        {
            GetSettingsLayout().AddSettingsLayout(aLayout);
        }
    }

    public interface SettingsLayout
    {
        void AddSettingControl(ISettingControlBinding controlBinding);
        void AddSettingsLayout(SettingsLayout aLayout);
        Control GetControl();
        void AddKeyword(string aKeyword);
        void AddControlBinding(ISettingControlBinding controlBinding);
    }

    public abstract class BaseSettingsLayout : SettingsLayout
    {
        public readonly SettingsLayout ParentLayout;

        public BaseSettingsLayout(SettingsLayout aParentLayout)
        {
            ParentLayout = aParentLayout;
        }

        public void AddControlBinding(ISettingControlBinding aControlBinding)
        {
            ParentLayout.AddControlBinding(aControlBinding);
        }

        public void AddKeyword(string aKeyword)
        {
            ParentLayout.AddKeyword(aKeyword);
        }

        public void AddSettingControl(ISettingControlBinding aControlBinding)
        {
            AddKeyword(aControlBinding.GetSetting().Caption);
            AddControlBinding(aControlBinding);
            AddSettingControlImpl(aControlBinding);
        }

        public abstract void AddSettingControlImpl(ISettingControlBinding controlBinding);
        public abstract void AddSettingsLayout(SettingsLayout aLayout);
        public abstract Control GetControl();
    }

    public class TableSettingsLayout : BaseSettingsLayout
    {
        protected TableLayoutPanel Panel;
        private int currentRow = -1;

        public TableSettingsLayout(SettingsLayout aParentLayout, TableLayoutPanel aPanel)
            : base(aParentLayout)
        {
            Panel = aPanel;
        }

        public override void AddSettingControlImpl(ISettingControlBinding controlBinding)
        {
            currentRow++;
            var tableLayout = Panel;

            var caption = controlBinding.Caption();

            if (caption != null)
            {
                var label =
                    new Label
                    {
                        Text = controlBinding.Caption(),
                        AutoSize = true
                    };

                label.Anchor = AnchorStyles.Left;
                tableLayout.Controls.Add(label, 0, currentRow);
            }
            var control = controlBinding.GetControl();
            control.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(control, 1, currentRow);
        }

        public override void AddSettingsLayout(SettingsLayout aLayout)
        {
            currentRow++;
            var control = aLayout.GetControl();
            control.Dock = DockStyle.Fill;
            Panel.Controls.Add(control, 1, currentRow);
        }

        public override Control GetControl()
        {
            return Panel;
        }
    }

    public class GroupBoxSettingsLayout : TableSettingsLayout
    {
        protected GroupBox groupBox;

        public GroupBoxSettingsLayout(SettingsLayout aParentLayout, String aGroupBoxCaption)
            : base(aParentLayout, AutoLayoutSettingsPage.CreateDefaultTableLayoutPanel())
        {
            groupBox = new GroupBox();
            groupBox.Text = aGroupBoxCaption;
            groupBox.AutoSize = true;
            groupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox.Controls.Add(Panel);
        }

        public override Control GetControl()
        {
            return groupBox;
        }
    }

    public static class SettingsLayoutExt
    {
        public static void AddSetting(this SettingsLayout aLayout, ISetting aSetting)
        {
            aLayout.AddSettingControl(aSetting.CreateControlBinding());
        }

        public static void AddBoolSetting(this SettingsLayout aLayout, string aCaption, BoolNullableSetting aSetting)
        {
            aLayout.AddSetting(new BoolNullableISettingAdapter(aCaption, aSetting));
        }

        public static void AddStringSetting(this SettingsLayout aLayout, string aCaption, GitCommands.Settings.StringSetting aSetting)
        {
            aLayout.AddSetting(new StringISettingAdapter(aCaption, aSetting));
        }
    }

    public class BoolNullableISettingAdapter : GitUIPluginInterfaces.BoolSetting
    {
        public BoolNullableISettingAdapter(string aCaption, BoolNullableSetting setting)
            : base(setting.FullPath, aCaption, setting.DefaultValue.Value)
        { }
    }

    public class StringISettingAdapter : GitUIPluginInterfaces.StringSetting
    {
        public StringISettingAdapter(string aCaption, GitCommands.Settings.StringSetting setting)
            : base(setting.FullPath, aCaption, setting.DefaultValue)
        { }
    }

}
