using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands.Logging;
using GitCommands.Repository;
using System.Threading;

namespace GitCommands
{
    public static class Settings
    {
        //Constants
        public static readonly string GitExtensionsVersionString = "2.09";
        public static readonly int GitExtensionsVersionInt = 209;

        //semi-constants
        public static char PathSeparator = '\\';
        public static char PathSeparatorWrong = '/';




        static Settings()
        {
            if (!RunningOnWindows())
            {
                PathSeparator = '/';
                PathSeparatorWrong = '\\';
            }

            GitLog = new CommandLogger();
            ApplicationDataPath = Application.UserAppDataPath + Settings.PathSeparator.ToString();
        }

        private static bool? _showErrorsWhenStagingFiles;
        public static bool ShowErrorsWhenStagingFiles
        {
            get
            {
                if (_showErrorsWhenStagingFiles == null)
                    SafeSetBool("showerrorswhenstagingfiles", true, x => _showErrorsWhenStagingFiles = x);
                return _showErrorsWhenStagingFiles.Value;
            }
            set
            {
                _showErrorsWhenStagingFiles = value;
                Application.UserAppDataRegistry.SetValue("showerrorswhenstagingfiles", _showErrorsWhenStagingFiles);
            }
        }

        private static string _lastCommitMessage;
        public static string LastCommitMessage
        {
            get
            {
                if (_lastCommitMessage == null)
                    SafeSetString("lastCommitMessage", "", x => _lastCommitMessage = x);
                return _lastCommitMessage;
            }
            set
            {
                _lastCommitMessage = value;
                Application.UserAppDataRegistry.SetValue("lastCommitMessage", _lastCommitMessage);
            }
        }

        private static bool? _showGitStatusInBrowseToolbar;
        public static bool ShowGitStatusInBrowseToolbar
        {
            get
            {
                if (_showGitStatusInBrowseToolbar == null)
                    SafeSetBool("showgitstatusinbrowsetoolbar", false, x => _showGitStatusInBrowseToolbar = x);
                return _showGitStatusInBrowseToolbar.Value;
            }
            set
            {
                _showGitStatusInBrowseToolbar = value;
                Application.UserAppDataRegistry.SetValue("showgitstatusinbrowsetoolbar", _showGitStatusInBrowseToolbar);
            }
        }

        public static bool CommitInfoShowContainedInBranches
        {
            get
            {
                return CommitInfoShowContainedInBranchesLocal ||
                    CommitInfoShowContainedInBranchesRemote ||
                    CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            }
        }

        private static bool? _commitInfoShowContainedInBranchesLocal;
        public static bool CommitInfoShowContainedInBranchesLocal
        {
            get
            {
                if (_commitInfoShowContainedInBranchesLocal == null)
                    SafeSetBool("commitinfoshowcontainedinbrancheslocal", true, x => _commitInfoShowContainedInBranchesLocal = x);
                return _commitInfoShowContainedInBranchesLocal.Value;
            }
            set
            {
                _commitInfoShowContainedInBranchesLocal = value;
                Application.UserAppDataRegistry.SetValue("commitinfoshowcontainedinbrancheslocal", _commitInfoShowContainedInBranchesLocal);
            }
        }

        private static bool? _commitInfoShowContainedInBranchesRemote;
        public static bool CommitInfoShowContainedInBranchesRemote
        {
            get
            {
                if (_commitInfoShowContainedInBranchesRemote == null)
                    SafeSetBool("commitinfoshowcontainedinbranchesremote", false, x => _commitInfoShowContainedInBranchesRemote = x);
                return _commitInfoShowContainedInBranchesRemote.Value;
            }
            set
            {
                _commitInfoShowContainedInBranchesRemote = value;
                if (value)
                    CommitInfoShowContainedInBranchesRemoteIfNoLocal = false;
                Application.UserAppDataRegistry.SetValue("commitinfoshowcontainedinbranchesremote", _commitInfoShowContainedInBranchesRemote);
            }
        }

        private static bool? _commitInfoShowContainedInBranchesRemoteIfNoLocal;
        public static bool CommitInfoShowContainedInBranchesRemoteIfNoLocal
        {
            get
            {
                if (_commitInfoShowContainedInBranchesRemoteIfNoLocal == null)
                    SafeSetBool("commitinfoshowcontainedinbranchesremoteifnolocal", false, x => _commitInfoShowContainedInBranchesRemoteIfNoLocal = x);
                return _commitInfoShowContainedInBranchesRemoteIfNoLocal.Value;
            }
            set
            {
                _commitInfoShowContainedInBranchesRemoteIfNoLocal = value;
                if (value)
                    CommitInfoShowContainedInBranchesRemote = false;
                Application.UserAppDataRegistry.SetValue("commitinfoshowcontainedinbranchesremoteifnolocal", _commitInfoShowContainedInBranchesRemoteIfNoLocal);
            }
        }

