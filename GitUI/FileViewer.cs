using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public partial class FileViewer : UserControl
    {
        public FileViewer()
        {
            InitializeComponent();
            TextEditor.ActiveTextAreaControl.TextArea.KeyUp += new KeyEventHandler(TextArea_KeyUp);

        }

        void TextArea_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
                Find();
        }

        private void ClearImage()
        {
            PictureBox.ImageLocation = "";

            if (PictureBox.Image is Image)
            {
                PictureBox.Image.Dispose();
                PictureBox.Image = null;
            }
        }

        FindAndReplaceForm findAndReplaceForm = new FindAndReplaceForm();

        public void Find()
        {
            findAndReplaceForm.ShowFor(TextEditor, false);
        }

        public void ViewFile(string fileName)
        {
            if (fileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".tif", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".tiff", StringComparison.CurrentCultureIgnoreCase)
                )
            {
                Image image;

                if (fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase))
                {
                    Icon icon = new Icon(GitCommands.Settings.WorkingDir + fileName);
                    image = icon.ToBitmap();
                    icon.Dispose();
                }
                else
                {
                    Image fileImage = Image.FromFile(GitCommands.Settings.WorkingDir + fileName);
                    image = (Image)fileImage.Clone();
                    fileImage.Dispose();
                }

                PictureBox.Visible = true;
                TextEditor.Visible = false;
                
                if (image.Size.Height > PictureBox.Size.Height ||
                    image.Size.Width > PictureBox.Size.Width)
                {
                    PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                } else
                {
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                }
                PictureBox.Image = image;
                

            }
            else
            if (fileName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".mpg", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".mpeg", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(".avi", StringComparison.CurrentCultureIgnoreCase))
            {
                ClearImage();
                PictureBox.Visible = false;
                TextEditor.Visible = true;
                TextEditor.Text = "Binary file: " + fileName;
                TextEditor.Refresh();
            } else
            {
                ClearImage();
                PictureBox.Visible = false;
                TextEditor.Visible = true;

                if (File.Exists(GitCommands.Settings.WorkingDir + fileName))
                    TextEditor.LoadFile(GitCommands.Settings.WorkingDir + fileName);
            }
        }


        public void ViewCurrentChanges(string fileName, string format, bool staged)
        {
            ClearImage();
            PictureBox.Visible = false;
            TextEditor.Visible = true;

            TextEditor.SetHighlighting("Patch");

            TextEditor.Text = GitCommands.GitCommands.GetCurrentChanges(fileName, staged);
            TextEditor.Refresh();
        }

        public void ViewPatch(string text)
        {
            ClearImage();
            PictureBox.Visible = false;
            TextEditor.Visible = true;

            TextEditor.SetHighlighting("Patch");

            TextEditor.Text = text;
            TextEditor.Refresh();
        }

        public void ViewText(string fileName, string text)
        {
            ClearImage();
            PictureBox.Visible = false;
            TextEditor.Visible = true;

            EditorOptions.SetSyntax(TextEditor, fileName);

            TextEditor.Text = text;
            TextEditor.Refresh();
        }

        public bool IsBinaryFile(string fileName)
        {
            return (fileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tif", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tiff", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".mpg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".mpeg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".avi", StringComparison.CurrentCultureIgnoreCase));
        }

        public void ViewGitItem(string fileName, string guid)
        {
            ClearImage();
            PictureBox.Visible = false;
            TextEditor.Visible = true;

            try
            {

                if (fileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tif", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tiff", StringComparison.CurrentCultureIgnoreCase))
                {
                    Stream stream = GitCommands.GitCommands.GetFileStream(guid);
                    Image images;

                    if (fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase))
                        images = new Icon(stream).ToBitmap();
                    else
                        images = Image.FromStream(stream);

                    stream.Close();

                    PictureBox.Visible = true;
                    TextEditor.Visible = false;

                    if (images.Size.Height > PictureBox.Size.Height ||
                        images.Size.Width > PictureBox.Size.Width)
                    {
                        PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                    PictureBox.Image = images;
                }
                else
                    if (fileName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(".mpg", StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(".mpeg", StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(".avi", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ClearImage();
                        TextEditor.Text = "Binary file: " + fileName;
                    }
                    else
                    {
                        ClearImage();
                        EditorOptions.SetSyntax(TextEditor, fileName);

                        TextEditor.Text = GitCommands.GitCommands.GetFileText(guid);
                    }
            }
            catch
            {
                TextEditor.Text = "Unsupported file";
            }
            TextEditor.Refresh();
        }

        private void TextEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void TextEditor_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Find();
        }

        private void TextEditor_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
