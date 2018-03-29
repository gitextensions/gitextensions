using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Gravatar;
using GravatarTests.Properties;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace GravatarTests
{
    [TestFixture]
    public class DirectoryImageCacheTests
    {
        private const string FileName = "aa.jpg";
        private readonly string _folderPath = @"C:\Users\user\AppData\Roaming\GitExtensions\GitExtensions\Images";
        private IFileSystem _fileSystem;
        private DirectoryBase _directory;
        private FileBase _file;
        private FileInfoBase _fileInfo;
        private IFileInfoFactory _fileInfoFactory;
        private IImageCache _cache;

        [SetUp]
        public void Setup()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _directory = Substitute.For<DirectoryBase>();
            _fileSystem.Directory.Returns(_directory);
            _file = Substitute.For<FileBase>();
            _fileSystem.File.Returns(_file);
            _fileInfo = Substitute.For<FileInfoBase>();
            _fileInfoFactory = Substitute.For<IFileInfoFactory>();
            _fileInfoFactory.FromFileName(Arg.Any<string>()).Returns(_fileInfo);
            _fileSystem.FileInfo.Returns(_fileInfoFactory);

            _cache = new DirectoryImageCache(_folderPath, 2, _fileSystem);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void AddImage_should_throw_if_filename_not_supplied(string fileName)
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _cache.GetImageAsync(fileName));

            _ = _fileInfo.DidNotReceive().LastWriteTime;
        }

        [Test]
        public void AddImage_should_throw_if_stream_null()
        {
            Assert.Throws<ArgumentNullException>(() => _cache.AddImage("file", null));

            _directory.DidNotReceive().Exists(_folderPath);
        }

        [Test]
        public void AddImage_should_create_if_folder_absent()
        {
            var fileSystem = new MockFileSystem();
            _cache = new DirectoryImageCache(_folderPath, 2, fileSystem);
            fileSystem.Directory.Exists(_folderPath).Should().BeFalse();

            _cache.AddImage("file", Resources.User);

            fileSystem.Directory.Exists(_folderPath).Should().BeTrue();
        }

        [Test]
        public void AddImage_should_create_image_from_stream()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Assert.NotNull(currentFolder);

            var folderPath = Path.Combine(currentFolder, "Images");
            var fileSystem = new FileSystem();

            _cache = new DirectoryImageCache(folderPath, 2, fileSystem);
            fileSystem.Directory.Exists(_folderPath).Should().BeFalse();

            _cache.AddImage("file.png", Resources.User);

            fileSystem.Directory.Exists(folderPath).Should().BeTrue();
            fileSystem.File.Exists(Path.Combine(folderPath, "file.png")).Should().BeTrue();
        }

        [Test]
        public void AddImage_should_raise_invalidate()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Assert.NotNull(currentFolder);

            var folderPath = Path.Combine(currentFolder, "Images");
            var fileSystem = new FileSystem();

            bool eventRaised = false;
            _cache = new DirectoryImageCache(folderPath, 2, fileSystem);
            _cache.Invalidated += (s, e) => eventRaised = true;

            _cache.AddImage("file.png", Resources.User);

            eventRaised.Should().BeTrue();
        }

        [Test]
        public async Task Clear_should_return_if_folder_absent()
        {
            _directory.Exists(Arg.Any<string>()).Returns(false);

            await _cache.ClearAsync();

            _directory.DidNotReceive().GetFiles(Arg.Any<string>());
        }

        [Test]
        public async Task Clear_should_remove_all()
        {
            var fileSystem = new MockFileSystem();
            _cache = new DirectoryImageCache(_folderPath, 2, fileSystem);

            fileSystem.AddFile(Path.Combine(_folderPath, "a@a.com.png"), new MockFileData(""));
            fileSystem.AddFile(Path.Combine(_folderPath, "b@b.com.png"), new MockFileData(""));
            fileSystem.AllFiles.Count().Should().Be(2);

            await _cache.ClearAsync();

            fileSystem.AllFiles.Count().Should().Be(0);
        }

        [Test]
        public async Task Clear_should_raise_invalidate()
        {
            _directory.Exists(Arg.Any<string>()).Returns(true);

            bool eventRaised = false;
            _cache.Invalidated += (s, e) => eventRaised = true;

            await _cache.ClearAsync();

            eventRaised.Should().BeTrue();
        }

        [Test]
        public void Clear_should_ignore_errors()
        {
            _directory.Exists(Arg.Any<string>()).Returns(true);
            _directory.GetFiles(_folderPath).Returns(new[] { "c:\\file.txt", "boot.sys" });
            _file.When(x => x.Delete(Arg.Any<string>()))
                .Do(x => throw new DivideByZeroException());

            Func<Task> act = async () =>
            {
                await _cache.ClearAsync();
            };
            act.Should().NotThrow();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void DeleteImage_should_throw_if_filename_not_supplied(string fileName)
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _cache.DeleteImageAsync(fileName));

            _ = _fileInfo.DidNotReceive().LastWriteTime;
        }

        [Test]
        public async Task DeleteImage_should_return_if_folder_absent()
        {
            _file.Exists(Arg.Any<string>()).Returns(false);

            await _cache.DeleteImageAsync(FileName);

            _file.DidNotReceive().Delete(Arg.Any<string>());
        }

        [Test]
        public async Task DeleteImage_should_delete()
        {
            _file.Exists(Arg.Any<string>()).Returns(true);

            await _cache.DeleteImageAsync(FileName);

            _file.Received(1).Delete(Arg.Any<string>());
        }

        [Test]
        public async Task DeleteImage_should_raise_invalidate()
        {
            _file.Exists(Arg.Any<string>()).Returns(true);

            bool eventRaised = false;
            _cache.Invalidated += (s, e) => eventRaised = true;

            await _cache.DeleteImageAsync(FileName);

            eventRaised.Should().BeTrue();
        }

        [Test]
        public void DeleteImage_should_ignore_errors()
        {
            _file.Exists(Arg.Any<string>()).Returns(true);
            _file.When(x => x.Delete(Arg.Any<string>()))
                .Do(x => throw new DivideByZeroException());

            Func<Task> act = async () =>
            {
                await _cache.DeleteImageAsync(FileName);
            };
            act.Should().NotThrow();
        }

        [Test]
        public void GetImageAsync_throws_if_filename_not_supplied()
        {
            Assert.Throws<ArgumentException>(() => _cache.GetImage(null));

            _ = _fileInfo.DidNotReceive().LastWriteTime;
        }

        [Test]
        public void GetImage_throws_if_filename_not_supplied()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _cache.GetImageAsync(null));

            _ = _fileInfo.DidNotReceive().LastWriteTime;
        }

        [Test]
        public async Task GetImage_return_null_if_file_absent()
        {
            _fileInfo.Exists.Returns(false);

            var image = await _cache.GetImageAsync(FileName);

            image.Should().BeNull();
            _ = _fileInfo.DidNotReceive().LastWriteTime;
        }

        [Test]
        public async Task GetImage_return_null_if_file_expired()
        {
            _fileInfo.Exists.Returns(true);
            _fileInfo.LastWriteTime.Returns(new DateTime(2010, 1, 1));

            var image = await _cache.GetImageAsync(FileName);

            image.Should().BeNull();
            _ = _fileInfo.Received(1).LastWriteTime;
        }

        [Test]
        public void GetImage_return_null_if_exception()
        {
            _fileInfo.Exists.Returns(true);
            _fileInfo.LastWriteTime.Returns(x => throw new DivideByZeroException());

            Func<Task> act = async () =>
            {
                await _cache.GetImageAsync(FileName);
            };
            act.Should().NotThrow();
        }
    }
}
