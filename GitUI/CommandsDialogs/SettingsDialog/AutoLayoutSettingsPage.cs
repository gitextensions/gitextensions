using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public abstract partial class AutoLayoutSettingsPage : RepoDistSettingsPage, ISettingsLayout
    {
        internal readonly IList<string> _autoGenKeywords = new List<string>();
        private ISettingsLayout _settingsLayout;

        protected override string GetCommaSeparatedKeywordList()
        {
            return string.Join(",", _autoGenKeywords);
        }

        protected override ISettingsSource GetCurrentSettings()
        {
            return CurrentSettings;
        }

        protected virtual ISettingsLayout GetSettingsLayout()
        {
            if (_settingsLayout == null)
            {
                _settingsLayout = CreateSettingsLayout();
                if (_settingsLayout.GetControl().Parent == null)
                {
                    Controls.Add(_settingsLayout.GetControl());
                }
            }

            return _settingsLayout;
        }

        protected virtual ISettingsLayout CreateSettingsLayout()
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

        public void AddKeyword(string keyword)
        {
            _autoGenKeywords.Add(keyword);
        }

        public void AddSettingsLayout(ISettingsLayout layout)
        {
            GetSettingsLayout().AddSettingsLayout(layout);
        }
    }

    public interface ISettingsLayout
    {
        void AddSettingControl(ISettingControlBinding controlBinding);
        void AddSettingsLayout(ISettingsLayout layout);
        Control GetControl();
        void AddKeyword(string keyword);
        void AddControlBinding(ISettingControlBinding controlBinding);
    }

    public abstract class BaseSettingsLayout : ISettingsLayout
    {
        public readonly ISettingsLayout ParentLayout;

        protected BaseSettingsLayout(ISettingsLayout parentLayout)
        {
            ParentLayout = parentLayout;
        }

        public void AddControlBinding(ISettingControlBinding aControlBinding)
        {
            ParentLayout.AddControlBinding(aControlBinding);
        }

        public void AddKeyword(string keyword)
        {
            ParentLayout.AddKeyword(keyword);
        }

        public void AddSettingControl(ISettingControlBinding aControlBinding)
        {
            AddKeyword(aControlBinding.GetSetting().Caption);
            AddControlBinding(aControlBinding);
            AddSettingControlImpl(aControlBinding);
        }

        public abstract void AddSettingControlImpl(ISettingControlBinding controlBinding);
        public abstract void AddSettingsLayout(ISettingsLayout layout);
        public abstract Control GetControl();
    }

    public class TableSettingsLayout : BaseSettingsLayout
    {
        protected TableLayoutPanel Panel { get; }
        private int _currentRow = -1;

        public TableSettingsLayout(ISettingsLayout parentLayout, TableLayoutPanel panel)
            : base(parentLayout)
        {
            Panel = panel;
        }

        public override void AddSettingControlImpl(ISettingControlBinding controlBinding)
        {
            _currentRow++;
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
                tableLayout.Controls.Add(label, 0, _currentRow);
            }

            var control = controlBinding.GetControl();
            control.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(control, 1, _currentRow);
        }

        public override void AddSettingsLayout(ISettingsLayout layout)
        {
            _currentRow++;
            var control = layout.GetControl();
            control.Dock = DockStyle.Fill;
            Panel.Controls.Add(control, 1, _currentRow);
        }

        public override Control GetControl()
        {
            return Panel;
        }
    }

    public class GroupBoxSettingsLayout : TableSettingsLayout
    {
        protected GroupBox groupBox;

        public GroupBoxSettingsLayout(ISettingsLayout parentLayout, string groupBoxCaption)
            : base(parentLayout, AutoLayoutSettingsPage.CreateDefaultTableLayoutPanel())
        {
            CreateGroupBox(groupBoxCaption);
        }

        private void CreateGroupBox(string groupBoxCaption)
        {
            var gbox = new GroupBox();
            groupBox = gbox;
            groupBox.Text = groupBoxCaption;
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
        public static void AddSetting(this ISettingsLayout layout, ISetting setting)
        {
            layout.AddSettingControl(setting.CreateControlBinding());
        }

        public static void AddBoolSetting(this ISettingsLayout layout, string caption, BoolNullableSetting setting)
        {
            layout.AddSetting(new BoolNullableISettingAdapter(caption, setting));
        }

        public static void AddStringSetting(this ISettingsLayout layout, string caption, GitCommands.Settings.StringSetting setting)
        {
            layout.AddSetting(new StringISettingAdapter(caption, setting));
        }
    }

    public class BoolNullableISettingAdapter : GitUIPluginInterfaces.BoolSetting
    {
        public BoolNullableISettingAdapter(string caption, BoolNullableSetting setting)
            : base(setting.FullPath, caption, setting.DefaultValue.Value)
        {
        }
    }

    public class StringISettingAdapter : GitUIPluginInterfaces.StringSetting
    {
        public StringISettingAdapter(string caption, GitCommands.Settings.StringSetting setting)
            : base(setting.FullPath, caption, setting.DefaultValue)
        {
        }
    }
}