        private static bool? _commitInfoShowContainedInTags;
        public static bool CommitInfoShowContainedInTags
        {
            get
            {
                if (_commitInfoShowContainedInTags == null)
                    SafeSetBool("commitinfoshowcontainedintags", true, x => _commitInfoShowContainedInTags = x);
                return _commitInfoShowContainedInTags.Value;
            }
            set
            {
                _commitInfoShowContainedInTags = value;
                Application.UserAppDataRegistry.SetValue("commitinfoshowcontainedintags", _commitInfoShowContainedInTags);
            }
        }

        public static string ApplicationDataPath { get; set; }

        private static string _translation;
        public static string Translation
        {
            get
            {
                if (_translation == null)
                    SafeSetString("translation", "", x => _translation = x);
                return _translation;
            }
            set
            {
                _translation = value;
                Application.UserAppDataRegistry.SetValue("translation", _translation != null ? _translation : "");
            }
        }

        private static bool? _userProfileHomeDir;
        public static bool UserProfileHomeDir
        {
            get
            {
                if (_userProfileHomeDir == null)
                    SafeSetBool("userprofilehomedir", false, x => _userProfileHomeDir = x);
                return _userProfileHomeDir.Value;
            }
            set
            {
                _userProfileHomeDir = value;
                Application.UserAppDataRegistry.SetValue("userprofilehomedir", _userProfileHomeDir);
            }
        }

        private static string _customHomeDir;
        public static string CustomHomeDir
        {
            get
            {
                if (_customHomeDir == null)
                    SafeSetString("customhomedir", "", x => _customHomeDir = x);
                return _customHomeDir;
            }
            set
            {
                _customHomeDir = value;
                Application.UserAppDataRegistry.SetValue("customhomedir", _customHomeDir);
            }
        }

        private static string _iconColor;
        public static string IconColor
        {
            get
            {
                if (_iconColor == null)
                    SafeSetString("iconcolor", "default", x => _iconColor = x);
                return _iconColor;
            }
            set
            {
                _iconColor = value;
                Application.UserAppDataRegistry.SetValue("iconcolor", _iconColor);
            }
        }

        private static int? _authorImageSize;
        public static int AuthorImageSize
        {
            get
            {
                if (_authorImageSize == null)
                    SafeSetInt("authorimagesize", 80, x => _authorImageSize = x);
                return _authorImageSize.Value;
            }
            set
            {
                _authorImageSize = value;
                Application.UserAppDataRegistry.SetValue("authorimagesize", _authorImageSize);
            }
        }
        private static int? _authorImageCacheDays;
        public static int AuthorImageCacheDays
        {
            get
            {
                if (_authorImageCacheDays == null)
                    SafeSetInt("authorimagecachedays", 5, x => _authorImageCacheDays = x);
                return _authorImageCacheDays.Value;
            }
            set
            {
                _authorImageCacheDays = value;
                Application.UserAppDataRegistry.SetValue("authorimagecachedays", _authorImageCacheDays);
            }
        }

        private static bool? _showAuthorGravatar;
        public static bool ShowAuthorGravatar
        {
            get
            {
                if (_showAuthorGravatar == null)
                    SafeSetBool("showauthorgravatar", true, x => _showAuthorGravatar = x);
                return _showAuthorGravatar.Value;
            }
            set
            {
                _showAuthorGravatar = value;
                Application.UserAppDataRegistry.SetValue("showauthorgravatar", _showAuthorGravatar);
            }
        }

        private static bool? _closeCommitDialogAfterCommit;
        public static bool CloseCommitDialogAfterCommit
        {
            get
            {
                if (_closeCommitDialogAfterCommit == null)
                    SafeSetBool("closecommitdialogaftercommit", true, x => _closeCommitDialogAfterCommit = x);
                return _closeCommitDialogAfterCommit.Value;
            }
            set
            {
                _closeCommitDialogAfterCommit = value;
                Application.UserAppDataRegistry.SetValue("closecommitdialogaftercommit", _closeCommitDialogAfterCommit);
            }
        }

