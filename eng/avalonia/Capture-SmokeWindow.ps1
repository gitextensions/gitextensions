param(
    [Parameter(Mandatory = $true)]
    [string]$TitlePattern,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [int]$TimeoutSeconds = 45,

    [int]$ProcessId = 0,

    [ValidateSet("PrintWindow", "Screen")]
    [string]$CaptureMethod = "PrintWindow",

    [switch]$OpenViewMenu,

    [switch]$CheckPlugins
)

$ErrorActionPreference = "Stop"

if ($CheckPlugins -and ($OpenViewMenu -or $ProcessId -le 0))
{
    throw "Plugin inspection requires an owned process and cannot be combined with the View menu."
}

# parity-scaffolding: records actual plugin exports exposed by the running application.
function Get-PluginMenuEvidence([IntPtr]$Handle, [int]$OwnedProcessId)
{
    $result = [ordered]@{
        schemaVersion = 1
        status = 'unverified'
        processId = $OwnedProcessId
        windowTitle = $title
        expectedExport = 'Find large files'
        automationPattern = $null
        pluginNames = @()
        reason = $null
        recordedUtc = [DateTime]::UtcNow.ToString('O')
    }
    try
    {
        Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
        $condition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::MenuItem)
        $menuInvoked = $false
        $pluginDeadline = [DateTime]::UtcNow.AddSeconds(30)
        while ([DateTime]::UtcNow -lt $pluginDeadline)
        {
            $root = [System.Windows.Automation.AutomationElement]::FromHandle($Handle)
            if ($root.Current.ProcessId -ne $OwnedProcessId)
            {
                throw 'The automation window does not belong to the smoke process.'
            }

            $pluginMenu = $null
            foreach ($item in $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition))
            {
                if ($item.Current.Name.Replace('_', '').Replace('&', '').Trim() -ceq 'Plugins')
                {
                    $pluginMenu = $item
                    break
                }
            }

            if ($null -ne $pluginMenu)
            {
                if (-not $pluginMenu.Current.IsEnabled)
                {
                    Start-Sleep -Milliseconds 250
                    continue
                }

                if (-not $menuInvoked)
                {
                    $pattern = $null
                    if ($pluginMenu.TryGetCurrentPattern(
                            [System.Windows.Automation.ExpandCollapsePattern]::Pattern, [ref]$pattern))
                    {
                        ([System.Windows.Automation.ExpandCollapsePattern]$pattern).Expand()
                        $result.automationPattern = 'ExpandCollapse'
                    }
                    elseif ($pluginMenu.TryGetCurrentPattern(
                            [System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern))
                    {
                        ([System.Windows.Automation.InvokePattern]$pattern).Invoke()
                        $result.automationPattern = 'Invoke'
                    }
                    else
                    {
                        if ([GitExtensionsSmokeWindows]::GetForeground() -ne $Handle)
                        {
                            throw 'The owned window is not foreground; cannot safely invoke the Plugins mnemonic.'
                        }

                        Add-Type -AssemblyName System.Windows.Forms
                        [System.Windows.Forms.SendKeys]::SendWait('%p')
                        $result.automationPattern = 'KeyboardMnemonic'
                    }

                    $menuInvoked = $true
                }
                $names = @(
                    foreach ($item in $pluginMenu.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition))
                    {
                        $name = $item.Current.Name.Replace('_', '').Replace('&', '').Trim()
                        if ($name -and $name -notin @('Loading...', 'Plugins settings...')) { $name }
                    }
                )
                $result.pluginNames = @($names | Sort-Object -Unique)
                if ($result.pluginNames -ccontains $result.expectedExport)
                {
                    $result.status = 'passed'
                    return [pscustomobject]$result
                }
            }

            Start-Sleep -Milliseconds 250
        }

        $result.reason = 'The expected runtime plugin export did not appear before the deadline.'
    }
    catch
    {
        $result.reason = $_.Exception.Message
    }

    return [pscustomobject]$result
}

# parity-scaffolding: captures WSLg runtime windows as evidence until the parity gate closes.
Add-Type -AssemblyName System.Drawing
if (-not ([System.Management.Automation.PSTypeName]'GitExtensionsSmokeWindows').Type)
{
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
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

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

    public static IntPtr FindVisibleWindow(string titlePattern, int processId, out string title)
    {
        IntPtr match = IntPtr.Zero;
        string matchedTitle = string.Empty;
        EnumWindows((window, parameter) =>
        {
            if (!IsWindowVisible(window))
            {
                return true;
            }

            GetWindowThreadProcessId(window, out uint windowProcessId);
            if (processId != 0 && windowProcessId != processId)
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
}

$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
$window = [IntPtr]::Zero
$title = ""
while ($window -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $deadline)
{
    $window = [GitExtensionsSmokeWindows]::FindVisibleWindow($TitlePattern, $ProcessId, [ref]$title)
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

$pluginsExpanded = $false
if ($CheckPlugins)
{
    $pluginEvidence = Get-PluginMenuEvidence $window $ProcessId
    $pluginPath = [IO.Path]::ChangeExtension([IO.Path]::GetFullPath($OutputPath), 'plugins.json')
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($pluginPath)) | Out-Null
    $pluginEvidence | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $pluginPath
    $pluginsExpanded = $pluginEvidence.status -eq 'passed' -or $null -ne $pluginEvidence.automationPattern
    if ($pluginsExpanded -and [GitExtensionsSmokeWindows]::GetForeground() -eq $window)
    {
        $CaptureMethod = 'Screen'
    }
}

if ($OpenViewMenu)
{
    if ($CaptureMethod -ne "Screen")
    {
        throw "A popup outside the owner window must be captured from the screen."
    }

    Add-Type -AssemblyName System.Windows.Forms
    [System.Windows.Forms.SendKeys]::SendWait("%v")
    Start-Sleep -Milliseconds 400
}

$rectangle = [GitExtensionsSmokeWindows]::GetRectangle($window)
$captureLeft = $rectangle.Left
$captureTop = $rectangle.Top
$captureRight = $rectangle.Right
$captureBottom = $rectangle.Bottom
if ($OpenViewMenu -or ($pluginsExpanded -and $CaptureMethod -eq 'Screen'))
{
    Add-Type -AssemblyName System.Windows.Forms
    # A popup extending beyond its owner need not appear as a separate top-level HWND.
    # Retain the owner's horizontal strip through the work area for visual inspection.
    $workArea = [System.Windows.Forms.Screen]::FromHandle($window).WorkingArea
    $captureTop = [Math]::Min($captureTop, $workArea.Top)
    $captureBottom = [Math]::Max($captureBottom, $workArea.Bottom)
}

$width = $captureRight - $captureLeft
$height = $captureBottom - $captureTop
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
                $captureLeft,
                $captureTop,
                0,
                0,
                [Drawing.Size]::new($width, $height),
                [Drawing.CopyPixelOperation]::SourceCopy)
        }
        $actualCaptureMethod = if ($printed) { "PrintWindow" } else { "Screen" }
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
Write-Output "bounds=$captureLeft,$captureTop,${width}x${height}"
Write-Output "captureMethod=$actualCaptureMethod"
Write-Output "viewMenuRequested=$([bool]$OpenViewMenu)"
Write-Output "pluginsCheckRequested=$([bool]$CheckPlugins)"
