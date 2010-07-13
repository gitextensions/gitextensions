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

        [TestMethod]
        public void TestWithSectionWithDot2()
        {
            { //TESTDATA
                StringBuilder content = new StringBuilder();

                content.AppendLine("[submodule.test.test]");
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

        [TestMethod]
        public void RandomTestCase1()
        {
            { //TESTDATA
                StringBuilder content = new StringBuilder();

                content.AppendLine("[merge]");
                content.AppendLine("	tool = kdiff3");
                content.AppendLine("[mergetool \"kdiff3\"]");
                content.AppendLine("	path = c:/Program Files (x86)/KDiff3/kdiff3.exe");
                content.AppendLine("[user]");
                content.AppendLine("	name = Sergey Pustovit");
                content.AppendLine("	email = sergiy.pustovit@sintez.co.za");
                content.AppendLine("[core]");
                content.AppendLine("	safecrlf = false");
                content.AppendLine("	editor = C:/Program Files (x86)/Notepad++/notepad++.exe");
                content.AppendLine("[diff]");
                content.AppendLine("	tool = kdiff3");
                content.AppendLine("[difftool \"kdiff3\"]");
                content.AppendLine("	path = c:/Program Files (x86)/KDiff3/kdiff3.exe");

                //Write test config
                File.WriteAllText(GetConfigFileName(), content.ToString(), Encoding.UTF8);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual("Sergey Pustovit", configFile.GetValue("user.name"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                configFile.SetValue("user.name", "newvalue");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual("newvalue", configFile.GetValue("user.name"));
            }
        }

        [TestMethod]
        public void RandomTestCase2()
        {
            { //TESTDATA
                StringBuilder content = new StringBuilder();

                content.AppendLine("[core]");
                content.AppendLine("	repositoryformatversion = 0");
                content.AppendLine("	filemode = false");
                content.AppendLine("	bare = false");
                content.AppendLine("	logallrefupdates = true");
                content.AppendLine("	symlinks = false");
                content.AppendLine("	ignorecase = true");
                content.AppendLine("	editor = C:/Program Files (x86)/Notepad++/notepad++.exe");
                content.AppendLine("[remote \"origin\"]");
                content.AppendLine("	fetch = +refs/heads/*:refs/remotes/origin/*");
                content.AppendLine("	url = git-sp_sable@free1.projectlocker.com:SDWH_Project_01.git");
                content.AppendLine("	puttykeyfile = C:\\Users\\sergiy.pustovit\\spustovit_sintez_key_1.ppk");
                content.AppendLine("[branch \"master\"]");
                content.AppendLine("	remote = origin");
                content.AppendLine("	merge = refs/heads/master");
                content.AppendLine("[gui]");
                content.AppendLine("	geometry = 917x503+25+25 201 191");
                content.AppendLine("[merge]");
                content.AppendLine("	tool = kdiff3");
                content.AppendLine("[remote \"test\"]");
                content.AppendLine("	url = git-sp_sable@free1.projectlocker.com:project1.git");
                content.AppendLine("	fetch = +refs/heads/*:refs/remotes/test/*");
                content.AppendLine("	puttykeyfile = C:/Users/sergiy.pustovit/spustovit_sintez_key_1.ppk");



                //Write test config
                File.WriteAllText(GetConfigFileName(), content.ToString(), Encoding.UTF8);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual("0", configFile.GetValue("core.repositoryformatversion"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                configFile.SetValue("core.repositoryformatversion", "1");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual("1", configFile.GetValue("core.repositoryformatversion"));
            }
        }

        [TestMethod]
        public void UncPathTest1()
        {
            { //TESTDATA
                StringBuilder content = new StringBuilder();

                content.AppendLine(@"[path]");
                content.AppendLine(@"	unc = //test/");

                //Write test config
                File.WriteAllText(GetConfigFileName(), content.ToString(), Encoding.UTF8);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual(@"//test/", configFile.GetValue("path.unc"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                configFile.SetValue("path.unc", @"//test/test2/");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual(@"//test/test2/", configFile.GetValue("path.unc"));
            }
        }

        [TestMethod]
        public void UncPathTest2()
        {
            { //TESTDATA
                StringBuilder content = new StringBuilder();

                content.AppendLine(@"[path]");
                content.AppendLine(@"	unc = \\\\test\\"); //<- escaped value in config file

                //Write test config
                File.WriteAllText(GetConfigFileName(), content.ToString(), Encoding.UTF8);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual(@"\\test\", configFile.GetValue("path.unc"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                configFile.SetValue("path.unc", @"\\test\test2\");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual(@"\\test\test2\", configFile.GetValue("path.unc"));
            }
        }

        [TestMethod]
        public void SpacesInSubSectionTest()
        {
            { //TESTDATA
                StringBuilder content = new StringBuilder();

                content.AppendLine("[section \"sub section\"]");
                content.AppendLine("	test = test");

                //Write test config
                File.WriteAllText(GetConfigFileName(), content.ToString(), Encoding.UTF8);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual(@"test", configFile.GetValue("section.sub section.test"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                configFile.SetValue("section.sub section.test", @"test2");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName());
                Assert.AreEqual(@"test2", configFile.GetValue("section.sub section.test"));
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
