using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    public class FormThemeEditor : GitExtensionsForm
    {
        private const string Title = "Theme editor";
        private const string HintOnResettingColor = "middle-click to reset";
        private readonly Size _cellSize;
        private readonly Padding _cellMargin;
        private readonly FlowLayoutPanel _layoutPanel;
        private readonly FormThemeEditorController _controller;
        private bool _resetting;

        public FormThemeEditor()
        {
            _cellSize = DpiUtil.Scale(new Size(128, 36));
            _cellMargin = DpiUtil.Scale(new Padding(3));
            _layoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };

            Controls.Add(_layoutPanel);

            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = Title;

            FormClosing += (sender, args) =>
            {
                if (args.CloseReason == CloseReason.UserClosing)
                {
                    args.Cancel = true;
                    ((Form)sender).Hide();
                }
            };
        }

        public FormThemeEditor(FormThemeEditorController controller)
            : this()
        {
            _controller = controller;

            var sysColors = Theme.SysColors.OrderBy(c => c.ToString(), StringComparer.InvariantCulture);
            foreach (var name in sysColors)
            {
                _layoutPanel.Controls.Add(
                    CreateColorPicker(
                        name,
                        (theme, n) => theme.GetColor(n),
                        (theme, n) => theme.GetDefaultColor(n),
                        (editor, n, color) => editor.SetColor(n, color),
                        (editor, n) => editor.Reset(n)));
            }

            var appColors = Theme.AppColors.OrderBy(c => c.ToString(), StringComparer.InvariantCulture);
            foreach (var name in appColors)
            {
                _layoutPanel.Controls.Add(
                    CreateColorPicker(
                        name,
                        (theme, n) => theme.GetColor(n),
                        (theme, n) => theme.GetDefaultColor(n),
                        (editor, n, color) => editor.SetColor(n, color),
                        (editor, n) => editor.Reset(n)));
            }

            _layoutPanel.SetFlowBreak(_layoutPanel.Controls[_layoutPanel.Controls.Count - 1], true);

            AddButtons();

            Closing += (s, e) =>
            {
                Hide();
                e.Cancel = true;
            };

            UpdateFormSize();
            InitializeComplete();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void BindColor(Control c)
        {
            UpdateBackColor(c);
            c.SetForeColorForBackColor();

            _controller.ColorChanged += () =>
            {
                UpdateBackColor(c);
                c.SetForeColorForBackColor();
            };
        }

        private void UpdateBackColor(Control c)
        {
            if (c.HasTag<AppColor>())
            {
                var name = c.GetTag<AppColor>();
                c.BackColor = _controller.GetColor(name);
            }
            else if (c.HasTag<KnownColor>())
            {
                var name = c.GetTag<KnownColor>();
                c.BackColor = _controller.GetColor(name);
            }
        }

        private void UpdateFormSize()
        {
            int colorPickersCount = Theme.AppColors.Count + Theme.SysColors.Count;
            int columnsCount = (int)Math.Ceiling(Math.Sqrt(colorPickersCount));
            int rowsCount = columnsCount != 0
                ? (int)Math.Ceiling((float)colorPickersCount / columnsCount)
                : 1;

            rowsCount++; // buttons

            var cellCount = new Size(columnsCount, rowsCount);

            ClientSize = _layoutPanel.Margin.Size +
                _cellMargin.Size.MultiplyBy(cellCount) +
                _cellSize.MultiplyBy(cellCount);
        }

        private Control CreateColorPicker<TName>(
            TName colorName,
            Func<FormThemeEditorController, TName, Color> getColor,
            Func<FormThemeEditorController, TName, Color> getDefaultColor,
            Action<FormThemeEditorController, TName, Color> setColor,
            Action<FormThemeEditorController, TName> resetColor)
        {
            var control = new Label
            {
                AutoSize = false,
                Size = _cellSize,
                Margin = _cellMargin,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.None
            };

            UpdateText();
            _controller.ColorChanged += UpdateText;

            void UpdateText()
            {
                var result = new StringBuilder();
                result.Append(colorName);

                if (getColor(_controller, colorName) != getDefaultColor(_controller, colorName))
                {
                    result.AppendLine("*");
                    result.Append(HintOnResettingColor);
                }

                control.Text = result.ToString();
            }

            control.SetTag(colorName);
            BindColor(control);

            control.MouseClick += (t, te) =>
            {
                var ctrl = (Control)t;
                switch (te.Button)
                {
                    case MouseButtons.Left:
                        var dialog = new ColorDialog();
                        dialog.Color = ctrl.BackColor;
                        var result = dialog.ShowDialog();
                        if (result != DialogResult.OK)
                        {
                            break;
                        }

                        var name = ctrl.GetTag<TName>();
                        var value = dialog.Color;
                        setColor(_controller, name, value);
                        ctrl.BackColor = dialog.Color;
                        ctrl.SetForeColorForBackColor();
                        break;

                    case MouseButtons.Middle:
                        resetColor(_controller, colorName);
                        break;
                }
            };

            return control;
        }

        private void AddButtons()
        {
            CreateButton("Reset all colors", (s, e) =>
            {
                if (_resetting)
                {
                    return;
                }

                _resetting = true;
                _controller.ResetAllColors();
                _resetting = false;
            });

            CreateButton("Save", (s, e) =>
            {
                _controller.SaveToFileDialog();
            });

            CreateButton("Load", (s, e) =>
            {
                _controller.ApplyThemeFromFileDialog();
            });
        }

        private void CreateButton(string text, EventHandler clickHandler)
        {
            var control = new Button
            {
                Size = _cellSize,
                Margin = _cellMargin,
                BackColor = SystemColors.Control,
                Text = text,
                Font = new Font(Font.FontFamily, Font.Size * 9f / 8f, FontStyle.Bold, Font.Unit),
            };

            BindColor(control);
            control.Click += clickHandler;
            _layoutPanel.Controls.Add(control);
        }
    }
}
