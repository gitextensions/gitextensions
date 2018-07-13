using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GitCommands;
using GitUI.Avatars;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    [TestFixture]
    public sealed class AvatarPersistentCacheTests : AvatarCacheTestBase
    {
        private string _folderPath;
        private IFileSystem _fileSystem;
        private DirectoryBase _directory;
        private FileBase _file;
        private FileInfoBase _fileInfo;
        private IFileInfoFactory _fileInfoFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _fileSystem = Substitute.For<IFileSystem>();
            _directory = Substitute.For<DirectoryBase>();
            _fileSystem.Directory.Returns(_directory);
            _file = Substitute.For<FileBase>();
            _fileSystem.File.Returns(_file);
            _fileInfo = Substitute.For<FileInfoBase>();
            _fileInfo.Exists.Returns(true);
            _fileInfoFactory = Substitute.For<IFileInfoFactory>();
            _fileInfoFactory.FromFileName(Arg.Any<string>()).Returns(_fileInfo);
            _fileSystem.FileInfo.Returns(_fileInfoFactory);

            _folderPath = AppSettings.AvatarImageCachePath;

            _cache = new AvatarPersistentCache(_inner, _fileSystem);
        }

        [Test]
        public async Task GetAvatarAsync_should_create_if_folder_absent()
        {
            var fileSystem = new MockFileSystem();
            _cache = new AvatarPersistentCache(_inner, fileSystem);
            fileSystem.Directory.Exists(_folderPath).Should().BeFalse();

            Assert.AreSame(_img1, await _cache.GetAvatarAsync(_email1, _size));

            fileSystem.Directory.Exists(_folderPath).Should().BeTrue();
        }

        [Test]
        public async Task GetAvatarAsync_should_create_image_from_stream()
        {
            var fileSystem = new MockFileSystem();
            _cache = new AvatarPersistentCache(_inner, fileSystem);
            fileSystem.Directory.Exists(_folderPath).Should().BeFalse();

            Assert.AreSame(_img1, await _cache.GetAvatarAsync(_email1, _size));

            fileSystem.Directory.Exists(_folderPath).Should().BeTrue();
            fileSystem.File.Exists(Path.Combine(_folderPath, $"{_email1}.{_size}px.png")).Should().BeTrue();
        }

        [Test]
        public async Task GetAvatarAsync_uses_inner_if_file_expired()
        {
            _fileInfo.Exists.Returns(true);
            _fileInfo.LastWriteTime.Returns(new DateTime(2010, 1, 1));
            _fileSystem.File.OpenWrite(Arg.Any<string>()).Returns(_ => new MemoryStream());
            _fileSystem.File.Delete(Arg.Any<string>());

            await MissAsync(_email1);

            _fileSystem.File.Received(1).Delete(Path.Combine(AppSettings.AvatarImageCachePath, $"{_email1}.{_size}px.png"));

            _file.OpenRead(Arg.Any<string>()).Returns(c => GetPngStream());
            _fileInfo.LastWriteTime.Returns(DateTime.Now);
            _fileSystem.ClearReceivedCalls();
            _fileInfo.ClearReceivedCalls();
            _file.ClearReceivedCalls();

            var image = await _cache.GetAvatarAsync(_email1, 16);

            image.Should().NotBeNull();
            _ = _fileInfo.Received(1).LastWriteTime;
            _fileSystem.File.Received(1).OpenRead(Path.Combine(AppSettings.AvatarImageCachePath, $"{_email1}.{_size}px.png"));
        }

        [Test]
        public async Task ClearCacheAsync_should_return_if_folder_absent()
        {
            _directory.Exists(Arg.Any<string>()).Returns(false);

            await _cache.ClearCacheAsync();

            _directory.DidNotReceive().GetFiles(Arg.Any<string>());
        }

        [Test]
        public async Task ClearCacheAsync_should_remove_all()
        {
            var fileSystem = new MockFileSystem();
            _cache = new AvatarPersistentCache(_inner, fileSystem);

            fileSystem.AddFile(Path.Combine(_folderPath, "a@a.com.16px.png"), new MockFileData(""));
            fileSystem.AddFile(Path.Combine(_folderPath, "b@b.com.16px.png"), new MockFileData(""));
            fileSystem.AllFiles.Count().Should().Be(2);

            await _cache.ClearCacheAsync();

            fileSystem.AllFiles.Count().Should().Be(0);
        }

        [Test]
        public void ClearCacheAsync_should_ignore_errors()
        {
            _directory.Exists(Arg.Any<string>()).Returns(true);
            _directory.GetFiles(_folderPath).Returns(new[] { "c:\\file.txt", "boot.sys" });
            _file.When(x => x.Delete(Arg.Any<string>()))
                .Do(x => throw new DivideByZeroException());

            Func<Task> act = () => _cache.ClearCacheAsync();
            act.Should().NotThrow();
        }
    }
}