        private static bool? _followRenamesInFileHistory;
        public static bool FollowRenamesInFileHistory
        {
            get
            {
                if (_followRenamesInFileHistory == null)
                    SafeSetBool("followrenamesinfilehistory", true, x => _followRenamesInFileHistory = x);
                return _followRenamesInFileHistory.Value;
            }
            set
            {
                _followRenamesInFileHistory = value;
                Application.UserAppDataRegistry.SetValue("followrenamesinfilehistory", _followRenamesInFileHistory);
            }
        }

        private static bool? _revisionGraphShowWorkingDirChanges;
        public static bool RevisionGraphShowWorkingDirChanges
        {
            get
            {
                if (_revisionGraphShowWorkingDirChanges == null)
                    SafeSetBool("revisiongraphshowworkingdirchanges", false, x => _revisionGraphShowWorkingDirChanges = x);
                return _revisionGraphShowWorkingDirChanges.Value;
            }
            set
            {
                _revisionGraphShowWorkingDirChanges = value;
                Application.UserAppDataRegistry.SetValue("revisiongraphshowworkingdirchanges", _revisionGraphShowWorkingDirChanges);
            }
        }

        private static bool? _revisionGraphDrawNonRelativesGray;
        public static bool RevisionGraphDrawNonRelativesGray
        {
            get
            {
                if (_revisionGraphDrawNonRelativesGray == null)
                    SafeSetBool("revisiongraphdrawnonrelativesgray", true, x => _revisionGraphDrawNonRelativesGray = x);
                return _revisionGraphDrawNonRelativesGray.Value;
            }
            set
            {
                _revisionGraphDrawNonRelativesGray = value;
                Application.UserAppDataRegistry.SetValue("revisiongraphdrawnonrelativesgray", _revisionGraphDrawNonRelativesGray);
            }
        }

        public static int RevisionGraphWidth { get; set; }

        public static float RevisionGraphThickness { get; set; }

        private static Encoding _encoding;
        public static Encoding Encoding
        {
            get
            {
                if (_encoding == null)
                {
                    string encoding = null;
                    SafeSetString("encoding", "", x => encoding = x);

                    if (string.IsNullOrEmpty(encoding))
                        _encoding = new UTF8Encoding(false);
                    else if (encoding.Equals("Default", StringComparison.CurrentCultureIgnoreCase))
                        _encoding = Encoding.Default;
                    else if (encoding.Equals("Unicode", StringComparison.CurrentCultureIgnoreCase))
                        _encoding = new UnicodeEncoding();
                    else if (encoding.Equals("ASCII", StringComparison.CurrentCultureIgnoreCase))
                        _encoding = new ASCIIEncoding();
                    else if (encoding.Equals("UTF7", StringComparison.CurrentCultureIgnoreCase))
                        _encoding = new UTF7Encoding();
                    else if (encoding.Equals("UTF32", StringComparison.CurrentCultureIgnoreCase))
                        _encoding = new UTF32Encoding(true, false);
                    else
                        _encoding = new UTF8Encoding(false);
                }
                return _encoding;
            }
            set
            {
                _encoding = value;

                if (Application.UserAppDataRegistry == null)
                    return;

                if (_encoding.EncodingName == Encoding.ASCII.EncodingName)
                    Application.UserAppDataRegistry.SetValue("encoding", "ASCII");
                else if (_encoding.EncodingName == Encoding.Unicode.EncodingName)
                    Application.UserAppDataRegistry.SetValue("encoding", "Unicode");
                else if (_encoding.EncodingName == Encoding.UTF7.EncodingName)
                    Application.UserAppDataRegistry.SetValue("encoding", "UTF7");
                else if (_encoding.EncodingName == Encoding.UTF8.EncodingName)
                    Application.UserAppDataRegistry.SetValue("encoding", "UTF8");
                else if (_encoding.EncodingName == Encoding.UTF32.EncodingName)
                    Application.UserAppDataRegistry.SetValue("encoding", "UTF32");
                else if (_encoding.EncodingName == Encoding.Default.EncodingName)
                    Application.UserAppDataRegistry.SetValue("encoding", "Default");
            }
        }

        private static string _pullMerge;
        public static string PullMerge
        {
            get
            {
                if (_pullMerge == null)
                    SafeSetString("pullmerge", "merge", x => _pullMerge = x);
                return _pullMerge;
            }
            set
            {
                _pullMerge = value;
                Application.UserAppDataRegistry.SetValue("pullmerge", _pullMerge);
            }
        }


