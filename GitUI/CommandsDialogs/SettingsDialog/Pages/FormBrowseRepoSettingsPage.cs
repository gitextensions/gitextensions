#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Shells;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormBrowseRepoSettingsPage : SettingsPageWithHeader
    {
        private readonly IShellService _shellService = new ShellService(new ShellRepository());

        private List<ShellProxy> _shellProxies = new List<ShellProxy>();

        public FormBrowseRepoSettingsPage()
        {
            InitializeComponent();

            Text = "Browse repository window";

            InitializeComplete();
        }

        protected override void SettingsToPage()
        {
            base.SettingsToPage();

            chkChowConsoleTab.Checked = AppSettings.ShowConEmuTab.Value;
            chkShowGpgInformation.Checked = AppSettings.ShowGpgInformation.Value;

            _shellProxies = _shellService.AllShells()
                .Select(ShellProxy.Create)
                .ToList();

            AddImagesToShellsListView(_shellsListView);
            BindProxies(_shellsListView, _shellPropertyGrid, _shellProxies);
        }

        protected override void PageToSettings()
        {
            base.PageToSettings();

            AppSettings.ShowConEmuTab.Value = chkChowConsoleTab.Checked;
            AppSettings.ShowGpgInformation.Value = chkShowGpgInformation.Checked;

            _shellService.Update(_shellProxies);
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(FormBrowseRepoSettingsPage));
        }

        private static void AddImagesToShellsListView(ListView listView)
        {
            var imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit
            };

            listView.SmallImageList = imageList;
            listView.LargeImageList = imageList;

            foreach (var image in ResourceImagesProvider.Images)
            {
                imageList.Images.Add(image.Key, image.Value);
            }
        }

        private void BindProxies(ListView listView, PropertyGrid propertyGrid, IEnumerable<ShellProxy> shellProxies)
        {
            listView.BeginUpdate();

            foreach (var shellProxy in shellProxies)
            {
                var listViewItem = CreateListViewItem(shellProxy);

                listView.Items.Add(listViewItem);
            }

            listView.EndUpdate();

            listView.SelectedIndexChanged += (sender, e) =>
            {
                if (listView.SelectedItems.Count == 0)
                {
                    propertyGrid.SelectedObject = null;
                    removeButton.Enabled = false;
                    return;
                }

                removeButton.Enabled = true;

                var listViewItem = listView.SelectedItems[0];
                var shellProxy = (ShellProxy)listViewItem.Tag;

                propertyGrid.SelectedObject = shellProxy;
            };

            listView.ItemChecked += (sender, e) =>
            {
                var checkable = (ICheckable)e.Item.Tag;

                checkable.Checked = e.Item.Checked;

                if (propertyGrid.SelectedObject == e.Item.Tag)
                {
                    propertyGrid.Refresh();
                }
            };
        }

        private ListViewItem CreateListViewItem(ShellProxy shellProxy)
        {
            var listViewItem = new ListViewItem();

            UpdateListViewItem(listViewItem, shellProxy);

            shellProxy.PropertyChanged += (sender, e) =>
            {
                UpdateListViewItem(listViewItem, shellProxy);

                if (e.PropertyName == nameof(ShellProxy.Default) && shellProxy.Default)
                {
                    _shellProxies.Where(x => x != shellProxy)
                        .ForEach(x => x.Default = false);
                }
            };

            return listViewItem;
        }

        private void UpdateListViewItem(ListViewItem listViewItem, ShellProxy shellProxy)
        {
            listViewItem.Tag = shellProxy;
            listViewItem.ToolTipText = $"{shellProxy.Command} {shellProxy.Arguments}";
            listViewItem.ImageKey = shellProxy.Icon;
            listViewItem.Checked = shellProxy.Default;

            foreach (var column in _shellsListView.Columns.OfType<ColumnHeader>())
            {
                var value = TypeDescriptor.GetProperties(shellProxy)[(string)column.Tag]?
                    .GetValue(shellProxy)?
                    .ToString() ?? string.Empty;

                var listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, value)
                {
                    ForeColor = shellProxy.Enabled
                        ? SystemColors.WindowText
                        : SystemColors.GrayText
                };

                listViewItem.SubItems.Insert(column.Index, listViewSubItem);
            }
        }

        private void AddShell()
        {
            var shellProxy = new ShellProxy();
            var listViewItem = CreateListViewItem(shellProxy);

            _shellProxies.Add(shellProxy);
            _shellsListView.Items.Add(listViewItem);
        }

        private void RemoveShell()
        {
            if (_shellsListView.SelectedItems.Count == 0)
            {
                return;
            }

            var listViewItem = _shellsListView.SelectedItems[0];
            var shellProxy = (ShellProxy)listViewItem.Tag;

            _shellsListView.Items.Remove(listViewItem);
            _shellProxies.Remove(shellProxy);
        }

        #region Events

        private void addButton_Click(object sender, EventArgs e)
        {
            AddShell();
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            RemoveShell();
        }

        #endregion Events

        private interface ICheckable
        {
            bool Checked { get; set; }
        }
    }
}
