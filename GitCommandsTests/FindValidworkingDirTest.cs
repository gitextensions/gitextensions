﻿#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Category = Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute;
#else
using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using GitCommands;

namespace GitCommandsTests
{
    /// <summary>

    /// </summary>
    [TestClass]
    public class FindValidworkingDirTest
    {
        private string GetCurrentDir()
        {
            string path = typeof(FindValidworkingDirTest).Assembly.Location;

            return path.Substring(0, path.LastIndexOf('\\'));
        }


        [TestMethod]
        public void TestWorkingDir()
        {
            Settings.WorkingDir = GetCurrentDir();
            CheckWorkingDir();
            Settings.WorkingDir = GetCurrentDir() + "\\testfile.txt";
            CheckWorkingDir();
            Settings.WorkingDir = GetCurrentDir() + "\\";
            CheckWorkingDir();
            Settings.WorkingDir = GetCurrentDir() + "\\\\";
            CheckWorkingDir();
            Settings.WorkingDir = GetCurrentDir() + "\\test\\test\\tralala";
            CheckWorkingDir();

        }

        private static void CheckWorkingDir()
        {
            //Should not contain double slashes -> \\
            Assert.IsFalse(Settings.WorkingDir.Contains("\\\\"));

            //Should end with slash
            Assert.IsTrue(Settings.WorkingDir.EndsWith("\\"));
        }
    }
}