        private static string _smtp;
        public static string Smtp
        {
            get
            {
                if (_smtp == null)
                    SafeSetString("smtp", "", x => _smtp = x);
                return _smtp;
            }
            set
            {
                _smtp = value;
                Application.UserAppDataRegistry.SetValue("smtp", _smtp);
            }
        }


        private static bool? _autoStash;
        public static bool AutoStash
        {
            get
            {
                if (_autoStash == null)
                    SafeSetBool("autostash", false, x => _autoStash = x);
                return _autoStash.Value;
            }
            set
            {
                _autoStash = value;
                Application.UserAppDataRegistry.SetValue("autostash", _autoStash);
            }
        }

        private static bool? _orderRevisionByDate;
        public static bool OrderRevisionByDate
        {
            get
            {
                if (_orderRevisionByDate == null)
                    SafeSetBool("orderrevisionbydate", true, x => _orderRevisionByDate = x);
                return _orderRevisionByDate.Value;
            }
            set
            {
                _orderRevisionByDate = value;
                Application.UserAppDataRegistry.SetValue("orderrevisionbydate", _orderRevisionByDate);
            }
        }

        private static string _dictionary;
        public static string Dictionary
        {
            get
            {
                if (string.IsNullOrEmpty(_dictionary))
                    SafeSetString("dictionary", "en-US", x => _dictionary = x);
                return _dictionary;
            }
            set
            {
                _dictionary = value;
                Application.UserAppDataRegistry.SetValue("dictionary", _dictionary);
            }
        }

        private static bool? _showGitCommandLine;
        public static bool ShowGitCommandLine
        {
            get
            {
                if (_showGitCommandLine == null)
                    SafeSetBool("showgitcommandline", false, x => _showGitCommandLine = x);
                return _showGitCommandLine.Value;
            }
            set
            {
                _showGitCommandLine = value;
                Application.UserAppDataRegistry.SetValue("showgitcommandline", _showGitCommandLine);
            }
        }

        private static bool? _showStashCount;
        public static bool ShowStashCount
        {
            get
            {
                if (_showStashCount == null)
                    SafeSetBool("showstashcount", false, x => _showStashCount = x);
                return _showStashCount.Value;
            }
            set
            {
                _showStashCount = value;
                Application.UserAppDataRegistry.SetValue("showstashcount", _showStashCount);
            }
        }

        private static bool? _relativeDate;
        public static bool RelativeDate
        {
            get
            {
                if (_relativeDate == null)
                    SafeSetBool("relativedate", true, x => _relativeDate = x);
                return _relativeDate.Value;
            }
            set
            {
                _relativeDate = value;
                Application.UserAppDataRegistry.SetValue("relativedate", _relativeDate);
            }
        }

        private static bool? _useFastChecks;
        public static bool UseFastChecks
        {
            get
            {
                if (_useFastChecks == null)
                    SafeSetBool("usefastchecks", false, x => _useFastChecks = x);
                return _useFastChecks.Value;
            }
            set
            {
                _useFastChecks = value;
                Application.UserAppDataRegistry.SetValue("usefastchecks", _useFastChecks);
            }
        }

        private static bool? _showRevisionGraph;
        public static bool ShowRevisionGraph
        {
            get
            {
                if (_showRevisionGraph == null)
                    SafeSetBool("showrevisiongraph", true, x => _showRevisionGraph = x);
                return _showRevisionGraph.Value;
            }
            set
            {
                _showRevisionGraph = value;
                Application.UserAppDataRegistry.SetValue("showrevisiongraph", _showRevisionGraph);
            }
        }

        private static bool? _showAuthorDate;
        public static bool ShowAuthorDate
        {
            get
            {
                if (_showAuthorDate == null)
                    SafeSetBool("showauthordate", true, x => _showAuthorDate = x);
                return _showAuthorDate.Value;
            }
            set
            {
                _showAuthorDate = value;
                Application.UserAppDataRegistry.SetValue("showauthordate", _showAuthorDate);
            }
        }

        private static bool? _closeProcessDialog;
        public static bool CloseProcessDialog
        {
            get
            {
                if (_closeProcessDialog == null)
                    SafeSetBool("closeprocessdialog", false, x => _closeProcessDialog = x);
                return _closeProcessDialog.Value;
            }
            set
            {
                _closeProcessDialog = value;
                Application.UserAppDataRegistry.SetValue("closeprocessdialog", _closeProcessDialog);
            }
        }

