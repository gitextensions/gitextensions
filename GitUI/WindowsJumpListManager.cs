using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitCommands.Utils;
using Microsoft;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI
{
    public interface IWindowsJumpListManager : IDisposable
    {
        bool NeedsJumpListCreation { get; }

        void AddToRecent(string workingDir);
        void CreateJumpList(IntPtr windowHandle, WindowsThumbnailToolbarButtons buttons);
        void DisableThumbnailToolbar();
        void UpdateCommitIcon(Image image);
    }

    /// <summary>
    /// Provides access to Windows taskbar jumplists features.
    /// </summary>
    /// <seealso href="https://www.sevenforums.com/news/44368-developing-windows-7-taskbar-thumbnail-toolbars.html" />
    /// <seealso href="https://github.com/jlnewton87/Programming/blob/master/C%23/Windows%20API%20Code%20Pack%201.1/source/WindowsAPICodePack/Shell/Taskbar/JumpList.cs" />
    /// <inheritdoc />
    [Export(typeof(IWindowsJumpListManager))]
    public sealed class WindowsJumpListManager : IWindowsJumpListManager
    {
        private static readonly Dictionary<Image, Icon> _iconByImage = new();
        private readonly IRepositoryDescriptionProvider _repositoryDescriptionProvider;
        private ThumbnailToolBarButton? _commitButton;
        private ThumbnailToolBarButton? _pushButton;
        private ThumbnailToolBarButton? _pullButton;
        private string? _deferredAddToRecent;
        private bool ToolbarButtonsCreated => _commitButton is not null;

        static WindowsJumpListManager()
        {
            if (TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.ApplicationId = AppSettings.ApplicationId;
            }
        }

        [ImportingConstructor]
        public WindowsJumpListManager(IRepositoryDescriptionProvider repositoryDescriptionProvider)
        {
            _repositoryDescriptionProvider = repositoryDescriptionProvider;
        }

        // MEF will dispose instantiated parts when the container is disposed. There is no need to include a finalizer here.
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
        private static bool IsSupportedAndVisible => EnvUtils.RunningOnWindowsWithMainWindow() && TaskbarManager.IsPlatformSupported;

        /// <summary>
        /// Adds the given working directory to the list of Recent for future quick access.
        /// </summary>
        public void AddToRecent(string workingDir)
        {
            if (!IsSupported)
            {
                return;
            }

            if (!ToolbarButtonsCreated)
            {
                _deferredAddToRecent = workingDir;
                return;
            }

            if (string.IsNullOrWhiteSpace(workingDir))
            {
                throw new ArgumentException(nameof(workingDir));
            }

            SafeInvoke(() =>
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
                StringBuilder sb = new(repositoryDescription);
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    sb.Replace(c, '_');
                }

                string path = Path.Combine(baseFolder, $"{sb}.gitext");
                File.WriteAllText(path, workingDir);
                JumpList.AddToRecent(path);

                if (!ToolbarButtonsCreated)
                {
                    return;
                }

                Validates.NotNull(_commitButton);
                Validates.NotNull(_pushButton);
                Validates.NotNull(_pullButton);

                _commitButton.Enabled = true;
                _pushButton.Enabled = true;
                _pullButton.Enabled = true;
            }, nameof(AddToRecent));
        }

        public void UpdateCommitIcon(Image image)
        {
            SafeInvoke(() =>
            {
                if (ToolbarButtonsCreated && IsSupportedAndVisible)
                {
                    Validates.NotNull(_commitButton);
                    _commitButton.Icon = MakeIcon(image, 48, true);
                }
            }, nameof(UpdateCommitIcon));
        }

        /// <summary>
        /// Indicates if the JumpList creation is still needed.
        /// </summary>
        public bool NeedsJumpListCreation => IsSupported && !ToolbarButtonsCreated;

        /// <summary>
        /// Creates a JumpList for the given application instance.
        /// It also adds thumbnail toolbars, which are a set of up to seven buttons at the bottom of the taskbar’s icon thumbnail preview.
        /// </summary>
        /// <param name="windowHandle">The application instance's main window handle.</param>
        /// <param name="buttons">The thumbnail toolbar buttons to be added.</param>
        public void CreateJumpList(IntPtr windowHandle, WindowsThumbnailToolbarButtons buttons)
        {
            if (ToolbarButtonsCreated || !IsSupported || windowHandle == IntPtr.Zero)
            {
                return;
            }

            SafeInvoke(() =>
            {
                // One ApplicationId, so all windows must share the same jumplist
                var jumpList = JumpList.CreateJumpList();
                jumpList.ClearAllUserTasks();
                jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Recent;
                jumpList.Refresh();

                CreateTaskbarButtons(windowHandle, buttons);
            }, nameof(CreateJumpList));

            if (ToolbarButtonsCreated && _deferredAddToRecent is not null)
            {
                var recentRepoAddToRecent = _deferredAddToRecent;
                _deferredAddToRecent = null;
                AddToRecent(recentRepoAddToRecent);
            }

            return;

            void CreateTaskbarButtons(IntPtr handle, WindowsThumbnailToolbarButtons thumbButtons)
            {
                _commitButton = new ThumbnailToolBarButton(MakeIcon(thumbButtons.Commit.Image, 48, true), thumbButtons.Commit.Text);
                _commitButton.Click += thumbButtons.Commit.Click;

                _pushButton = new ThumbnailToolBarButton(MakeIcon(thumbButtons.Push.Image, 48, true), thumbButtons.Push.Text);
                _pushButton.Click += thumbButtons.Push.Click;

                _pullButton = new ThumbnailToolBarButton(MakeIcon(thumbButtons.Pull.Image, 48, true), thumbButtons.Pull.Text);
                _pullButton.Click += thumbButtons.Pull.Click;

                // Call this method using reflection.  This is a workaround to *not* reference WPF libraries, because of how the WindowsAPICodePack was implemented.
                TaskbarManager.Instance.ThumbnailToolBars.AddButtons(handle, _commitButton, _pullButton, _pushButton);
            }
        }

        /// <summary>
        /// Disables display of thumbnail toolbars.
        /// </summary>
        public void DisableThumbnailToolbar()
        {
            if (!ToolbarButtonsCreated)
            {
                return;
            }

            SafeInvoke(() =>
            {
                Validates.NotNull(_commitButton);
                Validates.NotNull(_pushButton);
                Validates.NotNull(_pullButton);
                _commitButton.Enabled = false;
                _pushButton.Enabled = false;
                _pullButton.Enabled = false;
            }, nameof(DisableThumbnailToolbar));
        }

        /// <summary>
        /// Converts an image into an icon.  This was taken off of the interwebs.
        /// It's on a billion different sites and forum posts, so I would say its creative commons by now. -tekmaven.
        /// </summary>
        /// <param name="img">The image that shall become an icon.</param>
        /// <param name="size">The width and height of the icon. Standard
        /// sizes are 16x16, 32x32, 48x48, 64x64.</param>
        /// <param name="keepAspectRatio">Whether the image should be squashed into a
        /// square or whether whitespace should be put around it.</param>
        /// <returns>An icon!!.</returns>
        private static Icon MakeIcon(Image img, int size, bool keepAspectRatio)
        {
            if (_iconByImage.TryGetValue(img, out Icon icon))
            {
                return icon;
            }

            using Bitmap square = new(size, size); // create new bitmap
            using Graphics g = Graphics.FromImage(square); // allow drawing to it

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
            icon = square.ToIcon();
            _iconByImage.Add(img, icon);
            return icon;
        }

        private static void SafeInvoke(Action action, string callerName)
        {
            try
            {
                action();
            }
            catch (Exception ex)
                when (

                    // reported in https://github.com/gitextensions/gitextensions/issues/6760
                    // reported in https://github.com/gitextensions/gitextensions/issues/8234
                    ex is Microsoft.WindowsAPICodePack.Shell.ShellException ||

                    // reported in https://github.com/gitextensions/gitextensions/issues/2269
                    ex is COMException ||

                    // reported in https://github.com/gitextensions/gitextensions/issues/6767
                    ex is UnauthorizedAccessException ||

                    // reported in https://github.com/gitextensions/gitextensions/issues/4549
                    // looks like a regression in Windows 10.0.16299 (1709)
                    ex is IOException ||

                    // observed during integration tests: A valid active Window is needed to update the Taskbar.
                    ex is InvalidOperationException)
            {
                Trace.WriteLine(ex.Message, callerName);
            }
        }
    }
}
