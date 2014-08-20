using System;
using System.IO;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitExtensionsTest.Helpers
{
    [TestClass]
    public class FileInfoExtensionsTest
    {
        private const string File1Name = "c05706a2-1419-4f90-b2fc-0c1619086ea6";
        private const string File2Name = "32cc832d-b89b-4ab6-8925-44e079ff3209";
        private const string File3Name = "637d303e-bac7-42d7-8f81-ab9251932945";

        private string _file1Path;
        private string _file2Path;
        private string _file3Path;

        private void FillFilePaths()
        {
            _file1Path = Path.Combine(Path.GetTempPath(), File1Name);
            _file2Path = Path.Combine(Path.GetTempPath(), File2Name);
            _file3Path = Path.Combine(Path.GetTempPath(), File3Name);
        }

        private void CreateTestFiles()
        {
            using (var writer = new StreamWriter(_file1Path))
            {
                writer.WriteLine("this is a test");
                writer.WriteLine("from a test file");
                writer.Flush();
            }

            using (var writer = new StreamWriter(_file2Path))
            {
                writer.WriteLine("this is a test");
                writer.WriteLine("from a test file");
                writer.Flush();
            }

            using (var writer = new StreamWriter(_file3Path))
            {
                writer.WriteLine("this is a test");
                writer.WriteLine("from a test file\n");
                writer.Flush();
            }
        }

        [TestMethod]
        public void TestCompare()
        {

            FillFilePaths();
            CreateTestFiles();

            var file1Info = new FileInfo(_file1Path);
            var file2Info = new FileInfo(_file2Path);
            var file3Info = new FileInfo(_file3Path);

            Assert.IsTrue(file1Info.Compare(_file2Path));
            Assert.IsFalse(file1Info.Compare(_file3Path));
            Assert.IsTrue(file1Info.Compare(file2Info));
            Assert.IsFalse(file1Info.Compare(file3Info));

        }

        [TestMethod]
        public void TestChecksum()
        {
            FillFilePaths();
            CreateTestFiles();

            var file1CheckSum = new FileInfo(_file1Path).CheckSum();
            var file2CheckSum = new FileInfo(_file2Path).CheckSum();

            Assert.AreEqual(file1CheckSum, file2CheckSum);
        }

        [TestMethod]
        public void TestCompareWithNonExistingFile()
        {
            Assert.IsFalse(new FileInfo("inexistentFile").Compare("anotherInexistentFile"));
            Assert.IsFalse(new FileInfo(String.Format("inexistentFolder{0}inexistentFile", Path.PathSeparator)).Compare(String.Format("anotherInexistentFolder{0}anotherInexistentFile", Path.PathSeparator)));
        }

        [TestMethod]
        public void TestCompareWithNullFileInfo()
        {
            Assert.IsFalse(new FileInfo("inexistentFile").Compare(new FileInfo("anotherInexistentFile")));
        }

        [TestCleanup]
        public void Cleanup()
        {
            if(File.Exists(_file1Path))
                File.Delete(_file1Path);
            if(File.Exists(_file2Path))
                File.Delete(_file2Path);
            if(File.Exists(_file3Path))
                File.Delete(_file3Path);
        }

    }
}