        private static bool? _showCurrentBranchOnly;
        public static bool ShowCurrentBranchOnly
        {
            get
            {
                if (_showCurrentBranchOnly == null)
                    SafeSetBool("showcurrentbranchonly", false, x => _showCurrentBranchOnly = x);
                return _showCurrentBranchOnly.Value;
            }
            set
            {
                _showCurrentBranchOnly = value;
                Application.UserAppDataRegistry.SetValue("showcurrentbranchonly", _showCurrentBranchOnly);
            }
        }

        private static bool? _branchFilterEnabled;
        public static bool BranchFilterEnabled
        {
            get
            {
                if (_branchFilterEnabled == null)
                    SafeSetBool("branchfilterenabled", false, x => _branchFilterEnabled = x);
                return _branchFilterEnabled.Value;
            }
            set
            {
                _branchFilterEnabled = value;
                Application.UserAppDataRegistry.SetValue("branchfilterenabled", _branchFilterEnabled);
            }
        }

        private static int? _revisionGridQuickSearchTimeout;
        public static int RevisionGridQuickSearchTimeout
        {
            get
            {
                if (_revisionGridQuickSearchTimeout == null)
                    SafeSetInt("revisiongridquicksearchtimeout", 750, x => _revisionGridQuickSearchTimeout = x);
                return _revisionGridQuickSearchTimeout.Value;
            }
            set
            {
                _revisionGridQuickSearchTimeout = value;
                Application.UserAppDataRegistry.SetValue("revisiongridquicksearchtimeout", _revisionGridQuickSearchTimeout);
            }
        }

        private static string _gravatarFallbackService;
        public static string GravatarFallbackService
        {
            get
            {
                if (_gravatarFallbackService == null)
                    SafeSetString("gravatarfallbackservice", "Identicon", x => _gravatarFallbackService = x);
                return _gravatarFallbackService;
            }
            set
            {
                _gravatarFallbackService = value;
                Application.UserAppDataRegistry.SetValue("gravatarfallbackservice", _gravatarFallbackService);
            }
        }

        private static string _gitCommand;
        public static string GitCommand
        {
            get
            {
                if (_gitCommand == null)
                    SafeSetString("gitcommand", "git", x => _gitCommand = x);
                return _gitCommand;
            }
            set
            {
                _gitCommand = value;
                Application.UserAppDataRegistry.SetValue("gitcommand", _gitCommand);
            }
        }

        private static string _gitBinDir;
        public static string GitBinDir
        {
            get
            {
                if (_gitBinDir == null)
                    SafeSetString("gitbindir", "", x => GitBinDir = x);
                return _gitBinDir;
            }
            set
            {
                _gitBinDir = value;
                if (_gitBinDir.Length > 0 && _gitBinDir[_gitBinDir.Length - 1] != PathSeparator)
                    _gitBinDir += PathSeparator;
                Application.UserAppDataRegistry.SetValue("gitbindir", _gitBinDir);

                //if (string.IsNullOrEmpty(_gitBinDir))
                //    return;

                //var path =
                //    Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Process) + ";" +
                //    _gitBinDir;
                //Environment.SetEnvironmentVariable("path", path, EnvironmentVariableTarget.Process);
            }
        }

        private static int? _maxCommits;
        public static int MaxCommits
        {
            get
            {
                if (_maxCommits == null)
                    SafeSetInt("maxcommits", 2000, x => _maxCommits = x);
                return _maxCommits.Value;
            }
            set
            {
                _maxCommits = value;
                Application.UserAppDataRegistry.SetValue("maxcommits", _maxCommits);
            }
        }

        public delegate void WorkingDirChangedEventHandler(string oldDir, string newDir);
        public static event WorkingDirChangedEventHandler WorkingDirChanged;

        private static string _workingdir;
        public static string WorkingDir
        {
            get
            {
                return _workingdir;
            }
            set
            {
                string old = _workingdir;
                _workingdir = GitCommandHelpers.FindGitWorkingDir(value.Trim());
                if (WorkingDirChanged != null)
                {
                    WorkingDirChanged(old, _workingdir);
                }
            }
        }

        public static CommandLogger GitLog { get; private set; }

        private static string _plink;
        public static string Plink
        {
            get
            {
                if (_plink == null)
                    SafeSetString("plink", "", x => _plink = x);
                return _plink;
            }
            set
            {
                _plink = value;
                Application.UserAppDataRegistry.SetValue("plink", _plink);
            }
        }
        private static string _puttygen;
        public static string Puttygen
        {
            get
            {
                if (_puttygen == null)
                    SafeSetString("puttygen", "", x => _puttygen = x);
                return _puttygen;
            }
            set
            {
                _puttygen = value;
                Application.UserAppDataRegistry.SetValue("puttygen", _puttygen);
            }
        }

