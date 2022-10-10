﻿using GitCommands;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CommonTestUtils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class TestAppSettingsAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Suite;

        public void BeforeTest(ITest test)
        {
            AppSettings.CheckForUpdates = false;
            AppSettings.ShowAvailableDiffTools = false;
        }

        public void AfterTest(ITest test)
        {
        }
    }
}
