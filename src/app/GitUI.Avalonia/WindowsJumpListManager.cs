using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI.Compat;
using Microsoft;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

public interface IWindowsJumpListManager : IDisposable
{
    bool NeedsJumpListCreation { get; }

    void AddToRecent(string workingDir);
    void CreateJumpList(IntPtr windowHandle, WindowsThumbnailToolbarButtons buttons);
    void EnableThumbnailToolbar(bool enable);
    void UpdateCommitIcon(IImage image);
}

/// <summary>
/// Provides access to Windows taskbar jumplists features.
/// </summary>
/// <seealso href="https://www.sevenforums.com/news/44368-developing-windows-7-taskbar-thumbnail-toolbars.html" />
/// <seealso href="https://github.com/jlnewton87/Programming/blob/master/C%23/Windows%20API%20Code%20Pack%201.1/source/WindowsAPICodePack/Shell/Taskbar/JumpList.cs" />
/// <inheritdoc />
public sealed class WindowsJumpListManager : IWindowsJumpListManager
{
    private static readonly Dictionary<IImage, TaskbarIcon> _iconByImage = [];
    private readonly IRepositoryDescriptionProvider _repositoryDescriptionProvider;
    private ThumbnailToolBarButton? _closeAllButton;
    private ThumbnailToolBarButton? _commitButton;
    private ThumbnailToolBarButton? _pushButton;
    private ThumbnailToolBarButton? _pullButton;
    private string? _deferredAddToRecent;
    private bool ToolbarButtonsCreated => _commitButton is not null;

    static WindowsJumpListManager()
    {
        if (TaskbarManager.IsPlatformSupported)
        {
            // Coexistence constraint: the WinForms application ships side by side and keeps the original taskbar identity.
            string id = $"{AppSettings.ApplicationId}.Avalonia";
            TaskbarManager.Instance.ApplicationId = AppSettings.IsPortable()
                ? $"{id}.{Convert.ToBase64String(SHA1.HashData(Encoding.UTF8.GetBytes(WinFormsShims.Application.ExecutablePath)))}"
                : id;
        }
    }

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
            _closeAllButton?.Dispose();
            _commitButton?.Dispose();
            _pushButton?.Dispose();
            _pullButton?.Dispose();
        }
    }

    // Cross-platform constraint: the taskbar shell also carries the macOS Dock, so it decides support and the main-window check.
    private static bool IsSupported => TaskbarManager.IsPlatformSupported;
    private static bool IsSupportedAndVisible => TaskbarManager.RunningWithMainWindow && TaskbarManager.IsPlatformSupported;

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

        ArgumentException.ThrowIfNullOrWhiteSpace(workingDir);

        SafeInvoke(() =>
        {
            string repositoryDescription = _repositoryDescriptionProvider.GetDescriptiveUnique(workingDir);
            if (string.IsNullOrWhiteSpace(repositoryDescription))
            {
                return;
            }

            string baseFolder = Path.Join(AppSettings.ApplicationDataPath.Value!, "Recent");
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }

            // sanitise
            StringBuilder sb = new(repositoryDescription);
            sb.Replace(@"\\wsl$\", "").Replace(@":\", "_");
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                sb.Replace(c, '_');
            }

            string path = Path.Join(baseFolder, $"{sb}.gitext");
            File.WriteAllText(path, workingDir);
            JumpList.AddToRecent(path);
            UpdateJumpList(); // in order to refresh at once

            if (!ToolbarButtonsCreated)
            {
                return;
            }

            Validates.NotNull(_closeAllButton);
            Validates.NotNull(_commitButton);
            Validates.NotNull(_pushButton);
            Validates.NotNull(_pullButton);

            _closeAllButton.Enabled = true;
            _commitButton.Enabled = true;
            _pushButton.Enabled = true;
            _pullButton.Enabled = true;
        }, nameof(AddToRecent));
    }

    public void UpdateCommitIcon(IImage image)
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
            UpdateJumpList();

            CreateTaskbarButtons(windowHandle, buttons);
        }, nameof(CreateJumpList));

        if (ToolbarButtonsCreated && _deferredAddToRecent is not null)
        {
            string recentRepoAddToRecent = _deferredAddToRecent;
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

            _closeAllButton = new ThumbnailToolBarButton(MakeIcon(thumbButtons.CloseAll.Image, 48, true), thumbButtons.CloseAll.Text);
            _closeAllButton.Click += thumbButtons.CloseAll.Click;

            // Call this method using reflection.  This is a workaround to *not* reference WPF libraries, because of how the WindowsAPICodePack was implemented.
            TaskbarManager.Instance.ThumbnailToolBars.AddButtons(handle, _commitButton, _pullButton, _pushButton, _closeAllButton);
        }
    }

    /// <summary>
    ///  Sets the enabled state of thumbnail toolbar buttons.
    /// </summary>
    public void EnableThumbnailToolbar(bool enable)
    {
        if (!ToolbarButtonsCreated)
        {
            return;
        }

        SafeInvoke(() =>
        {
            Validates.NotNull(_closeAllButton);
            Validates.NotNull(_commitButton);
            Validates.NotNull(_pushButton);
            Validates.NotNull(_pullButton);

            // _closeAllButton is applicable always

            _commitButton.Enabled = enable;
            _pushButton.Enabled = enable;
            _pullButton.Enabled = enable;
        }, nameof(EnableThumbnailToolbar));
    }

    /// <summary>
    ///  Updates the jump list to show recent repositories in the Start menu and at the taskbar icon.
    ///  Uses the built-in Windows localized "Recent" category.
    /// </summary>
    private static void UpdateJumpList()
    {
        JumpList jumpList = JumpList.CreateJumpList();
        jumpList.ClearAllUserTasks();
        jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Recent;
        jumpList.Refresh();
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
    private static TaskbarIcon MakeIcon(IImage img, int size, bool keepAspectRatio)
    {
        if (_iconByImage.TryGetValue(img, out TaskbarIcon? icon))
        {
            return icon;
        }

        // Framework constraint: Avalonia draws into a render-target bitmap through a drawing context instead of GDI+ Graphics.
        using RenderTargetBitmap square = new(new PixelSize(size, size)); // create new bitmap
        DrawingContext g = square.CreateDrawingContext(); // allow drawing to it

        int x, y, w, h; // dimensions for new image

        if (!keepAspectRatio || img.Size.Height == img.Size.Width)
        {
            // just fill the square
            x = y = 0; // set x and y to 0
            w = h = size; // set width and height to size
        }
        else
        {
            // work out the aspect ratio
            float r = (float)img.Size.Width / (float)img.Size.Height;

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
        using (g.PushRenderOptions(new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.HighQuality }))
        {
            g.DrawImage(img, new Rect(x, y, w, h)); // draw image with specified dimensions
        }

        g.Dispose(); // make sure all drawing operations complete before we get the icon

        // following line would work directly on any image, but then
        // it wouldn't look as nice.
        icon = TaskbarIcon.FromBitmap(square);
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
                ex is ShellException ||

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
