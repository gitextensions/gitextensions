using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using GitCommands;

namespace GitCommandsTests
{
    /// <summary>
    /// Tests for configfile class. 
    /// The configfile class should respond the same as "git config".
    /// Since .gitconfig is often hidden, also make sure this is tested.
    /// </summary>
    [TestClass]
    public class ConfigFileTest
    {
        private string GetTempFolder()
        {
            return System.IO.Path.GetTempPath();
        }

        private string GetConfigFileName()
        {
            return string.Concat(GetTempFolder() + @"\testconfigfile");
        }

        private string GetDefaultConfigFileContent()
        {
            StringBuilder content = new StringBuilder();
            content.AppendLine("[section1]");
            content.AppendLine("key1=value1");
            content.AppendLine("[section2.subsection]");
            content.AppendLine("key2=value2");
            content.AppendLine("[section3 \"subsection\"]");
            content.AppendLine("key3=value3");
            return content.ToString();
        }

        [TestMethod]
        public void TestWithHiddenFile()
        {
            { //TESTDATA
                //Write test config
                File.WriteAllText(GetConfigFileName(), GetDefaultConfigFileContent(), Encoding.UTF8);

                //Make sure it is hidden
                FileInfo configFile = new FileInfo(GetConfigFileName());
                configFile.Attributes = FileAttributes.Hidden;
            }

            { //PERFORM TEST
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual("value1", configFile.GetValue("section1.key1"));
                Assert.AreEqual("value2", configFile.GetValue("section2.subsection.key2"));
                Assert.AreEqual("value3", configFile.GetValue("section3.subsection.key3"));

                configFile.SetValue("section1.key1", "newvalue1");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual("newvalue1", configFile.GetValue("section1.key1"));
            }
        }

        [TestMethod]
        public void TestWithDirectories()
        {
            { //TESTDATA
                //Write test config
                File.WriteAllText(GetConfigFileName(), GetDefaultConfigFileContent(), Encoding.UTF8);
            }

            { //PERFORM TEST
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                configFile.SetValue("directory.first", @"c:\program files\gitextensions\gitextensions.exe");
                configFile.Save();
            }

            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual(@"c:/program files/gitextensions/gitextensions.exe", configFile.GetValue("directory.first"));
            }
        }

        [TestMethod]
        public void TestNonExistingFile()
        {

            { //PERFORM TEST
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                configFile.SetValue("directory.first", @"c:\program files\gitextensions\gitextensions.exe");
                configFile.Save();
            }

            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual(@"c:/program files/gitextensions/gitextensions.exe", configFile.GetValue("directory.first"));
            }
        }

        [TestMethod]
        public void TestWithSectionWithDot()
        {
            { //TESTDATA
                StringBuilder content = new StringBuilder();

                content.AppendLine("[submodule \"test.test\"]");
                content.AppendLine("path = test.test");

                //Write test config
                File.WriteAllText(GetConfigFileName(), content.ToString(), Encoding.UTF8);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual("test.test", configFile.GetValue("submodule.test.test.path"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                configFile.SetValue("submodule.test.test.path", "newvalue");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual("newvalue", configFile.GetValue("submodule.test.test.path"));
            }
        }

        /// <summary>
        /// Always delete the test config file after each test
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(GetConfigFileName()))
            {
                //Make sure it is hidden
                FileInfo configFile = new FileInfo(GetConfigFileName());
                configFile.Attributes = FileAttributes.Normal;

                File.Delete(GetConfigFileName());
            }
        }
    }
}
