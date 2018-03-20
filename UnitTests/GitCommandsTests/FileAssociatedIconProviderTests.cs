using System;
using System.IO;
using System.IO.Abstractions;
using FluentAssertions;
using GitCommands;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class FileAssociatedIconProviderTests
    {
        private FileBase _file;
        private IFileSystem _fileSystem;
        private FileAssociatedIconProvider _iconProvider;

        [SetUp]
        public void Setup()
        {
            _file = Substitute.For<FileBase>();
            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(_file);
            _iconProvider = new FileAssociatedIconProvider(_fileSystem);
            _iconProvider.ResetCache();
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        public void Get_should_return_null_if_path_null_or_empty(string workingDirectory, string relativeFilePath)
        {
            _iconProvider.Get(workingDirectory, relativeFilePath).Should().BeNull();
        }

        [Test]
        public void Get_should_return_null_for_extensionless_file()
        {
            _iconProvider.Get(@"c:\folder", "file").Should().BeNull();
        }

        [Test]
        public void Get_if_file_does_not_exist_create_temp()
        {
            const string folder = @"c:\non_existent_folder";
            const string file = @"file.txt";
            _file.Exists(file).Returns(false);

            _iconProvider.Get(folder, file).Should().BeNull();

            var tempPath = Path.Combine(Path.GetTempPath(), file);
            _file.Received(1).WriteAllText(tempPath, string.Empty);
        }

        [Test]
        public void Get_if_temp_file_cant_be_delete_ignore()
        {
            const string folder = @"c:\non_existent_folder";
            const string file = @"file.txt";
            _file.Exists(Arg.Any<string>()).Returns(false);
            _file.When(x => x.Delete(Arg.Any<string>()))
                .Do(x => throw new DivideByZeroException("boom"));

            _iconProvider.Get(folder, file).Should().BeNull();

            var tempPath = Path.Combine(Path.GetTempPath(), file);
            _file.Received(1).WriteAllText(tempPath, string.Empty);
        }

        [Test]
        public void Get_should_add_entry_for_extension_once()
        {
            _iconProvider = new FileAssociatedIconProvider(); // use real file system
            _iconProvider.ResetCache();

            var tempFile = Path.GetTempFileName();
            string folder = Path.GetDirectoryName(tempFile);
            string file = Path.GetFileName(tempFile);

            _iconProvider.CacheCount.Should().Be(0);
            _iconProvider.Get(folder, file);
            _iconProvider.Get(folder, file);
            _iconProvider.Get(folder, file);
            _iconProvider.Get(folder, file);
            _iconProvider.CacheCount.Should().Be(1);

            try
            {
                File.Delete(tempFile);
            }
            catch
            {
                // do nothing
            }
        }
    }
}