﻿Changelog
=========
### Version 2.49 (no due date)
* Cherry pick selected file/selected lines.
* Added an option to remember the ignore-white-spaces preference for all the diff viewers.
* Fixed an intermittent bug where ObjectDisposedException occurs on launch.
* Fixed a bug where branch filter throws null reference exception when no repository selected
* Fixed a bug where unable to restore the windows after minimized by Win+M or Win+D when multiple instances of GitExtensions are running

### Version 2.48.05 (16 May 2015)
* Fixed issue #2493: StartBrowseDialog failed after clone
* Fixed issue #2783: Fixed crash when right click on blank line in 'File Tree'
* Enter/Return in file tree acts as double click
* Support Git for Windows path for Linux tools

### Version 2.48.04 (8 May 2015)
* Fixed issue #1643: Do stage of 16506 files and GUI becomes Not Respoding
* Fixed issue #2591: VSAddin solutionItem.ProjectItem == null when selected 'References' item in C# project
* Fixed issue #2587, #2601: VSAddin fixed StackOverflowException
* Fixed issue #2584: Escape the last backslash from paths before running GitExtensions to avoid escaping the double-quote
* Fixed issue #2574: MSysGit updated to version 1.9.5-preview20141217
* Fixed issue #2649: Refreshing the ignored files set every 10 minutes instead of every 500 miliseconds
* Fixed issue #2525: Additional handling for strings passed to RichTextBox
* Fixed issue #2700: Fix filtering by branch name
* Fixed 'ArgumentOutOfRangeException is thrown when right clicking of the table header of 'Resolve merge conflicts' window'
* Fix performance for RevisionGrid

### Version 2.48.03 (9 December 2014)
* Fixed issue #2538: Fix crash happening when deleting a remote branch
* Fixed issue #2498: VS Plugin use solution scope if no active document

### Version 2.48.02 (29 November 2014)
* Updated msysgit to 1.9.4 20140929
* VS plugin menu hotkey changed to Alt+I
* Updated kdiff3 to version 0.9.98-2 64bit
* Form pull "Manage remotes" button fixed
* Resolving conflict for removed submodule fixed
* Svn clone prefix fixed
* Fixed issue #2509: Backslash correction turned off for URLs
* Fixed issue #2420: Indicate change when repository changed
* Fixed issue #2454: Support changed path to DiffMerge
* Fixed issue #2450: There is no verification that settings can not contain invalid xml characters
* Fixed issue #2407: Double appearance of "Current unstaged changes" fixed
* Fixed issue #2269: Ignore COM exceptions in UpdateJumplist
* Fixed issue #2165: Pushing HEAD fixed
* Fixed issue #2463: Crash fixed when selecting a file with no base in the "Resolve merge conflicts" dialog
* Fixed issue #2448: Freeze while commit many files with warnings
* Fixed issue #2467, #2483: Installing GitCredentialsHelper fixed
* Fixed issue #1493: Should fix command bar position saving in some cases

### Version 2.48 (20 August 2014)
* Git credential helper now optional product
* Sort branches and tags in commit info and display only first 20
* Checkout remote branch dialog try find tracking branch first
* Show full shell extension menu with Shift pressed
* Disabled installing MSysGit Git-Cheetah shell extension
* Visual Studio plugin menu renamed to GitExt
* Button Browse... and Manage Remotes fixed in form Pull
* Support machine level HOME environment variable
* Error string in list of branches fixed
* Fixed issue #2079: Selection order after refresh fixed
* Fixed issue #2178: Disabled caching diffs for artificial revisions
* Fixed issue #2387: Remove pdf manual from installer
* Fixed issue #2389: Never try to checkout an empty-named branch
* Fixed issue #2397: Cannot close GitEx window with opened repository
* Updated translations

