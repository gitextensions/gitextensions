using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Repository;
using GitUI.Properties;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public sealed partial class RecentRepositoryItem : GitExtensionsControl
    {
        private readonly TranslationString _banchTooltip = new TranslationString("Branch: {0}");
        private readonly TranslationString _categoryTooltip = new TranslationString("Category: {0}");
        private static readonly Color DefaultBranchNameColor = SystemColors.HotTrack;
        private static readonly Color DefaultCategoryColor = SystemColors.GrayText;
        private static readonly Color DefaultFavouriteColor = Color.DarkGoldenrod;
        private static readonly Color DefaultHoverColor = SystemColors.ControlDark;
        private readonly Repository _repository;
        private readonly ToolTip _toolTip;
        private Color _branchNameColor = DefaultBranchNameColor;
        private Color _categoryColor = DefaultCategoryColor;
        private Color _favouriteColor = DefaultFavouriteColor;
        private Color _hoverColor = DefaultHoverColor;
        private bool _isContextMenuOpened;
        private bool _isHover;
        private string _category;
        private string _path;
        private string _branchName;
        private bool? _isPinned;
        private Color _backColor;


        public event EventHandler<RepositoryEventArgs> RepositorySelected;
        public event EventHandler<RepositoryEventArgs> RepositoryFavouriteChanged;


        public RecentRepositoryItem()
        {
            InitializeComponent();
            Translate();
            ApplyScaling();

            _toolTip = new ToolTip
            {
                InitialDelay = 1,
                AutomaticDelay = 1,
                AutoPopDelay = 5000,
                UseFading = false,
                UseAnimation = false,
                ReshowDelay = 1
            };

            BranchName = string.Empty;
            Path = string.Empty;

            MouseEnter += RecentRepositoryItem_MouseEnter;
            var controls = new Control[] { _NO_TRANSLATE_BranchName, _NO_TRANSLATE_Path, pbIcon, _NO_TRANSLATE_Category, flpnlRow2, tlpnlContainer };
            foreach (Control control in controls)
            {
                control.MouseEnter += RecentRepositoryItem_MouseEnter;
                control.MouseLeave += (s, e) => OnMouseLeave(e);
                control.MouseDown += (s, e) =>
                {
                    if (s == pbIcon)
                    {
                        return;
                    }
                    if (e.Button == MouseButtons.Right)
                    {
                        ContextMenuStrip?.Show(this, e.Location);
                    }
                    else
                    {
                        OnMouseClick(e);
                    }
                };
            }
        }


        public RecentRepositoryItem(Repository repository)
            : this()
        {
            _repository = repository;
            if (_repository == null)
            {
                return;
            }

            Category = _repository.Category;
            Path = _repository.Path;
            Favourite = _repository.Anchor == Repository.RepositoryAnchor.MostRecent;

            if (AppSettings.DashboardShowCurrentBranch)
            {
                using (var branchNameLoader = new AsyncLoader())
                {
                    branchNameLoader.Load(() =>
                    {
                        if (!GitModule.IsBareRepository(repository.Path))
                        {
                            return GitModule.GetSelectedBranchFast(repository.Path);
                        }
                        return string.Empty;
                    },
                    branchName => BranchName = branchName);
                }
            }
        }


        [Category("Appearance")]
        [DefaultValue("")]
        public string BranchName
        {
            get { return _branchName; }
            set
            {
                if (string.Equals(_branchName, value, StringComparison.Ordinal))
                {
                    return;
                }
                _branchName = value;
                _NO_TRANSLATE_BranchName.Text = value;
                _NO_TRANSLATE_BranchName.Visible = !string.IsNullOrWhiteSpace(value);
                _NO_TRANSLATE_BranchName.Invalidate();
                _toolTip.SetToolTip(_NO_TRANSLATE_BranchName, string.Format(_banchTooltip.Text, value));
                Invalidate();
                Parent?.PerformLayout();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "HotTrack")]
        public Color BranchNameColor
        {
            get { return _branchNameColor; }
            set
            {
                _branchNameColor = value;
                _NO_TRANSLATE_BranchName.ForeColor = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        public string Category
        {
            get { return _category; }
            set
            {
                if (string.Equals(_category, value, StringComparison.Ordinal))
                {
                    return;
                }
                _category = value;
                if (_repository != null)
                {
                    _repository.Category = value;
                }
                _NO_TRANSLATE_Category.Text = value;
                _NO_TRANSLATE_Category.Invalidate();
                _NO_TRANSLATE_Category.Visible = !string.IsNullOrWhiteSpace(value);
                _toolTip.SetToolTip(_NO_TRANSLATE_Category, string.Format(_categoryTooltip.Text, value));
                Invalidate();
                Parent?.PerformLayout();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "GrayText")]
        public Color CategoryColor
        {
            get { return _categoryColor; }
            set
            {
                _categoryColor = value;
                _NO_TRANSLATE_Category.ForeColor = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool Favourite
        {
            get { return _isPinned ?? false; }
            set
            {
                if (_isPinned == value)
                {
                    return;
                }
                _isPinned = value;
                pbIcon.Image = value ? Resources.Star : null;
                pbIcon.Invalidate();
                SetTextColor();
                OnRepositoryFavouriteChanged();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DarkGoldenrod")]
        public Color FavouriteColor
        {
            get { return _favouriteColor; }
            set
            {
                _favouriteColor = value;
                SetTextColor();
            }
        }

        [Category("Appearance")]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                base.ForeColor = _NO_TRANSLATE_Path.LinkColor = value;
                SetTextColor();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "ControlDark")]
        public Color HoverColor
        {
            get { return _hoverColor; }
            set { _hoverColor = value; }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public string Path
        {
            get { return _path; }
            set
            {
                if (string.Equals(_path, value, StringComparison.Ordinal))
                {
                    return;
                }
                _path = value;
                _NO_TRANSLATE_Path.Text = value;
                _NO_TRANSLATE_Path.Invalidate();
                _toolTip.SetToolTip(_NO_TRANSLATE_Path, Path);
            }
        }

        public Repository Repository => _repository;

        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return base.ContextMenuStrip;
            }
            set
            {
                if (base.ContextMenuStrip == value)
                {
                    return;
                }
                if (base.ContextMenuStrip != null)
                {
                    base.ContextMenuStrip.Opened -= ContextMenuStrip_Opened;
                    base.ContextMenuStrip.Closed -= ContextMenuStrip_Closed;
                }
                base.ContextMenuStrip = value;
                if (value != null)
                {
                    base.ContextMenuStrip.Opened += ContextMenuStrip_Opened;
                    base.ContextMenuStrip.Closed += ContextMenuStrip_Closed;
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_repository != null && e.Button == MouseButtons.Left)
            {
                OnRepositorySelected();
            }
            base.OnMouseClick(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            ResetHighlight();
            base.OnMouseLeave(e);
        }


        private void ApplyScaling()
        {
            _NO_TRANSLATE_Path.Font = AppSettings.Font;
            _NO_TRANSLATE_BranchName.Font =
                _NO_TRANSLATE_Category.Font = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints - 1);

            var scaleFactor = GetScaleFactor();

            ApplyPaddingScaling(this, scaleFactor);

            var textSize = TextRenderer.MeasureText("ABC", _NO_TRANSLATE_Path.Font);
            tlpnlContainer.RowStyles[0].Height = textSize.Height;
            tlpnlContainer.ColumnStyles[0].Width = textSize.Height;
            textSize = TextRenderer.MeasureText("ABC", _NO_TRANSLATE_BranchName.Font);
            tlpnlContainer.RowStyles[1].Height = textSize.Height;

            Height = (int)(Padding.Top + Padding.Bottom + tlpnlContainer.RowStyles[0].Height + tlpnlContainer.RowStyles[1].Height);
        }

        private void OnRepositorySelected()
        {
            var handler = RepositorySelected;
            handler?.Invoke(this, new RepositoryEventArgs(_repository));
        }

        private void OnRepositoryFavouriteChanged()
        {
            var handler = RepositoryFavouriteChanged;
            handler?.Invoke(this, new RepositoryEventArgs(_repository));
        }

        private void ResetHighlight()
        {
            if (!_isHover || _isContextMenuOpened)
            {
                return;
            }
            _isHover = false;
            if (_repository == null)
            {
                return;
            }
            pbIcon.Image = Favourite ? Resources.Star : null;
            BackColor = _backColor;
        }

        private void SetHighlight()
        {
            if (_isHover)
            {
                return;
            }
            _isHover = true;
            if (_repository == null)
            {
                return;
            }

            _backColor = BackColor;
            BackColor = HoverColor;
            pbIcon.Image = Resources.StarBw;
        }

        private void SetTextColor()
        {
            _NO_TRANSLATE_Path.LinkColor = Favourite ? FavouriteColor : ForeColor;
            _NO_TRANSLATE_Path.Font = new Font(_NO_TRANSLATE_Path.Font, Favourite ? FontStyle.Bold : FontStyle.Regular);
            _NO_TRANSLATE_Path.Invalidate();
        }

        private bool ShouldSerializeBranchNameColor()
        {
            return BranchNameColor != DefaultBranchNameColor;
        }

        private bool ShouldSerializeCategoryColor()
        {
            return CategoryColor != DefaultCategoryColor;
        }

        private bool ShouldSerializeFavouriteColor()
        {
            return FavouriteColor != DefaultFavouriteColor;
        }

        private bool ShouldSerializeHoverColor()
        {
            return HoverColor != DefaultHoverColor;
        }


        private void ContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            _isContextMenuOpened = false;
            if ((sender as ContextMenuStrip)?.SourceControl != this)
            {
                return;
            }
            ResetHighlight();
        }

        private void ContextMenuStrip_Opened(object sender, EventArgs e)
        {
            if ((sender as ContextMenuStrip)?.SourceControl != this)
            {
                return;
            }
            _isContextMenuOpened = true;
            SetHighlight();
        }

        private void RecentRepositoryItem_MouseEnter(object sender, EventArgs e)
        {
            SetHighlight();
        }

        private void PbIcon_Click(object sender, EventArgs e)
        {
            if (_repository == null)
            {
                return;
            }
            Favourite = !Favourite;
        }
    }
}