        private static string _pageant;
        public static string Pageant
        {
            get
            {
                if (_pageant == null)
                    SafeSetString("pageant", "", x => _pageant = x);
                return _pageant;
            }
            set
            {
                _pageant = value;
                Application.UserAppDataRegistry.SetValue("pageant", _pageant);
            }
        }

        private static bool? _autoStartPageant;
        public static bool AutoStartPageant
        {
            get
            {
                if (_autoStartPageant == null)
                    SafeSetBool("autostartpageant", true, x => _autoStartPageant = x);
                return _autoStartPageant.Value;
            }
            set
            {
                _autoStartPageant = value;
                Application.UserAppDataRegistry.SetValue("autostartpageant", _autoStartPageant);
            }
        }

        private static bool? _markIllFormedLinesInCommitMsg;
        public static bool MarkIllFormedLinesInCommitMsg
        {
            get
            {
                if (_markIllFormedLinesInCommitMsg == null)
                    SafeSetBool("markillformedlinesincommitmsg", false, x => _markIllFormedLinesInCommitMsg = x);
                return _markIllFormedLinesInCommitMsg.Value;
            }
            set
            {
                _markIllFormedLinesInCommitMsg = value;
                Application.UserAppDataRegistry.SetValue("markillformedlinesincommitmsg", _markIllFormedLinesInCommitMsg);
            }
        }

        #region Colors

        private static Color? _otherTagColor;
        public static Color OtherTagColor
        {
            get
            {
                if (_otherTagColor == null)
                    SafeSetHtmlColor("othertagcolor", Color.Gray, x => _otherTagColor = x);
                return _otherTagColor.Value;
            }
            set
            {
                _otherTagColor = value;
                Application.UserAppDataRegistry.SetValue("othertagcolor", ColorTranslator.ToHtml(_otherTagColor.Value));
            }
        }
        private static Color? _tagColor;
        public static Color TagColor
        {
            get
            {
                if (_tagColor == null)
                    SafeSetHtmlColor("tagcolor", Color.DarkBlue, x => _tagColor = x);
                return _tagColor.Value;
            }
            set
            {
                _tagColor = value;
                Application.UserAppDataRegistry.SetValue("tagcolor", ColorTranslator.ToHtml(_tagColor.Value));
            }
        }

        private static Color? _graphColor;
        public static Color GraphColor
        {
            get
            {
                if (_graphColor == null)
                    SafeSetHtmlColor("graphcolor", Color.DarkRed, x => _graphColor = x);
                return _graphColor.Value;
            }
            set
            {
                _graphColor = value;
                Application.UserAppDataRegistry.SetValue("graphcolor", ColorTranslator.ToHtml(_graphColor.Value));
            }
        }

        private static Color? _branchColor;
        public static Color BranchColor
        {
            get
            {
                if (_branchColor == null)
                    SafeSetHtmlColor("branchcolor", Color.DarkRed, x => _branchColor = x);
                return _branchColor.Value;
            }
            set
            {
                _branchColor = value;
                Application.UserAppDataRegistry.SetValue("branchcolor", ColorTranslator.ToHtml(_branchColor.Value));
            }
        }

        private static Color? _remoteBranchColor;
        public static Color RemoteBranchColor
        {
            get
            {
                if (_remoteBranchColor == null)
                    SafeSetHtmlColor("remotebranchcolor", Color.Green, x => _remoteBranchColor = x);
                return _remoteBranchColor.Value;
            }
            set
            {
                _remoteBranchColor = value;
                Application.UserAppDataRegistry.SetValue("remotebranchcolor", ColorTranslator.ToHtml(_remoteBranchColor.Value));
            }
        }

        private static Color? _diffSectionColor;
        public static Color DiffSectionColor
        {
            get
            {
                if (_diffSectionColor == null)
                    SafeSetHtmlColor("diffsectioncolor", Color.FromArgb(230, 230, 230), x => _diffSectionColor = x);
                return _diffSectionColor.Value;
            }
            set
            {
                _diffSectionColor = value;
                Application.UserAppDataRegistry.SetValue("diffsectioncolor", ColorTranslator.ToHtml(_diffSectionColor.Value));
            }
        }

