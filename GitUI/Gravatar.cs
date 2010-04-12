using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GitUI.Properties;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;

namespace GitUI
{
    public partial class Gravatar : UserControl
    {
        private readonly SynchronizationContext syncContext;

        public Gravatar()
        {
            syncContext = SynchronizationContext.Current;

            InitializeComponent();

            imgGravatar.Visible = false;
        }

        /// <summary>
        /// Email Property
        /// </summary>
        string theEmail;

        public string email
        {
            get { return theEmail; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    imgGravatar.Visible = true;
                theEmail = value;
                UpdateGravatar();
            }
        }


        /// <summary>
        /// Small MD5 Function
        /// </summary>
        /// <param name="theEmail"></param>
        /// <returns>Hash of the email address passed.</returns>
        public string MD5(string theEmail)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Obj =
                new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] bytesToHash = System.Text.Encoding.ASCII.GetBytes(theEmail);

            bytesToHash = md5Obj.ComputeHash(bytesToHash);

            string strResult = "";

            foreach (byte b in bytesToHash)
            {
                strResult += b.ToString("x2");
            }

            return strResult;
        }

        /// <summary>
        /// Update the Gravatar anytime an attribute is changed
        /// </summary>
        private void UpdateGravatar()
        {
            //resize our control (I'm not using AutoSize for a reason)
            this.Size = new System.Drawing.Size(GitCommands.Settings.AuthorImageSize, GitCommands.Settings.AuthorImageSize);
            imgGravatar.Size = new System.Drawing.Size(GitCommands.Settings.AuthorImageSize, GitCommands.Settings.AuthorImageSize);

            if (!GitCommands.Settings.ShowAuthorGravatar || string.IsNullOrEmpty(theEmail))
            {
                imgGravatar.Image = Resources.User;
                return;
            }

            ThreadPool.QueueUserWorkItem(delegate
            {
                string imageFileName = string.Concat(theEmail, ".jpg");

                IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

                //If the user image is not cached yet, download it from gravatar and store it in the isolatedStorage
                if (isolatedStorage.GetFileNames(imageFileName).Length == 0)
                {
                    syncContext.Post(delegate
                    {
                        imgGravatar.Image = Resources.User;
                    }, null
                    );

                    GetImageFromGravatar(imageFileName, isolatedStorage);
                }

                syncContext.Post(delegate
                {
                    if (isolatedStorage.GetFileNames(imageFileName).Length != 0)
                    {
                        try
                        {
                            using (IsolatedStorageFileStream iStream = new IsolatedStorageFileStream(imageFileName, FileMode.Open, isolatedStorage))
                            {

                                imgGravatar.Image = Image.FromStream(iStream);
                            }
                        }
                        catch (ArgumentException)
                        {
                            //The file is not a valid image, delete it
                            isolatedStorage.DeleteFile(imageFileName);

                            imgGravatar.Image = Resources.User;
                        }
                    }
                    else
                    {
                        imgGravatar.Image = Resources.User;
                    }

                }, null);
            });
        }

        private void GetImageFromGravatar(string imageFileName, IsolatedStorageFile isolatedStorage)
        {
            try
            {
                string BaseURL = string.Concat("http://www.gravatar.com/avatar/{0}?d=identicon&s=", GitCommands.Settings.AuthorImageSize, "&r=g");

                //hash the email address
                string emailHash = MD5(theEmail.ToLower());
                //format our url to the Gravatar
                string imageUrl = string.Format(BaseURL, emailHash);

                WebClient webClient = new WebClient();
                Stream imageStream = webClient.OpenRead(imageUrl);

                using (IsolatedStorageFileStream output = new IsolatedStorageFileStream(imageFileName, FileMode.Create, isolatedStorage))
                {
                    byte[] buffer = new byte[1024];
                    int read;

                    while ((read = imageStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                    }
                }
            }
            catch
            {
                //catch IO errors
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string imageFileName = string.Concat(theEmail, ".jpg");
            IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            isolatedStorage.DeleteFile(imageFileName);

            UpdateGravatar();
        }

        private void registerAtGravatarcomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = @"http://www.gravatar.com";

                proc.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void clearImagecacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearImageCache();

            UpdateGravatar();
        }

        public static void ClearImageCache()
        {
            IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            foreach (string gravatarFileName in isolatedStorage.GetFileNames("*.jpg"))
                isolatedStorage.DeleteFile(gravatarFileName);
        }
    }
}
