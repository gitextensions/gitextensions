using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GitCommands;
using GitCommands.Config;
#if !NUNIT
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
            return Path.GetTempPath();
        }

        private string GetConfigFileName()
        {
            return Path.Combine(GetTempFolder(), "testconfigfile");
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
		
		private void AddConfigValue(string cfgFile, string section, string value)
		{
			string args = "config -f " + "\"" +cfgFile + "\"" + " --add " + section + " " + value;
			Settings.Module.RunGitCmd(args);			
		}

        [TestMethod]
        public void TestWithInvalidFileName()
        {
            { //TESTDATA
                //Write test config
                File.WriteAllText(GetConfigFileName(), GetDefaultConfigFileContent(), Settings.AppEncoding);
            }
            ConfigFile configFile = new ConfigFile(GetConfigFileName() + "\\", false);
            
            Assert.IsNotNull(configFile);
        }

        [TestMethod]
        public void TestWithInexistentFile()
        {
            try
            {
                ConfigFile file = new ConfigFile(null, true);
                file.GetValue("inexistentSetting");
            }
            catch (Exception e)
            {
                Assert.AreEqual("invalid setting name: inexistentsetting", e.Message.ToLower());
            }
            
        }

        [TestMethod]
        public void TestHasSection()
        {
            { //TESTDATA
                //Write test config
                File.WriteAllText(GetConfigFileName(), GetDefaultConfigFileContent(), Encoding.UTF8);
            }
            ConfigFile file = new ConfigFile(GetConfigFileName(), true);
            Assert.IsTrue(file.HasConfigSection("section1"));
            Assert.IsFalse(file.HasConfigSection("inexistent.section"));
            Assert.IsFalse(file.HasConfigSection("inexistent"));
        }

        [TestMethod]
        public void TestHasValue()
        {
            { //TESTDATA
                //Write test config
                File.WriteAllText(GetConfigFileName(), GetDefaultConfigFileContent(), Encoding.UTF8);
            }
            ConfigFile file = new ConfigFile(GetConfigFileName(), true);
            Assert.IsTrue(file.HasValue("section1.key1"));
        }

        [TestMethod]
        public void TestRemoveSection()
        {
            { //TESTDATA
                //Write test config
                File.WriteAllText(GetConfigFileName(), GetDefaultConfigFileContent(), Encoding.UTF8);
            }
            ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
            Assert.IsTrue(configFile.GetConfigSections().Count == 3);
            configFile.RemoveConfigSection("section1");
            Assert.IsTrue(configFile.GetConfigSections().Count == 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWithNullSettings()
        {
            ConfigFile file = new ConfigFile(GetConfigFileName(), true);
            file.GetValue(null);
        }

        [TestMethod]
        public void TestWithHiddenFile()
        {
            { //TESTDATA
                //Write test config
                File.WriteAllText(GetConfigFileName(), GetDefaultConfigFileContent(), Settings.AppEncoding);

                //Make sure it is hidden
                FileInfo configFile = new FileInfo(GetConfigFileName());
                configFile.Attributes = FileAttributes.Hidden;
            }

            { //PERFORM TEST
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("value1", configFile.GetValue("section1.key1"));
                Assert.AreEqual("value2", configFile.GetValue("section2.subsection.key2"));
                Assert.AreEqual("value3", configFile.GetValue("section3.subsection.key3"));

                configFile.SetValue("section1.key1", "newvalue1");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("newvalue1", configFile.GetValue("section1.key1"));
            }
        }

        [TestMethod]
        public void TestWithDirectories()
        {
            { //TESTDATA
                //Write test config
                File.WriteAllText(GetConfigFileName(), GetDefaultConfigFileContent(), Settings.AppEncoding);
            }

            { //PERFORM TEST
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetPathValue("directory.first", @"c:\program files\gitextensions\gitextensions.exe");
                configFile.Save();
            }

            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual(@"c:/program files/gitextensions/gitextensions.exe", configFile.GetPathValue("directory.first"));
            }
        }

        [TestMethod]
        public void TestNonExistingFile()
        {

            { //PERFORM TEST
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetPathValue("directory.first", @"c:\program files\gitextensions\gitextensions.exe");
                configFile.Save();
            }

            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual(@"c:/program files/gitextensions/gitextensions.exe", configFile.GetPathValue("directory.first"));
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
                File.WriteAllText(GetConfigFileName(), content.ToString(), Settings.AppEncoding);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("test.test", configFile.GetPathValue("submodule.test.test.path"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetPathValue("submodule.test.test.path", "newvalue");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("newvalue", configFile.GetPathValue("submodule.test.test.path"));
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
                File.WriteAllText(GetConfigFileName(), content.ToString(), Settings.AppEncoding);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("test.test", configFile.GetPathValue("submodule.test.test.path"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetPathValue("submodule.test.test.path", "newvalue");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("newvalue", configFile.GetPathValue("submodule.test.test.path"));
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
                File.WriteAllText(GetConfigFileName(), content.ToString(), Settings.AppEncoding);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("Sergey Pustovit", configFile.GetValue("user.name"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetValue("user.name", "newvalue");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
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
                File.WriteAllText(GetConfigFileName(), content.ToString(), Settings.AppEncoding);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("0", configFile.GetValue("core.repositoryformatversion"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetValue("core.repositoryformatversion", "1");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("1", configFile.GetValue("core.repositoryformatversion"));
            }
        }

        [TestMethod]
        public void NewLineTest()
        {
            { //TESTDATA
                StringBuilder content = new StringBuilder();

                content.AppendLine("[bugtraq]");
                content.AppendLine("	url = http://192.168.0.1:8080/browse/%BUGID%");
                content.AppendLine("	message = This commit fixes %BUGID%");
                content.AppendLine("	append = true");
                content.AppendLine("	label = Key:");
                content.AppendLine("	number = true");
                content.AppendLine("	logregex = \\n([A-Z][A-Z0-9]+-/d+)");

                //Write test config
                File.WriteAllText(GetConfigFileName(), content.ToString(), Settings.AppEncoding);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("\\n([A-Z][A-Z0-9]+-/d+)", configFile.GetValue("bugtraq.logregex"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetValue("bugtraq.logregex", "data\\nnewline");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual("data\\nnewline", configFile.GetValue("bugtraq.logregex"));
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
                File.WriteAllText(GetConfigFileName(), content.ToString(), Settings.AppEncoding);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual(@"//test/", configFile.GetPathValue("path.unc"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetPathValue("path.unc", @"//test/test2/");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual(@"//test/test2/", configFile.GetPathValue("path.unc"));
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
                File.WriteAllText(GetConfigFileName(), content.ToString(), Settings.AppEncoding);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual(@"\\test\", configFile.GetPathValue("path.unc"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetPathValue("path.unc", @"\\test\test2\");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual(@"\\test\test2\", configFile.GetPathValue("path.unc"));
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
                File.WriteAllText(GetConfigFileName(), content.ToString(), Settings.AppEncoding);
            }

            //CHECK GET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual(@"test", configFile.GetValue("section.sub section.test"));
            }

            //CHECK SET CONFIG VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.SetValue("section.sub section.test", @"test2");
                configFile.Save();
            }

            //CHECK WRITTEN VALUE
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                Assert.AreEqual(@"test2", configFile.GetValue("section.sub section.test"));
            }
        }

        [TestMethod]
        public void CanSaveMultipleValuesForSameKeyInSections()
        {
            // create test data
            {
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);

                configFile.AddValue("remote.origin.fetch", "+mypath");
                configFile.AddValue("remote.origin.fetch", "+myotherpath");

                configFile.Save();
            }

            // verify
            {

                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);

                IList<string> values = configFile.GetValues("remote.origin.fetch");

                Assert.IsTrue(values.SingleOrDefault(x => x == "+mypath") != null);
                Assert.IsTrue(values.SingleOrDefault(x => x == "+myotherpath") != null);
            }
        }


        [TestMethod]
        public void CaseSensitive()
        {

			// create test data
			{
                ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);
                configFile.AddValue("branch.BranchName1.remote", "origin1");
                configFile.Save();

				AddConfigValue(GetConfigFileName(), "branch.\"BranchName2\".remote", "origin2");
				AddConfigValue(GetConfigFileName(), "branch.\"branchName2\".remote", "origin3");
			}
            // verify
            {

				ConfigFile configFile = new ConfigFile(GetConfigFileName(), true);

				string remote = "branch.BranchName1.remote";
				Assert.AreEqual("origin1", configFile.GetValue(remote), remote);
				
				remote = "branch.branchName1.remote";
				Assert.AreEqual("origin1", configFile.GetValue(remote), remote);
				
				remote = "branch \"branchName1\".remote";
				Assert.AreNotEqual("origin1", configFile.GetValue(remote), remote);
				
				remote = "branch \"BranchName2\".remote";
				Assert.AreEqual("origin2", configFile.GetValue(remote), remote);
				
				remote = "branch \"branchName2\".remote";
				Assert.AreNotEqual("origin2", configFile.GetValue(remote), remote);
				
				remote = "branch \"branchName2\".remote";
				Assert.AreEqual("origin3", configFile.GetValue(remote), remote);
				
				remote = "branch \"branchname2\".remote";
				Assert.AreEqual("", configFile.GetValue(remote), remote);
				
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
