param(
    [Parameter(Mandatory = $true)]
    [string]$TitlePattern,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [int]$TimeoutSeconds = 45,

    [ValidateSet("PrintWindow", "Screen")]
    [string]$CaptureMethod = "PrintWindow"
)

$ErrorActionPreference = "Stop"

# parity-scaffolding: captures WSLg runtime windows as evidence until the parity gate closes.
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public static class GitExtensionsSmokeWindows
{
    public delegate bool EnumWindowsProc(IntPtr window, IntPtr parameter);

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr window, out WindowRectangle rectangle);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr window, StringBuilder text, int maximum);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr window);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr window);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr window, int command);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr window,
        IntPtr insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [DllImport("user32.dll", EntryPoint = "PrintWindow")]
    private static extern bool PrintWindowNative(IntPtr window, IntPtr deviceContext, uint flags);

    public static IntPtr FindVisibleWindow(string titlePattern, out string title)
    {
        IntPtr match = IntPtr.Zero;
        string matchedTitle = string.Empty;
        EnumWindows((window, parameter) =>
        {
            if (!IsWindowVisible(window))
            {
                return true;
            }

            StringBuilder text = new StringBuilder(1024);
            GetWindowText(window, text, text.Capacity);
            string candidate = text.ToString();
            if (candidate.IndexOf(titlePattern, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return true;
            }

            match = window;
            matchedTitle = candidate;
            return false;
        }, IntPtr.Zero);

        title = matchedTitle;
        return match;
    }

    public static WindowRectangle GetRectangle(IntPtr window)
    {
        WindowRectangle rectangle;
        if (!GetWindowRect(window, out rectangle))
        {
            throw new InvalidOperationException("Unable to read the smoke window bounds.");
        }

        return rectangle;
    }

    public static void Activate(IntPtr window)
    {
        const int ShowRestore = 9;
        const uint NoMoveNoSizeShow = 0x43;
        IntPtr topmost = new IntPtr(-1);
        ShowWindow(window, ShowRestore);
        SetWindowPos(window, topmost, 0, 0, 0, 0, NoMoveNoSizeShow);
        SetForegroundWindow(window);
    }

    public static IntPtr GetForeground()
    {
        return GetForegroundWindow();
    }

    public static void Minimize(IntPtr window)
    {
        const int ShowMinimized = 6;
        ShowWindow(window, ShowMinimized);
    }

    public static void Restore(IntPtr window)
    {
        const int ShowRestore = 9;
        const uint NoMoveNoSizeShow = 0x43;
        IntPtr notTopmost = new IntPtr(-2);
        ShowWindow(window, ShowRestore);
        SetWindowPos(window, notTopmost, 0, 0, 0, 0, NoMoveNoSizeShow);
        SetForegroundWindow(window);
    }

    public static bool PrintFullWindow(IntPtr window, IntPtr deviceContext)
    {
        const uint RenderFullContent = 2;
        return PrintWindowNative(window, deviceContext, RenderFullContent);
    }

    public static string[] GetVisibleTitles()
    {
        List<string> titles = new List<string>();
        EnumWindows((window, parameter) =>
        {
            if (IsWindowVisible(window))
            {
                StringBuilder text = new StringBuilder(1024);
                GetWindowText(window, text, text.Capacity);
                if (text.Length > 0)
                {
                    titles.Add(text.ToString());
                }
            }

            return true;
        }, IntPtr.Zero);

        return titles.ToArray();
    }
}
"@

$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
$window = [IntPtr]::Zero
$title = ""
while ($window -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $deadline)
{
    $window = [GitExtensionsSmokeWindows]::FindVisibleWindow($TitlePattern, [ref]$title)
    if ($window -eq [IntPtr]::Zero)
    {
        Start-Sleep -Milliseconds 500
    }
}

if ($window -eq [IntPtr]::Zero)
{
    $visibleTitles = [GitExtensionsSmokeWindows]::GetVisibleTitles() -join [Environment]::NewLine
    throw "No visible window matching '$TitlePattern' appeared. Visible titles:`n$visibleTitles"
}

$previousForeground = [GitExtensionsSmokeWindows]::GetForeground()
if ($CaptureMethod -eq "Screen" -and
    $previousForeground -ne [IntPtr]::Zero -and
    $previousForeground -ne $window)
{
    [GitExtensionsSmokeWindows]::Minimize($previousForeground)
}

[GitExtensionsSmokeWindows]::Activate($window)
Start-Sleep -Milliseconds 750

$rectangle = [GitExtensionsSmokeWindows]::GetRectangle($window)
$width = $rectangle.Right - $rectangle.Left
$height = $rectangle.Bottom - $rectangle.Top
if ($width -le 0 -or $height -le 0)
{
    throw "Window '$title' has invalid bounds ${width}x${height}."
}

$outputDirectory = [IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($OutputPath))
[IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
$bitmap = [Drawing.Bitmap]::new($width, $height)
try
{
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try
    {
        $printed = $false
        if ($CaptureMethod -eq "PrintWindow")
        {
            $deviceContext = $graphics.GetHdc()
            try
            {
                $printed = [GitExtensionsSmokeWindows]::PrintFullWindow($window, $deviceContext)
            }
            finally
            {
                $graphics.ReleaseHdc($deviceContext)
            }
        }

        if (-not $printed)
        {
            $graphics.CopyFromScreen(
                $rectangle.Left,
                $rectangle.Top,
                0,
                0,
                [Drawing.Size]::new($width, $height),
                [Drawing.CopyPixelOperation]::SourceCopy)
        }
    }
    finally
    {
        $graphics.Dispose()
    }

    $bitmap.Save([IO.Path]::GetFullPath($OutputPath), [Drawing.Imaging.ImageFormat]::Png)
}
finally
{
    $bitmap.Dispose()
    if ($previousForeground -ne [IntPtr]::Zero -and $previousForeground -ne $window)
    {
        [GitExtensionsSmokeWindows]::Restore($previousForeground)
    }
}

Write-Output "title=$title"
Write-Output "bounds=$($rectangle.Left),$($rectangle.Top),${width}x${height}"
Write-Output "captureMethod=$CaptureMethod"
