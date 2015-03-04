﻿using System;
using System.Collections.Generic;
using GitCommands;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitExtensionsTest.Git
{
    [TestClass]
    public class GitCommandsHelperTest
    {
        [TestMethod]
        public void CanGetRelativeDateString()
        {
            AppSettings.CurrentTranslation = "English";
            Assert.AreEqual("0 seconds ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now));
            Assert.AreEqual("1 second ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddSeconds(-1)));
            Assert.AreEqual("1 minute ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddMinutes(-1)));
            Assert.AreEqual("1 hour ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddMinutes(-45)));
            Assert.AreEqual("1 hour ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddHours(-1)));
            Assert.AreEqual("1 day ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-1)));
            Assert.AreEqual("1 week ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-7)));
            Assert.AreEqual("1 month ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-30)));
            Assert.AreEqual("12 months ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-364)));
            Assert.AreEqual("1 year ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-365)));

            Assert.AreEqual("2 seconds ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddSeconds(-2)));
            Assert.AreEqual("2 minutes ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddMinutes(-2)));
            Assert.AreEqual("2 hours ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddHours(-2)));
            Assert.AreEqual("2 days ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-2)));
            Assert.AreEqual("2 weeks ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-14)));
            Assert.AreEqual("2 months ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-60)));
            Assert.AreEqual("2 years ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-730)));
        }

        [TestMethod]
        public void CanGetRelativeNegativeDateString()
        {
            AppSettings.CurrentTranslation = "English";
            Assert.AreEqual("-1 second ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddSeconds(1)));
            Assert.AreEqual("-1 minute ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddMinutes(1)));
            Assert.AreEqual("-1 hour ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddMinutes(45)));
            Assert.AreEqual("-1 hour ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddHours(1)));
            Assert.AreEqual("-1 day ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(1)));
            Assert.AreEqual("-1 week ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(7)));
            Assert.AreEqual("-1 month ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(30)));
            Assert.AreEqual("-12 months ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(364)));
            Assert.AreEqual("-1 year ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(365)));

            Assert.AreEqual("-2 seconds ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddSeconds(2)));
            Assert.AreEqual("-2 minutes ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddMinutes(2)));
            Assert.AreEqual("-2 hours ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddHours(2)));
            Assert.AreEqual("-2 days ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(2)));
            Assert.AreEqual("-2 weeks ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(14)));
            Assert.AreEqual("-2 months ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(60)));
            Assert.AreEqual("-2 years ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(730)));
        }

        [TestMethod]
        public void TestGetAllChangedFilesFromString()
        {
            GitModule module = new GitModule(null);

            {//git diff -M -C -z --cached --name-status
                string statusString = "\r\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\r\nThe file will have its original line endings in your working directory.\r\nwarning: LF will be replaced by CRLF in FxCop.targets.\r\nThe file will have its original line endings in your working directory.\r\nM\0testfile.txt\0";
                List<GitItemStatus> status = GitCommandHelpers.GetAllChangedFilesFromString(module, statusString, true);
                Assert.IsTrue(status.Count == 1);
                Assert.IsTrue(status[0].Name == "testfile.txt");
            }
            {//git diff -M -C -z --cached --name-status
                string statusString = "\0\r\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\r\nThe file will have its original line endings in your working directory.\r\nwarning: LF will be replaced by CRLF in FxCop.targets.\r\nThe file will have its original line endings in your working directory.\r\nM\0testfile.txt\0";
                List<GitItemStatus> status = GitCommandHelpers.GetAllChangedFilesFromString(module, statusString, true);
                Assert.IsTrue(status.Count == 1);
                Assert.IsTrue(status[0].Name == "testfile.txt");
            }
            {//git diff -M -C -z --cached --name-status
                string statusString = "\0\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in FxCop.targets.\nThe file will have its original line endings in your working directory.\nM\0testfile.txt\0";
                List<GitItemStatus> status = GitCommandHelpers.GetAllChangedFilesFromString(module, statusString, true);
                Assert.IsTrue(status.Count == 1);
                Assert.IsTrue(status[0].Name == "testfile.txt");
            }
            {//git diff -M -C -z --cached --name-status
                string statusString = "M  testfile.txt\0\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in FxCop.targets.\nThe file will have its original line endings in your working directory.\n";
                List<GitItemStatus> status = GitCommandHelpers.GetAllChangedFilesFromString(module, statusString, true);
                Assert.IsTrue(status.Count == 1);
                Assert.IsTrue(status[0].Name == "testfile.txt");
            }
            { //git status --porcelain --untracked-files=no -z
                string statusString = "M  adfs.h\0M  dir.c\0\r\nwarning: LF will be replaced by CRLF in adfs.h.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in dir.c.\nThe file will have its original line endings in your working directory.";
                List<GitItemStatus> status = GitCommandHelpers.GetAllChangedFilesFromString(module, statusString, false);
                Assert.IsTrue(status.Count == 2);
                Assert.IsTrue(status[0].Name == "adfs.h");
                Assert.IsTrue(status[1].Name == "dir.c");
            }
        }

        [TestMethod]
        public void GetFullBranchNameTest()
        {
            Assert.AreEqual(null, GitCommandHelpers.GetFullBranchName(null));
            Assert.AreEqual("", GitCommandHelpers.GetFullBranchName(""));
            Assert.AreEqual("", GitCommandHelpers.GetFullBranchName("    "));
            Assert.AreEqual("4e0f0fe3f6add43557913c354de02560b8faec32", GitCommandHelpers.GetFullBranchName("4e0f0fe3f6add43557913c354de02560b8faec32"));
            Assert.AreEqual("refs/heads/master", GitCommandHelpers.GetFullBranchName("master"));
            Assert.AreEqual("refs/heads/master", GitCommandHelpers.GetFullBranchName(" master "));
            Assert.AreEqual("refs/heads/master", GitCommandHelpers.GetFullBranchName("refs/heads/master"));
            Assert.AreEqual("refs/heads/release/2.48", GitCommandHelpers.GetFullBranchName("refs/heads/release/2.48"));
            Assert.AreEqual("refs/tags/my-tag", GitCommandHelpers.GetFullBranchName("refs/tags/my-tag"));
        }

        [TestMethod]
        public void TestGetPlinkCompatibleUrl_Incompatible()
        {
            // Test urls that are incompatible and need to be changed      
            string inUrl, expectUrl, outUrl;

            // ssh urls can cause problems
            inUrl = "ssh://user@example.com/path/to/project.git";
            expectUrl = "\"user@example.com:path/to/project.git\"";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual(expectUrl, outUrl);

            inUrl = "ssh://user@example.com:29418/path/to/project.git";
            expectUrl = "-P 29418 \"user@example.com:path/to/project.git\"";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual(expectUrl, outUrl);

            // ssh, no user
            inUrl = "ssh://example.com/path/to/project.git";
            expectUrl = "\"example.com:path/to/project.git\"";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual(expectUrl, outUrl);
        }

        [TestMethod]
        public void TestGetPlinkCompatibleUrl_Compatible()
        {
            // Test urls that are already compatible, these shouldn't be changed
            string inUrl, outUrl;

            // ssh in compatible form
            inUrl = "git@github.com:gitextensions/gitextensions.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // ssh in compatible form, no user
            inUrl = "example.org:some/path/to/repo.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);
        }

        [TestMethod]
        public void TestGetPlinkCompatibleUrl_NoPlink()
        {
            // Test urls that are no valid uris, these should be ignored    
            string inUrl, outUrl;

            // git protocol does not have authentication
            inUrl = "git://server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // git protocol, different port
            inUrl = "git://server:123/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // we don't need plink for http
            inUrl = "http://user@server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // http, different port
            inUrl = "http://user@server:123/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // http, no user
            inUrl = "http://server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // we don't need plink for https
            inUrl = "https://user@server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // https, different port
            inUrl = "https://user@server:123/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // https, no user
            inUrl = "https://server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);
        }

        [TestMethod]
        public void TestGetPlinkCompatibleUrl_Invalid()
        {
            // Test urls that are no valid uris, these should be ignored  
            string inUrl, outUrl;

            inUrl = "foo://server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            inUrl = @"ssh:\\server\path\to\project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);
        }
    }
}
