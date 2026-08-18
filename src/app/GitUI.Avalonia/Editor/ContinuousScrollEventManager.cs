using Avalonia.Input;
using GitCommands;

namespace GitUI.Editor;

public sealed class ContinuousScrollEventManager
{
    public EventHandler? BottomScrollReached;
    public EventHandler? TopScrollReached;

    private readonly Func<bool> _getAutomaticContinuousScroll;
    private readonly Func<int> _getAutomaticContinuousScrollDelay;
    private readonly Func<DateTime> _getCurrentTime;

    public ContinuousScrollEventManager()
        : this(
            () => AppSettings.AutomaticContinuousScroll,
            () => AppSettings.AutomaticContinuousScrollDelay,
            () => DateTime.Now)
    {
    }

    // parity-scaffolding: injects settings and time so the original throttle contract is deterministic in tests.
    internal ContinuousScrollEventManager(
        Func<bool> getAutomaticContinuousScroll,
        Func<int> getAutomaticContinuousScrollDelay,
        Func<DateTime> getCurrentTime)
    {
        _getAutomaticContinuousScroll = getAutomaticContinuousScroll;
        _getAutomaticContinuousScrollDelay = getAutomaticContinuousScrollDelay;
        _getCurrentTime = getCurrentTime;
    }

    private bool IsScrollDisabled(KeyModifiers keyModifiers)
        => keyModifiers != KeyModifiers.Alt && !_getAutomaticContinuousScroll();

    private bool IsScrollTooFast(DateTime currentTime)
        => currentTime - LastScrollEventFiredDate < TimeSpan.FromMilliseconds(_getAutomaticContinuousScrollDelay());

    private DateTime LastScrollEventFiredDate { get; set; } = DateTime.MinValue;

    public void RaiseBottomScrollReached(object sender, EventArgs e)
        => RaiseBottomScrollReached(GetKeyModifiers(e));

    // parity-scaffolding: exposes Avalonia's captured pointer modifiers to deterministic tests.
    internal bool RaiseBottomScrollReached(KeyModifiers keyModifiers)
    {
        DateTime currentTime = _getCurrentTime();
        if (IsScrollDisabled(keyModifiers) || IsScrollTooFast(currentTime))
        {
            return false;
        }

        LastScrollEventFiredDate = currentTime;
        BottomScrollReached?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void RaiseTopScrollReached(object sender, EventArgs e)
        => RaiseTopScrollReached(GetKeyModifiers(e));

    // parity-scaffolding: exposes Avalonia's captured pointer modifiers to deterministic tests.
    internal bool RaiseTopScrollReached(KeyModifiers keyModifiers)
    {
        DateTime currentTime = _getCurrentTime();
        if (IsScrollDisabled(keyModifiers) || IsScrollTooFast(currentTime))
        {
            return false;
        }

        LastScrollEventFiredDate = currentTime;
        TopScrollReached?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private static KeyModifiers GetKeyModifiers(EventArgs e)
        => e is PointerEventArgs pointerEventArgs ? pointerEventArgs.KeyModifiers : KeyModifiers.None;
}
