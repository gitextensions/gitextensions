namespace ResourceManager;

public interface IHotkeySettingsReader
{
    HotkeyCommand[] LoadHotkeys(string hotkeySettingsName);
}
