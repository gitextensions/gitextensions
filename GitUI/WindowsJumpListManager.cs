using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitCommands.Utils;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI
{
    /// <summary>
    /// Provides access to Windows taskbar jumplists features.
    /// </summary>
    /// <seealso href="https://www.sevenforums.com/news/44368-developing-windows-7-taskbar-thumbnail-toolbars.html" />
    /// <seealso href="https://github.com/jlnewton87/Programming/blob/master/C%23/Windows%20API%20Code%20Pack%201.1/source/WindowsAPICodePack/Shell/Taskbar/JumpList.cs" />
    /// <inheritdoc />
    public sealed class WindowsJumpListManager : IDisposable
    {
        private ThumbnailToolBarButton _commitButton;
        private ThumbnailToolBarButton _pushButton;
        private ThumbnailToolBarButton _pullButton;
        private bool _toolbarButtonsCreated;
        private readonly IRepositoryDescriptionProvider _repositoryDescriptionProvider;

        public WindowsJumpListManager(IRepositoryDescriptionProvider repositoryDescriptionProvider)
        {
            _repositoryDescriptionProvider = repositoryDescriptionProvider;
        }

        ~WindowsJumpListManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _commitButton?.Dispose();
                _pushButton?.Dispose();
                _pullButton?.Dispose();
            }
        }

        private static bool IsSupported => EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported;

        /// <summary>
        /// Adds the given working directory to the list of Recent for future quick access.
        /// </summary>
        [ContractAnnotation("workingDir:null=>halt")]
        public void AddToRecent([NotNull] string workingDir)
        {
            if (!IsSupported)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(workingDir))
            {
                throw new ArgumentException(nameof(workingDir));
            }

            try
            {
                string repositoryDescription = _repositoryDescriptionProvider.Get(workingDir);
                if (string.IsNullOrWhiteSpace(repositoryDescription))
                {
                    return;
                }

                string baseFolder = Path.Combine(AppSettings.ApplicationDataPath.Value, "Recent");
                if (!Directory.Exists(baseFolder))
                {
                    Directory.CreateDirectory(baseFolder);
                }

                // sanitise
                var sb = new StringBuilder(repositoryDescription);
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    sb.Replace(c, '_');
                }

                string path = Path.Combine(baseFolder, $"{sb}.gitext");
                File.WriteAllText(path, workingDir);
                JumpList.AddToRecent(path);
            }
            catch (COMException ex)
            {
                // reported in https://github.com/gitextensions/gitextensions/issues/2269
                Trace.WriteLine(ex.Message, "UpdateJumplist");
            }
            catch (IOException ex)
            {
                // reported in https://github.com/gitextensions/gitextensions/issues/4549
                // looks like a regression in Windows 10.0.16299 (1709)
                Trace.WriteLine(ex.Message, "UpdateJumplist");
            }
        }

        /// <summary>
        /// Creates a JumpList for the given application instance.
        /// It also adds thumbnail toolbars, which are a set of up to seven buttons at the bottom of the taskbar’s icon thumbnail preview.
        /// </summary>
        /// <param name="windowHandle">The application instance's main window handle.</param>
        /// <param name="buttons">The thumbnail toolbar buttons to be added.</param>
        public void CreateJumpList(IntPtr windowHandle, WindowsThumbnailToolbarButtons buttons)
        {
            if (!IsSupported)
            {
                return;
            }

            CreateJumpList();

            CreateTaskbarButtons(windowHandle, buttons);

            return;

            void CreateJumpList()
            {
                try
                {
                    var jumpList = JumpList.CreateJumpListForIndividualWindow(TaskbarManager.Instance.ApplicationId, windowHandle);
                    jumpList.ClearAllUserTasks();
                    jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Recent;
                    jumpList.Refresh();
                }
                catch
                {
                    // have seen a COM exception here that caused the UI to stop loading
                }
            }

            void CreateTaskbarButtons(IntPtr handle, WindowsThumbnailToolbarButtons thumbButtons)
            {
                if (!_toolbarButtonsCreated)
                {
                    _commitButton = new ThumbnailToolBarButton(MakeIcon(thumbButtons.Commit.Image, 48, true), thumbButtons.Commit.Text);
                    _commitButton.Click += thumbButtons.Commit.Click;

                    _pushButton = new ThumbnailToolBarButton(MakeIcon(thumbButtons.Push.Image, 48, true), thumbButtons.Push.Text);
                    _pushButton.Click += thumbButtons.Push.Click;

                    _pullButton = new ThumbnailToolBarButton(MakeIcon(thumbButtons.Pull.Image, 48, true), thumbButtons.Pull.Text);
                    _pullButton.Click += thumbButtons.Pull.Click;

                    _toolbarButtonsCreated = true;

                    // Call this method using reflection.  This is a workaround to *not* reference WPF libraries, becuase of how the WindowsAPICodePack was implimented.
                    TaskbarManager.Instance.ThumbnailToolBars.AddButtons(handle, _commitButton, _pullButton, _pushButton);
                    TaskbarManager.Instance.ApplicationId = "GitExtensions";
                }

                _commitButton.Enabled = true;
                _pushButton.Enabled = true;
                _pullButton.Enabled = true;
            }
        }

        /// <summary>
        /// Disables display of thumbnail toolbars.
        /// </summary>
        public void DisableThumbnailToolbar()
        {
            if (!IsSupported || !_toolbarButtonsCreated)
            {
                return;
            }

            _commitButton.Enabled = false;
            _pushButton.Enabled = false;
            _pullButton.Enabled = false;
        }

        /// <summary>
        /// Converts an image into an icon.  This was taken off of the interwebs.
        /// It's on a billion different sites and forum posts, so I would say its creative commons by now. -tekmaven
        /// </summary>
        /// <param name="img">The image that shall become an icon</param>
        /// <param name="size">The width and height of the icon. Standard
        /// sizes are 16x16, 32x32, 48x48, 64x64.</param>
        /// <param name="keepAspectRatio">Whether the image should be squashed into a
        /// square or whether whitespace should be put around it.</param>
        /// <returns>An icon!!</returns>
        private static Icon MakeIcon(Image img, int size, bool keepAspectRatio)
        {
            var square = new Bitmap(size, size); // create new bitmap
            Graphics g = Graphics.FromImage(square); // allow drawing to it

            int x, y, w, h; // dimensions for new image

            if (!keepAspectRatio || img.Height == img.Width)
            {
                // just fill the square
                x = y = 0; // set x and y to 0
                w = h = size; // set width and height to size
            }
            else
            {
                // work out the aspect ratio
                float r = img.Width / (float)img.Height;

                // set dimensions accordingly to fit inside size^2 square
                if (r > 1)
                {
                    // w is bigger, so divide h by r
                    w = size;
                    h = (int)(size / r);
                    x = 0;
                    y = (size - h) / 2; // center the image
                }
                else
                {
                    // h is bigger, so multiply w by r
                    w = (int)(size * r);
                    h = size;
                    y = 0;
                    x = (size - w) / 2; // center the image
                }
            }

            // make the image shrink nicely by using HighQualityBicubic mode
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, x, y, w, h); // draw image with specified dimensions
            g.Flush(); // make sure all drawing operations complete before we get the icon

            // following line would work directly on any image, but then
            // it wouldn't look as nice.
            return Icon.FromHandle(square.GetHicon());
        }
    }
}