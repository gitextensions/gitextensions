using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using CommonTestUtils;
using GitCommands;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid;

[TestFixture]
public sealed class IndexWatcherContentTests
{
    private const uint _regularFileMode = 0x81A4; // 100644
    private const uint _executableFileMode = 0x81ED; // 100755

    [TestCase(2u)]
    [TestCase(3u)]
    [TestCase(4u)]
    public void GetIndexContentHash_should_ignore_the_file_system_data_refreshed_by_git_status(uint version)
    {
        byte[]? hash = IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(version, statData: 1, new Entry("a.txt"), new Entry("dir/b.txt")));

        hash.Should().NotBeNull();
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(version, statData: 2, new Entry("a.txt"), new Entry("dir/b.txt")))
            .Should().Equal(hash);
    }

    [TestCase(2u)]
    [TestCase(3u)]
    [TestCase(4u)]
    public void GetIndexContentHash_should_detect_changed_entries(uint version)
    {
        byte[]? hash = IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(version, statData: 1, new Entry("a.txt"), new Entry("dir/b.txt")));

        hash.Should().NotBeNull();
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(version, statData: 1, new Entry("a.txt", ObjectId: 2), new Entry("dir/b.txt")))
            .Should().NotEqual(hash, "the content of a file has been staged");
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(version, statData: 1, new Entry("a.txt", Mode: _executableFileMode), new Entry("dir/b.txt")))
            .Should().NotEqual(hash, "the mode of a file has been staged");
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(version, statData: 1, new Entry("a.txt"), new Entry("dir/c.txt")))
            .Should().NotEqual(hash, "a file has been renamed");
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(version, statData: 1, new Entry("a.txt")))
            .Should().NotEqual(hash, "a file has been removed");
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(version, statData: 1, new Entry("a.txt", Stage: 2), new Entry("dir/b.txt")))
            .Should().NotEqual(hash, "a conflict has been recorded");
    }

    [Test]
    public void GetIndexContentHash_should_detect_changed_extended_flags()
    {
        byte[]? hash = IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(3, statData: 1, new Entry("a.txt", ExtendedFlags: 0x2000)));

        hash.Should().NotBeNull();
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(3, statData: 2, new Entry("a.txt", ExtendedFlags: 0x2000)))
            .Should().Equal(hash);
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(3, statData: 1, new Entry("a.txt", ExtendedFlags: 0x4000)))
            .Should().NotEqual(hash, "the file has been marked skip-worktree instead of intent-to-add");
    }

    [Test]
    public void GetIndexContentHash_should_return_null_for_invalid_data()
    {
        byte[] index = CreateIndex(2, statData: 1, new Entry("a.txt"));

        IndexWatcher.TestAccessor.GetIndexContentHash([]).Should().BeNull();
        IndexWatcher.TestAccessor.GetIndexContentHash(Encoding.ASCII.GetBytes("initial")).Should().BeNull();
        IndexWatcher.TestAccessor.GetIndexContentHash(index[..30]).Should().BeNull("the entry is truncated");
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(5, statData: 1, new Entry("a.txt"))).Should().BeNull("the version is unknown");
        IndexWatcher.TestAccessor.GetIndexContentHash(index[..^1]).Should().BeNull("the checksum is truncated");
        IndexWatcher.TestAccessor.GetIndexContentHash(CreateIndex(3, statData: 1, new Entry("a.txt", ExtendedFlags: 0x2000))[..(12 + 62)])
            .Should().BeNull("the extended flags are truncated");
    }

    [Test]
    public void GetIndexContentHash_should_return_null_for_an_invalid_checksum()
    {
        byte[] index = CreateIndex(2, statData: 1, new Entry("a.txt"));
        byte[] withCorruptedChecksum = (byte[])index.Clone();
        withCorruptedChecksum[^1] ^= 0xFF;

        IndexWatcher.TestAccessor.GetIndexContentHash(index).Should().NotBeNull();
        IndexWatcher.TestAccessor.GetIndexContentHash(withCorruptedChecksum).Should().BeNull("the checksum does not match the content");
    }

    [Test]
    public void GetIndexContentHash_should_skip_extensions_but_not_hash_a_split_index()
    {
        byte[] index = CreateIndex(2, statData: 1, new Entry("a.txt"));
        byte[] withUntrackedCache = InsertExtension(index, "UNTR"u8.ToArray(), [1, 2, 3]);

        IndexWatcher.TestAccessor.GetIndexContentHash(withUntrackedCache).Should().Equal(IndexWatcher.TestAccessor.GetIndexContentHash(index));
        IndexWatcher.TestAccessor.GetIndexContentHash(InsertExtension(index, "link"u8.ToArray(), [])).Should().BeNull("deleted and replaced entries of the shared index are not hashed");
        IndexWatcher.TestAccessor.GetIndexContentHash(InsertExtension(index, "UNTR"u8.ToArray(), [1, 2, 3])[..^22]).Should().BeNull("the extension is truncated");

        static byte[] InsertExtension(byte[] index, byte[] signature, byte[] data)
        {
            byte[] size = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(size, (uint)data.Length);
            byte[] content = [.. index[..^20], .. signature, .. size, .. data];
            using IncrementalHash checksum = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
            checksum.AppendData(content);
            return [.. content, .. checksum.GetHashAndReset()];
        }
    }

    [Test]
    public void Notification_of_the_index_refresh_should_not_mark_the_index_as_changed()
    {
        SelfInducedIndexRewrite setup = CreateSelfInducedIndexRewrite();
        using (setup.Repo)
        using (setup.IndexWatcher)
        {
            setup.Accessor.RaiseIndexChanged();
            setup.Accessor.IndexChanged.Should().BeFalse();

            File.WriteAllText(setup.File, "changed");
            setup.Repo.Module.GitExecutable.GetOutput("add a.txt");

            setup.Accessor.RaiseIndexChanged();
            setup.Accessor.IndexChanged.Should().BeTrue();
        }
    }

    [Test]
    public void Notification_should_re_capture_the_baseline_if_the_initial_capture_raced_with_a_rewrite()
    {
        SelfInducedIndexRewrite setup = CreateSelfInducedIndexRewrite();
        using (setup.Repo)
        using (setup.IndexWatcher)
        {
            // Simulate the initial capture racing with the index rewrite: the baseline is null
            setup.Accessor.SimulateCaptureRacedWithRewrite();

            setup.Accessor.RaiseIndexChanged();
            setup.Accessor.IndexChanged.Should().BeFalse("the re-captured baseline matches the index, so the self-induced rewrite is ignored");
        }
    }

    private sealed record SelfInducedIndexRewrite(GitModuleTestHelper Repo, string File, IndexWatcher IndexWatcher, IndexWatcher.TestAccessor Accessor);

    private static SelfInducedIndexRewrite CreateSelfInducedIndexRewrite()
    {
        GitModuleTestHelper repo = new();
        string file = repo.CreateRepoFile("a.txt", "a");
        repo.Module.GitExecutable.GetOutput("add a.txt");
        repo.Module.GitExecutable.GetOutput("commit -m initial");
        IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
        uiCommandsSource.UICommands.Module.Returns(repo.Module);
        IndexWatcher indexWatcher = new(uiCommandsSource);
        IndexWatcher.TestAccessor accessor = indexWatcher.GetTestAccessor();
        string indexPath = Path.Join(repo.Module.WorkingDirGitDir, "index");

        indexWatcher.Reset();
        accessor.WaitForIndexContentHashAtReset(TimeSpan.FromSeconds(10)).Should().BeTrue();

        // Like after "git stash": the file times do not match the index, so the next status refreshes and rewrites the index
        DateTime indexTimeBefore = File.GetLastWriteTimeUtc(indexPath);
        File.SetLastWriteTimeUtc(file, DateTime.UtcNow.AddHours(1));
        repo.Module.GitExecutable.GetOutput("update-index --refresh");
        File.GetLastWriteTimeUtc(indexPath).Should().NotBe(indexTimeBefore, "the index refresh must have rewritten the index");

        return new(repo, file, indexWatcher, accessor);
    }

    private static byte[] CreateIndex(uint version, uint statData, params Entry[] entries)
    {
        const int statDataLength = 40;
        const int objectIdLength = 20;
        const int modeOffset = 24;

        using MemoryStream stream = new();
        stream.Write("DIRC"u8);
        WriteUInt32(version);
        WriteUInt32((uint)entries.Length);
        foreach (Entry entry in entries)
        {
            long start = stream.Position;
            for (int offset = 0; offset < statDataLength; offset += sizeof(uint))
            {
                WriteUInt32(offset == modeOffset ? entry.Mode : statData);
            }

            stream.Write(Enumerable.Repeat(entry.ObjectId, objectIdLength).ToArray());
            byte[] path = Encoding.UTF8.GetBytes(entry.Path);
            WriteUInt16((ushort)((entry.ExtendedFlags != 0 ? 0x4000 : 0) | (entry.Stage << 12) | path.Length));
            if (entry.ExtendedFlags != 0)
            {
                WriteUInt16(entry.ExtendedFlags);
            }

            if (version == 4)
            {
                // No prefix to strip from the previous path
                stream.WriteByte(0);
                stream.Write(path);
                stream.WriteByte(0);
            }
            else
            {
                stream.Write(path);
                do
                {
                    stream.WriteByte(0);
                }
                while ((stream.Position - start) % 8 != 0);
            }
        }

        // Compute the trailing SHA-1 checksum of the preceding bytes, as git does
        byte[] content = stream.ToArray();
        using IncrementalHash checksum = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        checksum.AppendData(content);
        stream.Write(checksum.GetHashAndReset());

        return stream.ToArray();

        void WriteUInt16(ushort value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
            stream.Write(buffer);
        }

        void WriteUInt32(uint value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
            stream.Write(buffer);
        }
    }

    private sealed record Entry(string Path, byte ObjectId = 1, uint Mode = _regularFileMode, int Stage = 0, ushort ExtendedFlags = 0);
}