        private static Color? _diffRemovedColor;
        public static Color DiffRemovedColor
        {
            get
            {
                if (_diffRemovedColor == null)
                    SafeSetHtmlColor("diffremovedcolor", Color.FromArgb(255, 200, 200), x => _diffRemovedColor = x);
                return _diffRemovedColor.Value;
            }
            set
            {
                _diffRemovedColor = value;
                Application.UserAppDataRegistry.SetValue("diffremovedcolor", ColorTranslator.ToHtml(_diffRemovedColor.Value));
            }
        }

        private static Color? _diffRemovedExtraColor;
        public static Color DiffRemovedExtraColor
        {
            get
            {
                if (_diffRemovedExtraColor == null)
                    SafeSetHtmlColor("diffremovedextracolor", Color.FromArgb(255, 150, 150), x => _diffRemovedExtraColor = x);
                return _diffRemovedExtraColor.Value;
            }
            set
            {
                _diffRemovedExtraColor = value;
                Application.UserAppDataRegistry.SetValue("diffremovedextracolor", ColorTranslator.ToHtml(_diffRemovedExtraColor.Value));
            }
        }

        private static Color? _diffAddedColor;
        public static Color DiffAddedColor
        {
            get
            {
                if (_diffAddedColor == null)
                    SafeSetHtmlColor("diffaddedcolor", Color.FromArgb(200, 255, 200), x => _diffAddedColor = x);
                return _diffAddedColor.Value;
            }
            set
            {
                _diffAddedColor = value;
                Application.UserAppDataRegistry.SetValue("diffaddedcolor", ColorTranslator.ToHtml(_diffAddedColor.Value));
            }
        }

        private static Color? _diffAddedExtraColor;
        public static Color DiffAddedExtraColor
        {
            get
            {
                if (_diffAddedExtraColor == null)
                    SafeSetHtmlColor("diffaddedextracolor", Color.FromArgb(135, 255, 135), x => _diffAddedExtraColor = x);
                return _diffAddedExtraColor.Value;
            }
            set
            {
                _diffAddedExtraColor = value;
                Application.UserAppDataRegistry.SetValue("diffaddedextracolor", ColorTranslator.ToHtml(_diffAddedExtraColor.Value));
            }
        }

        #endregion

        private static bool? _multicolorBranches;
        public static bool MulticolorBranches
        {
            get
            {
                if (_multicolorBranches == null)
                    SafeSetBool("multicolorbranches", true, x => _multicolorBranches = x);
                return _multicolorBranches.Value;
            }
            set
            {
                _multicolorBranches = value;
                Application.UserAppDataRegistry.SetValue("multicolorbranches", _multicolorBranches);
            }
        }

        private static bool? _stripedBranchChange;
        public static bool StripedBranchChange
        {
            get
            {
                if (_stripedBranchChange == null)
                    SafeSetBool("stripedbranchchange", true, x => _stripedBranchChange = x);
                return _stripedBranchChange.Value;
            }
            set
            {
                _stripedBranchChange = value;
                Application.UserAppDataRegistry.SetValue("stripedbranchchange", _stripedBranchChange);
            }
        }

        private static bool? _branchBorders;
        public static bool BranchBorders
        {
            get
            {
                if (_branchBorders == null)
                    SafeSetBool("branchborders", true, x => _branchBorders = x);
                return _branchBorders.Value;
            }
            set
            {
                _branchBorders = value;
                Application.UserAppDataRegistry.SetValue("branchborders", _branchBorders);
            }
        }

        private static string _lastFormatPatchDir;
        public static string LastFormatPatchDir
        {
            get
            {
                if (_lastFormatPatchDir == null)
                    SafeSetString("lastformatpatchdir", "", x => _lastFormatPatchDir = x);
                return _lastFormatPatchDir;
            }
            set
            {
                _lastFormatPatchDir = value;
                Application.UserAppDataRegistry.SetValue("lastformatpatchdir", _lastFormatPatchDir);
            }
        }



        public static string GetDictionaryDir()
        {
            return GetInstallDir() + "\\Dictionaries\\";
        }


        public static string GetInstallDir()
        {
            var result = "";
            SafeSetString("InstallDir", "", x => result = x);
            return result;
        }

        public static void SetInstallDir(string dir)
        {
            if (Application.UserAppDataRegistry != null)
                Application.UserAppDataRegistry.SetValue("InstallDir", dir);
        }

        public static bool ValidWorkingDir()
        {
            return ValidWorkingDir(WorkingDir);
        }

        public static bool ValidWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return false;

            if (Directory.Exists(dir + PathSeparator + ".git"))
                return true;

