﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildDurationFormatter
    {
        string Format(long? durationMilliseconds);
    }

    public class BuildDurationFormatter : IBuildDurationFormatter
    {
        public string Format(long? durationMilliseconds)
        {
            if (durationMilliseconds.HasValue)
            {
                return " (" + TimeSpan.FromMilliseconds(durationMilliseconds.Value).ToString(@"mm\:ss") + ")";
            }

            return string.Empty;
        }
    }
}
