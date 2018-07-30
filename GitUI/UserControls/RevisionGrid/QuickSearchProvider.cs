using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.UserControls.RevisionGrid;

namespace GitUI
{
    /// <summary>
    /// Provides a 'quick search' capability to <see cref="RevisionGridControl"/> whereby the user may type directly
    /// into the control in order to search for the typed word.
    /// </summary>
    internal sealed class QuickSearchProvider
    {
        private readonly Label _label;
        private readonly RevisionDataGridView _gridView;
        private readonly Timer _quickSearchTimer;
        private readonly IGitRevisionTester _gitRevisionTester;

        private string _lastQuickSearchString = "";
        private string _quickSearchString = "";

        public QuickSearchProvider(RevisionDataGridView gridView, Func<string> getWorkingDir)
        {
            _gridView = gridView;

            _gitRevisionTester = new GitRevisionTester(new FullPathResolver(getWorkingDir));

            _label = new Label
            {
                Location = new Point(0, 0),
                Padding = new Padding(7, 5, 5, 5),
                BorderStyle = BorderStyle.None,
                ForeColor = SystemColors.InfoText,
                BackColor = SystemColors.Info,
                Font = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold),
                Visible = false,
                UseMnemonic = false
            };

            _quickSearchTimer = new Timer { Interval = AppSettings.RevisionGridQuickSearchTimeout };
            _quickSearchTimer.Tick += (sender, e) =>
            {
                _quickSearchTimer.Stop();
                _quickSearchString = "";
                HideQuickSearchString();
            };

            _gridView.Controls.Add(_label);
        }

        public void OnKeyPress(KeyPressEventArgs e)
        {
            var curIndex = _gridView.SelectedRows.Count > 0
                ? _gridView.SelectedRows[0].Index
                : -1;

            curIndex = curIndex >= 0 ? curIndex : 0;

            if (e.KeyChar == 8 && _quickSearchString.Length > 1)
            {
                // backspace
                RestartQuickSearchTimer();

                _quickSearchString = _quickSearchString.Substring(0, _quickSearchString.Length - 1);

                FindNextMatch(curIndex, _quickSearchString, false);
                _lastQuickSearchString = _quickSearchString;

                e.Handled = true;
                ShowQuickSearchString();
            }
            else if (!char.IsControl(e.KeyChar))
            {
                RestartQuickSearchTimer();

                // The code below is meant to fix the weird key values when pressing keys e.g. ".".
                _quickSearchString = string.Concat(_quickSearchString, char.ToLower(e.KeyChar));

                FindNextMatch(curIndex, _quickSearchString, false);
                _lastQuickSearchString = _quickSearchString;

                e.Handled = true;
                ShowQuickSearchString();
            }
            else
            {
                _quickSearchString = "";
                HideQuickSearchString();
                e.Handled = false;
            }
        }

        public void NextResult(bool down)
        {
            var curIndex = -1;
            if (_gridView.SelectedRows.Count > 0)
            {
                curIndex = _gridView.SelectedRows[0].Index;
            }

            RestartQuickSearchTimer();

            bool reverse = !down;
            var nextIndex = 0;
            if (curIndex >= 0)
            {
                nextIndex = reverse ? curIndex - 1 : curIndex + 1;
            }

            _quickSearchString = _lastQuickSearchString;
            FindNextMatch(nextIndex, _quickSearchString, reverse);
            ShowQuickSearchString();
        }

        private void ShowQuickSearchString()
        {
            _label.Visible = true;
            _label.BringToFront();
            _label.Text = _quickSearchString;
            _label.AutoSize = true;
        }

        private void HideQuickSearchString()
        {
            _label.Visible = false;
        }

        private void RestartQuickSearchTimer()
        {
            _quickSearchTimer.Stop();
            _quickSearchTimer.Interval = AppSettings.RevisionGridQuickSearchTimeout;
            _quickSearchTimer.Start();
        }

        private void FindNextMatch(int startIndex, string searchString, bool reverse)
        {
            if (_gridView.RowCount == 0)
            {
                return;
            }

            var matchIndex = reverse
                ? SearchBackwards()
                : SearchForward();

            if (matchIndex.HasValue)
            {
                _label.ForeColor = SystemColors.InfoText;

                // Prevent flickering when further typing is selecting the same row
                if (_gridView.SelectedRows.Count != 1 || _gridView.SelectedRows[0].Index != matchIndex)
                {
                    using (WaitCursorScope.Enter())
                    {
                        _gridView.ClearSelection();
                        _gridView.Rows[matchIndex.Value].Selected = true;

                        _gridView.CurrentCell = _gridView.Rows[matchIndex.Value].Cells[1];
                    }
                }
            }
            else
            {
                _label.ForeColor = Color.DarkRed;
            }

            int? SearchForward()
            {
                // Check for out of bounds roll over if required
                int index;
                if (startIndex < 0 || startIndex >= _gridView.RowCount)
                {
                    startIndex = 0;
                }

                for (index = startIndex; index < _gridView.RowCount; ++index)
                {
                    if (_gitRevisionTester.Matches(_gridView.GetRevision(index), searchString))
                    {
                        return index;
                    }
                }

                // We didn't find it so start searching from the top
                for (index = 0; index < startIndex; ++index)
                {
                    if (_gitRevisionTester.Matches(_gridView.GetRevision(index), searchString))
                    {
                        return index;
                    }
                }

                return null;
            }

            int? SearchBackwards()
            {
                // Check for out of bounds roll over if required
                int index;
                if (startIndex < 0 || startIndex >= _gridView.RowCount)
                {
                    startIndex = _gridView.RowCount - 1;
                }

                for (index = startIndex; index >= 0; --index)
                {
                    if (_gitRevisionTester.Matches(_gridView.GetRevision(index), searchString))
                    {
                        return index;
                    }
                }

                // We didn't find it so start searching from the bottom
                for (index = _gridView.RowCount - 1; index > startIndex; --index)
                {
                    if (_gitRevisionTester.Matches(_gridView.GetRevision(index), searchString))
                    {
                        return index;
                    }
                }

                return null;
            }
        }
    }
}