            return !dir.Contains(".git") &&
                   Directory.Exists(dir + PathSeparator + "info") &&
                   Directory.Exists(dir + PathSeparator + "objects") &&
                   Directory.Exists(dir + PathSeparator + "refs");
        }

        public static bool IsBareRepository()
        {
            return !Directory.Exists(WorkingDir + PathSeparator + ".git");
        }

        public static string WorkingDirGitDir()
        {
            var workingDir = WorkingDir;

            if (Directory.Exists(workingDir + ".git"))
                return workingDir + ".git";

            if (Directory.Exists(workingDir + PathSeparator + ".git"))
                return workingDir + PathSeparator + ".git";

            return WorkingDir;
        }

        public static bool RunningOnWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return true;
                default:
                    return false;
            }
        }

        public static bool RunningOnUnix()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    return true;
                default:
                    return false;
            }
        }

        public static bool RunningOnMacOSX()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    return true;
                default:
                    return false;
            }
        }

        public static void SaveSettings()
        {
            try
            {
                Application.UserAppDataRegistry.SetValue("gitssh", GitCommandHelpers.GetSsh());
                if (Repositories.RepositoryHistoryLoaded)
                    Application.UserAppDataRegistry.SetValue("history", Repositories.SerializeHistoryIntoXml());
                if (Repositories.RepositoryCategoriesLoaded)
                    Application.UserAppDataRegistry.SetValue("repositories", Repositories.SerializeRepositories());
            }
            catch
            { }
        }


        public static void LoadSettings()
        {
            try
            {
                SafeSetString("gitssh", null, GitCommandHelpers.SetSsh);
            }
            catch
            { }

        }


        private static void SafeSetBool(string key, bool defaultValue, Action<bool> actionToPerformIfValueExists)
        {
            SafeSetString(key, null, x => actionToPerformIfValueExists(x == null ? defaultValue : x == "True"));
        }

        private static void SafeSetHtmlColor(string key, Color defaultValue, Action<Color> actionToPerformIfValueExists)
        {
            SafeSetString(key, null, x => actionToPerformIfValueExists(x == null ? defaultValue : ColorTranslator.FromHtml(x)));
        }

        private static void SafeSetInt(string key, int defaultValue, Action<int> actionToPerformIfValueExists)
        {
            SafeSetString(key, null,
                                x =>
                                {
                                    int result;
                                    if (x != null && int.TryParse(x, out result))
                                    {
                                        actionToPerformIfValueExists(result);
                                    }
                                    else
                                    {
                                        actionToPerformIfValueExists(defaultValue);
                                    }
                                });
        }

        private static void SafeSetString(string key, string defaultValue, Action<string> actionToPerformIfValueExists)
        {
            if (Application.UserAppDataRegistry == null) return;
            var value = Application.UserAppDataRegistry.GetValue(key);
            if (value == null)
                actionToPerformIfValueExists(defaultValue);
            else
                actionToPerformIfValueExists(value.ToString());
        }
        private static string _ownScripts;
        private static string ownScripts
        {
            get
            {
                if (_ownScripts == null)
                    SafeSetString("ownScripts", "", x => _ownScripts = x);
                return _ownScripts;
            }
            set
            {
                _ownScripts = value;
                Application.UserAppDataRegistry.SetValue("ownScripts", _ownScripts);
            }
        }

        private const string PARAM_SEPARATOR = "<_PARAM_SEPARATOR_>";
        private const string SCRIPT_SEPARATOR = "<_SCRIPT_SEPARATOR_>";

        public static void SaveScripts(string[][] scripts)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < scripts.Length; i++)
            {
                for (int j = 0; j < scripts[i].Length; j++)
                    sb.Append(scripts[i][j] + PARAM_SEPARATOR);
                sb.Append(SCRIPT_SEPARATOR);
            }
            ownScripts = sb.ToString();
        }

        public static string[][] GetScripts()
        {
            string[] scripts = Settings.ownScripts.Split(new string[] { Settings.SCRIPT_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            string[][] scripts_params = new string[scripts.Length][];
            for (int i = 0; i < scripts.Length; i++)
            {
                string[] parameters = scripts[i].Split(new string[] { Settings.PARAM_SEPARATOR }, StringSplitOptions.None);
                scripts_params[i] = parameters;
            }
            return scripts_params;
        }

        public static string[] GetScript(string key)
        {
            string[][] scripts = GetScripts();
            foreach (string[] parameters in scripts)
                if (parameters[0].Equals(key))
                    return parameters;
            return null;
        }
    }
}