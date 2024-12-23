﻿using System.CodeDom.Compiler;
using System.Reflection;
using FluentAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitCommandsTests.Settings
{
    [TestFixture]
    internal sealed class AppSettingsTests
    {
        private const string SettingsFileContent = @"<?xml version=""1.0"" encoding=""utf-8""?><dictionary />";

        [TestCase(null, "https://git-extensions-documentation.readthedocs.org/en/main/")]
        [TestCase("", "https://git-extensions-documentation.readthedocs.org/en/main/")]
        [TestCase("\t", "https://git-extensions-documentation.readthedocs.org/en/main/")]
        [TestCase("33.33", "https://git-extensions-documentation.readthedocs.org/en/main/")]
        [TestCase("33.33.33", "https://git-extensions-documentation.readthedocs.org/en/main/")]
        [TestCase("33.33.33.33", "https://git-extensions-documentation.readthedocs.org/en/main/")]
        [TestCase("a", "https://git-extensions-documentation.readthedocs.org/en/main/")]
        [TestCase("5", "https://git-extensions-documentation.readthedocs.org/en/main/")]
        [TestCase("v4.5", "https://git-extensions-documentation.readthedocs.org/en/main/")]
        [TestCase("4.5", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
        [TestCase("4.5.", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
        [TestCase("4.5.0", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
        [TestCase("4.5.2", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
        [TestCase("4.5.2.1", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
        [TestCase("4.5.2x", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
        [TestCase("40.501.123", "https://git-extensions-documentation.readthedocs.org/en/release-40.501/")]
        public void SetDocumentationBaseUrl_should_currectly_append_version(string version, string expected)
        {
            AppSettings.GetTestAccessor().ResetDocumentationBaseUrl();

            AppSettings.SetDocumentationBaseUrl(version);
            AppSettings.DocumentationBaseUrl.Should().Be(expected);
        }

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void Should_return_default_value(PropertyInfo property, object value, object defaultValue, bool isISetting)
        {
            // Arrange
            object root = null;

            if (isISetting)
            {
                root = property.GetValue(null);

                property = property.PropertyType
                    .GetProperty(nameof(ISetting<string>.Value));
            }

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");
            tempFiles.AddFile(filePath + ".backup", keepFile: false);

            File.WriteAllText(filePath, SettingsFileContent);

            using GitExtSettingsCache cache = GitExtSettingsCache.Create(filePath);
            DistributedSettings container = new(lowerPriority: null, cache, SettingLevel.Unknown);
            object storedValue = null;

            // Act
            AppSettings.UsingContainer(container, () =>
            {
                storedValue = property.GetValue(root);
            });

            // Assert
            Assert.That(storedValue, Is.EqualTo(defaultValue));
        }

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void Should_save_value(PropertyInfo property, object value, object defaultValue, bool isISetting)
        {
            // Arrange
            object root = null;

            if (isISetting)
            {
                root = property.GetValue(null);

                property = property.PropertyType
                    .GetProperty(nameof(ISetting<string>.Value));
            }

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");
            tempFiles.AddFile(filePath + ".backup", keepFile: false);

            File.WriteAllText(filePath, SettingsFileContent);

            using GitExtSettingsCache cache = GitExtSettingsCache.Create(filePath);
            DistributedSettings container = new(lowerPriority: null, cache, SettingLevel.Unknown);
            object storedValue = null;

            // Act
            AppSettings.UsingContainer(container, () =>
            {
                property.SetValue(root, value);

                storedValue = property.GetValue(root);
            });

            // Assert
            if (Type.GetTypeCode(property.PropertyType) == TypeCode.String)
            {
                if (isISetting)
                {
                    Assert.That(storedValue, Is.EqualTo(value ?? string.Empty));
                }
                else
                {
                    Assert.That(storedValue, Is.EqualTo(value ?? defaultValue));
                }
            }
            else if (Type.GetTypeCode(property.PropertyType) == TypeCode.DateTime)
            {
                // We keep only the date
                Assert.That(storedValue, Is.EqualTo(((DateTime)value).Date));
            }
            else
            {
                Assert.That(storedValue, Is.EqualTo(value));
            }
        }

        #region Test Cases

        private static IEnumerable<object[]> TestCases()
        {
            foreach ((PropertyInfo property, object defaultValue, bool isNullable, bool isISetting) in PropertyInfos())
            {
                if (isNullable)
                {
                    yield return new object[] { property, null, defaultValue, isISetting };
                }

                Type? propertyType = property.PropertyType;
                if (isISetting)
                {
                    propertyType = propertyType.GetProperty(nameof(ISetting<string>.Value))?.PropertyType;
                }

                propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                Validates.NotNull(propertyType);

                foreach (object value in Values())
                {
                    Type valueType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
                    if (valueType == propertyType)
                    {
                        yield return new object[] { property, value, defaultValue, isISetting };
                    }
                }
            }

            static IEnumerable<(PropertyInfo property, object defaultValue, bool isNullable, bool isISetting)> PropertyInfos()
            {
                Dictionary<string, PropertyInfo> properties = typeof(AppSettings).GetProperties()
                    .ToDictionary(x => x.Name, x => x);

                const bool isISetting = true;
                const bool isNoISetting = false;
                const bool isNotNullable = false;

                yield return (properties[nameof(AppSettings.TelemetryEnabled)], null, true, false);
                yield return (properties[nameof(AppSettings.AutoNormaliseBranchName)], true, false, false);
                yield return (properties[nameof(AppSettings.RememberAmendCommitState)], true, false, false);
                yield return (properties[nameof(AppSettings.StashKeepIndex)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmStashDrop)], false, false, false);
                yield return (properties[nameof(AppSettings.ApplyPatchIgnoreWhitespace)], false, false, false);
                yield return (properties[nameof(AppSettings.ApplyPatchSignOff)], true, false, false);
                yield return (properties[nameof(AppSettings.UseHistogramDiffAlgorithm)], false, false, false);
                yield return (properties[nameof(AppSettings.UseGitColoring)], true, false, true);
                yield return (properties[nameof(AppSettings.ReverseGitColoring)], true, false, true);
                yield return (properties[nameof(AppSettings.ShowErrorsWhenStagingFiles)], true, false, false);
                yield return (properties[nameof(AppSettings.EnsureCommitMessageSecondLineEmpty)], true, false, false);
                yield return (properties[nameof(AppSettings.LastCommitMessage)], string.Empty, true, false);
                yield return (properties[nameof(AppSettings.CommitDialogNumberOfPreviousMessages)], 6, false, false);
                yield return (properties[nameof(AppSettings.CommitDialogShowOnlyMyMessages)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowCommitAndPush)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowResetWorkTreeChanges)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowResetAllChanges)], true, false, false);

                yield return (properties[nameof(AppSettings.ShowConEmuTab)], true, false, true);
                yield return (properties[nameof(AppSettings.ConEmuStyle)], "<Solarized Light>", true, true);
                yield return (properties[nameof(AppSettings.ConEmuTerminal)], "bash", true, true);
                yield return (properties[nameof(AppSettings.OutputHistoryDepth)], 20, isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.OutputHistoryPanelVisible)], false, isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.ShowOutputHistoryAsTab)], true, isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.UseBrowseForFileHistory)], true, false, true);
                yield return (properties[nameof(AppSettings.UseDiffViewerForBlame)], false, false, true);
                yield return (properties[nameof(AppSettings.ShowGpgInformation)], true, false, true);

                yield return (properties[nameof(AppSettings.ShowSplitViewLayout)], true, false, false);
                yield return (properties[nameof(AppSettings.ProvideAutocompletion)], true, false, false);
                yield return (properties[nameof(AppSettings.TruncatePathMethod)], TruncatePathMethod.None, false, false);
                yield return (properties[nameof(AppSettings.ShowGitStatusInBrowseToolbar)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowGitStatusForArtificialCommits)], true, false, false);
                yield return (properties[nameof(AppSettings.RevisionSortOrder)], RevisionSortOrder.GitDefault, false, true);
                yield return (properties[nameof(AppSettings.CommitInfoShowContainedInBranchesLocal)], true, false, false);
                yield return (properties[nameof(AppSettings.CheckForUncommittedChangesInCheckoutBranch)], true, false, false);
                yield return (properties[nameof(AppSettings.AlwaysShowCheckoutBranchDlg)], false, false, false);
                yield return (properties[nameof(AppSettings.CommitAndPushForcedWhenAmend)], false, false, false);
                yield return (properties[nameof(AppSettings.CommitInfoShowContainedInBranchesRemote)], false, false, false);
                yield return (properties[nameof(AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal)], false, false, false);
                yield return (properties[nameof(AppSettings.CommitInfoShowContainedInTags)], true, false, false);
                yield return (properties[nameof(AppSettings.CommitInfoShowTagThisCommitDerivesFrom)], true, false, false);
                yield return (properties[nameof(AppSettings.AvatarImageCacheDays)], 13, false, false);
                yield return (properties[nameof(AppSettings.ShowAuthorAvatarInCommitInfo)], true, false, false);
                yield return (properties[nameof(AppSettings.AvatarProvider)], AvatarProvider.None, false, false);
                yield return (properties[nameof(AppSettings.Translation)], string.Empty, true, false);
                yield return (properties[nameof(AppSettings.UserProfileHomeDir)], false, false, false);
                yield return (properties[nameof(AppSettings.CustomHomeDir)], string.Empty, true, false);
                yield return (properties[nameof(AppSettings.EnableAutoScale)], true, false, false);
                yield return (properties[nameof(AppSettings.CloseCommitDialogAfterCommit)], true, false, false);
                yield return (properties[nameof(AppSettings.CloseCommitDialogAfterLastCommit)], true, false, false);
                yield return (properties[nameof(AppSettings.RefreshArtificialCommitOnApplicationActivated)], false, false, false);
                yield return (properties[nameof(AppSettings.StageInSuperprojectAfterCommit)], true, false, false);
                yield return (properties[nameof(AppSettings.FollowRenamesInFileHistory)], true, false, false);
                yield return (properties[nameof(AppSettings.FollowRenamesInFileHistoryExactOnly)], false, false, false);
                yield return (properties[nameof(AppSettings.FullHistoryInFileHistory)], false, false, false);
                yield return (properties[nameof(AppSettings.SimplifyMergesInFileHistory)], false, false, false);
                yield return (properties[nameof(AppSettings.LoadFileHistoryOnShow)], true, false, false);
                yield return (properties[nameof(AppSettings.LoadBlameOnShow)], true, false, false);
                yield return (properties[nameof(AppSettings.DetectCopyInFileOnBlame)], false, false, false);
                yield return (properties[nameof(AppSettings.DetectCopyInAllOnBlame)], false, false, false);
                yield return (properties[nameof(AppSettings.IgnoreWhitespaceOnBlame)], true, false, false);
                yield return (properties[nameof(AppSettings.OpenSubmoduleDiffInSeparateWindow)], false, false, false);
                yield return (properties[nameof(AppSettings.RevisionGraphShowArtificialCommits)], true, false, false);
                yield return (properties[nameof(AppSettings.RevisionGraphDrawAlternateBackColor)], true, false, false);
                yield return (properties[nameof(AppSettings.RevisionGraphDrawNonRelativesGray)], true, false, false);
                yield return (properties[nameof(AppSettings.RevisionGraphDrawNonRelativesTextGray)], false, false, false);
                yield return (properties[nameof(AppSettings.DefaultPullAction)], GitPullAction.Merge, false, false);
                yield return (properties[nameof(AppSettings.FormPullAction)], GitPullAction.Merge, false, false);
                yield return (properties[nameof(AppSettings.SmtpServer)], "smtp.gmail.com", true, false);
                yield return (properties[nameof(AppSettings.SmtpPort)], 465, false, false);
                yield return (properties[nameof(AppSettings.SmtpUseSsl)], true, false, false);
                yield return (properties[nameof(AppSettings.AutoStash)], false, false, false);
                yield return (properties[nameof(AppSettings.RebaseAutoStash)], false, false, false);
                yield return (properties[nameof(AppSettings.CheckoutBranchAction)], LocalChangesAction.DontChange, false, false);
                yield return (properties[nameof(AppSettings.UseDefaultCheckoutBranchAction)], false, false, false);
                yield return (properties[nameof(AppSettings.DontShowHelpImages)], false, false, false);
                yield return (properties[nameof(AppSettings.AlwaysShowAdvOpt)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmAmend)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmCommitIfNoBranch)], false, false, false);
                yield return (properties[nameof(AppSettings.ConfirmBranchCheckout)], false, false, false);
                yield return (properties[nameof(AppSettings.AutoPopStashAfterPull)], null, true, false);
                yield return (properties[nameof(AppSettings.AutoPopStashAfterCheckoutBranch)], null, true, false);
                yield return (properties[nameof(AppSettings.AutoPullOnPushRejectedAction)], null, true, false);
                yield return (properties[nameof(AppSettings.DontConfirmPushNewBranch)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmAddTrackingRef)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmCommitAfterConflictsResolved)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmSecondAbortConfirmation)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmRebase)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmResolveConflicts)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmUndoLastCommit)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmFetchAndPruneAll)], false, false, false);
                yield return (properties[nameof(AppSettings.DontConfirmSwitchWorktree)], false, false, false);
                yield return (properties[nameof(AppSettings.IncludeUntrackedFilesInAutoStash)], false, false, false);
                yield return (properties[nameof(AppSettings.IncludeUntrackedFilesInManualStash)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowRemoteBranches)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowReflogReferences)], false, false, true);
                yield return (properties[nameof(AppSettings.ShowSuperprojectTags)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowSuperprojectBranches)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowSuperprojectRemoteBranches)], false, false, false);
                yield return (properties[nameof(AppSettings.UpdateSubmodulesOnCheckout)], null, true, false);
                yield return (properties[nameof(AppSettings.DontConfirmUpdateSubmodulesOnCheckout)], null, true, false);
                yield return (properties[nameof(AppSettings.ShowGitCommandLine)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowStashCount)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowAheadBehindData)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowSubmoduleStatus)], false, false, false);
                yield return (properties[nameof(AppSettings.RelativeDate)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowGitNotes)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowAnnotatedTagsMessages)], true, false, false);
                yield return (properties[nameof(AppSettings.HideMergeCommits)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowTags)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowRevisionGridGraphColumn)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowAuthorAvatarColumn)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowAuthorNameColumn)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowDateColumn)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowObjectIdColumn)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowBuildStatusIconColumn)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowBuildStatusTextColumn)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowAuthorDate)], true, false, false);
                yield return (properties[nameof(AppSettings.CloseProcessDialog)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowCurrentBranchOnly)], false, false, true);
                yield return (properties[nameof(AppSettings.ShowSimplifyByDecoration)], false, false, false);
                yield return (properties[nameof(AppSettings.BranchFilterEnabled)], false, false, true);
                yield return (properties[nameof(AppSettings.ShowOnlyFirstParent)], false, false, false);
                yield return (properties[nameof(AppSettings.CommitDialogSelectionFilter)], false, false, false);
                yield return (properties[nameof(AppSettings.DefaultCloneDestinationPath)], string.Empty, true, false);
                yield return (properties[nameof(AppSettings.RevisionGridQuickSearchTimeout)], 4000, false, false);
                yield return (properties[nameof(AppSettings.MaxRevisionGraphCommits)], 100000, false, false);
                yield return (properties[nameof(AppSettings.ShowDiffForAllParents)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowFindInCommitFilesGitGrep)], false, isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.ShowAvailableDiffTools)], true, isNotNullable, isNoISetting);
                yield return (properties[nameof(AppSettings.DiffVerticalRulerPosition)], 0, isNotNullable, isNoISetting);
                yield return (properties[nameof(AppSettings.GitGrepUserArguments)], "", isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.GitGrepIgnoreCase)], false, isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.GitGrepMatchWholeWord)], false, isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.RecentWorkingDir)], null, true, false);
                yield return (properties[nameof(AppSettings.StartWithRecentWorkingDir)], false, false, false);
                yield return (properties[nameof(AppSettings.AutoStartPageant)], true, false, false);
                yield return (properties[nameof(AppSettings.MarkIllFormedLinesInCommitMsg)], true, false, false);
                yield return (properties[nameof(AppSettings.UseSystemVisualStyle)], true, false, false);
                yield return (properties[nameof(AppSettings.MulticolorBranches)], true, false, false);
                yield return (properties[nameof(AppSettings.HighlightAuthoredRevisions)], true, false, false);
                yield return (properties[nameof(AppSettings.FillRefLabels)], false, false, false);
                yield return (properties[nameof(AppSettings.MergeGraphLanesHavingCommonParent)], true, false, true);
                yield return (properties[nameof(AppSettings.RenderGraphWithDiagonals)], true, false, true);
                yield return (properties[nameof(AppSettings.StraightenGraphDiagonals)], true, false, true);
                yield return (properties[nameof(AppSettings.StraightenGraphSegmentsLimit)], 80, false, true);
                yield return (properties[nameof(AppSettings.LastFormatPatchDir)], string.Empty, true, false);
                yield return (properties[nameof(AppSettings.IgnoreWhitespaceKind)], IgnoreWhitespaceKind.None, false, true);
                yield return (properties[nameof(AppSettings.RememberIgnoreWhiteSpacePreference)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowNonPrintingChars)], false, false, true);
                yield return (properties[nameof(AppSettings.RememberShowNonPrintingCharsPreference)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowEntireFile)], false, false, true);
                yield return (properties[nameof(AppSettings.RememberShowEntireFilePreference)], false, false, false);
                yield return (properties[nameof(AppSettings.DiffDisplayAppearance)], DiffDisplayAppearance.Patch, isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.RememberDiffDisplayAppearance)], false, isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.RememberNumberOfContextLines)], false, false, false);
                yield return (properties[nameof(AppSettings.ShowSyntaxHighlightingInDiff)], true, false, true);
                yield return (properties[nameof(AppSettings.RememberShowSyntaxHighlightingInDiff)], true, false, false);
                yield return (properties[nameof(AppSettings.ShowRepoCurrentBranch)], true, false, false);
                yield return (properties[nameof(AppSettings.OwnScripts)], string.Empty, true, false);
                yield return (properties[nameof(AppSettings.RecursiveSubmodules)], 1, false, false);
                yield return (properties[nameof(AppSettings.ShorteningRecentRepoPathStrategy)], ShorteningRecentRepoPathStrategy.None, false, false);
                yield return (properties[nameof(AppSettings.MaxTopRepositories)], 0, false, false);
                yield return (properties[nameof(AppSettings.RecentRepositoriesHistorySize)], 30, false, false);
                yield return (properties[nameof(AppSettings.HideTopRepositoriesFromRecentList)], false, isNotNullable, isISetting);
                yield return (properties[nameof(AppSettings.RecentReposComboMinWidth)], 0, false, false);
                yield return (properties[nameof(AppSettings.SerializedHotkeys)], null, true, false);
                yield return (properties[nameof(AppSettings.SortTopRepos)], false, false, false);
                yield return (properties[nameof(AppSettings.SortRecentRepos)], false, false, false);
                yield return (properties[nameof(AppSettings.DontCommitMerge)], false, false, false);
                yield return (properties[nameof(AppSettings.CommitValidationMaxCntCharsFirstLine)], 0, false, false);
                yield return (properties[nameof(AppSettings.CommitValidationMaxCntCharsPerLine)], 0, false, false);
                yield return (properties[nameof(AppSettings.CommitValidationSecondLineMustBeEmpty)], false, false, false);
                yield return (properties[nameof(AppSettings.CommitValidationIndentAfterFirstLine)], true, false, false);
                yield return (properties[nameof(AppSettings.CommitValidationAutoWrap)], true, false, false);
                yield return (properties[nameof(AppSettings.CommitValidationRegEx)], string.Empty, true, false);
                yield return (properties[nameof(AppSettings.CommitTemplates)], string.Empty, true, false);
                yield return (properties[nameof(AppSettings.CreateLocalBranchForRemote)], false, false, false);
                yield return (properties[nameof(AppSettings.UseFormCommitMessage)], true, false, false);
                yield return (properties[nameof(AppSettings.CommitAutomaticallyAfterCherryPick)], false, false, false);
                yield return (properties[nameof(AppSettings.AddCommitReferenceToCherryPick)], false, false, false);
                yield return (properties[nameof(AppSettings.LastUpdateCheck)], default(DateTime), false, false);
                yield return (properties[nameof(AppSettings.CheckForUpdates)], true, false, false);
                yield return (properties[nameof(AppSettings.CheckForReleaseCandidates)], false, false, false);
                yield return (properties[nameof(AppSettings.OmitUninterestingDiff)], false, false, false);
                yield return (properties[nameof(AppSettings.UseConsoleEmulatorForCommands)], true, false, false);
                yield return (properties[nameof(AppSettings.RefsSortBy)], GitRefsSortBy.Default, false, false);
                yield return (properties[nameof(AppSettings.RefsSortOrder)], GitRefsSortOrder.Descending, false, false);
                yield return (properties[nameof(AppSettings.DiffListSorting)], DiffListSortType.FilePath, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeShowBranches)], true, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeShowRemotes)], true, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeShowTags)], true, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeShowStashes)], true, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeShowSubmodules)], true, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeBranchesIndex)], 0, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeRemotesIndex)], 1, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeTagsIndex)], 2, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeSubmodulesIndex)], 3, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeStashesIndex)], 4, false, false);
                yield return (properties[nameof(AppSettings.RepoObjectsTreeFavoritesIndex)], 5, false, false);
                yield return (properties[nameof(AppSettings.BlameDisplayAuthorFirst)], false, false, false);
                yield return (properties[nameof(AppSettings.BlameShowAuthor)], true, false, false);
                yield return (properties[nameof(AppSettings.BlameShowAuthorDate)], true, false, false);
                yield return (properties[nameof(AppSettings.BlameShowAuthorTime)], true, false, false);
                yield return (properties[nameof(AppSettings.BlameShowLineNumbers)], false, false, false);
                yield return (properties[nameof(AppSettings.BlameShowOriginalFilePath)], true, false, false);
                yield return (properties[nameof(AppSettings.BlameShowAuthorAvatar)], true, false, false);
                yield return (properties[nameof(AppSettings.AutomaticContinuousScroll)], false, false, false);
                yield return (properties[nameof(AppSettings.AutomaticContinuousScrollDelay)], 600, false, false);
                yield return (properties[nameof(AppSettings.IsEditorSettingsMigrated)], false, isNotNullable, isISetting);
            }

            static IEnumerable<object> Values()
            {
                yield return string.Empty;
                yield return " ";
                yield return "0";

                yield return false;
                yield return true;

                yield return char.MinValue;
                yield return char.MaxValue;
                yield return ' ';
                yield return '0';

                yield return byte.MinValue;
                yield return byte.MaxValue;

                yield return int.MinValue;
                yield return int.MaxValue;
                yield return 0;
                yield return 1;
                yield return -1;

                yield return float.MinValue;
                yield return float.MaxValue;
                yield return float.Epsilon;
                yield return float.PositiveInfinity;
                yield return float.NegativeInfinity;
                yield return float.NaN;
                yield return 0f;
                yield return 1f;
                yield return -1f;

                yield return double.MinValue;
                yield return double.MaxValue;
                yield return double.Epsilon;
                yield return double.PositiveInfinity;
                yield return double.NegativeInfinity;
                yield return double.NaN;
                yield return 0d;
                yield return 1d;
                yield return -1d;

                yield return decimal.MinValue;
                yield return decimal.MaxValue;
                yield return decimal.Zero;
                yield return decimal.One;
                yield return decimal.MinusOne;

                yield return DateTime.MinValue;
                yield return DateTime.MaxValue;
                yield return DateTime.Today;

                Type[] enumTypes = new Type[]
                {
                    typeof(TruncatePathMethod),
                    typeof(AvatarProvider),
                    typeof(GitPullAction),
                    typeof(LocalChangesAction),
                    typeof(IgnoreWhitespaceKind),
                    typeof(ShorteningRecentRepoPathStrategy),
                    typeof(GitRefsSortBy),
                    typeof(GitRefsSortOrder),
                    typeof(DiffListSortType),
                    typeof(RevisionSortOrder),
                };

                foreach (Type enumType in enumTypes)
                {
                    foreach (object enumValue in Enum.GetValues(enumType))
                    {
                        yield return enumValue;
                    }
                }
            }
        }

        #endregion Test Cases
    }
}
