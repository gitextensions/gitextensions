using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommands.Settings
{
    public class MemorySettings : SettingsContainer<MemorySettings, MemorySettingsCache>
    {
        public MemorySettings()
            : base(null, new MemorySettingsCache())
        {

        }
    }
}
