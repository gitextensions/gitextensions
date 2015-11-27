using GitUIPluginInterfaces;

namespace ProxySwitcher
{
    public static class SettingsKey
    {
        public static StringSetting Username = new StringSetting("Username", string.Empty);
        public static StringSetting Password = new StringSetting("Password", string.Empty);
        public static StringSetting HttpProxy= new StringSetting("HTTP proxy", string.Empty);
        public static StringSetting HttpProxyPort = new StringSetting("HTTP proxy port", "8080");
    }
}