### Version 2.48RC (13 July 2014)
* Updated msysgit to 1.9.4
* Implemented auto completion for commit message window.
* Support intergration with TeamCity and Jenkins build server
* Support pull request for Atlassian Stash
* GitExt suggest update submodules after changing revision. PR #2176
* Show commit changes (i.e: -1+5) on Checkout Branch, CheckoutR evision, Create Branch and Create Tag dialogs
* Sepatate windows to merge submodules
* Increased performance and lowered memory footprint of DvcsGraph
* Allow Create branch in Commit dialog
* Added support for remote branches to the DeleteUnusedBranches plugin
* Revision grid will show superproject tags/branches/remote branches and conflict Base/Remote/Local
* Added Sublime Text 3 to editor list
* Added p4merge to the list of difftools
* Added BeyondCompare4 to the list of diff and merge tools
* Added SemanticMerge to the list of diff and merge tools
* Added hotkey to close repositry via CTRL+W
* Open .git/config fixed
* "Back" button and history
* Disabled by default: include untracked files in stash
* Commiter name added to commit dialog status bar. PR #1812
* Check ValidSvnWorkindDir before do svn commands. Method GitSvnCommandHelpers.ValidSvnWorkindDir work not correct on submodule repo
* Fixed undetected working directory in root directory (the additional "dir.rfind" in the while condition stopped the loop **before** e.g. "C:" has been reached)
* "Initialize repository" renamed to "Create repository"
* "working dir" and "working tree" renamed to "working directory" to simplify translation
* Preffer Putty from GitExtensions
* New settings management
* Translation format changed to XLIFF (you can help on [Transifex](https://www.transifex.com/organization/git-extensions/dashboard/git-extensions) website)
* Fixed issue #2349: Bug fixed with file history for file outside of the solution
* Fixed issue #2294: Commit dialog hangs for hours on selecting or deselecting many files
* Fixed issue #2250: Shell Extensions: IsValidGitDir UNC path performance bug fix
* Fixed issue #2240: Allow push by commit hash
* Fixed issue #2235: Allow the user to pick Git.exe no matter
where it is installed on their system
* Fixed issue #2142: Fetch in 2.47.3 creating remote ref from remote HEAD using current local branch's name
* Fixed issue #2140: Fix slow settings load because of UNC paths
* Fixed issue #2139: Double click on submodule fails to open the submodule in a new instance of GitExtensions
* Fixed issue #2137: Strange push behaviour
* Fixed issue #2136: Git credential helper does not load
* Fixed issue #2135: In v2.47.03 the SSH setting defaults to OpenSSH even though PuTTY is selected during installation
* Fixed issue #2131: No horizontal scrollbar in the main window Diff tab file list
* Fixed issue #2110: VS Plugin make CommandBar permanent (position handling of CommandBar)
* Fixed issue #2013: "the branch seems to be new for the remote" is shown but the branch is already there
* Fixed issue #230: Don't spawn another git status if the previous one is still running
* Stop runaway git process creation under Windows
* Updated translations

### Version 2.47.3 (15 November 2013)
* Fixed issue #2124: GitHub integration not working in 2.47.1

### Version 2.47.1 (14 November 2013)
* Fixed issue #2006: Check for updates using GitHub api. There is a bug in WebClient that causes AV
* Fixed issue #2117: Create branch/tag command in VS fixed
* Fixed issue #2114: "Keep dialog open" doesn't work
* Added IconFileStatusUnknown - when git exits with error then GitExt crashes with IndexOutOfBounds

### Version 2.47 (8 November 2013)
* Main menu restructured. Issues: #1576, #1629
* Added BackgroundFetch plugin in order to allow perioding fetching of all remotes automatically
* Putty updated to version beta 0.63 (released 2013-08-06)
* Display diff files list for each parent in separete collapsible group
* Autopull from remote to which push was rejected. Closes #1887
* Added support for installing GitPlugin in VS2013
* ShellExtensionSettingsPage: add simple preview for context menu items. PR #1661
* Allow pushing a non-branch source rev to a remote branch. PR #1676
* FormResolveConflicts show submodule hashes for merge conflicts
* FormChooseCommit: double click behaves like OK button instead of opening the CommitInfo dialog. PR #1681
* Remove old Create tag dialog and related changes. PR #1680
* FormCheckoutRevision: make similar to CreateBranch/Tag dialogs and use in RevisionGrid. PR #1685
* FormChooseCommit: add button that opens the Go to commit dialog that helps to find a specific commit. PR #1723
* GitPlugin icon transparency fixed
* FormPuttyError Retry() button fixed
* Add Help link that opens GitExt manual in browser at predefined section. PR #1739
* Push dialog now asks pull changes if push was rejected
* Improvements for FormResetCurrentBranch. PR #1750
* FormFormatPatch support sending from gmail server
* LoadPuttyKey for all remotes
* Close commit dialog when all changes are commited - now considers new file as a change
* Disabled offer commit for resolve conflicts dialog when it called from commit window. Closes #1623
* Support pull latest submodule changes from FormSubmodules dialog window
* Asynchronous RepositoryHistory loading implemented
* Switch for selection gravatar image size added
* Menu in revision grid sorted to simply common branch operations
* Suggest prune branches if remote branch no longer exist
* Added a "Show tags" menu item. PR #1814
* Branch filter wildcards. PR #1815
* Disallow to cache stash diffs
* Menu ShortcutKey improvements. PR #1863
* FormBrowse: Diff Tab: new context menu item: "Show in File tree". PR #1850
* File tree tab: "Open containing folder" now also works on directory nodes (instead of file nodes only)
* Diff tab: If a file does not exist then "Open containing folder" now opens the parent directory of the non-existing file
* Add "Archive..." to context menu of File tree tab / Archive filter by path. PR #1868
* Blame previous revision fixed
* Combined diff processing improved
* Don't commit merge - now generic option. Closes #1882
* Current checkout detection fixed for bare repositories
* Updating current file in commit window improved
* Fix Plugin: Release Notes Generator. PR #1907
* Set the default buttons in the "Reset Current Branch" popup to fix Enter/Escape keys not working. PR #1911
* Selecting items in FormCommit improved, should update diff view only when needed
* Open corresponding SettingsPage directly from Plugin. PR #1925
* Fix diff for initial branch commit
* Fixed issue #1633: Move Plugins --> Update menu option to Help menu
* Fixed issue #1590: Checking "Show current branch only" results in unexpected behaviour
* Fixed issue #1655: Add ellipsis to menu items which will require input
* Fixed issue #1575: Remove old "Create branch" dialog
* Fixed issue #1317: Exception after closing File History dialog (from Commit Dialog) which was not completely loaded
* Fixed issue #1704: The link to the git-review command page moved from project openstack-ci to openstack-infra
* Fixed issue #1658: Portable Settings
* Fixed issue #1790: GitCommands.Settings#GetGitExtensionsDirectory: Crash if fileName.LastIndexOfAny returns -1
* Fixed issue #1799: Refresh dontSetAsDefaultToolStripMenuItem.Checked
* Fixed issue #1874: "Show changes" crash fixed in Blame dialog
* Fixed issue #1889: Push dialog remembers "Push all tags" but does not show it to the user
* Fixed issue #1883: Script on event ShowInUserMenuBar very large icon
* Fixed issue #1604: Add configuration option to specify a default clone destination directory
* Fixed issue #1899: Commit dialog steals focus
* Fixed issue #1923: File History shows entire history graph in gray
* Fixed issue #1361: Export difference between revisions
* Fixed issue #1939: Running script with "{cHash}" in arguments using hotkey causes runtime exception
* Fixed issue #1957: Search file inside Visual Studio does not open file
* Fixed issue #1209, #2001: Remote branch autofill corrected
* Many bugfixes and minor changes
* Updated Spanish translation
* Updated French translation
* Updated Japanese translation
* Updated Dutch translation
* Updated Russian translation
* Updated German translation
* Updated Simplified Chinese translation
* Added Korean translation

### Version 2.46 (26 June 2013)
* Fixed issue 1387: Shell extensions not work under Windows XP

### Version 2.45 (26 June 2013)
* Setup files moved to sourceforge http://sourceforge.net/projects/gitextensions/
* Putty updated to version 0.62.9768.0 (80% faster for me when cloning repository from GitHub)
* FormCheckoutBranch behavior fixed again when called from commit dialog
* Fixed navigation in the blame committer list when double clicking
* Fixed FormFileHistory selection current revision
* Fixed issue #1585: IsBinaryAccordingToGitAttribute() rewrited using "git check-attr"
* Fixed issue #1590: "Show current branch only" fixed
* Fixed issue #1622: "Show Changes" from Blame window crash fixed
* Fixed issue #1631: Font size reading from settings fixed
* Fixed issue #1687: GetSuperprojectCurrentCheckout() now called asynchronously
* Fixed issue #1727: CreatePullRequestForm crash fixed
* Updated msysgit to build of version 1.8.3
* Updated French translation
* Updated German translation

### Version 2.44 (18 February 2013)
* Fixed issue #710: Added icons to shell extensions
* Fixed issue #1336: Prefix context menu entries with
* Fixed issue #1354: Dashboard item description label is cut off at the top
* Fixed issue #1371: Different icons for submodule status icons
* Fixed issue #1388: Created icons for submodules
* Fixed issue #1396: Mergetool command suggest button does not work
* Fixed issue #1397: Shell exceptions broken
* Fixed issue #1407: Reorder context menu items in commit dialog to better match other context menus
* Fixed issue #1419: Command line argument "commit --quiet" doesn't work anymore
* Fixed issue #1428: Uncheck "Amend Commit" checkbox after commiting
* Fixed issue #1372: Stage submodule after commit
* Fixed Issue #1430: Replaced settings dialog with more user friendlue dialog
* Fixed issue #1432: Add icon for "Revert commit" in revision grid context menu
* Fixed issue #1433: Improved layout for commit information in various dialogs (e.g. cherry pick)
* Fixed issue #1434: Use FolderBrowserButton in FormOpenDirectory, FormPull and FormPush
* Fixed issue #1471: Make "Add submodule" Window resizable
* Fixed issue #1475: Fixed tab order in create tag dialog
* Fixed issue #1477: Quick filter by commit sha
* Fixed issue #1478: Bug fixes and improvements to Gerrit plugin
* Fixed issue #1483: Staging a file which undoes all current staged changes leaves the file available in "Working dir changes"
* Fixed issue #1488: Default pull action not run
* Fixed issue #1506: Do not refresh revisions in grid when Delete Tag dialog is canceled
* Fixed issue #1505: New help images for Fetch, Rebase, Merge actions in pull dialog
* Fixed issue #1513: Moved plugin settings to settings dialog
* Fixed issue #1533: Font size incorrect when changing culture
* Fixed issue #1561: Allow creating orphan branches
* Fixed issue #1565: Tab Order is wrong on Delete Branch form
* Fixed issue #1585: Binary file detection fixed
* Fixed issue: Amend commit doesn't work when using Ctrl+Enter key
* Fixed issue: shell extension registration in settings dialog fixed with UAC
* Fixed issue: "0 hours" and "0 year" displayed in revision grid
* Fixed issue: Do not refresh revisions in grid when Delete Tag dialog is canceled
* Add "dirty" versions of Submodule icons
* Added button to go to superproject
* Support a separate font for the commit message
* Add red coloring to long subject line in commit message
* Add option to turn off automatic word-wrapping in commit messages
* Stash window properly display information about submodules
* GoToCommit dialog: added "jump to tag" and "jump to branch" with autocomplete
* Added several icons
* Auto pull option behavior improved
* Git SVN Clone now supports custom trunk, tags and branches directory
* Suggested local branch name fixed for svn remote branches
* Added "Open with difftool" command to Visual Studio addin
* Added hotkey for "Amend commit"
* Updated Spanish translation
* Updated Dutch translations
* Updated Japanese translation
* Updated German translation
* Updated Russian translation
* Updated French translation

### Version 2.43 (31 October 2012)
* Fixed issue #47: Support non-standard SSH port for Test Connection
* Fixed issue #85: Integrated git-credential-winstore to installer to handle HTTP authentication
* Fixed issue #407: Added links to parent commit, branches and tags to commit info
* Fixed issue #517: Added button to go to superproject
* Fixed issue #528: Correctly terminate processes tree on abort
* Fixed issue #650: Enable reset for unstaged files
* Fixed issue #737: Support local commit.template
* Fixed issue #945: Delete tag from remote repository added
* Fixed issue #1021: Added option to always show checkout dialog
* Fixed issue #1135: Auto suggestion disabled in the Clone Repository dialog
* Fixed issue #1161: Jump list fixed
* Fixed issue #1173: Integraded NBug
* Fixed issue #1195: Email address HTML encoding fixed
* Fixed issue #1199: Warn user when reset file changes failed because it is in use
* Fixed issue #1201: Fixed url to MSysGit project page
* Fixed issue #1201: Git.cmd used instead of git.exe
* Fixed issue #1269: Default button and focus for checkout branch dialog
* Fixed issue #1276: Diff fixed when diff.mnemonicprefix = true
* Fixed issue #1290: Automatic file encoding detection improved
* Fixed issue #1295: Do not refresh revision grid after cancel create tag
* Fixed issue #1301: Escaping characters for git.cmd added
* Fixed issue #1303: Displaying pulled new repositories without HEAD
* Fixed issue #1309: Fix stash apply and drop error
* Fixed issue #1312: Fixed FormStatus layout
* Fixed issue #1319: Fixed FormStatus layout
* Fixed issue #1321: Fix Fetch all from toolbar
* Fixed description for repositories on dashboard
* Fixed two way merge fixed for Perforce Merge
* Fixed bug in rendering revision graph that a lane was grayed out
* Fixed commit dialog splitter position saving
* Use a checkbox for Commit Amend
* Improved support for high DPI systems
* Reset changes dialog improved for submodules
* Added generating text for submodules in file tree
* Added new icons
* Added "No tags" checkbox to Pull dialog and allow fetch to new local branch
* Show local changes radio group if state of working dir is unknown
* The diff tab now shows current diff
* Added options to "Reset file to" in diff tab
* Added "Reset file to this revision" to file tree
* Added hint to "Go to commit" dialog
* Added "Archive revision" to revision grid context menu
* Archive revision dialog improved
* Renamed Visual Studio plugin renamed from Visual Git to Git Extensions
* Updated merge scripts

### Version 2.41 (22 September 2012)
* Fixed issue #1254: Visual Studio 2005 plugin not working
* Fixed issue #1225: Visual Studio 2008 plugin not working
* Fixed window position in multi monitor configuration
* Fill title with the most recent commit message
* New dialog for checkout branch
* Remote repositories dialog layout changes
* Added option to not use the commit dialog to specify the commit message
* Added option to change application font
* Updated msysgit to a more stable build of version 1.7.11
* German translation updated

### Version 2.40 (19 August 2012)
* fixed issue #79: Don't offer to commit when resolving a stash conflict
* Fixed issue #875: Show untracked files in stash
* Fixed issue #980: spell check only covers first 5 visible lines in commit dialog
* Fixed issue #988: status bar for commit message
* Fixed issue #1018: Git Extensions sets core.editor to its own program without asking the user
* Fixed issue #1038: Add multiple selected files to gitignore
* Fixed issue #1042: Added option for force add submodule
* Fixed issue #1053: Exception when processing patch
* Fixed issue #1054: Backslash in URL caused invalid config file
* Fixed issue #1076: Added warning before dropping stash
* Fixed issue #1082: Always show commit button
* Fixed issue #1089: Fixed "n seconds ago" in revision grid
* Fixed issue #1092: Launching an external difftool (KDiff, or DiffMerge etc) leaves more and more processes (git, less and perl) open
* Fixed issue #1113: Show friendly error when deleting current branch
* Fixed issue #1113: Show friendly error when deleting unmerged branch without selecting force option
* Fixed issue #1114: Hide remode HEAD
* Fixed issue #1116: In some cases file differences are shown incorrect
* Fixed issue #1128: Removed buttons from Visual Studion Xml Editor toolbar
* Fixed syntax highlighting when + or - is in text
* Added line and column position to commit window
* Implement Mergetool/Difftool command suggestions for p4merge (Merge only) and BeyondCompare3 (Merge and Diff)
* Allow to specify custom local branch name during remote branch checkout
* Added extra highlighting for file differences in blocks
* Go to parent revision context menu item in revision grid added
* Cherry pick all selected revisions
* Gource plugin now support users avatars
* Plugin GitImpact can now display information including submodules
* Commit per user statistic now include information for submodules
* Added plugin for Gerrit code review
* Fixed solve merge conflicts on Linux when using kdiff3
* Russian translation updated
* Updated msysgit to version 1.7.11

### Version 2.33 (6 June 2012)
* Fixed issue 843: toolbar is disabled when in a wxs file
* Fixed issue 922: error during merge conflict resolve
* Fixed issue 951: install Git Extensions into 'All Progams' instead of 'All Programs\Git Extensions'
* Fixed issue 954: improve RSS feed deletion
* Fixed issue 955: GitHub plugin fixed for GitHub api 3
* Fixed issue 965: integrated text editor usability improvements
* Fixed issue 995: support github-windows and git URL link protocol
* Fixed issue 1000: added option to sign-off commits
* Added French translation
* Seprate commit button status icon if dirty only submodules
* Many bugfixes and minor changes

### Version 2.32 (20 May 2012)
* Fixed issue 85: http authentication support
* Fixed issue 357: commit Author name not using the correct encoding
* Fixed issue 570: Cut, Copy, Paste, Delete, Select all menu items in the Commit message field
* Fixed issue 605: submodule improvements
* Fixed issue 772: push tag on creation
* Fixed issue 773: file history dialog fails on large directory
* Fixed issue 825: add Lua & Perl sources support to statistics plugin
* Fixed issue 839: splash screen doesn't refresh
* Fixed issue 851: crash in AutoCheckForUpdates fixed
* Fixed issue 868: github username/apitoken invalid or network down
* Fixed issue 876: enable option "show current changes in toolbar" by default
* Fixed issue 876: enable option "show current branch in Visual Studio" by default
* Fixed issue 898: Github plugin disabled because it doesn't work with new GitHub API v3
* Fixed issue 906: fixed exception in blame dialog
* Fixed issue 915: checkout branch dialog added if no branch selected at starting commit dialog
* Fixed issue 925: apply patch should recognise unified format
* Fixed issue: select all files in commit dialog performance fix
* Fixed issue: merge conflict dialog crashwhen "Diff-Scripts" folder not exist
* Updated msysgit to version 1.7.10
* Added option "Open last working dir on startup"
* Added search dialog for diff files
* Added question to save changes after editing gitignore/gitmailmap/gitattributes
* Added syntax highlighting for javascript
* Added scrolling without focus
* Added "find/remove large files" plugin
* Added option to copy commit hash
* Added option to add reference when cherry pick a commit
* Improved commit templates
* Visual Studio 11 support added
* Ctrl-g as default hotkey for GoToLine
* Modified behavior of ToolStrips and MenuStrips to allow click-through behavior, eliminating the need for a double-click if window isn't focused.
* Disable menu items which actions require a repository if no repository is selected.
* Improved "missing SSH key" flow
* New "reset changes" dialog
* Stash untracked files is only supported in msysgit >= 1.7.7
* Fixed diff between "copied from file" vs "copied to file"
* Many bugfixes and minor changes

### Version 2.31 (18 March 2012)
* Fixed issue 745: test connection does not work with non-uri
* Fixed issue 829: Browse submodules menu item does not work
* Fixed issue 831: Remote Repositories dialog does not show Url
* Fixed issue 840: Positions of splitters of main screen aren't persisted
* Fix for "Index out of range exception" when no recent repositories are present.

### Version 2.29 (10 March 2012)
* Fixed issue 276: Make splitter visible on Dashboard
* Fixed issue 745: Test connection does not work with non-uri
* Fixed issue 756: Push dialog advanced options are cut off
* Fixed issue 651: Diff view does not clear out the file pane when switching to a commit that has no changes
* Fixed issue 727: Fix ours/theirs menu labels in the Resolve merge conflicts dialog
* Fixed issue 767: An exception is thrown in the revision graph
* Fixed issue 780: Relative author/commit date in the commit history graph is incorrect
* Fixed issue 783: 'View diff' Windows context menu item executes the item directly above 'Git Extensions'
* Fixed issue 791: Korean letters are not breaking in Translate dialog anymore
* Fixed issue 796: Remember 'No fast forward' turned on when merge
* Fixed issue 812: Fix occurrence of exception in FileTree when .gitattributes is not valid
* Fixed issue 816: Invalid path chars in FormBrowse.UpdateJumplist
* Fixed issue: GitExtensions crashes when detected wrong git version
* Fixed issue: shell extensions are duplicated in 64bit environment
* Fixed issue: an exception is thrown after moving the install folder (portable edition)
* Added support for some svn commands
* Added option to change Encoding in diff view
* Added “View commit” context menu item to “File History” dialog
* Added warning of not committed changes before checkout
* Added option to stash untracked files
* Russian translation updated
* Traditional Chinese translation updated
* Spanish translation updated
* Dutch translation updated
* German translation updated
* Many bugfixes and minor changes


### Version 2.28 (11 December 2011)
* Fixed issue 738: RevisionGrid error when option "Show current working dir changes" is enabled
* Fixed issue: all tabs are trimmed in blame control

### Version 2.27 (7 December 2011)
* Fixed issue 124: Submodules form now can remove submodule
* Fixed issue 127: Preview disabled for files larger than 5Mb, but available by link
* Fixed issue 138: Avoid auto stash when there is nothing to stash
* Fixed issue 139: Disable image paste in EditNetSpell
* Fixed issue 460: TortoiseMerge added to diff tools
* Fixed issue 566: QuickPull with Shift
* Fixed issue 581: improved revision graph loading performance. Replaced O(n) with O(1).
* Fixed issue 614: Filter for tags
* Fixed issue 627: The start page does not refresh upon changing the start pages settings fixed
* Fixed issue 631: Adding a new item to the start page settings auto-select that item
* Fixed issue 667: Push command support recursive submodules check
* Fixed issue 668: Git directory for submodules fixed with Git 1.7.7
* Fixed issue 718: Exception while filtering revision grid.
* Fixed issue 736: "Downloads" link fixed on project home page
* Fixed issue 735: ArgumentOutOfRangeException in "Format patch" dialog for first commit
* Fixed clone repository to bare repository
* Fixed open terminal emulator in Mono/Linux
* Fixed KdiffPath for diff and merge tool in Mono/Linux
* Fixed autostash with submodules
* Fixed 2 way merge for TortoiseMerge
* Updated msysgit to version 1.7.8
* Added support for staging/unstaging files with non-ASCI characters
* Added "Open containing folder" entry to the context menu of difference files
* Added "Rename branch" to context menu in revision grid
* Added Notepad++ to supported editors list
* Added clone button for uninitialized folders
* Added special context menu for submodules in the commit dialog
* Added blame window tooltip on authors
* Added email links to commit header
* Added hotkeys to translation form (Alt-Up, Alt-Down and Control-Down)
* Added option "Full history" to context menu in file history
* Added "Manipulate commit" context menu to file history
* Added support for merge multiple branches
* Added support for delete multiple branches
* Added support for drag & drop folders and urls onto dashboard
* Dashboard Editor displaying buttons fixed on monitor with big DPI
* Pull form now correctly updated after calling "Manage Repositories"
* When pushing a new branch and there is only one remote, this remote will be selected
* Diff warnings removing fixed
* Russian translation updated
* Spanish translation updated
* Dutch translation updated
* German translation updated
* Many bugfixes and minor changes

### Version 2.26 (29 October 2011)
* Fixed issue 629: Support of separate Fetch/Pull Urls for remotes
* Fixed issue 685: Unable to set location of global git config file in Environment tab
* Fixed revert commit dialog
* Added selection filter for unstaged files in commit dialog (ctrl+f)
* When trying to pull-rebase a merge commit a warning is given
* Updated msysgit to version 1.7.7.1
* Traditional Chinese translation updated
* German transation updated
* Many bugfixes and minor changes

### Version 2.25 (16 October 2011)
* Fixed issue 283: Splitter position on dashboard is not remembered
* Fixed issue 543: Show current branch in main screen
* Fixed issue 533: UI/translation bugs on lost commits recover form
* Fixed issue 587: do not try to commit empty changeset
* Fixes issue 616: Suggested command for TortoiseMerge is incorrect
* Fixed issue 628: the start page settings page has improper alignment
* Fixed issue 632: RSS Feeds on the Start Page scroll unnecessarily
* Fixed issue 652: Opening a new repository does not reset the current commit view
* Fixed issue 656: Error viewing Blame Tab on binary file
* Fixed issue 661: "Refresh on form focus" option in the Commit dialog does not display its setting.
* Fixed issue: 'clone/fork GitHub repo' throws an exception if no GitHub credentials are known
* Fixed issue: remove '.git' from targer directory if the original repository ends with '.git'
* Fixed issue: in some cases warnings are shown in the commit dialog instead of the staged files
* Fixed issue: the merge dialog does not close after all merge conflicts are solved
* Updated msysgit to version 1.7.7
* Updated command for Beyond Compare's 3-way merge
* Updated Dutch translation
* Updated Italian translation
* Exclude remote tracking branch from the "merge with" and "rebase on" menu items
* Highlight remote tracking branch in the browse dialog
* When searching in the diff view, all files are searched
* Added option to GitHub plugin to clear account info
* Added a new variable '{UserInput}' which can be used in scripts to ask user for input
* Added "Add filepattern to .gitignore" dialog
* Added support for global configuration commit.template
* Added more hotkeys
* Added menu in revision grid to manipulate commit (revert, cherry pick, squash, fixup)
* Added new gravatar fallback option Retro and None
* Added German translation
* Many bugfixes and minor changes

### Version 2.24 (25 July 2011)
* Fixed revision filter

### Version 2.23 (23 July 2011)
* Fixed issue 452: "Show repository status" always updates status every second
* Fixed issue 455: Cannot initialize repository in Visual Studio solution root
* Fixed issue 464: Added option to add custom script to toolbar in browse dialog
* Fixed issue 465: "Create branch" dialog closes immediately after opening by enter key
* Fixed issue 471: Implement option to use "Patience diff" algorithm
* Fixed issue 479: Added the 'keep index' option to the 'ApplyStash' form
* Fixed issue 500: number of changed files is shown wrong on the browse form toolbar
* Fixed issue 505: Visual Studio toolbar does not show current branch name
* Fixed issue 512: Error when opening "Translate" dialog
* Fixed short name creation for repositories located in root directories
* Hotkeys can be assigned to custom scripts
* Difference view font can be changed
* Revision log style updated
* The current branch is highlighted in commit log
* The current branch is shown in caption
* Added support for Windows 7 "Recent" Jumplist to quickly open repositories
* Added 'ignore-whitespace' option when applying patches
* Added option to save stash with custom message
* Added setting to choose "Truncate long filenames" method
* Save setting for 'ignore whitespace' checkbox on 'ApplyPatch' form
* Optimized "Recover lost objects" dialogs
* Clear FileViewer when only unstaged file is reverted
* Updated msysgit to version 1.7.6
* New application logo
* Many bugfixes and minor changes

### Version 2.22 (16 May 2011)
* Fixed issue 142: Changed main window title to repository name instead of path
* Fixed issue 375: Save on Commit in Visual Studio
* Fixed issue 436: Cannot order branches by date in "Delete obsolete branches" plugin
* Fixed issue 439: .gitconfig file is created in Program Files instead of Home path
* Fixed problem with line endings warning in commit dialog
* Fixed wrong text on stash refresh button tooltip
* Application can be installed for all users
* Added drag file from filetree in browse dialog
* Added Traditional Chinese translation
* Added plugin to create local tracking branches for all remote branches

### Version 2.21 (16 April 2011)
* Fixed issue 423: "GitExtensions has stopped working" at startup

### Version 2.20 (14 April 2011)
* Fixed issue 406: keyboard language changes in commit dialog
* Fixed issue 415: Diffs in Stash view are reversed
* Fixed issue 417: popup complains about missing .gitconfig on Linux
* Fixed browse dialog size issues on Linux
* Fixed issue when deleting remote branches
* Fixed translations on Linux
* Fixed SaveAs problems
* Added hotkeys for ShowAllBranches and ShowCurrentBranchOnly
* Added more commandline options
* Show current branch in Visual Studio toolbar (default off)
* Scripts can be executed before and after actions* *

### Version 2.17 (15 March 2011)
* Fixed exception thrown while loading Visual Studio plugin
* Fixed refresh issues

### Version 2.16 (13 March 2011)
* Fixed refresh issues

### Version 2.15 (10 March 2011)
* Fixed issue 399: blame is showing source code double space
* Fixed issue 400: commit summary with polish letters in author names shown incorrect
* Fixed "show ignored files" in commit dialog
* Fixed context menu on web project in Visual Studio
* Fixed view pull request in GitHub plugin
* Fixed "Open with difftool" when comparing two or more commits
* Fixed problem with polish "n" letter and sudden font change
* Added "Follow file history" to file history context menu
* Added hotkeys for FileViewer and FormResolveConflicts
* Added advanced option to merge dialog
* Added "Go to commit"
* Installer checks for .Net 3.5
* Updated stash view
* Updated kdiff3 to version 0.9.95-2
* Various bugfixes and minor changes

### Version 2.14 (5 March 2011)
* Fixed crash when viewing binary file in blame
* Fixed settings dialog on Linux host
* Fixed commit dialog on Linux host
* Added option to edit git notes
* Added settings page to configure hotkeys
* Removed shell extensions check when it is not installed

### Version 2.13 (3 March 2011)
* Fixed issue 391: application crashes when repository favourites contains empty item
* Double click on line in blame jumps to commit instead of opening commit

### Version 2.12 (2 March 2011)
* Fixed issue 368: Browse button does not open dialog in HOME dialog
* Fixed issue 373: Archive command is not working
* Fixed issue 376: Added "Save As" button to file history
* Fixed issue 379: Find Window Sticks in Windows XP Taskbar
* Fixed issue 381: autostash is not needed when fetching without merge or rebase
* Fixed "Open With" and "Open" on binairy files
* Added "dirty index" icon to formbrowse
* Added hotkey support to CommitForm for keyboard navigation (Ctrl+1 t/m Ctrl+5)
* Added impact graph plugin
* Added Simplified Chinese translation
* Added support for renamed and copied files
* Added support for git notes
* Various bugfixes and minor changes

### Version 2.11 (28 January 2011)
* Fixed crash when refreshing repository grid

### Version 2.10 (27 January 2011)
* Fixed issue 78: refresh commit dialog on focus (optional)
* Fixed "Recover lost objects"
* Fixed layout issues in gource plugin dialog
* Fix FileTree behaviour when right-clicked on empty space below last node
* Spanish translation updated
* Layout commit dialog optimized
* Patch files can now be dropped from file explorer onto the revision grid
* Added option to enter a different author for a commit to the commit dialog options
* Added syntax for Pascal, Python and Ruby files to Statistics plugin
* Added Github integration
* Added "Reset selected line(s)" to context menu in commit dialog
* Added option "Draw non relatives text grey" to render commit message grey when commit is not a relative
* Added autocomplete to various dropdown boxes
* Added icons to most messageboxes
* Added "pull all" button to pull dialog

### Version 2.09 (30 December 2010)
* Fixed issue 92: added AcceptButton on Push dialog and fixed tab order in push and pull dialog.
* Fixed issue 289: added support for MonsterId, Identicon and Wavatar when user has no Gravatar
* Fixed issue 304: cannot load commit log
* Fixed issue 305: binary files are not saved properly when using "save as..."
* Fixed issue 318: when pushing new branch, track it automaticall
* Fixed bug: commands Clone and Initialize in Visual Studio plugin are not always enabled
* Fixed "automatically configure the default push..." feature when adding remote
* Fixed a pretty significant slowdown caused by the toolbar status when browsing large repositories
* Fixed the following keys in quicksearch: '#', '_'
* Fixed error page in browse dialog when git is not configured properly
* Added "stage selected lines" option to commit dialog
* Added Italian translation
* Added Russian translation
* Added Russian spelling dictionary
* Added dialog to choose language at startup when no language is set
* Added resolved directories to FormFixHome to make it easier to choose the correct option
* Added support for merging office documents (doc, docx, odt, ods, sxw)
* Added "Diff contains" filter to search for commits containing text
* Added support for Beyond Compare 3
* Added combobox to branch in browse dialog to allow quick switching between local branches
* Added shortcuts 's' and 'u' to stage/unstage files in commit dialog
* Added images to file tree
* Added Ctrl + Enter shortcut to commit quickly
* Added "Reset file changes" to the explorer context menu
* The statistics plugin does its calculates in background thread
* Doubleclick on file in commit dialog (un)stages the file
* Improved merge conflict handling
* Various bugfixes and minor changes

### Version 2.08 (25 November 2010)
* Fixed bug: installing MSysGit or KDiff3 using complete settup doesn't work when UAC is enabled
* Fixed bug: slashes in tag name not supported
* Fixed bug: filtering does not work with characters outside ASCII range
* Fixed bug: bash does not work using Linux or cygwin
* Fixed bug: "gitk" and "git gui" does not work on linux
* Fixed bug: file history does not work when started from file tree in browse dialog
* Added shortcut key F3 to open diff tool op file
* Added stash count to toolbar (is turned off by default)

### Version 2.07 (15 November 2010)
* Fixed issue 263: spanish translation added
* Fixed issue 262: application crashes on system where only .Net 4.0 is installed
* Fixed issue 255, 257, 261, 266: application crashes in Windows 7
* Fixed bug: Git Gui & GitK not started in working dir
* Fixed bug: Application sometimes closes for no reason
* Fixed bug: GitExtensions now uses .gitattributes to determine if a file is binary
* Added bisect function

### Version 2.06 (10 November 2010)
* Fixed issue 218: GitCommandLog now in LIFO order
* Fixed issue 235, 242 and 248: GitExtensions hangs when loading putty key into pageant
* Fixed bug: comboboxes affected each other in clone dialog
* Fixed bug: sporadic crashes during checkout process when status is visible in toolbar
* Fixed bug: virtual commits showing uncommitted and staged changes are only visible when needed
* Added checkbox to allow interactive rebase
* Added support for Windows 7 taskbar progress bar display
* Added support for cherry pick merge commit
* The "Format patch" dialog now remembers the last (valid) directory
* Startup speed improved

### Version 2.05 (30 October 2010)
* Fixed issue 194: Remote branches combo contained tags also
* Fixed bug: fixed pre-selecting tracking branch in push dialog
* Fixed bug: pull form no longer shows default remote branch to pull, this caused unwanted behaviour
* Fixed bug: file history doesn't work in some cases when casing on disk is different then in git
* Fixed bug: selecting initial revision on RevisionGrid launched from FileStatusList
* Fixed bug: characters '.', ',' and '-' do not work in quick filter
* Fixed bug: pull using rebase didn't work properly when no remote branch is selected
* Japanese translation improved
* Improved filter
* Added current working dir changes and commit index to revision grid, default disabled
* Added "Push & Commit" button to commit dialog
* Added branch filter to browse dialog
* Added "save as" button to diff view in browse dialog
* Added statusbar for warnings
* Added option to also show remote branches that contain the selected commit in the commitinfo

### Version 2.04 (30 September 2010)
* Fixed issue 173: Unhandled exception when selecting files in Commit dialog - Working dir changes
* Fixed bug: spellings checker allows using Japanese
* Added possibility to pull non-default remote branch into current branch
* Added contributors to about box
* Added option to choose merge strategy for merge
* Improved startup speed
* Non-relative branches are drawn gray
* Pull dialog shows default remote branch if one is configured for local branch
* Removed ctr-c key from commit info control to allow copy part of commit info
* GitExtensions now can be used in Linux using Mono

### Version 2.03 (18 September 2010)
* Fixed bug: exception when deleting repository form dashboad using dashboardeditor
* Fixed bug: Settings for autocrlf are now the same as in the msysgit installer
* Fixed bug: revision header in commitinfo was not cleard between changing selection
* Fixed bug: branches with '/' are not handled correct
* Fixed bug: merge conflict dialog doesn't support spaces in filename
* Added option "showErrorsWhenStagingFiles" to hide wasernings when staging files
* Added basic file editor to replace vi
* Updated git to Git-1.7.2.3-preview20100911.exe
* Improved performance for rendering graph
* Added "Delete obsolete branches" plugin
* Improved cygwin support

### Version 2.02 (1 September 2010)
* Fixed issue 149: the system just hangs during a fetch
* Fixed bug: changing encoding in settings is not saved properly
* Fixed bug: no branches found in remotes dialog
* Fixed bug: application shows exception and fails to close when application settings file is corrupt
* Fixed bug: "No commits" screen in revision grid not shown
* Fixed bug: "download gource" not working in gource plugin
* Fixed bug: encoding not handled correctly in commit message and filenames
* Fixed bug: blame doesn't refresh properly
* Added "Copy filename to clipboard" to context menu's in browse dialog
* Last commit message is stored in settings so it can be used in multiple repositories

### Version 2.01 (26 August 2010)
* Fixed issue 144: encoding in 'Diff' view ignores settings
* Fixed issue 143: hitting refresh in browse dialog causes application to freeze
* Fixed bug where multiple value entries in a config file section were being stripped from config file

### Version 2.00 (22 August 2010)
* Fixed bug: refreshing and loading gravatars caused GitExtensions to crash
* Fixed bug: settings check not shown properly when a problem was found

### Version 1.99
* Fixed issue 81: when staging a file, all partially staged files are removed from unstaged list
* Fixed issue 107: alt+f4 not working when revisiongrid has focus
* Fixed issue 111: Annotated Tags Appear Twice in Push Dialog
* Fixed issue 113: Statistics: "Lines of Code per type" and "Lines of test code" fail in empty or non code repo
* Fixed issue 114: Adding filter which results in no commits shows "empty repository" UI
* Fixed issue 119: show files in FileHistory also prior to rename
* Fixed issue 120: choose branch when adding new submodule doesn't work
* Fixed issue 121: Local branch and remote tracking branch reverse order on selection
* Fixed issue 122: Visual Studio 2010 solution file
* Fixed issue 123: Translation String is missing in Delete branch conformation dialog
* Fixed issue 126: Added editor for .gitattributes
* Fixed issue 129: loading submodules submenu is very slow
* Fixed issue 131: Add a blame funtion (commandline: GitExtensions blame [filename])
* Fixed issue 135: settings not saved when closing application
* Gravatars are not longer stored in the IsolatedStorage, but use the ApplicationData path
* Default windows font is used instead of Segoe UI
* Add search file function to file tree in browse dialog (ctrl+f)

### Version 1.98
* Fixed issue 105: Allow to open "gitex browse" with a given filter.
* Fixed issue 106: Show all branches which "contain" a given commit in their history.
* Fixed issue 107: Alt+f4 and other function keys not working when rvision graph has focus.
* Fixed issue 108: Apply patch files from directory not working.
* Fixed bug: Git Extensions crashes when opening certain repositories (e.g. linux kernel)
* After opening the FileHistory window the selected revision will be displayed.
* Only administrators can install Git Extensions.

### Version 1.97
* Fixed bug: Empty local config file is saved to c:\ when not in a repository

### Version 1.96
* Fixed issue 95: The colored application icons are broken.
* Fixed issue 96: System.Exception: Invalid section name: submodule
* Fixed issue 100: Adding a remote and providing a private key file will cause the .git/config file to be corrupted.
* Fixed issue 101: Searching for an Author name in Filter field makes Git Extensions crash
* Fixed issue 103: Added check when initializing repository on a file
* Fixed bug: Progress dialog could cause application to crash
* Added branch filter to advanced filter dialog
* Tweaked the graph to try to keep lanes that get merged together close to each other so there aren't as many lane crossings

### Version 1.95
* Fixed bug: difftool only worked in git > 1.7.0
* Fixed bug: dashboard caused application to crash
* Fixed bug: refreshing gravatar that was already deleted caused an exception
* Fixed bug: when clicking "dictionary" submenu in spelling checker context menu the current word was replaced
* Fixed bug: ctrl-a in FileStatusListBox not working
* Fixed bug: starting "gitextensions init" from the commandline without path causes an exception
* Fixed bug: AccessDeniedException thrown when saving hidden .gitignore or .mailmap file
* Fixed bug: Removed empty root node from tree in file tree
* Fixed bug: If HOMEDRIVE is defined, launching Git Bash would result in a different home directory than GitExtensions
* Fixed bug: Squares where shown instead of Russian chars
* Fixed bug: Revision graph is drawn incorrect in some cases
* Fixed bug: When merging files the autocrlf settings are obeyed
* Fixed bug: Run the GUI difftool locked the application until closing the tool
* Added Japanese translation
* Added Dutch translation
* Added functionality for creating and editing translations
* Added link to report issues to dashboard
* Added option to mark ill formed commit messages
* Added the ability to move to the prev/next quickseach string by hitting alt+arrowup/alt+arrowdown
* Added plugin to start gource repository visualizer
* Added toolbar to diff viewer to jump to next change
* Added option to show nonprinting characters in file viewer
* Added file explorer option to the file menu
* Added from and to branch to push dialog
* Added option to create annotated tag
* Added splash screen
* Added warning when about to push a branch that doesn't exist on a remote yet
* Added difftool option to FileHistory
* There is a new revision graph (thanks to Nick Mayer)
* Quicksearch now also searches in branches
* All windows positions are now saved
* Settings dialog performance improved
* The scoll position in file viewer is saved when switching revision.
* Author image size can be set in context menu of gravatar control
* The diff viewer now shows the old sha1 and new sha1 when viewing a submodule diff
* The default web proxy is used to connect to internet
* Replaced checkbox on merge branch dialog for more user friendly radio buttons

### Version 1.93
* Fixed bug: wrong icon drawn in commit dialog for deleted files
* Reset selected files in the commit dialog asks to delete new files.
* Author images are only cached for 5 days now
* Added start page
* Added 'force' option to checkout revision and checkout branch
* Added 'rebase' to context menu in revision grid
* Added 'reset chunk of file' option to commit dialog
* Added drop down menu to the 'open' button in the browse dialog
* Added support for WinMerge as diff tool

### Version 1.92
* Fixed bug: the application hangs randomly
* Fixed bug: quotes in local settings were not escaped properly
* Fixed progress bar when pulling and fetching under git > 1.7.1
* There is a new installer to address the following issues (thanks to Jacob Stanley)
* * Updating existing installations
* * Install both 32bits and 64bits shell extension on 64bit systems
* Added icons to indicate if a files is added, modified or removed
* Author images from gravatar.com added to commit info panel
* User Manual updated
* Added quicksearch on author and message in revision grid
* Added commitinfo panel to blame view

### Version 1.91
* Fixed bug: tooltips drawn incorrect

### Version 1.90
* Fixed bug: images in .gif and .ico format could not be displayed.
* Fixed bug: extra empty lines are shown in the blame view.
* The application icon can be changed
* Added extra diff highlighting for single line changes
* Added asynchronous loading of files for diffs and general viewing to improve responsiveness
* Added "Show relative date" option to context menu of revision grid
* Copy from diff viewer copies code without '+', '-' or ' ' markings
* Added "Open with difftool" option to diff view in browse dialog
* Added tooltip to show long filenames in commit dialog
* Added tooltip to show long filenames in diff view of browse

### Version 1.89
* Fixed bug: Changes of an unstaged file are not shown when the file path contains spaces
* Fixed bug: Filehistory not working when opened from Visual Studio because filename is too long
* Fixed bug: Copy selected text from file viewer now copies the complete selected text
* Fixed bug: A slash is added at the end of all url's in the history, breaking some urls
* The author date and commit date are both shown when they differ
* The author and committer are both shown when they differ
* Added option to show author date in revision graph instead of commit date
* Added "No fast-forward" option to merge dialog
* Added "Open With Difftool" to context menu in commit dialog
* Added drag file support to unstaged items in commit dialog

### Version 1.88
* Fixed a bug stopping the commit dialog from refreshing after a commit
* Removed the tooltip delay on recent repository list
* Fixed settings when using git.exe

### Version 1.87
* Small fixes in layout commit dialog
* Only close commit dialog when no modified files are left
* Added 'Open' and 'Open With' to context menu of commit dialog

### Version 1.86
* Changed the layout of the commit dialog
* Added support for Cygwin
* Added cleanup function

### Version 1.85
* Added context menu options to manipulate Diff viewer.
* Fixed alt-c as shortcut to commit.
* Added cherry pick commit to context menu of revision grid
* Added 'treat file as text' option to diff viewer

### Version 1.84
* Added support for Visual Studio 2010 (beta)
* Fixed 'Add to .gitignore' function in commit dialog.

### Version 1.83
* Added "Merge with branch" context menu item to the revision grid
* Added Ctrl+M as a shortcut key for Merge
* Added Ctrl+. as a shortcut key for Checkout Branch

### Version 1.82
* Added 'Add to .gitignore' button in context menu in commit dialog
* When no commit message is entered and 'Amend to last commit' is pressed, the last commit message will be entered as commit message.
* A menu is added to the 'Commit message' label.
* Menu's under labels in the commit dialog are indicated by a icon
* 'Show ignored files' option added to the 'Working dir changes' menu in the commit dialog

### Version 1.81
* Fixed bug in file history
* Scroll position in blame view will be saved when selecting other revision
* Follow renames in file history is now optional.

### Version 1.80
* File history follows file renames
* Added paragraph about ssh to User Manual
* Added force option to add files dialog

### Version 1.79
* Added push force support for branch pushing
* Added push force support for tag pushing
* Added appearance tab to settings dialog.
* Added alt-c, alt-a and alt-r as keys for commit, amend and rescan.
* Added checkbox "close dialog after commit" on commit dialog
* Removed Load button from view patch dialog. Patch is now loaded when file is selected.
* Added context menu to merge conflict dialog
* Added "Abort" button to merge conflict dialog
* Most dialogs changed to support larger DPI settings.
* Delete commit message after succesful commit.
* Added waitcursor to FormResolveConflicts.
* Set width to submodules submenu and recent repositories submenu.
* Added F3 and Shift+F3 to code dialog.
* Bug fixed that the branch drop down box in the pull dialog was empty.

### Version 1.78
* Fixed bug causing the commitmessage to contain encoding marker bytes
* Fixed bug preventing the bash to show
* Recent repository list is updated when opening submodule or loading from Visual Studio
* File history fixed for deleted files

### Version 1.77
* Fixed bug in version 1.76 that file history was broken when started from Visual Studio
* Commit message now supports multiline (first line is the summary)
* Revision grid resizes graph column automatically.
* Settings user.name and user.email are only saved when changed. This to work around a bug that utf8 chars are only allowed when editing the .config file manually.
* Recent repository list handles doubles better.

### Version 1.76
* Fixed ut8 national chars con### Version
* Added setting to ignore directories from statistics
* Added syntax and find to blame view
* Toolbar in Visual Studio learned to remember the last position

### Version 1.74
* Diff syntax highlighting improved
* Various small bug fixes
* Added statistics plugin

### Version 1.73
* All dialogs can be closed with the ESC key
* Modal dialogs are not resizable anymore
* Added context menu on revision file tree (Save As, Open, Open With)
* When selection a revision, the file tree remembers the last selected file
* Fixed crash when starting from command line with invalid path
* Added tooltips to startpage
* "Auto compile submodules" plugin can be started from menu even when disabled

### Version 1.72
* Fixed bug in "Auto compile submodules" plugin
* Added progress dialgo to "Check for updates" plugin

### Version 1.71
* Added support for plugin
* Added "Check for updates" plugin
* Added "Auto compile submodules" plugin
* Improved start page
* Added Close to file menu
* Packed with Git-1.6.4
* Packed with KDiff3 0.9.95

### Version 1.70
* Added support for mergeconflict on submodules
* Local AutoCRLF now has three options: true, false, input
* Main windows opens on last position
* Added shortcuts and default buttons

### Version 1.69
* Fixed updating submodules recursive when having multiple submodules

### Version 1.68
* Improved 'Recover lost objects' dialog. Now all lost objects can be found and finding a specific commit should be much easier now.
* Use Environment.NewLine instead of "\n".
* Fixed a bug in default .gitignore file, it would show the [Db]ebug directory. It should be [Dd]ebug directory ofcourse.
* Added Resharper directories to .gitignore file.
* Added recursive submode commands. When cloning a submodule containing nested submodules, all nested submodules can be initialized. There are also recursive initialize, update and synchronize commands added to the submodule menu.
* Added checkout branch to the revision graph context menu.
* When adding a remote repository the remote branches can be configured automatically.

### Version 1.67
* Fixed exception when AutoCRLF is left empty
* Clicking 'Rescan changes' twice no longer results in
* Mergeconflict dialog closes when there are no mergeconflicts left
* Pull dialog closes after successful pull
* Delete branch from revision grid now uses the correct dialog
* Fixed spelling mistake in warning dialog for 'Amend to last commit'
* Packed with Git-1.6.3.2

### Version 1.66
* Fixed exception when AutoCRLF is left empty

### Version 1.65
* Option added to order revision by date or by branch
* Patches can be mailed from the format-patch dialog
* Global AutoCRLF now has three options: true, false, input
* Bug solved: When an item is staged when opening the commit dialog, unstaging it always marked it as untracked

### Version 1.64
* Commits are now ordered by date
* Fixed bug in commit dialog

### Version 1.63
* Added file filter to revision graph filter
* Shortened title of browse window
* Added checkout revision context menu in the Browse list
* Added a Recent Repositories submenu in the File menu
* Added commit hash to commit info dialogs
* Close solve mergeconflicts dialog automatically after all conflicts are solved
* Commit message is now remembered when commit dialog is closed without making the commit
* Packed with Git-1.6.3

### Version 1.62
* Fixed exception when right click on gridheaders in revision log
* Last selection and scroll position in revision log are remembered
* Added "Delete brach" function to context menu revision log
* Packed with Git-1.6.2.2

### Version 1.61
* Fixed bug that mergeconflicts of files listed in .gitignore are hidden

### Version 1.60
* Fixed bug in Manage remotes dialog

### Version 1.59
* Remote branches can be checked out and tracked from the checkout branch dialog
* Manage remote branch dialog improved

### Version 1.58
* Added support for Araxis merge and DiffMerge

### Version 1.57
* Spelling checker performance improved
* Red wiggly lines added to mark incorrect spelled words
* Added search dialog to most file viewers
* Added delete tag function to context menu revision grid
* Added add tag function to recover lost objects dialog
* Added enter support to the small create tag and create branch dialogs
* Fixed file history view on deleted or moved files
* Packed with Git-1.6.1 to avoid a bug in Git-1.6.2
* When installing 64bit version, both 32bit and 64bit shell extensions will be registered
* Application settings are saved when closing settings dialog instead of when application exits
* Revert commit handles merge conflicts better
* Diff in browse dialog now shows the diff between revisions if 2 revisions are selected
* Bug solved: files in diff viewer are not shown correctly when 2 revisions are selected
* Format path dialog improved
* Clone and Initialize dialogs start with directory filled in

### Version 1.56
* Settings are saved when dialog is closed not only on OK
* Load PuTTY key dialog is closed after loading key
* Mergeconflicts during apply stash after pull are shown
* Revision log is now refreshed correctly after using stash from toolbar
* Added spelling checker

### Version 1.55
* Fetching a branch that does not exists locally will create a new branch with the same name locally
* Open repository after clone improved
* Added 'quick' stash and stash pop
* Some settings are solved silent
* Packed with Git-1.6.2-preview20090308.exe

### Version 1.54
* Pull dialog will not be closed if pull failed

### Version 1.53
* Fixed open repository after clone

### Version 1.52
* Multiline commit messages can be viewed
* Fixed bug preventing multiline message to be committed correctly
* Added option to show relative date in commit log
* Added recover lost objects function
* Fixed small refresh bug in FileSystemWatcher
* Performance FileSystemWatcher increased a little
* Fixed enter key in filter
* Basic filter now ignores case and searches in commit message and author
* Added advanced filter in context menu commit log
* Added autocrln option
* Added option to push tags
* Added submodule support
* Many small bugfixes

### Version 1.51
* Staging/unstaging is much faster
* Whitespaces are ignored in blame view
* Settings dialog starts up faster
* Added sorting
* FileSystemWatcher is used to check if index is changed
* Menu's are disabled when not in a repository

### Version 1.50
* Added double click on file in diff-tree in browse window
* Process dialog is can now set to close automatically when process succeeds
* Added waitcursors
* Option added to show the Git commandline dialog during a process
* Revision graph can be disabled in settings
* Added doubleclick in commit dialog
* Added stage/unstage all buttons to commit dialog ("Files to commit" is now a dropdown menu)
* Most windows are now shown before initializing, to show users that GitExtensions is busy
* Added push and pull to the toolbar in the browse dialog
* Added picture viewer to commit dialog

### Version 1.49
* Fixed crash when loading some repositores (git://git.kernel.org/pub/scm/git/git.git)

### Version 1.48
* Added ChangeLog

### Version 1.47
* Commit count per user now counts all branches
* Fixed bug that local settings could not be saved
* Fixed bug that remote branches could not be saved

### Version 1.45
* No more taskbar terror!
* Added "prune" button in remotes dialog
* Browse window title includes working dir
* Added diff view to browse window
* Commit dialog improved (working dir changes is now a menu!!!)
* Default .gitignore file added (for use in Visual Studio and/or resharper)
* Fixed a bug letting GitExtensions hang when using OpenSSH
* Fixed a bug that some mergeconflicts where ignored
* Fixed some minor bugs

### Version 1.44
* Fixed some problems in new Visual Studio plugin

### Version 1.42
* Fixed VS2005 plugin

### Version 1.41
* Fixed a bug in Visual Studio plugin

### Version 1.40
* Better Visual Studio intergration.

### Version 1.38
* Most dialogs are closed after a task is finished with success
* Clone dialog is easier to understand
* Fixed a spelling mistake
* Settings dialog will now have a normal size when started on gitextensions startup
* Filter feature added

### Version 1.37
* Added diff highlighting (+/-) highlighting
* Auto-fix settings refreshes automatically

### Version 1.36
* Improved performance of commit dialog a lot.
* Minor changes

### Version 1.35
* Fixed bug causing multiple config entries
* Improved solve mergeconflict features
* Small changes to improve usuability

### Version 1.30
* Added support for custom mergetools
* Fixed settings for git 1.6.1.xxx
* Improved patch and rebase features
* Removed a lot of annoying mergeconlict popups
* 32 bit and 64bit support is now in same setup
* Fixed (and probably created) some bugs
* Some small 64bit Windows improvements (auto-settings, some paths)

### Version 1.28
* Added 64-bit support
* Added installer that also installs git and kdiff3
* Added commandline support (gitex.cmd), see gitex help for list of commands
* Improved auto resolve settings
* Added commit revert
* Various small improvements

### Version 1.27
* Removed usage of git-clone.exe because it cause problems.

### Version 1.26
* Fixed shell extensions....

### Version 1.25
* PuTTY is working fine not (at least for me)
* Setup is improved
* Rebase improved
* As long as you use PuTTY and not OpenSSH, all is fine.

### Version 1.21
* Fixed some small bugs that sneeked in version 1.20

### Version 1.20
* Added PuTTY support.
* PuTTY can now be used instead of OpenSSH.
* When using PuTTY the commandline windows that are needed for entering
* OpenSSH passprase are not needed anymore.
* PuTTY private keys can be configured per remote, so key is automatilly loaded.

### Version 1.14
* Improved rebase features a bit.
* Minor bug fixes.

### Version 1.13
* I'm still focussing on the push and pull features, because I use this a lot myself.
* Improved auto-settings-correct features
* Added rebase features
* Improved merge conflict handling a bit.

### Version 1.12
* Fixed lots of remote feature mistakes and added some missing features.
* Push/pull/fetch should work as suppost to.
* Added multiple stash support.
* Did some testing, fixed minor bugs.
* Know bug: delay loading commit graph is broken, I will fix this in the next release!

### Version 1.11
* Added remote repository functions push/pull/fetch improved delete improved some small changes and bug fixes

### Version 1.09
* Added Visual Studio 2005 and Visual Studio 2008 plugin to the setup as options.
* Added .mailmap edit
* Minor changes No bugfixes

### Version 1.08
* Fixed drop down boxes in clone dialog
* Added archive function
* Fixed commit count
* Fixed using " (quote) in commit message

### Version 1.07
* Path of git.cmd can be edited
* Format path fixed
* Auto-config: register files fixed

### Version 1.06
* Fixed file history diff, broke since using git.cmd

### Version 1.05
* Unstage files problems fixed Problems cause by replace git.exe for git.cmd fixed (I hope)
* Added progress dialog for a few commands

### Version 1.04
* Using git.cmd instead of git.exe.

### Version 1.03
* Bugs in commit solved
* Added support for empty repositories

### Version 1.02
* Added some usability changes
* Added delete tag function
* Added "reset branch" function

### Version 1.01
* Blame fucntion added to file history
* Fixed working dir detection
* Small performance improvements

### Version 1.00
* Bugs in delay loading fixed
* Added amend to commit dialog
* Added init dialog
* Added init central/bare repository
* Added support for bare repository
* Minor performance increase
* Several minor bugfixes
* Fixed bug in mergetool.keepBackup setting, this should work now

### Version 0.98
* Performance and stability increased
* Bug that not all files where staged properly fixed
* Added commit count per user dialog
* Memory usage optimized

### Version 0.93
* Performance of commit is increased (about 2x faster).
* The installer is not needed anymore.
* The program will check if it is installed correct and offers to fix the problems.
* The installer is still recommended, because it adds icons and uninstaller.

### Version 0.92
* Fixed a bug in clone/push/pull.

* For this version I also added a non-installer version. This is just a zip file that cointains the binairy files.
* Please note that this is just the standalone application without shell extensions!

### Version 0.91
* Rewritten most of commit logic. This works better now.
* Colors added on tag/branch/stach labels
* I also added a directory history on open/push/pull/clone, just to increase useabillity

### Version 0.9
* Removed Visual Studio plugin
* First stable release.
* Please uninstall older versions carefully.

### Version 0.71
* There was a nasty bug in the shell extensions in the last release, this caused explorer to crash. This is fixed now.
* Lots of cosmetic changes too.
* Reinstalling since version 0.7 shouldn't be a pain anymore singe there is no .net registration.

### Version 0.7
* Rewritten shell extensions in c++, the program is a lot more stable now.
* Fixed some small bugs, added lots of warnings and messages to prevent users from making mistakes.
* Some small features added, nothing mayor since 0.6.

### Version 0.6
* Add files
* Apply patch
* Create branch
* Checkout branch
* Checkout revision
* Cherry pick
* Delete branch
* Clone
* Commit
* Format patch
* Init new repository
* Pull
* Run mergetool to solve merge conflicts
* Push
* Basic settings
* Stash
* View diff
* View patch file

### Version 0.5
* Pull/Push/Patch improved
* Better feedback when an error occurs
* Shell extension also works on directories
* Dialogs are modal now
* VS2008 doesn't crash on errors anymore (which was very anoying!)

### Version 0.4
* Apply patch is now working
* View patch is now ok

### Version 0.31
* Added icons in the setup

### Version 0.30
* Added install
* Many bugfixes

### Version 0.2
* Added apply patch interactive
* Added change basic git settings
* Added syntax highlighting (using ICSharpCode)
* Performance increased a bit
* Many bugfixes

### Version 0.1
* Added shell extension
* Added visual studio 2008 plugin
* Added standalone UI
