using System.ComponentModel;
using System.Reflection;
using System.Text;
using CommonTestUtils;
using GitCommands;
using GitCommands.Settings;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public sealed class ExecutableExtensionsTests
    {
        private MockExecutable _executable;
        private Executable _gitExecutable;
        private string _appPath;

        [SetUp]
        public void SetUp()
        {
            _executable = new MockExecutable();

            // Work around: When running unittest, Application.UserAppDataPath always points to
            // %APPDATA%Roaming\Microsoft Corporation\Microsoft.TestHost.x86
            // We need to correct it to %APPDATA%\GitExtensions\GitExtensions for v3 at least
            var userAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingPath = Path.Combine(userAppDataPath, "GitExtensions\\GitExtensions\\GitExtensions.settings");
            DistributedSettings settingContainer = new(lowerPriority: null, GitExtSettingsCache.FromCache(settingPath), SettingLevel.Unknown);
            _appPath = settingContainer.GetString("gitcommand", "git.exe");

            // Execute process in GitExtension working directory, so that git will return success exit-code
            // git always return non-zero exit code when run git reset outside of git repository
            // NUnit working directory always default to MS test host
            var workingDir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ExecutableExtensionsTests)).Location);
            _gitExecutable = new Executable(_appPath, workingDir);
        }

        [TearDown]
        public void TearDown()
        {
            _executable.Verify();
        }

        [Test]
        public void GetOutput_with_cache_hit()
        {
            const string arguments = "abc";

            CommandCache cache = new();

            cache.Add(
                arguments,
                output: GitModule.SystemEncoding.GetBytes("Hello"),
                error: GitModule.SystemEncoding.GetBytes("World!"));

            var output = _executable.GetOutput(arguments, cache: cache);

            Assert.AreEqual($"Hello{Environment.NewLine}World!", output);

            // Cache should still have a single item
            Assert.AreEqual(1, cache.GetCachedCommands().Count);
        }

        [Test]
        public void GetOutput_with_cache_miss()
        {
            const string arguments = "abc";
            const string commandOutput = "Hello World!";

            // Empty cache
            CommandCache cache = new();

            using (_executable.StageOutput(arguments, commandOutput))
            {
                var output = _executable.GetOutput(arguments, cache: cache);

                Assert.AreEqual(commandOutput, output);
            }

            // Validate data stored in cache afterwards
            Assert.AreEqual(1, cache.GetCachedCommands().Count);
            Assert.IsTrue(cache.TryGet(arguments, out var outputBytes, out var errorBytes));
            Assert.AreEqual(GitModule.SystemEncoding.GetBytes(commandOutput), outputBytes);
            Assert.IsEmpty(errorBytes);
        }

        // Process argument upper bound is actually (short.MaxValue - 1)
        [TestCase(32766, 32766, 32767, 2, new int[] { 1, 1 })]
        [TestCase(32764, 1, 32767, 1, new int[] { 2 })]
        public void RunBatchCommand_can_handle_max_length_arguments(int arg1Len, int arg2Len,
            int maxLength, int argCount, int[] expectedProcessedCounts)
        {
            // 3: double quotes + ' '
            // 9: 'reset -- '
            // 1: ' ' added after second Add in ArgumentBuilder
            int appLength = _appPath.Length + 3;
            ArgumentBuilder builder = new() { "reset --" };
            int len = builder.ToString().Length;
            List<BatchArgumentItem> args = builder.BuildBatchArguments(new string[]
            {
                GenerateStringByLength(Math.Max(1, arg1Len - appLength - len - 1)),
                GenerateStringByLength(Math.Max(1, arg2Len - appLength - len - 1))
            }, appLength, maxLength);

            Assert.AreEqual(argCount, args.Count);

            // The reset command runs in the GE repo dir, so the result depends on workTree contents
            int index = 0;
            ExecutionResult? result = _gitExecutable.RunBatchCommand(args, (eventArgs) =>
            {
                Assert.IsTrue(eventArgs.ExecutionResult);
                Assert.AreEqual(expectedProcessedCounts[index], eventArgs.ProcessedCount);
                index++;
            });
        }

        [TestCase(32766 - 8, 32766 - 8, int.MaxValue)]
        [TestCase(32766 - 9, 1, int.MaxValue)]
        public void RunBatchCommand_throw_when_cmd_exceed_max_length(int arg1Len, int arg2Len,
            int maxLength)
        {
            var args = new ArgumentBuilder() { "reset --" }
                .BuildBatchArguments(new string[]
                {
                    GenerateStringByLength(Math.Max(1, arg1Len - _appPath.Length - 4)),
                    GenerateStringByLength(Math.Max(1, arg2Len - _appPath.Length - 4))
                }, _appPath.Length + 3, maxLength);

            var ex = Assert.Throws<ExternalOperationException>(() => _gitExecutable.RunBatchCommand(args));
            Assert.IsInstanceOf<Win32Exception>(ex.InnerException);
        }

        private static string GenerateStringByLength(int length)
        {
            StringBuilder sb = new(length);

            for (int i = 0; i < length; i++)
            {
                sb.Append((char)((i % 26) + 97));
            }

            return sb.ToString();
        }
    }
}
