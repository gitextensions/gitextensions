using CommonTestUtils;
using CommonTestUtils.MEF;
using FluentAssertions;
using FluentAssertions.Execution;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using GitUITests;
using Microsoft.VisualStudio.Composition;
using NSubstitute;
using static GitUI.CommandsDialogs.FormBrowse;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class SplitterPersistenceTests
    {
        private IWindowPositionManager _windowPositionManager;
        private MemorySettings _settings;

        // Created once for the fixture
        private TestComposition _composition;
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            _composition = TestComposition.Empty
                .AddParts(typeof(MockLinkFactory))
                .AddParts(typeof(MockWindowsJumpListManager))
                .AddParts(typeof(MockRepositoryDescriptionProvider))
                .AddParts(typeof(MockAppTitleGenerator));
            ExportProvider mefExportProvider = _composition.ExportProviderFactory.CreateExportProvider();
            ManagedExtensibility.SetTestExportProvider(mefExportProvider);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _settings = new();

            // Stop loading custom diff tools
            AppSettings.ShowAvailableDiffTools = false;

            // We don't want avatars during tests, otherwise we will be attempting to download them from gravatar....
            AppSettings.ShowAuthorAvatarColumn = false;

            _windowPositionManager = Substitute.For<IWindowPositionManager>();

            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [TearDown]
        public void TearDown()
        {
            // Since we set AppSettings, we need to delete the backing file so that any subsequent tests won't load
            // outdated settings.
            File.Delete(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath);
        }

        [Test]
        public async Task SplitterPositionsShouldBeDefault()
        {
            AppSettings.CommitInfoPosition.Should().Be(CommitInfoPosition.BelowList);

            await RunFormTestAsync(
                async form =>
                {
                    FormBrowse.TestAccessor ta = form.GetTestAccessor();

                    await WaitForRevisionsToBeLoadedAsync(ta.RevisionGrid);
                });
        }

        [TestCase(CommitInfoPosition.BelowList)]
        [TestCase(CommitInfoPosition.LeftwardFromList)]
        [TestCase(CommitInfoPosition.RightwardFromList)]
        public async Task SplitterPositionsShouldBeSaved(CommitInfoPosition commitInfoPosition)
        {
            AppSettings.CommitInfoPosition = commitInfoPosition;

            int leftPanelWidth = 300;
            int revisionGridHeight = 125;
            int commitInfoWidth = 124;
            int fileTreeWidth = 127;
            int diffListWidth = 126;
            int expectedCommitInfoWidth;

            switch (commitInfoPosition)
            {
                case CommitInfoPosition.BelowList:
                    // Since the Commit Info is on the bottom, the size is largely immaterial
                    // because RevisionsSplitContainer_Panel2Collapsed=true
                    expectedCommitInfoWidth = commitInfoWidth + /* splitter width */6;
                    break;

                case CommitInfoPosition.LeftwardFromList:
                    commitInfoWidth = 300;

                    // Since the Commit Info is on the left, the expected width is the same
                    expectedCommitInfoWidth = commitInfoWidth;
                    break;

                case CommitInfoPosition.RightwardFromList:
                    commitInfoWidth = 350;

                    // Since the Commit Info is on the right, the expected width is the same
                    expectedCommitInfoWidth = commitInfoWidth - /* some magic number... no idea */38;
                    break;

                default:
                    throw new NotSupportedException();
            }

            SplitterInfo splitterInfoMainSplitContainer = new(_settings, "MainSplitContainer")
            {
                Size = 911,
                Distance = leftPanelWidth
            };

            SplitterInfo splitterInfoRightSplitContainer = new(_settings, "RightSplitContainer")
            {
                Size = 507,
                Distance = revisionGridHeight
            };

            SplitterInfo splitterInfoRevisionsSplitContainer = new(_settings, "RevisionsSplitContainer")
            {
                Size = 645,
                Distance = commitInfoWidth
            };

            SplitterInfo splitterInfoFileTreeSplitContainer = new(_settings, "FileTreeSplitContainer")
            {
                Size = 643,
                Distance = fileTreeWidth
            };

            SplitterInfo splitterInfoDiffSplitContainer = new(_settings, "DiffSplitContainer")
            {
                Size = 643,
                Distance = diffListWidth
            };

            await RunFormTestAsync(
                async form =>
                {
                    FormBrowse.TestAccessor ta = form.GetTestAccessor();

                    await WaitForRevisionsToBeLoadedAsync(ta.RevisionGrid);
                });

            using (new AssertionScope())
            {
                splitterInfoMainSplitContainer.Distance.Should().Be(leftPanelWidth);
                splitterInfoRightSplitContainer.Distance.Should().Be(revisionGridHeight);
                splitterInfoRevisionsSplitContainer.Distance.Should().Be(expectedCommitInfoWidth);
                splitterInfoFileTreeSplitContainer.Distance.Should().Be(fileTreeWidth);
                splitterInfoDiffSplitContainer.Distance.Should().Be(diffListWidth);
            }
        }

        private async Task RunFormTestAsync(Func<FormBrowse, Task> testDriverAsync)
        {
            Dictionary<string, string> splitters = new();

            UITest.RunForm(
                showForm: () =>
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    FormBrowse form = new(_commands, new BrowseArguments(), _settings);
#pragma warning restore CS0618 // Type or member is obsolete

                    form.Load += (_, _) =>
                    {
                        // Collect the persisted splitter settings in a dictionary so that we can perform snapshot validation
                        // instead of manually validating each individual setting.
                        foreach (SplitterManager.SplitterData splitterData in form.GetTestAccessor().SplitterManager.GetTestAccessor().Splitters)
                        {
                            splitters[splitterData.SizeSettingsKey] = null;
                            splitters[splitterData.DpiSettingsKey] = null;
                            splitters[splitterData.DistanceSettingsKey] = null;
                            splitters[splitterData.FontSizeSettingsKey] = null;
                            splitters[splitterData.Panel1CollapsedSettingsKey] = null;
                            splitters[splitterData.Panel2CollapsedSettingsKey] = null;
                        }
                    };

                    var test = form.GetGitExtensionsFormTestAccessor();
                    test.WindowPositionManager = _windowPositionManager;

                    if (Application.MessageLoop)
                    {
                        form.Show(owner: null);
                    }
                    else
                    {
                        Application.Run(form);
                    }
                },
                testDriverAsync);

            // Snapshot validation
            foreach (string key in splitters.Keys)
            {
                if (key.EndsWith("Collapsed"))
                {
                    splitters[key] = _settings.GetBool(key).ToString();
                }
                else
                {
                    splitters[key] = _settings.GetInt(key, -1).ToString();
                }
            }

            await Verifier.Verify(splitters);
        }

        private static async Task WaitForRevisionsToBeLoadedAsync(RevisionGridControl revisionGridControl)
        {
            UITest.ProcessUntil("Loading Revisions", () => revisionGridControl.GetTestAccessor().IsDataLoadComplete);
            try
            {
                await AsyncTestHelper.JoinPendingOperationsAsync(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // ignore the timeout and continue
            }
        }

        private record SplitterInfo
        {
            private readonly MemorySettings _settings;
            private readonly string _settingName;

            public SplitterInfo(MemorySettings settings, string settingName)
            {
                _settings = settings;
                _settingName = settingName;
            }

            // These are a copy from SplitterManager.SplitterData

            public string SizeSettingsKey => _settingName + "_Size";
            public string DpiSettingsKey => _settingName + "_Dpi";
            public string DistanceSettingsKey => _settingName + "_Distance";
            public string FontSizeSettingsKey => _settingName + "_FontSize";
            public string Panel1CollapsedSettingsKey => _settingName + "_Panel1Collapsed";
            public string Panel2CollapsedSettingsKey => _settingName + "_Panel2Collapsed";

            public int Distance
            {
                get => _settings.GetInt(DistanceSettingsKey) ?? throw new InvalidOperationException();
                set => _settings.SetInt(DistanceSettingsKey, value);
            }

            public int Size
            {
                get => _settings.GetInt(SizeSettingsKey) ?? throw new InvalidOperationException();
                set => _settings.SetInt(SizeSettingsKey, value);
            }
        }
    }
}
