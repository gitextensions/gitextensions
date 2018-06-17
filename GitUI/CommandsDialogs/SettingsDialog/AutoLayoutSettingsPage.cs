using System;
using System.Drawing;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public abstract partial class AutoLayoutSettingsPage : RepoDistSettingsPage, ISettingsLayout
    {
        private ISettingsLayout _settingsLayout;

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
            return new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 3,
                ColumnStyles =
                {
                    new ColumnStyle(),
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle()
                },
                Dock = DockStyle.Top,
                Location = new Point(0, 0),
                RowCount = 0,
                Size = new Size(951, 518)
            };
        }

        public void AddSettingControl(ISettingControlBinding controlBinding)
        {
            GetSettingsLayout().AddSettingControl(controlBinding);
        }

        public Control GetControl()
        {
            throw new NotImplementedException();
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

        public void AddSettingControl(ISettingControlBinding aControlBinding)
        {
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
                var label = new Label
                {
                    Text = controlBinding.Caption(),
                    AutoSize = true,
                    Anchor = AnchorStyles.Left
                };

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
}
