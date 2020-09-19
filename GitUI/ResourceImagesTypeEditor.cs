#nullable enable

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using GitUI.Shells;

namespace GitUI
{
    internal sealed class ResourceImagesTypeEditor : UITypeEditor
    {
        private const int LongestItemWidth = -1;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            var imageKey = e.Value?.ToString();

            if (imageKey != null)
            {
                var rectangle = e.Bounds;

                rectangle.Inflate(-2, -1);

                e.Graphics.DrawImage(ResourceImagesProvider.Images[imageKey], rectangle);
            }
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider
                .GetService(typeof(IWindowsFormsEditorService));

            var listView = new ListView
            {
                Scrollable = true,
                View = View.Details,
                MultiSelect = false,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.None
            };

            var header = new ColumnHeader
            {
                Text = string.Empty,
                Name = "column",
                Width = LongestItemWidth
            };

            listView.Columns.Add(header);

            var imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit
            };

            listView.SmallImageList = imageList;
            listView.LargeImageList = imageList;

            foreach (var image in ResourceImagesProvider.Images)
            {
                imageList.Images.Add(image.Key, image.Value);
                listView.Items.Add(image.Key, image.Key);
            }

            listView.SelectedIndexChanged += (sender, e) =>
            {
                editorService.CloseDropDown();
            };

            editorService.DropDownControl(listView);

            if (listView.SelectedItems.Count > 0)
            {
                return listView.SelectedItems[0].Text;
            }

            return value;
        }
    }
}
