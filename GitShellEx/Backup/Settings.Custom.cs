using System;
using System.Collections.Generic;
using System.Text;

namespace FileHashShell.Properties
{
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        static Settings()
        {
        }
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public List<ColumnConfig> CustomColumnConfig
        {
            get
            {
                try
                {
                    return ((List<ColumnConfig>)(this["CustomColumnConfig"]));
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                this["CustomColumnConfig"] = value;
            }
        }
    }
}
