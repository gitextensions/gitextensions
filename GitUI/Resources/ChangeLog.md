Changelog
=========


### Version 4.0.2 (15 Jan 2023)

#### Changes:
* [#10621] Add high contrast theme file
* [#10604] Stabilize FormBrowseTests regarding Left Panel
* [#10601] Avoid ambiguity of branch filter
* [#10590] ConEmu 221218
* [#10581] Store plugins translations in *.Plugins.xlf
* [#10559] Notify Left Panel of filtering for current branch
* [#10556] Delete files if resetting new, not yet committed files
* [#10548] Bump Microsoft.VisualStudio.Composition version
* [#10546] Avoid double quoting while showing File History dialog
* [#10545] Do not throw if SSH key is missing
* [#10544] Load/store console style settings
* [#10532] Avoid losing git exception in background
* [#10526] Change default hotkey of `CheckoutBranch` to `Ctrl+.`
* [#10525] FormBrowse: Refresh revisions on main thread
* [#10524] 9659 MEF assemblies fixes (for jira hint plugin)
* [#10475] Fixup null check for #10434
* [#10434] Do not add BOM when commitEncoding is set to upcase UTF-8
* [#10341] GitFlow: Do not throw if git-flow is not init
* [#10309] Misc fixes


[#10621]:https://github.com/gitextensions/gitextensions/pull/10621
[#10604]:https://github.com/gitextensions/gitextensions/pull/10604
[#10601]:https://github.com/gitextensions/gitextensions/pull/10601
[#10590]:https://github.com/gitextensions/gitextensions/pull/10590
[#10581]:https://github.com/gitextensions/gitextensions/pull/10581
[#10559]:https://github.com/gitextensions/gitextensions/pull/10559
[#10556]:https://github.com/gitextensions/gitextensions/pull/10556
[#10548]:https://github.com/gitextensions/gitextensions/pull/10548
[#10546]:https://github.com/gitextensions/gitextensions/pull/10546
[#10545]:https://github.com/gitextensions/gitextensions/pull/10545
[#10544]:https://github.com/gitextensions/gitextensions/pull/10544
[#10532]:https://github.com/gitextensions/gitextensions/pull/10532
[#10526]:https://github.com/gitextensions/gitextensions/pull/10526
[#10525]:https://github.com/gitextensions/gitextensions/pull/10525
[#10524]:https://github.com/gitextensions/gitextensions/pull/10524
[#10475]:https://github.com/gitextensions/gitextensions/pull/10475
[#10434]:https://github.com/gitextensions/gitextensions/pull/10434
[#10341]:https://github.com/gitextensions/gitextensions/pull/10341
[#10309]:https://github.com/gitextensions/gitextensions/pull/10309



### Version 4.0.1 (15 Dec 2022)

#### Changes:
* [10521] Git 2.39 bugs out for fetch --jobs=0
* [10516] theme: remove dark themes from deliverables
* [10514] Unable to run git commands from Git Extensions
* [10487] Create new user script in enabled state
* [10484] Report git fatal errors as not app errors
* [10482] Fixup text search in diffs (multiple groups)
* [10480] Filters_should_behave_as_expected: Current branch is persisted
* [10477] [NBug] An error occurred trying to start process
* [10455] Stop endless dashboard repaint loop
* [10453] Make "blame" setting non-sticky
* [10451] Set custom difftool list to empty if parsing is cancelled
* [10438] Quote file name for the File History form
* [10436] Better handle git security exception
* [10431] Persist Current Branch setting
* [10418] Fixup "Derived from tag"
* [10396] Fix 'Delete obsolete branches' plugin
* [10390] FormClone: Check that current module is valid
* [10388] SidePanel: Context menu at no selection
* [10357] Verify utility's file exists before invoking it
* [10355] Retain only Microsoft.WindowsDesktop.App in *.runtimeconfig.json
* [10341] GitFlow: Do not throw if git-flow is not init
* [10340] SidePanel: Context menu at no selection
* [10339] SidePanel: AheadBehind null check
* [10316] Fixup standalone blame


[10521]:https://github.com/gitextensions/gitextensions/pull/10521
[10516]:https://github.com/gitextensions/gitextensions/pull/10516
[10514]:https://github.com/gitextensions/gitextensions/issues/10514
[10487]:https://github.com/gitextensions/gitextensions/pull/10487
[10484]:https://github.com/gitextensions/gitextensions/pull/10484
[10482]:https://github.com/gitextensions/gitextensions/pull/10482
[10480]:https://github.com/gitextensions/gitextensions/pull/10480
[10477]:https://github.com/gitextensions/gitextensions/pull/10477
[10455]:https://github.com/gitextensions/gitextensions/pull/10455
[10453]:https://github.com/gitextensions/gitextensions/pull/10453
[10451]:https://github.com/gitextensions/gitextensions/pull/10451
[10438]:https://github.com/gitextensions/gitextensions/pull/10438
[10436]:https://github.com/gitextensions/gitextensions/pull/10436
[10431]:https://github.com/gitextensions/gitextensions/pull/10431
[10418]:https://github.com/gitextensions/gitextensions/pull/10418
[10396]:https://github.com/gitextensions/gitextensions/pull/10396
[10390]:https://github.com/gitextensions/gitextensions/pull/10390
[10388]:https://github.com/gitextensions/gitextensions/pull/10388
[10357]:https://github.com/gitextensions/gitextensions/pull/10357
[10355]:https://github.com/gitextensions/gitextensions/pull/10355
[10341]:https://github.com/gitextensions/gitextensions/pull/10341
[10340]:https://github.com/gitextensions/gitextensions/pull/10340
[10339]:https://github.com/gitextensions/gitextensions/pull/10339
[10316]:https://github.com/gitextensions/gitextensions/pull/10316


### Version 4.0.0 (31 Oct 2022)

#### Changes:
* [10294] FormBrowse: Hide Reflog button by default
* [10286] Git 2.38.1
* [10285] Refresh the left panel when it's shown
* [10283] Preventing popup "Revision X not being visible in the revision grid" on right mouse button click
* [10262] Cleanup handling of AllOutput for processes
* [10261] Git Executable: return stderr also if not throwOnErrorExit is suppressed
* [10260] Removed changes of PR #3833, suspend/resume redrawn of ToolStripEx when dropdown menu is opened/closed
* [10258] SidePanel: Show Stashes
* [10253] Redo selection of the first item to fix shift-select of multiple items
* [10248] Hotkey: Add a hotkey for "Stash Staged" menu item
* [10245] Form rebase: some small fixes
* [10244] Update FormManageWorktree layout
* [10243] Enable <ImplicitUsings> and clean unneeded namespace declarations.
* [10240] Custom options to git-log
* [10239] Revision filter combobox
* [10234] Restore the order of the branch view options
* [10232] Update DiffViewerSettingsPage layout
* [10229] Update to latest 6.0 .Net Runtime/SDK
* [10228] ShowOnlyCurrentBranch were not showing commits
* [10227] Do not cache current GetRevision data
* [10224] Worktree: redesign 'Manage worktree' form
* [10222] Ignore .gitmodules without path
* [10217] ShellExtension: rename item in the setting form with same name it have in shell extension menu
* [10216] Disable reloading of Explorer when uninstalling.
* [10212] Update filter toolbar when filters change
* [10208] ConEmu 220807
* [10205] Move revision filters to advanced filter form
* [10203] FormDeleteRemoteBranch: GetMergedRemoteBranches() in background
* [10193] Protect AppSettings saving using global mutex
* [10191] Add Update-DotnetVersion script
* [10190] Tests: Disable some commonly failed tests
* [10189] Reorder items in View menu
* [10188] Blame: fix path displayed in the gutter
* [10183] Blame: fix blame revision older revision when the file has been renamed
* [10180] Update .NET references
* [10172] Decouple filter options
* [10169] Tests: Disable some commonly failed tests
* [10164] RevGrid: Custom tools were only loaded at settings changes
* [10163] Insert artificial commits if filter without any matches is applied
* [10161] FormStash: Display index of stashes
* [10159] Settings: Don't clear setting search when leaving the search box
* [10154] Set RuntimeFrameworkVersion for DotnetRuntimeBootstrapper
* [10153] Do not call DoDragDrop when mouse is up
* [10149] Reword/Edit: Rebase on actual parent
* [10146] Sidepanel: Keep revisions opening a new instance
* [10145] Open new instance if double click on current submodule
* [10139] VS Code build support
* [10138] Show Stash in rev grid
* [10137] Do not start NBug if mergetool exits with other than 0
* [10136] BranchFilter text was added backwards
* [10135] Support User local paths for VsCode merge tools
* [10130] Eliminate static methods in RevisionReader
* [10129] Cleanup git-log filter option handling
* [10127] Fix search not launching/working when changing filter selection
* [10125] Improve menu responsiveness with SuspendLayout
* [10124] Set branch name in repo menu async
* [10123] Add `rebase.autosquash` setting in Git Advanced settings panel
* [10119] Restore WindowState when activating
* [10118] FormDiff:  Show range diff with merge base
* [10117] Show tags by default in side panel
* [10116] Allow merge base to parent with three selected
* [10101] RevDiff: Align rules for hotkey execution to menu items
* [10090] fix: Cannot cherry-pick new file from stash
* [10088] Use WinForms API to check for minimized
* [10086] ConEmu 220418
* [10085] Git 2.37.1
* [10084] Add most recent repos to all recent list
* [10083] Select remote branch on Alt+Click in side panel
* [10082] RevGrid: Optimize onlyFirstParent handling
* [10079] Improve the clarity of tooltips for Rebase UI date options
* [10076] Exclude boundary commits with --grep
* [10073] Blame: No change of control at double click
* [10072] FormDiff: Set HEAD for diff calculator
* [10030] RevisionGrid: Cleanup PerformRefreshRevisions
* [10027] Replace hard-coded `Ctrl+O` shortcuts with `Hotkey`s
* [10026] `CommitInfo`: Ensure empty line between sections
* [10021] Do not throw when cancelling current token
* [10011] CommitInfo: Fixup revision links in commit message
* [10005] Add menu item "Show in explorer" in Resolve merge conflicts window
* [10003] Use grayscaled icon for "Reset another branch to here"
* [9992] Refresh regardless of formProcess results
* [9989] Show reword commit dialog only once
* [9985] Enable artificial commits for empty repos
* [9983] Add statistics for 1C Enterprise and 1Script files
* [9982] Reset pathfilter by disabling checkbox
* [9981] Do not assert for head when creating branches
* [9975] Left panel/overview node inheritance
* [9970] Make SetDiffsAsync cancellable
* [9968] More xml-related fixes
* [9967] Left panel: split up large code files
* [9966] left panel: reverted menu icon scale to match item font size
* [9965] Remove designer-specific workarounds
* [9963] Migrates FabricBot automation to "Config-as-Code"
* [9962] Ensure commit message textbox is visible
* [9961] Fix FormCreateBranch layout
* [9960] Correct label alignment
* [9958] Show 'no changes' in grid for artificial
* [9956] git-fetch --jobs==0 if no configuration
* [9949] FileViewer: Display error text rather than popup
* [9948] Load grid to selected revisions
* [9947] Show multi revision diff also with no HEAD
* [9943] CommitInfo: Workaround for `Show all tags`
* [9942] FormResolveConflicts: Clearer label and confirmation messages for Reset button
* [9940] Register CodePagesEncodingProvider to support more encodings
* [9936] Adjust color for build in progress indication
* [9933] CommitInfo: Clear RevisionInfo if no real revision
* [9932] Increase contrast between graph colors
* [9931] Replace `ShortcutKeys` `Ctrl+F` with Hotkeys
* [9930] Fix test View_reflects_applied_branch_filter
* [9924] Correct ownership of Push taskdialog
* [9923] Package missing assemblies
* [9919] Handle detached HEAD for orphan branches
* [9917] FormCommit: Reset commit options after push
* [9912] Upgrade pkg SmartFormat to v3.0.0
* [9908] RevDiff: async SetDiff
* [9905] Main menu: Standardize capitalization and use US English
* [9904] Display assigned hotkeys in View menu on startup
* [9903] Remove unused `TreeGuid`
* [9900] RevisionGrid: Add option to fill git ref labels again
* [9898] Make common hotkeys visible as Hotkeys
* [9897] Browse: Clarify RefreshRevisions() handling
* [9896] Share gitRefs when reloading after changing sort criterias
* [9890] Enable fetching moved tags
* [9888] Init sidepanel only when loading
* [9887] RevisionGrid: Set accessibility values
* [9886] RevGrid: Save currently selected before clearing the grid
* [9885] RevGrid: Lazy handling for current branch
* [9879] Show CurrentCommitId for super project for submodules
* [9878] Add SelectNextForkPointAsDiffBase hotkey
* [9871] IndexWatcher: Use for refresh icon only
* [9864] RefreshGrid: async reading refs and logs in parallel
* [9862] FileHistory mode at browse startup
* [9860] Bump to .NET 6.0
* [9856] Tests: Protect RefRepos with mutex before reset
* [9855] FormPull: fix button size
* [9848] Add git stash --staged
* [9846] RevisionGrid: Use Hotkeys for navigation
* [9842] FileTree: Goto line in Blame from RevDiff
* [9841] Pull rebase: Fix detection of merge commits
* [9833] RevGrid: Show popup when a revision is not in the revision grid
* [9832] Blame: Start on line also when control already exists
* [9831] RevDiff: Setting to show Blame in FileTree
* [9829] LostObjects: better `log` command and regex
* [9828] RevisionDiffControl: Add hotkey for selecting all changes (in the first group)
* [9827] Route hotkeys to all visible controls
* [9826] FileViewer: Add hotkeys
* [9819] Ignore nonexisting submodule paths
* [9817] Blame settings: Auto size for checkboxes
* [9816] BlameControl: cancellation token for git-blame
* [9815] Reset theme to default
* [9808] ShowSimplifyByDecoration: Not dependent on branch filter
* [9807] AdvancedFilter: Disable filters button dropdown
* [9806] AppTitle: File name in path filter first
* [9805] Blame: Start on line also when control already exists
* [9804] RevDiff: Persist Blame until another file is selected
* [9803] ShowFullHistory in View menu
* [9801] Add Blame Settings page
* [9800] Adjust BrowseRepoSettings
* [9799] Support colorblind variation of default theme
* [9798] FileSystemWatcher is not working for \\wsl$
* [9797] Decrease CustomDiffMergeToolProvider start delay
* [9795] make ToolStripEx [great again] use ToolStripExSystemRenderer by default as before #9608
* [9793] Conemu 210912
* [9791] Replace AppSettings.UseFastChecks with ShowGitStatusInBrowseToolbar
* [9790] Do not report Git error for a command seemingly executed in the background
* [9789] Updated tooltip for ShowDiffForAllParents
* [9788] FileHistory: Launch commandline in FormBrowse
* [9787] Theming readme update
* [9786] GitNotes: Reduce overhead when Body exists
* [9785] BuildServer: Correct settings paths
* [9784] Load app colors from CSS themes
* [9781] signed CLA
* [9780] Fix #8918: Mergetool and difftool commands are now populated if paths…
* [9778] Allow amend commits to also add --reset-author
* [9767] Update .NET Runtime Bootstrapper
* [9765] Add mnemonics to filter toolbar
* [9764] Simplify FileStatus list image
* [9763] GitVersion: Remove support for obsolete versions
* [9762] range-diff: Handle artificial commits
* [9760] Settings could not be read from \\wsl$
* [9759] RevisionGrid: Option for git log with --topo-order
* [9757] RevDiff: Move RangeDiff to A->B group
* [9753] Tests: Ignore flaky UI tests
* [9749] Allow running TranslationApp in Debug
* [9747] Fix tests
* [9745] Adjust Debug.Assert for tests
* [9744] Make the Debug build debuggable
* [9735] Share GitRef() at startup
* [9734] LeftPanel was initiated twice at startup
* [9733] PathFilter: Set FallbackFollowedFile at every filter update
* [9732] appveyor: avoid warnings about crlf
* [9729] FormBrowse init optimizations
* [9728] RevisionFilter: Indicate filters with button state and tooltip
* [9727] Let RTB extension `GetPlainText` ignore links
* [9725] AppTitle was set to previous repo branch
* [9723] Select and filter for multiple branches and tags from repo objects tree in left panel
* [9722] signed contributors.txt
* [9720] BASE diff: Icons for unique changes
* [9719] TweakPng for some icons
* [9718] RevDiff: Count number of changed files per group
* [9708] RevisionGridControl: prevent exception when switching repository
* [9704] Generate .NET installer
* [9702] Support Git in WSL
* [9700] GitStatusMonitor could start multiple commands
* [9698] Reset another branch to here path format
* [9696] Add Shortcut for Create Branch in Commit Form
* [9695] Fix interactive add for new files
* [9693] Some more mnemonics
* [9687] Change Revision Links separator (#9668)
* [9686] `Rebase --onto`: Display only the current branch to make selection easier
* [9685] Remove transitive dependencies already included by other projects
* [9684] Fix toolbar dpi scaling issue
* [9683] Fix running scripts without arguments
* [9682] Update Feature request template to use the new template
* [9680] GitVersion: Remove support for obsolete versions
* [9679] "--format" must be quoted
* [9678] PushLocalCmd: path must be absolute
* [9673] Make "Initialize submodules?" message box respect "Update submodules on checkout" settings option
* [9672] Make reset "Soft" the default option
* [9671] Revert: stack commit message
* [9666] NBug copy text were url encoded
* [9664] Fix build
* [9661] Fix loading user plugins.
* [9655] Add mnemonics to every menu item in FormBrowse
* [9652] Fix: Prevent the find-replace dialog from blocking Windows shutdown
* [9650] Fix revision lane order
* [9645] Add ellipses (...) to branch context menu items which require additional steps to complete; update item labels to match other menus
* [9639] Fixup splitter persistence for CommitInfo
* [9638] Update issue template and update BugReporter to use the new template
* [9635] Fix #9634: Minor PR template issue: body text being rendered as a header
* [9632] Fix #9119: Value was either too large or too small for an Int32
* [9630] Add support for Del key to RepoObjectsTree nodes
* [9629] Package plugins
* [9625] Remove `Rename` menu item for remote branches from Repo Tree
* [9623] Don't update filters or issue CD command to the terminal
* [9622] Pull dialog: add mnemonics, fix tab order
* [9621] Rebase & Apply Patch: add mnemonics, fix tab order, add a shortcut
* [9620] Fix #8211: Incorrect EOL in diff
* [9619] Remove "process: null" for FormProcess 
* [9616] Fix #6276: "Show messages of annotated tags" truncates to ten lines without notice
* [9611] Ensure all grid views can be sorted
* [9608] Fix branch copy and Open with difftool menu switching back to System style
* [9607] Stabilise left panel tests
* [9603] Add keyboard hotkeys to one menu and one dialog
* [9593] Clean up `FormBrowse` load sequence
* [9590] Fix leaking brushes
* [9586] Show toolbar grips
* [9583] Select HOME where .gitconfig is accessible first
* [9580] Sync advanced filter and filters toolbar
* [9564] NBug: Include GE specific exception info
* [9562] Do not throw if process exits with 0
* [9561] Check that directory exists before running submodule command
* [9557] Reduce amount of TaskCanceledException
* [9555] Allow hiding stash references on revision grid
* [9542] Reinstate SmartFormat legacy error handling
* [9541] Update translations at 75%+
* [9540] Remove CorePath class
* [9539] Simplify the ISettingsSource class
* [9528] Enhance the filters toolbar
* [9524] GetRangeDiff() should be Async
* [9522] Cancellation for Executable
* [9519] Truncate tooltips for blame commit long summary
* [9518] Add yerudako (myself) to the list of project contributors
* [9508] Update readme and changelog
* [9488] Execution interface cleanup
* [9483] SetDiff: Init for CommitDiff
* [9478] Takes 2 at trying to fix failing tests
* [9477] Reset revision filter when switching repos
* [9468] Avoid popup at successful FileViewer patching
* [9461] Stabilise RevisionGridControl filtering tests
* [9460] Don't show delete dialog on CTRL+ALT+DEL
* [9459] Artificial commits: Invalidate rowcache when inserting
* [9455] Improve handling of encodings with conflicting names
* [9454] Attempt to fix tests failure for MEF
* [9453] GitHub PR template - merge strategies
* [9446] Clean up `FilterToolBar` logic
* [9445] Blame/filter in Browse, replace formfilehistory
* [9444] Fixed Commit info varying indents. Fixes #9164
* [9443] Updated Nuget packages for all projects
* [9432] RevGrid commit count: Do not reset when creating artificial commits
* [9430] BashShell: Try git-bash.exe first
* [9426] Diff to actual parent in filtered grids
* [9425] ToggleBetweenArtificialAndHeadCommits: Go to WorkTree if HEAD is hidden
* [9424] RevFileTree: Blame instead of View
* [9413] Rename detection for Advanced filter
* [9406] Do not allow duplicate submodule paths
* [9398] Sign contributors document
* [9393] Show artificial commits also for filtered grid
* [9392] Do not blame folders
* [9389] Addendum to #9372
* [9387] Enable use of the designer
* [9386] Remove PuTTY binaries from installer
* [9385] Do not check if path exists before settings GIT_SSH
* [9384] Update contributors.txt
* [9380] Recent repo search
* [9372] Update layout of ConfirmationsSettingsPage
* [9371] Cleanup ListView control
* [9361] Fixup localization verification
* [9359] Log RevisionReader parse errors
* [9358]  RevisionReader allow empty subject
* [9357] Fixup main menu item `Git bash`
* [9356] Fixup user scripts running PS in foreground
* [9346] Update ChangeLog
* [9344] Remove BinaryFormatter. Fixes #9150
* [9336] Preparations for exceptions on git error (part 4 of 4)
* [9335] Browse: Advanced filter improvements
* [9334] Preparations for exceptions on git error (part 3 of 4)
* [9332] Add support for setting console emulator font name
* [9330] Reduce window flicker and loss of focus
* [9329] Fix/9166 gdi objects
* [9326] FormRebase: Avoid list refs if not advanced
* [9324] Remove custom ToHashSet() methods. Fixes #9159
* [9320] Quick search in whole commit message if loaded
* [9318] Create empty settings file if missing
* [9317] GitRevision: Remove Name property
* [9310] Load plugins
* [9309] Increase test robustness
* [9308] Stash: Keyboard navigation
* [9305] Fix PathUtil handling of spaces in file names
* [9301] Make showing commit message body in the revision graph optional
* [9300] Changes from release/3.5 to master
* [9299] Comprehensive fix of misspellings/typos
* [9286] Fixup partial graph rendering
* [9285] Add mnemonics to context menu of `RevisionDiffControl`
* [9284] Commit: Unstage new files did not display the worktree file
* [9282] Add mnemonics to frequently used dialogs
* [9277] Gray / hide selections in `FormBrowse` if not focused
* [9274] Handle `laneInfo` being `null`
* [9268] Allow rollForward for .NET 5.0 SDK version
* [9266] Update Clone dialog's handling of clipboard text
* [9265] FormCheckoutBranch add mnemonics
* [9264] Enable CA1416: Validate platform compatibility
* [9256] Limit superproject refs in rev grid
* [9252] Catch GDI exception
* [9246] Use static/cached GitVersion to resolve Git path
* [9243] RevisionReader: Parse raw commit body
* [9235] Use System.Windows.Forms.TaskDialog
* [9231] Preparations for exceptions on git error (part 2)
* [9226] Improve RevisionGridControlTests
* [9222] Git commands should never have colors
* [9217] Replace PathUtil.Combine and GetExtension with Path.
* [9216] Use fullPathResolver for viewing local files
* [9212] Preparations for exceptions on git error
* [9210] Delete temp files from settings tests
* [9206] Use enum interface for GetRefs()
* [9203] LogCaptureCallStacks in AppSettings
* [9197] Diff/Commit: Do not show Open for deleted items
* [9196] Do not try to show deleted images in diffviewer
* [9188] Keep persistent ssh settings in registry
* [9187] Ignore failure getting Process.Id for ShellExecute
* [9186] sign cla | change email
* [9184] Restore build pipeline
* [9182] Set GIT_SSH only if path exists
* [9172] Do not attempt to switch to a not created worktree
* [9171] fix: all unused imports removed
* [9170] Update contributors.txt
* [9161] Use Microsoft.Build.CentralPackageVersions
* [9149] OpenSSH presented as OtherSSH in settings
* [9148] sign off from @MrJithil
* [9145] Stash pop/apply: Refresh grid if command was successful
* [9141] `FileViewer`: Remove decoration of default encoding
* [9140] feat: add dot command to GitUICommands
* [9136] Update README
* [9132] Sanitize localized stack trace in exception tests
* [9129] Install `Newtonsoft.Json.dll` not only for plugins
* [9123] Let installer not activate disabled telemetry
* [9122] View changed submodule could hang GE
* [9107] Start using MEF
* [9106] Restore splitter persistence
* [9105] Fix stage/unstage implementations
* [9097] Reduce allocations for RevisionGraph lane straightening
* [9095] c# new var
* [9087] FormCommitDiff cannot handle artificial commits
* [9086] fix-9085 Change DialogResult only after saving before closing
* [9082] Fix settings saving for "Detailed" section by selected setting source
* [9081] Conemu v21.03.14
* [9078] Left Panel: Fixup "is merged" status issues
* [9075] Some GitHub plugin fixes
* [9073] Remove false comment regarding ColorSettingsPage
* [9072] GitHub token: Add a link to let the user generate its Personal Access Token
* [9066] Color settings page changes
* [9064] Do not reset a valid `AppSettings.GitBinDir`
* [9063] Report bugs in a separate process
* [9060] Suppress "Theme not found" if setting is empty
* [9059] Use exit status instead of IsGitErrorMessage()
* [9058] AutoComplete: Avoid Git for non tracked files
* [9057] rev-parse should use --quiet --verify
* [9056] Throw on non-zero exit code and on stderror output
* [9053] Color settings page events
* [9050] Straighten graph lanes over multiple rows
* [9049] Fix tab control flickering in dark theme
* [9045] Updated contributors.txt
* [9044] Added 'Allow Empty' checkbox to commit form, and updated command logic
* [9040] Remove unused memory settings
* [9039] Remove redundant creation of settings container
* [9038] Improve display of exceptions
* [9037] Handle errors accessing the amend state file
* [9034] Restore "Commit & push" button if amending
* [9028] Straighten graph lanes
* [9027] #9013 Use Close in Clean working directory
* [9025] Update UI only after save settings
* [9018] Ignore git clone prefix when checking clipboard for source URL
* [9011] Check Before script failure to abort action
* [9010] Allow opening files in Visual Studio
* [9009] FormBisect: Autosize form to prevent content to be clipped
* [9008] Push from empty FormCommit
* [9004] Add contributor
* [8996] Small bits
* [8995] Resolve PluginSettings key names issue
* [8994] Fix "Commit & push" button color when ameding is enabled
* [8993] Move ImpactLoader.cs from GitCommands.Statistics to GitExtensions.Plugins.GitImpact
* [8990] Include stash, bisect in grid if tags are shown
* [8986] Fixup the initial view menu item state of #8942
* [8984] Move all plugins to "GitExtensions.Plugins" namespace
* [8983] Move BackgroundFetch plugin to "GitExtensions.Plugins" namespace
* [8982] Move AutoCompileSubmodules plugin to "GitExtensions.Plugins" namespace
* [8981] Move GitImpact plugin to "GitExtensions.Plugins" namespace
* [8975] Move GitStatistics plugin to "GitExtensions.Plugins" namespace
* [8973] Remove redundant suppressions
* [8972] Use pattern matching
* [8967] Fix invalid null value assertion in plugin code
* [8964] Annotate plugins
* [8962] Optimise string split operations
* [8961] Report `SaveBlobAs` errors without crashing the app
* [8959] Remove redundant await
* [8958] Rename translation strings to avoid conflict
* [8957] Dialogs layout alignment - `FormDeleteRemoteBranch`
* [8956] Dialogs layout alignment - `FormDeleteBranch`
* [8954] Remove return value from AsyncLoader
* [8952] range-diff: Incorrect limit when selecting four commits
* [8950] Disable CodeCov integration
* [8949] Stop loading custom diff tools during tests
* [8943] Use null coalescing assignment
* [8942] FormBrowse Toolbar: Being able to hide each control separately
* [8939] Annotate resource manager project
* [8938] Stop AsyncLoader passing null on cancellation
* [8937] Fix invalid XML in DotSettings file
* [8936] Prevent exception when running unit tests
* [8935] Use switch expression
* [8934] Add missing solution items
* [8933] Remove invalid solution items
* [8932] Focus ComboBox Branches of FormResetAnotherBranch
* [8929] Remove redundant property setters
* [8928] Use null propagation
* [8927] Remove unused field from RevisionDiffController
* [8926] Merge sequential checks into pattern matching expressions
* [8925] Remove IExecutable.GetOutput
* [8924] Use null coalescing assignment
* [8923] Fix hang when requesting submodule status
* [8907] Hide theme fields for serialization from public interface
* [8903] Graph: Ensure branch colors are not equal to branchs they are merged to
* [8902] Sidepanel submodule: Recreate tree only at structure changes
* [8901] Test to force same colors in AppColorDefaults and invariant.css
* [8900] Add branch rendering improvements
* [8899] Fix tool strip border in dark theme
* [8898] Move ExternalOperationException to GitExtUtils
* [8897] Refactor PaintGraphCell
* [8896] Add UI improvements
* [8894] RevDiff: Request GitStatus updates at file manipulations
* [8888] Eliminate boxing allocation
* [8887] Eliminate closure allocations
* [8886] GitStatusMonitor increase minimum time between updates
* [8883] Use pattern matching
* [8882] Avoid boxing while unpacking flags
* [8881] Fix submodule status loading error
* [8878] Annotate GitUIPluginInterfaces
* [8877] Remove redundant #nullable directives
* [8876] Remove redundant null check
* [8875] Remove double execution of GetSubmoduleStatusAsync
* [8868] fix ListView column border in dark theme
* [8867] Submodule SidePanel updated too often
* [8866] Cache Git commands related to submodules
* [8860] Link style was not applied to the dashboard
* [8854] Browse: Avoid extra git-rev-parse at start
* [8846] Annotate GitExtensions and GitUI projects
* [8844] Experimental flipping of branch colors
* [8838] Remove /en/latest from Doc links
* [8836] Jira plugin configuration - Password renamed to Password/API token
* [8830] AppVeyor: Handle v2 tokens
* [8819] Correct padding increase
* [8809] Improve RevisionDataGridView responsiveness
* [8804] Introduce new interface for named git refs
* [8799] Revert "Add test accessors borrowed from https://github.com/dotnet/winforms/"
* [8797] C#8 simplified using to reduce nesting
* [8790] Cache azuredevops build results
* [8709] Implemented F2 rename branch in RepoObjectsTree.
* [8700] Custom difftool in RevDiff, FileHistory, Commit, RevGrid
* [8522] Migrate to .NET 5.0
* [8452] Allow highlight branch by ALT+Click in the grid
* [8156] Update packages.
* [8010] Feature/add windows explorer integration in settings


[10294]:https://github.com/gitextensions/gitextensions/pull/10294
[10286]:https://github.com/gitextensions/gitextensions/pull/10286
[10285]:https://github.com/gitextensions/gitextensions/pull/10285
[10283]:https://github.com/gitextensions/gitextensions/pull/10283
[10262]:https://github.com/gitextensions/gitextensions/pull/10262
[10261]:https://github.com/gitextensions/gitextensions/pull/10261
[10260]:https://github.com/gitextensions/gitextensions/pull/10260
[10258]:https://github.com/gitextensions/gitextensions/pull/10258
[10253]:https://github.com/gitextensions/gitextensions/pull/10253
[10248]:https://github.com/gitextensions/gitextensions/pull/10248
[10245]:https://github.com/gitextensions/gitextensions/pull/10245
[10244]:https://github.com/gitextensions/gitextensions/pull/10244
[10243]:https://github.com/gitextensions/gitextensions/pull/10243
[10240]:https://github.com/gitextensions/gitextensions/pull/10240
[10239]:https://github.com/gitextensions/gitextensions/pull/10239
[10234]:https://github.com/gitextensions/gitextensions/pull/10234
[10232]:https://github.com/gitextensions/gitextensions/pull/10232
[10229]:https://github.com/gitextensions/gitextensions/pull/10229
[10228]:https://github.com/gitextensions/gitextensions/pull/10228
[10227]:https://github.com/gitextensions/gitextensions/pull/10227
[10224]:https://github.com/gitextensions/gitextensions/pull/10224
[10222]:https://github.com/gitextensions/gitextensions/pull/10222
[10217]:https://github.com/gitextensions/gitextensions/pull/10217
[10216]:https://github.com/gitextensions/gitextensions/pull/10216
[10212]:https://github.com/gitextensions/gitextensions/pull/10212
[10208]:https://github.com/gitextensions/gitextensions/pull/10208
[10205]:https://github.com/gitextensions/gitextensions/pull/10205
[10203]:https://github.com/gitextensions/gitextensions/pull/10203
[10193]:https://github.com/gitextensions/gitextensions/pull/10193
[10191]:https://github.com/gitextensions/gitextensions/pull/10191
[10190]:https://github.com/gitextensions/gitextensions/pull/10190
[10189]:https://github.com/gitextensions/gitextensions/pull/10189
[10188]:https://github.com/gitextensions/gitextensions/pull/10188
[10183]:https://github.com/gitextensions/gitextensions/pull/10183
[10180]:https://github.com/gitextensions/gitextensions/pull/10180
[10172]:https://github.com/gitextensions/gitextensions/pull/10172
[10169]:https://github.com/gitextensions/gitextensions/pull/10169
[10164]:https://github.com/gitextensions/gitextensions/pull/10164
[10163]:https://github.com/gitextensions/gitextensions/pull/10163
[10161]:https://github.com/gitextensions/gitextensions/pull/10161
[10159]:https://github.com/gitextensions/gitextensions/pull/10159
[10154]:https://github.com/gitextensions/gitextensions/pull/10154
[10153]:https://github.com/gitextensions/gitextensions/pull/10153
[10149]:https://github.com/gitextensions/gitextensions/pull/10149
[10146]:https://github.com/gitextensions/gitextensions/pull/10146
[10145]:https://github.com/gitextensions/gitextensions/pull/10145
[10139]:https://github.com/gitextensions/gitextensions/pull/10139
[10138]:https://github.com/gitextensions/gitextensions/pull/10138
[10137]:https://github.com/gitextensions/gitextensions/pull/10137
[10136]:https://github.com/gitextensions/gitextensions/pull/10136
[10135]:https://github.com/gitextensions/gitextensions/pull/10135
[10130]:https://github.com/gitextensions/gitextensions/pull/10130
[10129]:https://github.com/gitextensions/gitextensions/pull/10129
[10127]:https://github.com/gitextensions/gitextensions/pull/10127
[10125]:https://github.com/gitextensions/gitextensions/pull/10125
[10124]:https://github.com/gitextensions/gitextensions/pull/10124
[10123]:https://github.com/gitextensions/gitextensions/pull/10123
[10119]:https://github.com/gitextensions/gitextensions/pull/10119
[10118]:https://github.com/gitextensions/gitextensions/pull/10118
[10117]:https://github.com/gitextensions/gitextensions/pull/10117
[10116]:https://github.com/gitextensions/gitextensions/pull/10116
[10101]:https://github.com/gitextensions/gitextensions/pull/10101
[10090]:https://github.com/gitextensions/gitextensions/pull/10090
[10088]:https://github.com/gitextensions/gitextensions/pull/10088
[10086]:https://github.com/gitextensions/gitextensions/pull/10086
[10085]:https://github.com/gitextensions/gitextensions/pull/10085
[10084]:https://github.com/gitextensions/gitextensions/pull/10084
[10083]:https://github.com/gitextensions/gitextensions/pull/10083
[10082]:https://github.com/gitextensions/gitextensions/pull/10082
[10079]:https://github.com/gitextensions/gitextensions/pull/10079
[10076]:https://github.com/gitextensions/gitextensions/pull/10076
[10073]:https://github.com/gitextensions/gitextensions/pull/10073
[10072]:https://github.com/gitextensions/gitextensions/pull/10072
[10030]:https://github.com/gitextensions/gitextensions/pull/10030
[10027]:https://github.com/gitextensions/gitextensions/pull/10027
[10026]:https://github.com/gitextensions/gitextensions/pull/10026
[10021]:https://github.com/gitextensions/gitextensions/pull/10021
[10011]:https://github.com/gitextensions/gitextensions/pull/10011
[10005]:https://github.com/gitextensions/gitextensions/pull/10005
[10003]:https://github.com/gitextensions/gitextensions/pull/10003
[9992]:https://github.com/gitextensions/gitextensions/pull/9992
[9989]:https://github.com/gitextensions/gitextensions/pull/9989
[9985]:https://github.com/gitextensions/gitextensions/pull/9985
[9983]:https://github.com/gitextensions/gitextensions/pull/9983
[9982]:https://github.com/gitextensions/gitextensions/pull/9982
[9981]:https://github.com/gitextensions/gitextensions/pull/9981
[9975]:https://github.com/gitextensions/gitextensions/pull/9975
[9970]:https://github.com/gitextensions/gitextensions/pull/9970
[9968]:https://github.com/gitextensions/gitextensions/pull/9968
[9967]:https://github.com/gitextensions/gitextensions/pull/9967
[9966]:https://github.com/gitextensions/gitextensions/pull/9966
[9965]:https://github.com/gitextensions/gitextensions/pull/9965
[9963]:https://github.com/gitextensions/gitextensions/pull/9963
[9962]:https://github.com/gitextensions/gitextensions/pull/9962
[9961]:https://github.com/gitextensions/gitextensions/pull/9961
[9960]:https://github.com/gitextensions/gitextensions/pull/9960
[9958]:https://github.com/gitextensions/gitextensions/pull/9958
[9956]:https://github.com/gitextensions/gitextensions/pull/9956
[9949]:https://github.com/gitextensions/gitextensions/pull/9949
[9948]:https://github.com/gitextensions/gitextensions/pull/9948
[9947]:https://github.com/gitextensions/gitextensions/pull/9947
[9943]:https://github.com/gitextensions/gitextensions/pull/9943
[9942]:https://github.com/gitextensions/gitextensions/pull/9942
[9940]:https://github.com/gitextensions/gitextensions/pull/9940
[9936]:https://github.com/gitextensions/gitextensions/pull/9936
[9933]:https://github.com/gitextensions/gitextensions/pull/9933
[9932]:https://github.com/gitextensions/gitextensions/pull/9932
[9931]:https://github.com/gitextensions/gitextensions/pull/9931
[9930]:https://github.com/gitextensions/gitextensions/pull/9930
[9924]:https://github.com/gitextensions/gitextensions/pull/9924
[9923]:https://github.com/gitextensions/gitextensions/pull/9923
[9919]:https://github.com/gitextensions/gitextensions/pull/9919
[9917]:https://github.com/gitextensions/gitextensions/pull/9917
[9912]:https://github.com/gitextensions/gitextensions/pull/9912
[9908]:https://github.com/gitextensions/gitextensions/pull/9908
[9905]:https://github.com/gitextensions/gitextensions/pull/9905
[9904]:https://github.com/gitextensions/gitextensions/pull/9904
[9903]:https://github.com/gitextensions/gitextensions/pull/9903
[9900]:https://github.com/gitextensions/gitextensions/pull/9900
[9898]:https://github.com/gitextensions/gitextensions/pull/9898
[9897]:https://github.com/gitextensions/gitextensions/pull/9897
[9896]:https://github.com/gitextensions/gitextensions/pull/9896
[9890]:https://github.com/gitextensions/gitextensions/pull/9890
[9888]:https://github.com/gitextensions/gitextensions/pull/9888
[9887]:https://github.com/gitextensions/gitextensions/pull/9887
[9886]:https://github.com/gitextensions/gitextensions/pull/9886
[9885]:https://github.com/gitextensions/gitextensions/pull/9885
[9879]:https://github.com/gitextensions/gitextensions/pull/9879
[9878]:https://github.com/gitextensions/gitextensions/pull/9878
[9871]:https://github.com/gitextensions/gitextensions/pull/9871
[9864]:https://github.com/gitextensions/gitextensions/pull/9864
[9862]:https://github.com/gitextensions/gitextensions/pull/9862
[9860]:https://github.com/gitextensions/gitextensions/pull/9860
[9856]:https://github.com/gitextensions/gitextensions/pull/9856
[9855]:https://github.com/gitextensions/gitextensions/pull/9855
[9848]:https://github.com/gitextensions/gitextensions/pull/9848
[9846]:https://github.com/gitextensions/gitextensions/pull/9846
[9842]:https://github.com/gitextensions/gitextensions/pull/9842
[9841]:https://github.com/gitextensions/gitextensions/pull/9841
[9833]:https://github.com/gitextensions/gitextensions/pull/9833
[9832]:https://github.com/gitextensions/gitextensions/pull/9832
[9831]:https://github.com/gitextensions/gitextensions/pull/9831
[9829]:https://github.com/gitextensions/gitextensions/pull/9829
[9828]:https://github.com/gitextensions/gitextensions/pull/9828
[9827]:https://github.com/gitextensions/gitextensions/pull/9827
[9826]:https://github.com/gitextensions/gitextensions/pull/9826
[9819]:https://github.com/gitextensions/gitextensions/pull/9819
[9817]:https://github.com/gitextensions/gitextensions/pull/9817
[9816]:https://github.com/gitextensions/gitextensions/pull/9816
[9815]:https://github.com/gitextensions/gitextensions/pull/9815
[9808]:https://github.com/gitextensions/gitextensions/pull/9808
[9807]:https://github.com/gitextensions/gitextensions/pull/9807
[9806]:https://github.com/gitextensions/gitextensions/pull/9806
[9805]:https://github.com/gitextensions/gitextensions/pull/9805
[9804]:https://github.com/gitextensions/gitextensions/pull/9804
[9803]:https://github.com/gitextensions/gitextensions/pull/9803
[9801]:https://github.com/gitextensions/gitextensions/pull/9801
[9800]:https://github.com/gitextensions/gitextensions/pull/9800
[9799]:https://github.com/gitextensions/gitextensions/pull/9799
[9798]:https://github.com/gitextensions/gitextensions/pull/9798
[9797]:https://github.com/gitextensions/gitextensions/pull/9797
[9795]:https://github.com/gitextensions/gitextensions/pull/9795
[9793]:https://github.com/gitextensions/gitextensions/pull/9793
[9791]:https://github.com/gitextensions/gitextensions/pull/9791
[9790]:https://github.com/gitextensions/gitextensions/pull/9790
[9789]:https://github.com/gitextensions/gitextensions/pull/9789
[9788]:https://github.com/gitextensions/gitextensions/pull/9788
[9787]:https://github.com/gitextensions/gitextensions/pull/9787
[9786]:https://github.com/gitextensions/gitextensions/pull/9786
[9785]:https://github.com/gitextensions/gitextensions/pull/9785
[9784]:https://github.com/gitextensions/gitextensions/pull/9784
[9781]:https://github.com/gitextensions/gitextensions/pull/9781
[9780]:https://github.com/gitextensions/gitextensions/pull/9780
[9778]:https://github.com/gitextensions/gitextensions/pull/9778
[9767]:https://github.com/gitextensions/gitextensions/pull/9767
[9765]:https://github.com/gitextensions/gitextensions/pull/9765
[9764]:https://github.com/gitextensions/gitextensions/pull/9764
[9763]:https://github.com/gitextensions/gitextensions/pull/9763
[9762]:https://github.com/gitextensions/gitextensions/pull/9762
[9760]:https://github.com/gitextensions/gitextensions/pull/9760
[9759]:https://github.com/gitextensions/gitextensions/pull/9759
[9757]:https://github.com/gitextensions/gitextensions/pull/9757
[9753]:https://github.com/gitextensions/gitextensions/pull/9753
[9749]:https://github.com/gitextensions/gitextensions/pull/9749
[9747]:https://github.com/gitextensions/gitextensions/pull/9747
[9745]:https://github.com/gitextensions/gitextensions/pull/9745
[9744]:https://github.com/gitextensions/gitextensions/pull/9744
[9735]:https://github.com/gitextensions/gitextensions/pull/9735
[9734]:https://github.com/gitextensions/gitextensions/pull/9734
[9733]:https://github.com/gitextensions/gitextensions/pull/9733
[9732]:https://github.com/gitextensions/gitextensions/pull/9732
[9729]:https://github.com/gitextensions/gitextensions/pull/9729
[9728]:https://github.com/gitextensions/gitextensions/pull/9728
[9727]:https://github.com/gitextensions/gitextensions/pull/9727
[9725]:https://github.com/gitextensions/gitextensions/pull/9725
[9723]:https://github.com/gitextensions/gitextensions/pull/9723
[9722]:https://github.com/gitextensions/gitextensions/pull/9722
[9720]:https://github.com/gitextensions/gitextensions/pull/9720
[9719]:https://github.com/gitextensions/gitextensions/pull/9719
[9718]:https://github.com/gitextensions/gitextensions/pull/9718
[9708]:https://github.com/gitextensions/gitextensions/pull/9708
[9704]:https://github.com/gitextensions/gitextensions/pull/9704
[9702]:https://github.com/gitextensions/gitextensions/pull/9702
[9700]:https://github.com/gitextensions/gitextensions/pull/9700
[9698]:https://github.com/gitextensions/gitextensions/pull/9698
[9696]:https://github.com/gitextensions/gitextensions/pull/9696
[9695]:https://github.com/gitextensions/gitextensions/pull/9695
[9693]:https://github.com/gitextensions/gitextensions/pull/9693
[9687]:https://github.com/gitextensions/gitextensions/pull/9687
[9686]:https://github.com/gitextensions/gitextensions/pull/9686
[9685]:https://github.com/gitextensions/gitextensions/pull/9685
[9684]:https://github.com/gitextensions/gitextensions/pull/9684
[9683]:https://github.com/gitextensions/gitextensions/pull/9683
[9682]:https://github.com/gitextensions/gitextensions/pull/9682
[9680]:https://github.com/gitextensions/gitextensions/pull/9680
[9679]:https://github.com/gitextensions/gitextensions/pull/9679
[9678]:https://github.com/gitextensions/gitextensions/pull/9678
[9673]:https://github.com/gitextensions/gitextensions/pull/9673
[9672]:https://github.com/gitextensions/gitextensions/pull/9672
[9671]:https://github.com/gitextensions/gitextensions/pull/9671
[9666]:https://github.com/gitextensions/gitextensions/pull/9666
[9664]:https://github.com/gitextensions/gitextensions/pull/9664
[9661]:https://github.com/gitextensions/gitextensions/pull/9661
[9655]:https://github.com/gitextensions/gitextensions/pull/9655
[9652]:https://github.com/gitextensions/gitextensions/pull/9652
[9650]:https://github.com/gitextensions/gitextensions/pull/9650
[9645]:https://github.com/gitextensions/gitextensions/pull/9645
[9639]:https://github.com/gitextensions/gitextensions/pull/9639
[9638]:https://github.com/gitextensions/gitextensions/pull/9638
[9635]:https://github.com/gitextensions/gitextensions/pull/9635
[9632]:https://github.com/gitextensions/gitextensions/pull/9632
[9630]:https://github.com/gitextensions/gitextensions/pull/9630
[9629]:https://github.com/gitextensions/gitextensions/pull/9629
[9625]:https://github.com/gitextensions/gitextensions/pull/9625
[9623]:https://github.com/gitextensions/gitextensions/pull/9623
[9622]:https://github.com/gitextensions/gitextensions/pull/9622
[9621]:https://github.com/gitextensions/gitextensions/pull/9621
[9620]:https://github.com/gitextensions/gitextensions/pull/9620
[9619]:https://github.com/gitextensions/gitextensions/pull/9619
[9616]:https://github.com/gitextensions/gitextensions/pull/9616
[9611]:https://github.com/gitextensions/gitextensions/pull/9611
[9608]:https://github.com/gitextensions/gitextensions/pull/9608
[9607]:https://github.com/gitextensions/gitextensions/pull/9607
[9603]:https://github.com/gitextensions/gitextensions/pull/9603
[9593]:https://github.com/gitextensions/gitextensions/pull/9593
[9590]:https://github.com/gitextensions/gitextensions/pull/9590
[9586]:https://github.com/gitextensions/gitextensions/pull/9586
[9583]:https://github.com/gitextensions/gitextensions/pull/9583
[9580]:https://github.com/gitextensions/gitextensions/pull/9580
[9564]:https://github.com/gitextensions/gitextensions/pull/9564
[9562]:https://github.com/gitextensions/gitextensions/pull/9562
[9561]:https://github.com/gitextensions/gitextensions/pull/9561
[9557]:https://github.com/gitextensions/gitextensions/pull/9557
[9555]:https://github.com/gitextensions/gitextensions/pull/9555
[9542]:https://github.com/gitextensions/gitextensions/pull/9542
[9541]:https://github.com/gitextensions/gitextensions/pull/9541
[9540]:https://github.com/gitextensions/gitextensions/pull/9540
[9539]:https://github.com/gitextensions/gitextensions/pull/9539
[9528]:https://github.com/gitextensions/gitextensions/pull/9528
[9524]:https://github.com/gitextensions/gitextensions/pull/9524
[9522]:https://github.com/gitextensions/gitextensions/pull/9522
[9519]:https://github.com/gitextensions/gitextensions/pull/9519
[9518]:https://github.com/gitextensions/gitextensions/pull/9518
[9508]:https://github.com/gitextensions/gitextensions/pull/9508
[9488]:https://github.com/gitextensions/gitextensions/pull/9488
[9483]:https://github.com/gitextensions/gitextensions/pull/9483
[9478]:https://github.com/gitextensions/gitextensions/pull/9478
[9477]:https://github.com/gitextensions/gitextensions/pull/9477
[9468]:https://github.com/gitextensions/gitextensions/pull/9468
[9461]:https://github.com/gitextensions/gitextensions/pull/9461
[9460]:https://github.com/gitextensions/gitextensions/pull/9460
[9459]:https://github.com/gitextensions/gitextensions/pull/9459
[9455]:https://github.com/gitextensions/gitextensions/pull/9455
[9454]:https://github.com/gitextensions/gitextensions/pull/9454
[9453]:https://github.com/gitextensions/gitextensions/pull/9453
[9446]:https://github.com/gitextensions/gitextensions/pull/9446
[9445]:https://github.com/gitextensions/gitextensions/pull/9445
[9444]:https://github.com/gitextensions/gitextensions/pull/9444
[9443]:https://github.com/gitextensions/gitextensions/pull/9443
[9432]:https://github.com/gitextensions/gitextensions/pull/9432
[9430]:https://github.com/gitextensions/gitextensions/pull/9430
[9426]:https://github.com/gitextensions/gitextensions/pull/9426
[9425]:https://github.com/gitextensions/gitextensions/pull/9425
[9424]:https://github.com/gitextensions/gitextensions/pull/9424
[9413]:https://github.com/gitextensions/gitextensions/pull/9413
[9406]:https://github.com/gitextensions/gitextensions/pull/9406
[9398]:https://github.com/gitextensions/gitextensions/pull/9398
[9393]:https://github.com/gitextensions/gitextensions/pull/9393
[9392]:https://github.com/gitextensions/gitextensions/pull/9392
[9389]:https://github.com/gitextensions/gitextensions/pull/9389
[9387]:https://github.com/gitextensions/gitextensions/pull/9387
[9386]:https://github.com/gitextensions/gitextensions/pull/9386
[9385]:https://github.com/gitextensions/gitextensions/pull/9385
[9384]:https://github.com/gitextensions/gitextensions/pull/9384
[9380]:https://github.com/gitextensions/gitextensions/pull/9380
[9372]:https://github.com/gitextensions/gitextensions/pull/9372
[9371]:https://github.com/gitextensions/gitextensions/pull/9371
[9361]:https://github.com/gitextensions/gitextensions/pull/9361
[9359]:https://github.com/gitextensions/gitextensions/pull/9359
[9358]:https://github.com/gitextensions/gitextensions/pull/9358
[9357]:https://github.com/gitextensions/gitextensions/pull/9357
[9356]:https://github.com/gitextensions/gitextensions/pull/9356
[9346]:https://github.com/gitextensions/gitextensions/pull/9346
[9344]:https://github.com/gitextensions/gitextensions/pull/9344
[9336]:https://github.com/gitextensions/gitextensions/pull/9336
[9335]:https://github.com/gitextensions/gitextensions/pull/9335
[9334]:https://github.com/gitextensions/gitextensions/pull/9334
[9332]:https://github.com/gitextensions/gitextensions/pull/9332
[9330]:https://github.com/gitextensions/gitextensions/pull/9330
[9329]:https://github.com/gitextensions/gitextensions/pull/9329
[9326]:https://github.com/gitextensions/gitextensions/pull/9326
[9324]:https://github.com/gitextensions/gitextensions/pull/9324
[9320]:https://github.com/gitextensions/gitextensions/pull/9320
[9318]:https://github.com/gitextensions/gitextensions/pull/9318
[9317]:https://github.com/gitextensions/gitextensions/pull/9317
[9310]:https://github.com/gitextensions/gitextensions/pull/9310
[9309]:https://github.com/gitextensions/gitextensions/pull/9309
[9308]:https://github.com/gitextensions/gitextensions/pull/9308
[9305]:https://github.com/gitextensions/gitextensions/pull/9305
[9301]:https://github.com/gitextensions/gitextensions/pull/9301
[9300]:https://github.com/gitextensions/gitextensions/pull/9300
[9299]:https://github.com/gitextensions/gitextensions/pull/9299
[9286]:https://github.com/gitextensions/gitextensions/pull/9286
[9285]:https://github.com/gitextensions/gitextensions/pull/9285
[9284]:https://github.com/gitextensions/gitextensions/pull/9284
[9282]:https://github.com/gitextensions/gitextensions/pull/9282
[9277]:https://github.com/gitextensions/gitextensions/pull/9277
[9274]:https://github.com/gitextensions/gitextensions/pull/9274
[9268]:https://github.com/gitextensions/gitextensions/pull/9268
[9266]:https://github.com/gitextensions/gitextensions/pull/9266
[9265]:https://github.com/gitextensions/gitextensions/pull/9265
[9264]:https://github.com/gitextensions/gitextensions/pull/9264
[9256]:https://github.com/gitextensions/gitextensions/pull/9256
[9252]:https://github.com/gitextensions/gitextensions/pull/9252
[9246]:https://github.com/gitextensions/gitextensions/pull/9246
[9243]:https://github.com/gitextensions/gitextensions/pull/9243
[9235]:https://github.com/gitextensions/gitextensions/pull/9235
[9231]:https://github.com/gitextensions/gitextensions/pull/9231
[9226]:https://github.com/gitextensions/gitextensions/pull/9226
[9222]:https://github.com/gitextensions/gitextensions/pull/9222
[9217]:https://github.com/gitextensions/gitextensions/pull/9217
[9216]:https://github.com/gitextensions/gitextensions/pull/9216
[9212]:https://github.com/gitextensions/gitextensions/pull/9212
[9210]:https://github.com/gitextensions/gitextensions/pull/9210
[9206]:https://github.com/gitextensions/gitextensions/pull/9206
[9203]:https://github.com/gitextensions/gitextensions/pull/9203
[9197]:https://github.com/gitextensions/gitextensions/pull/9197
[9196]:https://github.com/gitextensions/gitextensions/pull/9196
[9188]:https://github.com/gitextensions/gitextensions/pull/9188
[9187]:https://github.com/gitextensions/gitextensions/pull/9187
[9186]:https://github.com/gitextensions/gitextensions/pull/9186
[9184]:https://github.com/gitextensions/gitextensions/pull/9184
[9182]:https://github.com/gitextensions/gitextensions/pull/9182
[9172]:https://github.com/gitextensions/gitextensions/pull/9172
[9171]:https://github.com/gitextensions/gitextensions/pull/9171
[9170]:https://github.com/gitextensions/gitextensions/pull/9170
[9161]:https://github.com/gitextensions/gitextensions/pull/9161
[9149]:https://github.com/gitextensions/gitextensions/pull/9149
[9148]:https://github.com/gitextensions/gitextensions/pull/9148
[9145]:https://github.com/gitextensions/gitextensions/pull/9145
[9141]:https://github.com/gitextensions/gitextensions/pull/9141
[9140]:https://github.com/gitextensions/gitextensions/pull/9140
[9136]:https://github.com/gitextensions/gitextensions/pull/9136
[9132]:https://github.com/gitextensions/gitextensions/pull/9132
[9129]:https://github.com/gitextensions/gitextensions/pull/9129
[9123]:https://github.com/gitextensions/gitextensions/pull/9123
[9122]:https://github.com/gitextensions/gitextensions/pull/9122
[9107]:https://github.com/gitextensions/gitextensions/pull/9107
[9106]:https://github.com/gitextensions/gitextensions/pull/9106
[9105]:https://github.com/gitextensions/gitextensions/pull/9105
[9097]:https://github.com/gitextensions/gitextensions/pull/9097
[9095]:https://github.com/gitextensions/gitextensions/pull/9095
[9087]:https://github.com/gitextensions/gitextensions/pull/9087
[9086]:https://github.com/gitextensions/gitextensions/pull/9086
[9082]:https://github.com/gitextensions/gitextensions/pull/9082
[9081]:https://github.com/gitextensions/gitextensions/pull/9081
[9078]:https://github.com/gitextensions/gitextensions/pull/9078
[9075]:https://github.com/gitextensions/gitextensions/pull/9075
[9073]:https://github.com/gitextensions/gitextensions/pull/9073
[9072]:https://github.com/gitextensions/gitextensions/pull/9072
[9066]:https://github.com/gitextensions/gitextensions/pull/9066
[9064]:https://github.com/gitextensions/gitextensions/pull/9064
[9063]:https://github.com/gitextensions/gitextensions/pull/9063
[9060]:https://github.com/gitextensions/gitextensions/pull/9060
[9059]:https://github.com/gitextensions/gitextensions/pull/9059
[9058]:https://github.com/gitextensions/gitextensions/pull/9058
[9057]:https://github.com/gitextensions/gitextensions/pull/9057
[9056]:https://github.com/gitextensions/gitextensions/pull/9056
[9053]:https://github.com/gitextensions/gitextensions/pull/9053
[9050]:https://github.com/gitextensions/gitextensions/pull/9050
[9049]:https://github.com/gitextensions/gitextensions/pull/9049
[9045]:https://github.com/gitextensions/gitextensions/pull/9045
[9044]:https://github.com/gitextensions/gitextensions/pull/9044
[9040]:https://github.com/gitextensions/gitextensions/pull/9040
[9039]:https://github.com/gitextensions/gitextensions/pull/9039
[9038]:https://github.com/gitextensions/gitextensions/pull/9038
[9037]:https://github.com/gitextensions/gitextensions/pull/9037
[9034]:https://github.com/gitextensions/gitextensions/pull/9034
[9028]:https://github.com/gitextensions/gitextensions/pull/9028
[9027]:https://github.com/gitextensions/gitextensions/pull/9027
[9025]:https://github.com/gitextensions/gitextensions/pull/9025
[9018]:https://github.com/gitextensions/gitextensions/pull/9018
[9011]:https://github.com/gitextensions/gitextensions/pull/9011
[9010]:https://github.com/gitextensions/gitextensions/pull/9010
[9009]:https://github.com/gitextensions/gitextensions/pull/9009
[9008]:https://github.com/gitextensions/gitextensions/pull/9008
[9004]:https://github.com/gitextensions/gitextensions/pull/9004
[8996]:https://github.com/gitextensions/gitextensions/pull/8996
[8995]:https://github.com/gitextensions/gitextensions/pull/8995
[8994]:https://github.com/gitextensions/gitextensions/pull/8994
[8993]:https://github.com/gitextensions/gitextensions/pull/8993
[8990]:https://github.com/gitextensions/gitextensions/pull/8990
[8986]:https://github.com/gitextensions/gitextensions/pull/8986
[8984]:https://github.com/gitextensions/gitextensions/pull/8984
[8983]:https://github.com/gitextensions/gitextensions/pull/8983
[8982]:https://github.com/gitextensions/gitextensions/pull/8982
[8981]:https://github.com/gitextensions/gitextensions/pull/8981
[8975]:https://github.com/gitextensions/gitextensions/pull/8975
[8973]:https://github.com/gitextensions/gitextensions/pull/8973
[8972]:https://github.com/gitextensions/gitextensions/pull/8972
[8967]:https://github.com/gitextensions/gitextensions/pull/8967
[8964]:https://github.com/gitextensions/gitextensions/pull/8964
[8962]:https://github.com/gitextensions/gitextensions/pull/8962
[8961]:https://github.com/gitextensions/gitextensions/pull/8961
[8959]:https://github.com/gitextensions/gitextensions/pull/8959
[8958]:https://github.com/gitextensions/gitextensions/pull/8958
[8957]:https://github.com/gitextensions/gitextensions/pull/8957
[8956]:https://github.com/gitextensions/gitextensions/pull/8956
[8954]:https://github.com/gitextensions/gitextensions/pull/8954
[8952]:https://github.com/gitextensions/gitextensions/pull/8952
[8950]:https://github.com/gitextensions/gitextensions/pull/8950
[8949]:https://github.com/gitextensions/gitextensions/pull/8949
[8943]:https://github.com/gitextensions/gitextensions/pull/8943
[8942]:https://github.com/gitextensions/gitextensions/pull/8942
[8939]:https://github.com/gitextensions/gitextensions/pull/8939
[8938]:https://github.com/gitextensions/gitextensions/pull/8938
[8937]:https://github.com/gitextensions/gitextensions/pull/8937
[8936]:https://github.com/gitextensions/gitextensions/pull/8936
[8935]:https://github.com/gitextensions/gitextensions/pull/8935
[8934]:https://github.com/gitextensions/gitextensions/pull/8934
[8933]:https://github.com/gitextensions/gitextensions/pull/8933
[8932]:https://github.com/gitextensions/gitextensions/pull/8932
[8929]:https://github.com/gitextensions/gitextensions/pull/8929
[8928]:https://github.com/gitextensions/gitextensions/pull/8928
[8927]:https://github.com/gitextensions/gitextensions/pull/8927
[8926]:https://github.com/gitextensions/gitextensions/pull/8926
[8925]:https://github.com/gitextensions/gitextensions/pull/8925
[8924]:https://github.com/gitextensions/gitextensions/pull/8924
[8923]:https://github.com/gitextensions/gitextensions/pull/8923
[8907]:https://github.com/gitextensions/gitextensions/pull/8907
[8903]:https://github.com/gitextensions/gitextensions/pull/8903
[8902]:https://github.com/gitextensions/gitextensions/pull/8902
[8901]:https://github.com/gitextensions/gitextensions/pull/8901
[8900]:https://github.com/gitextensions/gitextensions/pull/8900
[8899]:https://github.com/gitextensions/gitextensions/pull/8899
[8898]:https://github.com/gitextensions/gitextensions/pull/8898
[8897]:https://github.com/gitextensions/gitextensions/pull/8897
[8896]:https://github.com/gitextensions/gitextensions/pull/8896
[8894]:https://github.com/gitextensions/gitextensions/pull/8894
[8888]:https://github.com/gitextensions/gitextensions/pull/8888
[8887]:https://github.com/gitextensions/gitextensions/pull/8887
[8886]:https://github.com/gitextensions/gitextensions/pull/8886
[8883]:https://github.com/gitextensions/gitextensions/pull/8883
[8882]:https://github.com/gitextensions/gitextensions/pull/8882
[8881]:https://github.com/gitextensions/gitextensions/pull/8881
[8878]:https://github.com/gitextensions/gitextensions/pull/8878
[8877]:https://github.com/gitextensions/gitextensions/pull/8877
[8876]:https://github.com/gitextensions/gitextensions/pull/8876
[8875]:https://github.com/gitextensions/gitextensions/pull/8875
[8868]:https://github.com/gitextensions/gitextensions/pull/8868
[8867]:https://github.com/gitextensions/gitextensions/pull/8867
[8866]:https://github.com/gitextensions/gitextensions/pull/8866
[8860]:https://github.com/gitextensions/gitextensions/pull/8860
[8854]:https://github.com/gitextensions/gitextensions/pull/8854
[8846]:https://github.com/gitextensions/gitextensions/pull/8846
[8844]:https://github.com/gitextensions/gitextensions/pull/8844
[8838]:https://github.com/gitextensions/gitextensions/pull/8838
[8836]:https://github.com/gitextensions/gitextensions/pull/8836
[8830]:https://github.com/gitextensions/gitextensions/pull/8830
[8819]:https://github.com/gitextensions/gitextensions/pull/8819
[8809]:https://github.com/gitextensions/gitextensions/pull/8809
[8804]:https://github.com/gitextensions/gitextensions/pull/8804
[8799]:https://github.com/gitextensions/gitextensions/pull/8799
[8797]:https://github.com/gitextensions/gitextensions/pull/8797
[8790]:https://github.com/gitextensions/gitextensions/pull/8790
[8709]:https://github.com/gitextensions/gitextensions/pull/8709
[8700]:https://github.com/gitextensions/gitextensions/pull/8700
[8522]:https://github.com/gitextensions/gitextensions/pull/8522
[8452]:https://github.com/gitextensions/gitextensions/pull/8452
[8156]:https://github.com/gitextensions/gitextensions/pull/8156
[8010]:https://github.com/gitextensions/gitextensions/pull/8010


### Version 3.5.4 (25 Sep 2021)

#### Changes:
* [9589] Show toolbar grips
* [9566] Fix leaking brushes
* [9551] fix: Show current branch resets when switch repos
* [9550] Restore splitter persistence
* [9521] Fix Clone URL Population
* [9505] CommitDiff: Do not Join() in Dispose()


[9589]:https://github.com/gitextensions/gitextensions/pull/9589
[9566]:https://github.com/gitextensions/gitextensions/pull/9566
[9551]:https://github.com/gitextensions/gitextensions/pull/9551
[9550]:https://github.com/gitextensions/gitextensions/pull/9550
[9521]:https://github.com/gitextensions/gitextensions/pull/9521
[9505]:https://github.com/gitextensions/gitextensions/pull/9505

### Version 3.5.3 (17 Aug 2021)

#### Changes:
* [9464] Show ChecklistSettings also for settings without an autosolver
* [9471] Unable to start ConEmu console
* [9396] Improve handling of encodings with conflicting names


[9464]:https://github.com/gitextensions/gitextensions/issues/9464#issuecomment-898202703
[9471]:https://github.com/gitextensions/gitextensions/issues/9471
[9396]:https://github.com/gitextensions/gitextensions/pull/9396


### Version 3.5.2 (06 Aug 2021)

#### Changes:
* [9374] Make showing commit message body in the revision graph optional
* [9351] Fixup user scripts running PS in foreground
* [9350] Fixup main menu item `Git bash`
* [9343] [NBug] An item with the same key has already been added.
* [9342] GitRef: Remote may be as long as Name


[9374]:https://github.com/gitextensions/gitextensions/pull/9374
[9351]:https://github.com/gitextensions/gitextensions/pull/9351
[9350]:https://github.com/gitextensions/gitextensions/pull/9350
[9343]:https://github.com/gitextensions/gitextensions/issues/9343
[9342]:https://github.com/gitextensions/gitextensions/pull/9342


### Version 3.5.1 (04 Jul 2021)

#### Changes:
* [9321] Fixup GDI object leaks
* [9314] Exclude vimdiff1 from custom difftools
* [9313] Fix PathUtil handling of spaces in file names (for 3.5)
* [9311] Reduce window flicker and loss of focus
* [9303] Open Delete Tag dialog from Left Panel with full tag name
* [9289] Create empty settings file if missing
* [9288] Settings not stored (on first run)
* [9281] Allow line patching for new worktree/index files
* [9280] Broken hunks stage/unstage for new files
* [9272] Fixup `AcceptButton` of `FormCheckoutBranch`
* [9253] RevGrid multiline indicators 3.5
* [9242] Use fullPathResolver for viewing local files
* [9233] Left Panel: Check `TreeViewNode.TreeView` for `null`
* [9230] Avoid `FormatException` from `FormMergeSubmodule`
* [9227] Log errors
* [9207] Accessibility: revision grid branch colors
* [9205] `CommitInfo`: Fix up threading issues
* [9201] FormCreateBranch: Fix branches list display on 2 lines
* [9173] Ignore failure getting `Process.Id` when opening URL
* [9167] Add `BugReporter.exe` to installer
* [9166] 3.5.0: Application randomly closes
* [9130] Quote path for opening with `Explorer.exe`
* [9125] Workaround for splitter persistence in `FormBrowse`
* [9121] Stash pop is not refreshing the window
* [9116] Update Clone dialog's handling of clipboard text
* [9114] Let installer not activate disabled telemetry
* [9112] SSH broken after upgrade to 3.5.0 (due to breaking setting change)
* [9111] Accessibility: contrast too low
* [9110] Missing Newtonsoft.Json.dll if installed with no plugins selected / Git Extensions (3.5.0.11713)
* [9026] 3.5 RC1 installer issues


[9321]:https://github.com/gitextensions/gitextensions/pull/9321
[9314]:https://github.com/gitextensions/gitextensions/pull/9314
[9313]:https://github.com/gitextensions/gitextensions/pull/9313
[9311]:https://github.com/gitextensions/gitextensions/pull/9311
[9303]:https://github.com/gitextensions/gitextensions/pull/9303
[9289]:https://github.com/gitextensions/gitextensions/pull/9289
[9288]:https://github.com/gitextensions/gitextensions/issues/9288
[9281]:https://github.com/gitextensions/gitextensions/pull/9281
[9280]:https://github.com/gitextensions/gitextensions/issues/9280
[9272]:https://github.com/gitextensions/gitextensions/pull/9272
[9253]:https://github.com/gitextensions/gitextensions/pull/9253
[9242]:https://github.com/gitextensions/gitextensions/pull/9242
[9233]:https://github.com/gitextensions/gitextensions/pull/9233
[9230]:https://github.com/gitextensions/gitextensions/pull/9230
[9227]:https://github.com/gitextensions/gitextensions/pull/9227
[9207]:https://github.com/gitextensions/gitextensions/pull/9207
[9205]:https://github.com/gitextensions/gitextensions/pull/9205
[9201]:https://github.com/gitextensions/gitextensions/pull/9201
[9173]:https://github.com/gitextensions/gitextensions/pull/9173
[9167]:https://github.com/gitextensions/gitextensions/pull/9167
[9166]:https://github.com/gitextensions/gitextensions/issues/9166
[9130]:https://github.com/gitextensions/gitextensions/pull/9130
[9125]:https://github.com/gitextensions/gitextensions/pull/9125
[9121]:https://github.com/gitextensions/gitextensions/issues/9121
[9116]:https://github.com/gitextensions/gitextensions/pull/9116
[9114]:https://github.com/gitextensions/gitextensions/pull/9114
[9112]:https://github.com/gitextensions/gitextensions/issues/9112
[9111]:https://github.com/gitextensions/gitextensions/issues/9111
[9110]:https://github.com/gitextensions/gitextensions/issues/9110
[9026]:https://github.com/gitextensions/gitextensions/issues/9026


### Version 3.5 (25 April 2021)

#### Changes:
* [9103] GitHub token: Add a link to let the user generate its Personal Access Token 
* [9102] Restore splitter persistence
* [9094] Handle more exceptions as failed external operation
* [9083] Report bugs in a separate process
* [9018] Ignore git clone prefix when checking clipboard for source URL
* [8971] Support Bitbucket repos with dots in the name

[9103]:https://github.com/gitextensions/gitextensions/pull/9103
[9102]:https://github.com/gitextensions/gitextensions/pull/9102
[9094]:https://github.com/gitextensions/gitextensions/pull/9094
[9083]:https://github.com/gitextensions/gitextensions/pull/9083
[9018]:https://github.com/gitextensions/gitextensions/pull/9018
[8971]:https://github.com/gitextensions/gitextensions/pull/8971


### Version 3.5-RC1 (21 March 2021)

#### Changes:
* [9017] Push from empty FormCommit
* [9012] #8684 Avatars are misaligned in blame form
* [9006] Avoid invalid line number in DiffHighlightService
* [9003] FileSettingsCache: No exception if Git config is inaccessible
* [9001] Check BeforeCommit script failure to abort commit
* [8992] Include spelling dictionaries
* [8991] Inject current branch slug into the documentation link
* [8988] Improve display of exceptions 3.5
* [8987] #8684 Avatars are misaligned in blame form
* [8979] GitStatusMonitor increase minimum time between updates 3.5
* [8978] Submodule: Recreate tree only at structure changes 3.5
* [8953] RevDiff: Show worktree/index changes similar to FormCommit
* [8908] RevDiff: Request GitStatus updates at file manipulations 3.5
* [8893] Remove /en/latest from Doc links 3.5
* [8892] Submodule status updated too often 3.5
* [8874] Left panel: Make "Expand all" and "Collapse all" based on selected node
* [8861] Fix ownership of FormUpdates
* [8856] 3.5 AppVeyor: Handle v2 tokens
* [8849] Add borders in Commit Form and set splitter width like in VS
* [8845] Notify of repo changes when unused branches were deleted
* [8840] Check if path exists before creating FileInfo
* [8833] Jenkins: requery new running jobs
* [8826] Restore/expose IGitUICommands.StartRemotesDialog
* [8824] Reset Another branch: prevent special characters in path to fail command
* [8821] Incorrect colors for left panel icons
* [8820] Fix dark theme visually corrupt after dpi change
* [8815] Fix tab control border in high dpi
* [8812] Prioritize main as master in CommitInfo
* [8807] Fix toolbar order (revisited)
* [8806] ImageView: View failures as dump
* [8805] Increase the number of commit templates
* [8801] Fix filter by branch with globbing characters *?[]
* [8786] Update open directory
* [8775] Pass 'skip' parameter
* [8763] Review feedback on PR #8731 (StageAll,UnstageAll)
* [8761] Change TextImageRelation For "Reset All" Button
* [8756] Translate ToolTips
* [8754] FileViewer: Return inapplicable hotkeys as unhandled
* [8751] Show both bisect and git operation notifications
* [8748] fix: Select diff when there are staged and unstaged files
* [8738] Use Executable for launching new GE instances
* [8736] fix crash on refresh when submodule nodes selected
* [8731] StageAll and UnstageAll improvement when Filter active #8596
* [8727] ResolveConflicts: Incorrect evaluation of mergetool path
* [8726] Sidepanel submodule improvements
* [8725] Verb "open" for ShellExecute
* [8721] Process.Start with ExternalOperationException
* [8719] Revert ability for move and show/hide toolbars
* [8718] Handle external I/O exceptions that user can fix
* [8716] Fix high CPU usage of TeamCityAdapter
* [8713] TranslationApp: Include "AccessibleName"
* [8708] FileViewer: Consistently use Stage/Reset  Lines
* [8703]  Submodule browse: Select both first/second commits
* [8697] Handle multi-part names for author initials
* [8690] FormResolveConflict: Allow open without cmd/path
* [8687] GitStatusMonitor: Display when inactive
* [8682] A variety of accessibility-related fixes for the contents of the main pane on the Config page 
* [8679] Don't persist or restore the state of the main menu
* [8678] Improve handling of Drag and drop
* [8667] Left panel enhancement
* [8661] Remove support for UTF-7
* [8660] Custom mergetool
* [8659] Render tabs in the text viewer
* [8648] Interactive rebase: Highlight 4 byte sha
* [8646] Submodule context menus: Only show if dir exists
* [8644] Default debug startup arguments
* [8639] FormResetAnotherBranch: display local tracking branches first
* [8638] Fix rebase status displayed during a rebase
* [8631] Fix #8630: better handle "forbidden" filename
* [8628] RangeDiff: Commit count in name
* [8627] FileHistory: Git command log
* [8626] Blame: Spinner not dismissed for empty files and errors
* [8622] Prefer correct local and remote branches sort to tags
* [8619] Fix left panel broken sort and filtering
* [8616] Exception closing GE when shutting down Windows
* [8615] diffmerge updates
* [8604] RevDiff context menu for cherry-pick
* [8601] Fix Open with Diff Tool does not work with staged renamed file in FormCommit dialog
* [8594] FormPush: fix tracked branches selection
* [8593] FormGoToCommit: set focus on commit textbox when no control has focus
* [8587] Do not  throw if Git is upgraded  while GE is started
* [8576] fix background / foreground contrast for gray text
* [8575] FullPathResolver: Avoid exception for illegal characters
* [8573] FormPush show ahead/behind for  multiple branches
* [8572] Break the toolstrip
* [8568] Reduced the number of draw calls on the commit info panel.
* [8564] Implement custom avatar providers and refactor avatar handling.
* [8563] Draw a separator line at the top of the footer panel, similar to what Task Dialog does
* [8562] Ahead/behind show up-to-date and gone
* [8559] Add dialog help that navigates to the docs
* [8557] Add paddings and borders for panels
* [8556] Fixes #7557 - Dashboard: GitHub clone shortcut missing at initial load
* [8555] update button mnemonic for Reset button on Reset Form to "E"
* [8550] gray additional commit lines in revision grid
* [8549] FormPush multiple: force-with-lease if rejected
* [8543] Add ability to show "Add submodule" Dialog on taskbar.
* [8541] Fixes #8327 push commit does clear commit message
* [8537] Fixes #8146 "Keyboard navigation : Enter Key should open a recent repository"
* [8534] RevisionGrid: Prevent inopportune crash
* [8533] BackgroundFetch & push --force-with-lease : add a warning message
* [8532] Detect when Azure DevOps token is invalid/expired
* [8529] Fix #8188: cannot force push tags
* [8528] Use SelectedNode property to track target for context menu
* [8527] updated removeFiles to use GetBatchOutput and added unit tests
* [8525] Submodules window now keeps the splitter position
* [8524] Ensure focus is on cancel button for Reset Form
* [8523] [#5499] Fix invisible row selection in the file status list control.
* [8520] FormPush: Add context menu to easily manage multiple push selection
* [8516] range-diff presentation
* [8515] Push multiple: Set default push to false
* [8511] Don't refresh revision grid invoking toolstrip actions
* [8510] Update layout
* [8509] Longer GPG key input box (8 chars -> 16 chars)
* [8506] FormEditor SaveCanges Changes
* [8504] Parallelized Repo Validity Checks during Startup
* [8503] Correct Sort context menu placement
* [8502] Add a button to be able to open Application and User theme folders
* [8501] Apply action to all selected files in solve merge conflict
* [8499] Fix JumpList being created too soon
* [8496] FileViewer: ViewMode to track what is currently displayed
* [8494] fix: Write and read enum setting as string not number
* [8491] Improve `FormCreateBranch` information
* [8490] Dialogs layout alignment - `FormCreateBranch`
* [8489] Reset filter when switch repos
* [8488] Add script options {sSubject} and {cSubject}
* [8487] fix: Resolve script "c"-variables
* [8485] fix: Branch filter isn't truly reset
* [8482] Show empty diffs as patch, not text
* [8471] support for range-diff
* [8469] Add tests for AppSettings
* [8465] Prevent adding "--prune --prune-tags" when run from toolbar
* [8458] Don't use "AddSettingBinding" in "FormBrowseRepoSettingsPage"
* [8456] Settings [cleanup]
* [8454] Fix 8351 - "Araxis does not show up in Mergetool/Difftool dropdown box"
* [8451] Settings updated event [cleanup]
* [8450] AppSettings Updated event
* [8449] Don't expand branches node on refresh
* [8444] Jenkins: Present most recent interesting build info
* [8435] Dark titlebar when using a dark theme on a recent Win10
* [8434] Cleanup `GitCommandHelpers`
* [8433] Add checkbox to prune tags
* [8427] Fix 6310 Provide ability to sort/order branches and tags
* [8426] Colorblind syntax colors
* [8424] .css inheritance in themes
* [8414] Filter out hidden branches
* [8413] Dont refresh after `FormInit` cancels
* [8409] Ensure consistent margins
* [8408] Theme variations for colorblindness
* [8405] Add 'Run script' command to branches pane context menu
* [8385] Fix up splitter positions
* [8384] Fixes #8348. Allow to create patch for the root commit.
* [8372] Optimise `FormSettings` load
* [8371] Added support for showing multi-line commit messages in the revision graph
* [8362] Update instructions for portable in README.md
* [8359] Dialogs layout alignment `FormCheckoutBranch`
* [8358] Better exception presentation
* [8357] fix test: SaveImpl_should_throw_if_invalid_path
* [8341] Constrain child windows to monitor bounds
* [8336] Avoid round brackets in testcase argument strings
* [8335] DiffTools: add 'tortoisediff' as a tool alias
* [8334] SubmoduleTests: Setup repos once for all tests
* [8322] Fix Gource avatar feature
* [8313] Add missing early return from OnRebaseInteractivelyClicked
* [8311] Clean up of status reporting of failed operations
* [8308] Avoid IEnumerable<> in test case signature
* [8307] Introduce a new console styles settings page under Appearance
* [8305] Refactor to replace magic hash string with variable
* [8303] Rework FormStatus, FormProgress and FormRemoteProgress
* [8298] Customise GitHub issue templates
* [8295] Refactor MessageBoxes
* [8294] Improve CommandLog
* [8292] Dialogs layout alignment
* [8291] Use TaskDialog from Microsoft.WindowsAPICodePack
* [8289] Add "Ignore Date" and "Committer Date Is Author Date" options to rebase dialog
* [8275] RevFile: Diff any two files
* [8273] Add a hint regarding updates of portable builds
* [8254] RevDiff: Remove parent diff
* [8239] RevDiff: Add hotkeys to stage/unstage/reset
* [8216] fix ignoring app.manifest
* [8215] Apply also font size setting to the ConEmu popup
* [8213] FormBrowse: Add all supported shells to the toolbar
* [8200] Use DataGridViewCellMouseEventArgs.RowIndex
* [8195] Do not use Cursor.Position in Mouse event handlers
* [8194] RevDiff custom difftool
* [8193] RevDiff: compare any two files
* [8157] Tooltip for merged branches
* [8130] Support 0 options in ScriptOptionsParser.AskToSpecify
* [8124] Restore splitter positions
* [8119] Submodule status: Throttle instead of drop frequent updates
* [8117] Submodule status: Present dirty submodules
* [8116] Bugfix/i8115 submodule gitstatus
* [8111] Fix/script without icon
* [8101] Limit number of diff revisions
* [8086] RevDiff: FirstRevision is null for the initial commit
* [8084] Improve "Limit number of commits" option
* [8078] Improve tooltip for diff selection
* [8076] Run FormFileHistory in a separate GE instance
* [8075] Catch all types of exception thrown by ReadAllText
* [8067] Fix 8055 add sign-off to FormApplyPatch 
* [8065] Align folder/copy for submodule
* [8049] Fix 7097 crash invalid escape character in .gitmodule
* [8038] Fix for #8030 URL in "Edit Remote Details"is case insensitive
* [8035] Indicate merged branches in left panel
* [7825] RevisionDiff: Stage/Unstage selected lines
* [6339] Add ability to Copy to Clipboard, similar to commit context menu from left panel


[9017]:https://github.com/gitextensions/gitextensions/pull/9017
[9012]:https://github.com/gitextensions/gitextensions/pull/9012
[9006]:https://github.com/gitextensions/gitextensions/pull/9006
[9003]:https://github.com/gitextensions/gitextensions/pull/9003
[9001]:https://github.com/gitextensions/gitextensions/pull/9001
[8992]:https://github.com/gitextensions/gitextensions/pull/8992
[8991]:https://github.com/gitextensions/gitextensions/pull/8991
[8988]:https://github.com/gitextensions/gitextensions/pull/8988
[8987]:https://github.com/gitextensions/gitextensions/pull/8987
[8979]:https://github.com/gitextensions/gitextensions/pull/8979
[8978]:https://github.com/gitextensions/gitextensions/pull/8978
[8953]:https://github.com/gitextensions/gitextensions/pull/8953
[8908]:https://github.com/gitextensions/gitextensions/pull/8908
[8893]:https://github.com/gitextensions/gitextensions/pull/8893
[8892]:https://github.com/gitextensions/gitextensions/pull/8892
[8874]:https://github.com/gitextensions/gitextensions/pull/8874
[8861]:https://github.com/gitextensions/gitextensions/pull/8861
[8856]:https://github.com/gitextensions/gitextensions/pull/8856
[8849]:https://github.com/gitextensions/gitextensions/pull/8849
[8845]:https://github.com/gitextensions/gitextensions/pull/8845
[8840]:https://github.com/gitextensions/gitextensions/pull/8840
[8833]:https://github.com/gitextensions/gitextensions/pull/8833
[8826]:https://github.com/gitextensions/gitextensions/pull/8826
[8824]:https://github.com/gitextensions/gitextensions/pull/8824
[8821]:https://github.com/gitextensions/gitextensions/pull/8821
[8820]:https://github.com/gitextensions/gitextensions/pull/8820
[8815]:https://github.com/gitextensions/gitextensions/pull/8815
[8812]:https://github.com/gitextensions/gitextensions/pull/8812
[8807]:https://github.com/gitextensions/gitextensions/pull/8807
[8806]:https://github.com/gitextensions/gitextensions/pull/8806
[8805]:https://github.com/gitextensions/gitextensions/pull/8805
[8801]:https://github.com/gitextensions/gitextensions/pull/8801
[8786]:https://github.com/gitextensions/gitextensions/pull/8786
[8775]:https://github.com/gitextensions/gitextensions/pull/8775
[8763]:https://github.com/gitextensions/gitextensions/pull/8763
[8761]:https://github.com/gitextensions/gitextensions/pull/8761
[8756]:https://github.com/gitextensions/gitextensions/pull/8756
[8754]:https://github.com/gitextensions/gitextensions/pull/8754
[8751]:https://github.com/gitextensions/gitextensions/pull/8751
[8748]:https://github.com/gitextensions/gitextensions/pull/8748
[8738]:https://github.com/gitextensions/gitextensions/pull/8738
[8736]:https://github.com/gitextensions/gitextensions/pull/8736
[8731]:https://github.com/gitextensions/gitextensions/pull/8731
[8727]:https://github.com/gitextensions/gitextensions/pull/8727
[8726]:https://github.com/gitextensions/gitextensions/pull/8726
[8725]:https://github.com/gitextensions/gitextensions/pull/8725
[8721]:https://github.com/gitextensions/gitextensions/pull/8721
[8719]:https://github.com/gitextensions/gitextensions/pull/8719
[8718]:https://github.com/gitextensions/gitextensions/pull/8718
[8716]:https://github.com/gitextensions/gitextensions/pull/8716
[8713]:https://github.com/gitextensions/gitextensions/pull/8713
[8708]:https://github.com/gitextensions/gitextensions/pull/8708
[8703]:https://github.com/gitextensions/gitextensions/pull/8703
[8697]:https://github.com/gitextensions/gitextensions/pull/8697
[8690]:https://github.com/gitextensions/gitextensions/pull/8690
[8687]:https://github.com/gitextensions/gitextensions/pull/8687
[8682]:https://github.com/gitextensions/gitextensions/pull/8682
[8679]:https://github.com/gitextensions/gitextensions/pull/8679
[8678]:https://github.com/gitextensions/gitextensions/pull/8678
[8667]:https://github.com/gitextensions/gitextensions/pull/8667
[8661]:https://github.com/gitextensions/gitextensions/pull/8661
[8660]:https://github.com/gitextensions/gitextensions/pull/8660
[8659]:https://github.com/gitextensions/gitextensions/pull/8659
[8648]:https://github.com/gitextensions/gitextensions/pull/8648
[8646]:https://github.com/gitextensions/gitextensions/pull/8646
[8644]:https://github.com/gitextensions/gitextensions/pull/8644
[8639]:https://github.com/gitextensions/gitextensions/pull/8639
[8638]:https://github.com/gitextensions/gitextensions/pull/8638
[8631]:https://github.com/gitextensions/gitextensions/pull/8631
[8628]:https://github.com/gitextensions/gitextensions/pull/8628
[8627]:https://github.com/gitextensions/gitextensions/pull/8627
[8626]:https://github.com/gitextensions/gitextensions/pull/8626
[8622]:https://github.com/gitextensions/gitextensions/pull/8622
[8619]:https://github.com/gitextensions/gitextensions/pull/8619
[8616]:https://github.com/gitextensions/gitextensions/pull/8616
[8615]:https://github.com/gitextensions/gitextensions/pull/8615
[8604]:https://github.com/gitextensions/gitextensions/pull/8604
[8601]:https://github.com/gitextensions/gitextensions/pull/8601
[8594]:https://github.com/gitextensions/gitextensions/pull/8594
[8593]:https://github.com/gitextensions/gitextensions/pull/8593
[8587]:https://github.com/gitextensions/gitextensions/pull/8587
[8576]:https://github.com/gitextensions/gitextensions/pull/8576
[8575]:https://github.com/gitextensions/gitextensions/pull/8575
[8573]:https://github.com/gitextensions/gitextensions/pull/8573
[8572]:https://github.com/gitextensions/gitextensions/pull/8572
[8568]:https://github.com/gitextensions/gitextensions/pull/8568
[8564]:https://github.com/gitextensions/gitextensions/pull/8564
[8563]:https://github.com/gitextensions/gitextensions/pull/8563
[8562]:https://github.com/gitextensions/gitextensions/pull/8562
[8559]:https://github.com/gitextensions/gitextensions/pull/8559
[8557]:https://github.com/gitextensions/gitextensions/pull/8557
[8556]:https://github.com/gitextensions/gitextensions/pull/8556
[8555]:https://github.com/gitextensions/gitextensions/pull/8555
[8550]:https://github.com/gitextensions/gitextensions/pull/8550
[8549]:https://github.com/gitextensions/gitextensions/pull/8549
[8543]:https://github.com/gitextensions/gitextensions/pull/8543
[8541]:https://github.com/gitextensions/gitextensions/pull/8541
[8537]:https://github.com/gitextensions/gitextensions/pull/8537
[8534]:https://github.com/gitextensions/gitextensions/pull/8534
[8533]:https://github.com/gitextensions/gitextensions/pull/8533
[8532]:https://github.com/gitextensions/gitextensions/pull/8532
[8529]:https://github.com/gitextensions/gitextensions/pull/8529
[8528]:https://github.com/gitextensions/gitextensions/pull/8528
[8527]:https://github.com/gitextensions/gitextensions/pull/8527
[8525]:https://github.com/gitextensions/gitextensions/pull/8525
[8524]:https://github.com/gitextensions/gitextensions/pull/8524
[8523]:https://github.com/gitextensions/gitextensions/pull/8523
[8520]:https://github.com/gitextensions/gitextensions/pull/8520
[8516]:https://github.com/gitextensions/gitextensions/pull/8516
[8515]:https://github.com/gitextensions/gitextensions/pull/8515
[8511]:https://github.com/gitextensions/gitextensions/pull/8511
[8510]:https://github.com/gitextensions/gitextensions/pull/8510
[8509]:https://github.com/gitextensions/gitextensions/pull/8509
[8506]:https://github.com/gitextensions/gitextensions/pull/8506
[8504]:https://github.com/gitextensions/gitextensions/pull/8504
[8503]:https://github.com/gitextensions/gitextensions/pull/8503
[8502]:https://github.com/gitextensions/gitextensions/pull/8502
[8501]:https://github.com/gitextensions/gitextensions/pull/8501
[8499]:https://github.com/gitextensions/gitextensions/pull/8499
[8496]:https://github.com/gitextensions/gitextensions/pull/8496
[8494]:https://github.com/gitextensions/gitextensions/pull/8494
[8491]:https://github.com/gitextensions/gitextensions/pull/8491
[8490]:https://github.com/gitextensions/gitextensions/pull/8490
[8489]:https://github.com/gitextensions/gitextensions/pull/8489
[8488]:https://github.com/gitextensions/gitextensions/pull/8488
[8487]:https://github.com/gitextensions/gitextensions/pull/8487
[8485]:https://github.com/gitextensions/gitextensions/pull/8485
[8482]:https://github.com/gitextensions/gitextensions/pull/8482
[8471]:https://github.com/gitextensions/gitextensions/pull/8471
[8469]:https://github.com/gitextensions/gitextensions/pull/8469
[8465]:https://github.com/gitextensions/gitextensions/pull/8465
[8458]:https://github.com/gitextensions/gitextensions/pull/8458
[8456]:https://github.com/gitextensions/gitextensions/pull/8456
[8454]:https://github.com/gitextensions/gitextensions/pull/8454
[8451]:https://github.com/gitextensions/gitextensions/pull/8451
[8450]:https://github.com/gitextensions/gitextensions/pull/8450
[8449]:https://github.com/gitextensions/gitextensions/pull/8449
[8444]:https://github.com/gitextensions/gitextensions/pull/8444
[8435]:https://github.com/gitextensions/gitextensions/pull/8435
[8434]:https://github.com/gitextensions/gitextensions/pull/8434
[8433]:https://github.com/gitextensions/gitextensions/pull/8433
[8427]:https://github.com/gitextensions/gitextensions/pull/8427
[8426]:https://github.com/gitextensions/gitextensions/pull/8426
[8424]:https://github.com/gitextensions/gitextensions/pull/8424
[8414]:https://github.com/gitextensions/gitextensions/pull/8414
[8413]:https://github.com/gitextensions/gitextensions/pull/8413
[8409]:https://github.com/gitextensions/gitextensions/pull/8409
[8408]:https://github.com/gitextensions/gitextensions/pull/8408
[8405]:https://github.com/gitextensions/gitextensions/pull/8405
[8385]:https://github.com/gitextensions/gitextensions/pull/8385
[8384]:https://github.com/gitextensions/gitextensions/pull/8384
[8372]:https://github.com/gitextensions/gitextensions/pull/8372
[8371]:https://github.com/gitextensions/gitextensions/pull/8371
[8362]:https://github.com/gitextensions/gitextensions/pull/8362
[8359]:https://github.com/gitextensions/gitextensions/pull/8359
[8358]:https://github.com/gitextensions/gitextensions/pull/8358
[8357]:https://github.com/gitextensions/gitextensions/pull/8357
[8341]:https://github.com/gitextensions/gitextensions/pull/8341
[8336]:https://github.com/gitextensions/gitextensions/pull/8336
[8335]:https://github.com/gitextensions/gitextensions/pull/8335
[8334]:https://github.com/gitextensions/gitextensions/pull/8334
[8322]:https://github.com/gitextensions/gitextensions/pull/8322
[8313]:https://github.com/gitextensions/gitextensions/pull/8313
[8311]:https://github.com/gitextensions/gitextensions/pull/8311
[8308]:https://github.com/gitextensions/gitextensions/pull/8308
[8307]:https://github.com/gitextensions/gitextensions/pull/8307
[8305]:https://github.com/gitextensions/gitextensions/pull/8305
[8303]:https://github.com/gitextensions/gitextensions/pull/8303
[8298]:https://github.com/gitextensions/gitextensions/pull/8298
[8295]:https://github.com/gitextensions/gitextensions/pull/8295
[8294]:https://github.com/gitextensions/gitextensions/pull/8294
[8292]:https://github.com/gitextensions/gitextensions/pull/8292
[8291]:https://github.com/gitextensions/gitextensions/pull/8291
[8289]:https://github.com/gitextensions/gitextensions/pull/8289
[8275]:https://github.com/gitextensions/gitextensions/pull/8275
[8273]:https://github.com/gitextensions/gitextensions/pull/8273
[8254]:https://github.com/gitextensions/gitextensions/pull/8254
[8239]:https://github.com/gitextensions/gitextensions/pull/8239
[8216]:https://github.com/gitextensions/gitextensions/pull/8216
[8215]:https://github.com/gitextensions/gitextensions/pull/8215
[8213]:https://github.com/gitextensions/gitextensions/pull/8213
[8200]:https://github.com/gitextensions/gitextensions/pull/8200
[8195]:https://github.com/gitextensions/gitextensions/pull/8195
[8194]:https://github.com/gitextensions/gitextensions/pull/8194
[8193]:https://github.com/gitextensions/gitextensions/pull/8193
[8157]:https://github.com/gitextensions/gitextensions/pull/8157
[8130]:https://github.com/gitextensions/gitextensions/pull/8130
[8124]:https://github.com/gitextensions/gitextensions/pull/8124
[8119]:https://github.com/gitextensions/gitextensions/pull/8119
[8117]:https://github.com/gitextensions/gitextensions/pull/8117
[8116]:https://github.com/gitextensions/gitextensions/pull/8116
[8111]:https://github.com/gitextensions/gitextensions/pull/8111
[8101]:https://github.com/gitextensions/gitextensions/pull/8101
[8086]:https://github.com/gitextensions/gitextensions/pull/8086
[8084]:https://github.com/gitextensions/gitextensions/pull/8084
[8078]:https://github.com/gitextensions/gitextensions/pull/8078
[8076]:https://github.com/gitextensions/gitextensions/pull/8076
[8075]:https://github.com/gitextensions/gitextensions/pull/8075
[8067]:https://github.com/gitextensions/gitextensions/pull/8067
[8065]:https://github.com/gitextensions/gitextensions/pull/8065
[8049]:https://github.com/gitextensions/gitextensions/pull/8049
[8038]:https://github.com/gitextensions/gitextensions/pull/8038
[8035]:https://github.com/gitextensions/gitextensions/pull/8035
[7825]:https://github.com/gitextensions/gitextensions/pull/7825
[6339]:https://github.com/gitextensions/gitextensions/pull/6339


### Version 3.4.3 (23 July 2020)

#### Changes:
* [8329](https://github.com/gitextensions/gitextensions/pull/8329) ConEmu 20.07.13
* [8323](https://github.com/gitextensions/gitextensions/pull/8323) Dark silver theme
* [8319](https://github.com/gitextensions/gitextensions/pull/8319) Resize also the backup/default avatar image
* [8318](https://github.com/gitextensions/gitextensions/pull/8318) FormCommit: No summary  for deleted submodules
* [8296](https://github.com/gitextensions/gitextensions/pull/8296) Disable Finish button while loading branches in GitFlow form.
* [8287](https://github.com/gitextensions/gitextensions/pull/8287) Restore FileStatusList focus after GoToChild/Parent
* [8277](https://github.com/gitextensions/gitextensions/pull/8277) Track theme colors via Telemetry
* [8274](https://github.com/gitextensions/gitextensions/pull/8274) Avoid console printouts when submodule updates are throttled
* [8266](https://github.com/gitextensions/gitextensions/pull/8266) Fix ssh not found
* [8250](https://github.com/gitextensions/gitextensions/pull/8250) Fix "selection only" text search on Diff view (#7784)
* [8212](https://github.com/gitextensions/gitextensions/pull/8212) Recover lost object: Add a file preview

### Version 3.4.2 (21 June 2020)

#### Changes:
* [8255] [NBug] Invalid diff/merge tool requestedParameter name: toolName
* [8252] Draw current commit message bold in detached-head mode
* [8246] Disable context menu items rather than hiding them
* [8240] Make WindowsJumpListManager calls safer
* [8238] Display 'Plugin Manager" next to "Plugin settings" in the menu
* [8237] ResetAnotherBranch: Quote path arguments
* [8231] Centrally handle user script errors


[8255]:https://github.com/gitextensions/gitextensions/pull/8255
[8252]:https://github.com/gitextensions/gitextensions/pull/8252
[8246]:https://github.com/gitextensions/gitextensions/pull/8246
[8240]:https://github.com/gitextensions/gitextensions/pull/8240
[8238]:https://github.com/gitextensions/gitextensions/pull/8238
[8237]:https://github.com/gitextensions/gitextensions/pull/8237
[8231]:https://github.com/gitextensions/gitextensions/pull/8231



### Version 3.4.1 (14 June 2020)

#### Changes:
* [8218] Restore missing app.manifest

[8218]:https://github.com/gitextensions/gitextensions/pull/8218


### Version 3.4 (6 June 2020)

#### Changes:
* [8182] Make scripts execution safer
* [8178] AppVeyor Exception at init
* [8177] Prevent NRE when getting AppVeyor build duration
* [8175] FileStatusList context menu order
* [8164] Link to read-the-docs for release version
* [8163] Shell extension icons
* [8155] Fix up splitter positions
* [8152] Improve display of merged branches
* [8144] Fix up restoring the selection of revisions on reload
* [8140] Add 'Reset another branch to here' feature
* [8138] Hide extra separator in context menu for folder item
* [8137] Fix `FormCommit` context menu issues
* [8133] fix: New remotes not saved, if exceed limit
* [8123] Fix 8122 Update Atlassian.SDK package to latest version
* [8122] Jira Commit Hint throws exception for some Jira issues
* [8118] Fix up user scripts without icon
* [8114] Limit number of diff revisions
* [8093] Add information icon to checkboxes in Settings window
* [8089] [NBug] Could not load file or assembly 'RestSharp, Version=106.1.0...
* [8068] Portable build doesn't contain PluginManager
* [8059] Correct filemode for source files
* [8058] Ignore inaccessible file in GetSelectedBranchFast
* [8056] Select multiple revisions using Left Panel
* [8051] git-diff override  diff.mnemonicprefix=true
* [8050] Unneeded rev-list for submodule status
* [8043] Ignore GitHub API rate limit induced errors when search for app updates
* [8042] Fix 3954 quotes in merge commit message
* [8041] Restore broken AutoCRLF tests
* [8040] Ignore Microsoft.WindowsAPICodePack.Shell.ShellException
* [8034] Cleanup git status before calling TranslationApp
* [8031] Fix compatibility with WSL (\\wsl$\) network resource
* [8020] Update artificial commit diff
* [8016] Prevent an ArgumentOutOfRangeException
* [8012] RevDiff, Commit: Delete untracked directories
* [8008] Fix #7937 Reflog form : index was out of range.
* [7999] Fix up double plural in ResourceManager.Strings
* [7993] Fix up show all branches / tags in history
* [7986] fix: Error while executing user script from RevisionGrid
* [7984] Deleted files presented as Unknown
* [7981] Redesign script config
* [7976] GitStatusMonitor: Avoid background updates if GUI is not visible
* [7974] Centralise removal of invalid repositories
* [7972] Unify the use of "Git Extensions"
* [7970] Revision Links: Detect GH issues like ".../i1111"
* [7968] Fix branch rendering
* [7962] Avoid invalid path exception on invalidated Module.WorkingDir
* [7961] Align context menu order for FileStatusList
* [7959] Fix rename file icon size
* [7958] [NBug] The path has an invalid format.
* [7955] Fix error during Publish build.
* [7951] Merge combined conflict set to IsConflict
* [7950] Uplift script handling
* [7948] fix: Locate diff tools in C:\Program Files\ folder
* [7942] LeftPanel: Fix size of the GitHub remote icon
* [7941] Windows TaskBar JumpList was not initiated
* [7938] formstash improvements
* [7932] Fix/7931
* [7929] Break the build if there are any errors in publish tasks
* [7928] Improve blame gutter display
* [7927] Move settings page titles from designer files
* [7926] Use ViewChangesAsync consistently for changes
* [7924] Align recent branch names
* [7923] RevDiff: Show common/unique files for BASE->selected(B)
* [7922] FileStatus icon: submodule before status updated
* [7921] Fix 3 not reproduced exceptions
* [7919] Settings: Color title label missing
* [7918] [NBug] Attempted to perform an unauthorized operation.
* [7917] Settings > Appearance > Colors has no title
* [7916] Prepopulate branch name in FormCreateBranch
* [7915] Ignore Git error messages in parsing
* [7914] Separate staged status from GetDiff
* [7913] git-status:  use config to set ignoreSubmodules
* [7912] RevDiff: follow selection only in first list group
* [7908] Bugfix/i7898 nre no parent
* [7904] Add root description to Git node in settings tree view
* [7903] Make DropDownWidth in BranchSelector adjust to longest item
* [7900] Simulate continuous scroll to display revision diff
* [7899] RevDiff: Show common files for BASE->A
* [7895] Remove Gerrit plugin
* [7893] Share common GitUI.Strings.Error
* [7891] FormCommit: F3 to diff large files
* [7888] Add hotkey for switching between artificial commits
* [7886] Bring into view already selected branch on ROT click
* [7884] UX: Swap OK and Cancel button Recent Repo Settings UI
* [7883] Add setting to show branch name in recent repos drop down
* [7881] BitBucket plugin update of RestSharp
* [7878] Centralise reference version management
* [7877] fix: Translation update issues
* [7876] More complete CanBeGitURL
* [7875] Tab "Diff" is not updated at changes for artificial commits
* [7872] Fixup translation (last master merge)
* [7869] submodules IsDirty item status
* [7868] Display text if an image cannot be displayed
* [7853] Handle gracefully a NRE on body message
* [7850] Fix slow plugins loading
* [7842] Plugins are extremely slow to load
* [7832] Files starting with space incorrectly handled
* [7831] Show new files as file, not diff 
* [7824] revdiff: Show new file in worktree as file, not diff
* [7820] Surround access to plugin collection with a `lock()`
* [7800] Stash: Form fail to load
* [7798] RevisionDiff: Try retain the file selection when switching commits
* [7797] Convert theme file to css palette
* [7781] Support merge.guitool
* [7779] SidePanel Submodules: Status for top module
* [7775] Fix incomplete reset in ReferenceRepository
* [7770] Adapt submodule button to context
* [7753] Avoid deleting test directories while operations may be in progress
* [7752] Avoid a 200ms delay when RunBackgroundAsync is cancelled
* [7750] Avoid attempting to update non-existent UI
* [7739] FormCommit: Inaccessible COMMITMESSAGE
* [7732] AzureDevOps CI: Fix and improvements
* [7720] improve file status list presentation
* [7719] Refactor: Compare with ObjectId rather than Guid
* [7715] removed TopMost attribute from the FormUpdates dialog
* [7714] GE should not crash when no email applications are available and user…
* [7711] gitex.cmd: handle quotes in commands
* [7694] Delete multiple files at a same time with popup menu
* [7690] FileViewer: Do not access the file system for git blobs
* [7687] RevisionGrid: Directory Diff HotKey
* [7686] Do not deregister from Application.ThreadException between tests
* [7685] Make folder removal safer
* [7678] Add pwsh as a choice of shell in Console tab
* [7677] bugfix: ignore specific ActiveDocument exception
* [7667] ObjectId ShortString: set default to 8 chars
* [7666] Add A/B to RevisionDiff revision descriptions #7626
* [7657] Keep Syntax Highlight Rules Synced With Resources
* [7655] merge conflict: Do not require .cmd or .path
* [7651] ls-tree: Use -- to escape files starting with "-"
* [7647] Improve [Form]CommitInfoTests
* [7641] NRE when GitHub token is invalid
* [7633] Check if path is valid before Path.Combine
* [7624] FileViewer context menu fixes
* [7619] Improve closing of FormCommit
* [7615] Convert and quote file names for BatchUnstageFiles
* [7612] Improve speed of directory change in console tab
* [7603] Merge branch dialog: Make form border style fixed and align checkboxes
* [7597] Improve restoring of the commit dialog geometry (Fixes #7588)
* [7596] Disable maximising/minimising of update form
* [7591] Improve determination of the GitExt directory (fix of issue #7587)
* [7585] Make message textbox bounds clear
* [7584] Prevent NRE in designers that use EditNetSpell
* [7567] Add feature to open repo in a new window
* [7562] Color scheme followup #2
* [7561] RevisionDiff: Show BASE diff
* [7560] RevisionDiff: Limit parents
* [7559] Gerrit: fix server version parsing
* [7558] RevisionDiff: No reset to CombinedDiff
* [7551] Remove obsolete 2-way merge tool names
* [7548] kdiff3: Incorrect merge cmd
* [7547] gitex: If no argument start browse in current workdir
* [7540] Color scheme followup
* [7516] Open repo directly on confirmation of folder dialog
* [7515] Reduce clipping in FormSettings
* [7496] Filter staged files
* [7490] OpenRemoteUrlInBrowser: Issue (#7439)
* [7489] Install with AppGet (https://appget.net/)
* [7485] BuildServerIntegration: Fix freeze due to polling interval adjustment
* [7482] ResetCurrentBranch: Correct tab order
* [7477] Azure devops ci: reduce calls to api when revision grid is refreshed 
* [7457] Add warning header
* [7452] BuildServerIntegration: Adjust polling interval for running builds
* [7445] Remove background thread from RevisionDataGridView
* [7413] NullReferenceException during update check
* [7406] Add view blame in GitHub
* [7334] Add icon to all MessageBox that don't have one
* [7329] Improve revisiongrid tooltips
* [7213] Dark theme
* [7044] Add and refactor diffmerge tools


[8182]:https://github.com/gitextensions/gitextensions/pull/8182
[8178]:https://github.com/gitextensions/gitextensions/pull/8178
[8177]:https://github.com/gitextensions/gitextensions/pull/8177
[8175]:https://github.com/gitextensions/gitextensions/pull/8175
[8164]:https://github.com/gitextensions/gitextensions/pull/8164
[8163]:https://github.com/gitextensions/gitextensions/pull/8163
[8155]:https://github.com/gitextensions/gitextensions/pull/8155
[8152]:https://github.com/gitextensions/gitextensions/pull/8152
[8144]:https://github.com/gitextensions/gitextensions/pull/8144
[8140]:https://github.com/gitextensions/gitextensions/pull/8140
[8138]:https://github.com/gitextensions/gitextensions/pull/8138
[8137]:https://github.com/gitextensions/gitextensions/pull/8137
[8133]:https://github.com/gitextensions/gitextensions/pull/8133
[8123]:https://github.com/gitextensions/gitextensions/pull/8123
[8122]:https://github.com/gitextensions/gitextensions/issues/8122
[8118]:https://github.com/gitextensions/gitextensions/pull/8118
[8114]:https://github.com/gitextensions/gitextensions/pull/8114
[8093]:https://github.com/gitextensions/gitextensions/pull/8093
[8089]:https://github.com/gitextensions/gitextensions/issues/8089
[8088]:https://github.com/gitextensions/gitextensions/pull/8088
[8068]:https://github.com/gitextensions/gitextensions/issues/8068
[8062]:https://github.com/gitextensions/gitextensions/pull/8062
[8059]:https://github.com/gitextensions/gitextensions/pull/8059
[8058]:https://github.com/gitextensions/gitextensions/pull/8058
[8056]:https://github.com/gitextensions/gitextensions/pull/8056
[8051]:https://github.com/gitextensions/gitextensions/pull/8051
[8050]:https://github.com/gitextensions/gitextensions/pull/8050
[8043]:https://github.com/gitextensions/gitextensions/pull/8043
[8042]:https://github.com/gitextensions/gitextensions/pull/8042
[8041]:https://github.com/gitextensions/gitextensions/pull/8041
[8040]:https://github.com/gitextensions/gitextensions/pull/8040
[8039]:https://github.com/gitextensions/gitextensions/pull/8039
[8037]:https://github.com/gitextensions/gitextensions/pull/8037
[8034]:https://github.com/gitextensions/gitextensions/pull/8034
[8031]:https://github.com/gitextensions/gitextensions/pull/8031
[8020]:https://github.com/gitextensions/gitextensions/pull/8020
[8016]:https://github.com/gitextensions/gitextensions/pull/8016
[8012]:https://github.com/gitextensions/gitextensions/pull/8012
[8008]:https://github.com/gitextensions/gitextensions/pull/8008
[7999]:https://github.com/gitextensions/gitextensions/pull/7999
[7998]:https://github.com/gitextensions/gitextensions/pull/7998
[7993]:https://github.com/gitextensions/gitextensions/pull/7993
[7987]:https://github.com/gitextensions/gitextensions/pull/7987
[7986]:https://github.com/gitextensions/gitextensions/pull/7986
[7984]:https://github.com/gitextensions/gitextensions/pull/7984
[7981]:https://github.com/gitextensions/gitextensions/pull/7981
[7976]:https://github.com/gitextensions/gitextensions/pull/7976
[7975]:https://github.com/gitextensions/gitextensions/pull/7975
[7974]:https://github.com/gitextensions/gitextensions/pull/7974
[7972]:https://github.com/gitextensions/gitextensions/pull/7972
[7970]:https://github.com/gitextensions/gitextensions/pull/7970
[7968]:https://github.com/gitextensions/gitextensions/pull/7968
[7962]:https://github.com/gitextensions/gitextensions/pull/7962
[7961]:https://github.com/gitextensions/gitextensions/pull/7961
[7960]:https://github.com/gitextensions/gitextensions/pull/7960
[7959]:https://github.com/gitextensions/gitextensions/pull/7959
[7958]:https://github.com/gitextensions/gitextensions/issues/7958
[7955]:https://github.com/gitextensions/gitextensions/pull/7955
[7951]:https://github.com/gitextensions/gitextensions/pull/7951
[7950]:https://github.com/gitextensions/gitextensions/pull/7950
[7948]:https://github.com/gitextensions/gitextensions/pull/7948
[7946]:https://github.com/gitextensions/gitextensions/pull/7946
[7942]:https://github.com/gitextensions/gitextensions/pull/7942
[7941]:https://github.com/gitextensions/gitextensions/pull/7941
[7938]:https://github.com/gitextensions/gitextensions/pull/7938
[7933]:https://github.com/gitextensions/gitextensions/pull/7933
[7932]:https://github.com/gitextensions/gitextensions/pull/7932
[7929]:https://github.com/gitextensions/gitextensions/pull/7929
[7928]:https://github.com/gitextensions/gitextensions/pull/7928
[7927]:https://github.com/gitextensions/gitextensions/pull/7927
[7926]:https://github.com/gitextensions/gitextensions/pull/7926
[7925]:https://github.com/gitextensions/gitextensions/pull/7925
[7924]:https://github.com/gitextensions/gitextensions/pull/7924
[7923]:https://github.com/gitextensions/gitextensions/pull/7923
[7922]:https://github.com/gitextensions/gitextensions/pull/7922
[7921]:https://github.com/gitextensions/gitextensions/pull/7921
[7919]:https://github.com/gitextensions/gitextensions/pull/7919
[7918]:https://github.com/gitextensions/gitextensions/issues/7918
[7917]:https://github.com/gitextensions/gitextensions/issues/7917
[7916]:https://github.com/gitextensions/gitextensions/pull/7916
[7915]:https://github.com/gitextensions/gitextensions/pull/7915
[7914]:https://github.com/gitextensions/gitextensions/pull/7914
[7913]:https://github.com/gitextensions/gitextensions/pull/7913
[7912]:https://github.com/gitextensions/gitextensions/pull/7912
[7911]:https://github.com/gitextensions/gitextensions/pull/7911
[7910]:https://github.com/gitextensions/gitextensions/pull/7910
[7908]:https://github.com/gitextensions/gitextensions/pull/7908
[7904]:https://github.com/gitextensions/gitextensions/pull/7904
[7903]:https://github.com/gitextensions/gitextensions/pull/7903
[7900]:https://github.com/gitextensions/gitextensions/pull/7900
[7899]:https://github.com/gitextensions/gitextensions/pull/7899
[7895]:https://github.com/gitextensions/gitextensions/pull/7895
[7893]:https://github.com/gitextensions/gitextensions/pull/7893
[7891]:https://github.com/gitextensions/gitextensions/pull/7891
[7888]:https://github.com/gitextensions/gitextensions/pull/7888
[7886]:https://github.com/gitextensions/gitextensions/pull/7886
[7884]:https://github.com/gitextensions/gitextensions/pull/7884
[7883]:https://github.com/gitextensions/gitextensions/pull/7883
[7881]:https://github.com/gitextensions/gitextensions/pull/7881
[7878]:https://github.com/gitextensions/gitextensions/pull/7878
[7877]:https://github.com/gitextensions/gitextensions/pull/7877
[7876]:https://github.com/gitextensions/gitextensions/pull/7876
[7875]:https://github.com/gitextensions/gitextensions/issues/7875
[7872]:https://github.com/gitextensions/gitextensions/pull/7872
[7869]:https://github.com/gitextensions/gitextensions/pull/7869
[7868]:https://github.com/gitextensions/gitextensions/pull/7868
[7865]:https://github.com/gitextensions/gitextensions/pull/7865
[7853]:https://github.com/gitextensions/gitextensions/pull/7853
[7850]:https://github.com/gitextensions/gitextensions/pull/7850
[7842]:https://github.com/gitextensions/gitextensions/issues/7842
[7837]:https://github.com/gitextensions/gitextensions/pull/7837
[7833]:https://github.com/gitextensions/gitextensions/pull/7833
[7832]:https://github.com/gitextensions/gitextensions/pull/7832
[7831]:https://github.com/gitextensions/gitextensions/pull/7831
[7824]:https://github.com/gitextensions/gitextensions/pull/7824
[7821]:https://github.com/gitextensions/gitextensions/pull/7821
[7820]:https://github.com/gitextensions/gitextensions/pull/7820
[7808]:https://github.com/gitextensions/gitextensions/pull/7808
[7800]:https://github.com/gitextensions/gitextensions/pull/7800
[7798]:https://github.com/gitextensions/gitextensions/pull/7798
[7797]:https://github.com/gitextensions/gitextensions/pull/7797
[7785]:https://github.com/gitextensions/gitextensions/pull/7785
[7781]:https://github.com/gitextensions/gitextensions/pull/7781
[7779]:https://github.com/gitextensions/gitextensions/pull/7779
[7775]:https://github.com/gitextensions/gitextensions/pull/7775
[7772]:https://github.com/gitextensions/gitextensions/pull/7772
[7771]:https://github.com/gitextensions/gitextensions/pull/7771
[7770]:https://github.com/gitextensions/gitextensions/pull/7770
[7769]:https://github.com/gitextensions/gitextensions/pull/7769
[7766]:https://github.com/gitextensions/gitextensions/pull/7766
[7765]:https://github.com/gitextensions/gitextensions/pull/7765
[7760]:https://github.com/gitextensions/gitextensions/pull/7760
[7759]:https://github.com/gitextensions/gitextensions/pull/7759
[7758]:https://github.com/gitextensions/gitextensions/pull/7758
[7756]:https://github.com/gitextensions/gitextensions/pull/7756
[7754]:https://github.com/gitextensions/gitextensions/pull/7754
[7753]:https://github.com/gitextensions/gitextensions/pull/7753
[7752]:https://github.com/gitextensions/gitextensions/pull/7752
[7750]:https://github.com/gitextensions/gitextensions/pull/7750
[7749]:https://github.com/gitextensions/gitextensions/pull/7749
[7747]:https://github.com/gitextensions/gitextensions/pull/7747
[7744]:https://github.com/gitextensions/gitextensions/pull/7744
[7741]:https://github.com/gitextensions/gitextensions/pull/7741
[7739]:https://github.com/gitextensions/gitextensions/pull/7739
[7735]:https://github.com/gitextensions/gitextensions/issues/7735
[7732]:https://github.com/gitextensions/gitextensions/pull/7732
[7729]:https://github.com/gitextensions/gitextensions/pull/7729
[7727]:https://github.com/gitextensions/gitextensions/pull/7727
[7720]:https://github.com/gitextensions/gitextensions/pull/7720
[7719]:https://github.com/gitextensions/gitextensions/pull/7719
[7718]:https://github.com/gitextensions/gitextensions/pull/7718
[7715]:https://github.com/gitextensions/gitextensions/pull/7715
[7714]:https://github.com/gitextensions/gitextensions/pull/7714
[7711]:https://github.com/gitextensions/gitextensions/pull/7711
[7703]:https://github.com/gitextensions/gitextensions/pull/7703
[7698]:https://github.com/gitextensions/gitextensions/pull/7698
[7696]:https://github.com/gitextensions/gitextensions/pull/7696
[7694]:https://github.com/gitextensions/gitextensions/pull/7694
[7691]:https://github.com/gitextensions/gitextensions/pull/7691
[7690]:https://github.com/gitextensions/gitextensions/pull/7690
[7687]:https://github.com/gitextensions/gitextensions/pull/7687
[7686]:https://github.com/gitextensions/gitextensions/pull/7686
[7685]:https://github.com/gitextensions/gitextensions/pull/7685
[7678]:https://github.com/gitextensions/gitextensions/pull/7678
[7677]:https://github.com/gitextensions/gitextensions/pull/7677
[7667]:https://github.com/gitextensions/gitextensions/pull/7667
[7666]:https://github.com/gitextensions/gitextensions/pull/7666
[7657]:https://github.com/gitextensions/gitextensions/pull/7657
[7655]:https://github.com/gitextensions/gitextensions/pull/7655
[7654]:https://github.com/gitextensions/gitextensions/pull/7654
[7651]:https://github.com/gitextensions/gitextensions/pull/7651
[7649]:https://github.com/gitextensions/gitextensions/pull/7649
[7648]:https://github.com/gitextensions/gitextensions/pull/7648
[7647]:https://github.com/gitextensions/gitextensions/pull/7647
[7646]:https://github.com/gitextensions/gitextensions/pull/7646
[7641]:https://github.com/gitextensions/gitextensions/issues/7641
[7633]:https://github.com/gitextensions/gitextensions/pull/7633
[7624]:https://github.com/gitextensions/gitextensions/pull/7624
[7619]:https://github.com/gitextensions/gitextensions/pull/7619
[7615]:https://github.com/gitextensions/gitextensions/pull/7615
[7612]:https://github.com/gitextensions/gitextensions/pull/7612
[7611]:https://github.com/gitextensions/gitextensions/pull/7611
[7610]:https://github.com/gitextensions/gitextensions/pull/7610
[7609]:https://github.com/gitextensions/gitextensions/pull/7609
[7603]:https://github.com/gitextensions/gitextensions/pull/7603
[7597]:https://github.com/gitextensions/gitextensions/pull/7597
[7596]:https://github.com/gitextensions/gitextensions/pull/7596
[7593]:https://github.com/gitextensions/gitextensions/pull/7593
[7591]:https://github.com/gitextensions/gitextensions/pull/7591
[7585]:https://github.com/gitextensions/gitextensions/pull/7585
[7584]:https://github.com/gitextensions/gitextensions/pull/7584
[7567]:https://github.com/gitextensions/gitextensions/pull/7567
[7562]:https://github.com/gitextensions/gitextensions/pull/7562
[7561]:https://github.com/gitextensions/gitextensions/pull/7561
[7560]:https://github.com/gitextensions/gitextensions/pull/7560
[7559]:https://github.com/gitextensions/gitextensions/pull/7559
[7558]:https://github.com/gitextensions/gitextensions/pull/7558
[7551]:https://github.com/gitextensions/gitextensions/pull/7551
[7548]:https://github.com/gitextensions/gitextensions/pull/7548
[7547]:https://github.com/gitextensions/gitextensions/pull/7547
[7540]:https://github.com/gitextensions/gitextensions/pull/7540
[7537]:https://github.com/gitextensions/gitextensions/pull/7537
[7516]:https://github.com/gitextensions/gitextensions/pull/7516
[7515]:https://github.com/gitextensions/gitextensions/pull/7515
[7497]:https://github.com/gitextensions/gitextensions/pull/7497
[7496]:https://github.com/gitextensions/gitextensions/pull/7496
[7490]:https://github.com/gitextensions/gitextensions/pull/7490
[7489]:https://github.com/gitextensions/gitextensions/pull/7489
[7485]:https://github.com/gitextensions/gitextensions/pull/7485
[7482]:https://github.com/gitextensions/gitextensions/pull/7482
[7477]:https://github.com/gitextensions/gitextensions/pull/7477
[7476]:https://github.com/gitextensions/gitextensions/pull/7476
[7457]:https://github.com/gitextensions/gitextensions/pull/7457
[7452]:https://github.com/gitextensions/gitextensions/pull/7452
[7445]:https://github.com/gitextensions/gitextensions/pull/7445
[7413]:https://github.com/gitextensions/gitextensions/issues/7413
[7406]:https://github.com/gitextensions/gitextensions/pull/7406
[7396]:https://github.com/gitextensions/gitextensions/pull/7396
[7334]:https://github.com/gitextensions/gitextensions/pull/7334
[7329]:https://github.com/gitextensions/gitextensions/pull/7329
[7321]:https://github.com/gitextensions/gitextensions/pull/7321
[7213]:https://github.com/gitextensions/gitextensions/pull/7213
[7044]:https://github.com/gitextensions/gitextensions/pull/7044


### Version 3.3.1 (7 Jan 2020)

#### Fixes

* Unable to update due to network configurations - PR [7576]
* gitex: If no argument start browse in current workdir - PR [7554]
* kdiff3: add configuration for merge arguments - PR [7549]
* Installer defaults to PuTTY - PR [7545]
* Prevent crash where _btnPreview is null when event triggered - PR [7536]


[7576]:https://github.com/gitextensions/gitextensions/pull/7576
[7554]:https://github.com/gitextensions/gitextensions/pull/7554
[7549]:https://github.com/gitextensions/gitextensions/pull/7549
[7545]:https://github.com/gitextensions/gitextensions/pull/7545
[7536]:https://github.com/gitextensions/gitextensions/pull/7536


### Version 3.3 (1 Dec 2019)

#### Features:
* Extends Squash feature to all supported types - PR [7401]
* Implement git-reset --merge and --keep options - PR [7367]
* CommitDialog: New Option "Show only my messages" - PR [7337]
* Display multiple pushurl in the Remote sidepanel tooltip - PR [7289]
* Implement quotePath-like codepoint escaping - PR [7288]
* Add Squash feature on GitFlow Plugin Form - PR [7268]
* Build Report: Open the report in the user browser - PR [7224]
* Allow multiple push url for git-remote - PR [7214]
* ConEmu 19.7.14 - PR [7190]
* Add hotkeys for continue rebase, resolve merge conflicts etc. - PR [7144]
* Set recommended Git version to 2.23.0 - PR [7119]
* Make plugin settings development easier - PR [7039]
* Replace git-diff patience with histogram - PR [6997]
* Remove use of possessive in "Cherry pick file's changes" - PR [6910]
* Generate GitHub OAuth token with github api - PR [6888]

#### Fixes:
* Removes extra dot from Save as revfiletree dialog - PR [7460]
* Ignore all potential errors from PowerShell during installation - PR [7442]
* AzureDevOps CI: prevent call if settings invalid - PR [7437]
* fix: SetRevisionFunc produce empty dropdown - PR [7423]
* fix: FormResolveConflicts incorrect row selection - PR [7408]
* catch cannot write to settings - PR [7400]
* fix: Fail attempting an empty path - PR [7383]
* Skip duplicate ref entries when building the Sorted Refs Dictionary - PR [7373]
* Add Submodule: Fill combobox with branches regardless of the repo URL type - PR [7371]
* Commit messages menu: Fix (sometimes) broken display - PR [7369]
* Fix 7364: "Fetch and Prune All" confirmation modal has weird focus behavior - PR [7365]
* increase FormCheckoutBranch Ok button size - PR [7362]
* Fix #7077: Hide push checkbox cells when there's no local branches - PR [7356]
* Truncate large hexdumps - PR [7333]
* Fix #7331: Update GitInfo dependency to get bug fix - PR [7332]
* Consistent display of binary files - PR [7330]
* Update syntax highlighting control - PR [7325]
* Fix #7263: Progress bar value/max value not initialized when resetting files. - PR [7324]
* Fix installer when PS is in ConstrainedLanguage mode - PR [7309]
* Fix #7250:  Error when fetching pull requests from local branch - PR [7307]
* Allow branch creation in folder context menu (#7013) - PR [7305]
* Allow remote repositories with local disk path to be saved in recents - PR [7276]
* Filter branches: display a placeholder when no branches found - PR [7271]
* prevent flickering when selecting files in staged/unstaged FileStatusList - PR [7267]
* Commit dialog, FileStatusList: Fix up/down selects unexpected file - PR [7247]
* Reordering of Appearance Options - PR [7243]
* Settings: Regenerate the Controls when previous instance was disposed - PR [7241]
* GitIndexWatcher: check that directory exists before enabling - PR [7238]
* Fix #3280: Optimize unstage/reset performance by git reset/unstage batch files - PR [7237]
* Don't confirm switch worktree option - PR [7230]
* Exclude "fixup!" and "squash!" prefixes from commit message RegEx validation - PR [7223]
* Blame: Fix missing commit metadata on some commits - PR [7222]
* Support older Gerrit API - PR [7216]
* Catch ConEmu exception for Done() - PR [7212]
* Popup when error starting mergetool - PR [7211]
* Branch list could contain '+' - PR [7209]
* Gerrit Plugin fixes/improvements - PR [7198]
* Improve some mnemonics of commit dialog - PR [7189]
* GitStatusMonitor: Improve logic for timer 25 days wrap - PR [7184]
* exename not set for tortoisemerge - PR [7172]
* FormSettings exception if plugin has no setting - PR [7164]
* No more "..." indicator for revisions with single-line commit message - PR [7148]
* PathUtil.GetDirectoryName did not accept root dirs - PR [7109]
* Let the copy menu entry appear expandable at startup - PR [7084]
* Clarify Settings-Stash label - PR [7072]
* Fix 3934 add force push with lease to the push dialog - PR [7047]
* Jira commit hint plugin: fix some string not translated - PR [7040]


[7460]:https://github.com/gitextensions/gitextensions/pull/7460
[7449]:https://github.com/gitextensions/gitextensions/pull/7449
[7448]:https://github.com/gitextensions/gitextensions/pull/7448
[7442]:https://github.com/gitextensions/gitextensions/pull/7442
[7437]:https://github.com/gitextensions/gitextensions/pull/7437
[7429]:https://github.com/gitextensions/gitextensions/pull/7429
[7426]:https://github.com/gitextensions/gitextensions/pull/7426
[7423]:https://github.com/gitextensions/gitextensions/pull/7423
[7408]:https://github.com/gitextensions/gitextensions/pull/7408
[7401]:https://github.com/gitextensions/gitextensions/pull/7401
[7400]:https://github.com/gitextensions/gitextensions/pull/7400
[7383]:https://github.com/gitextensions/gitextensions/pull/7383
[7375]:https://github.com/gitextensions/gitextensions/pull/7375
[7373]:https://github.com/gitextensions/gitextensions/pull/7373
[7371]:https://github.com/gitextensions/gitextensions/pull/7371
[7369]:https://github.com/gitextensions/gitextensions/pull/7369
[7367]:https://github.com/gitextensions/gitextensions/pull/7367
[7365]:https://github.com/gitextensions/gitextensions/pull/7365
[7362]:https://github.com/gitextensions/gitextensions/pull/7362
[7361]:https://github.com/gitextensions/gitextensions/pull/7361
[7356]:https://github.com/gitextensions/gitextensions/pull/7356
[7340]:https://github.com/gitextensions/gitextensions/pull/7340
[7337]:https://github.com/gitextensions/gitextensions/pull/7337
[7333]:https://github.com/gitextensions/gitextensions/pull/7333
[7332]:https://github.com/gitextensions/gitextensions/pull/7332
[7330]:https://github.com/gitextensions/gitextensions/pull/7330
[7325]:https://github.com/gitextensions/gitextensions/pull/7325
[7324]:https://github.com/gitextensions/gitextensions/pull/7324
[7310]:https://github.com/gitextensions/gitextensions/pull/7310
[7309]:https://github.com/gitextensions/gitextensions/pull/7309
[7307]:https://github.com/gitextensions/gitextensions/pull/7307
[7306]:https://github.com/gitextensions/gitextensions/pull/7306
[7305]:https://github.com/gitextensions/gitextensions/pull/7305
[7302]:https://github.com/gitextensions/gitextensions/pull/7302
[7296]:https://github.com/gitextensions/gitextensions/pull/7296
[7291]:https://github.com/gitextensions/gitextensions/pull/7291
[7289]:https://github.com/gitextensions/gitextensions/pull/7289
[7288]:https://github.com/gitextensions/gitextensions/pull/7288
[7284]:https://github.com/gitextensions/gitextensions/pull/7284
[7276]:https://github.com/gitextensions/gitextensions/pull/7276
[7275]:https://github.com/gitextensions/gitextensions/pull/7275
[7271]:https://github.com/gitextensions/gitextensions/pull/7271
[7268]:https://github.com/gitextensions/gitextensions/pull/7268
[7267]:https://github.com/gitextensions/gitextensions/pull/7267
[7251]:https://github.com/gitextensions/gitextensions/pull/7251
[7247]:https://github.com/gitextensions/gitextensions/pull/7247
[7243]:https://github.com/gitextensions/gitextensions/pull/7243
[7241]:https://github.com/gitextensions/gitextensions/pull/7241
[7238]:https://github.com/gitextensions/gitextensions/pull/7238
[7237]:https://github.com/gitextensions/gitextensions/pull/7237
[7230]:https://github.com/gitextensions/gitextensions/pull/7230
[7228]:https://github.com/gitextensions/gitextensions/pull/7228
[7225]:https://github.com/gitextensions/gitextensions/pull/7225
[7224]:https://github.com/gitextensions/gitextensions/pull/7224
[7223]:https://github.com/gitextensions/gitextensions/pull/7223
[7222]:https://github.com/gitextensions/gitextensions/pull/7222
[7220]:https://github.com/gitextensions/gitextensions/pull/7220
[7216]:https://github.com/gitextensions/gitextensions/pull/7216
[7214]:https://github.com/gitextensions/gitextensions/pull/7214
[7212]:https://github.com/gitextensions/gitextensions/pull/7212
[7211]:https://github.com/gitextensions/gitextensions/pull/7211
[7209]:https://github.com/gitextensions/gitextensions/pull/7209
[7206]:https://github.com/gitextensions/gitextensions/pull/7206
[7198]:https://github.com/gitextensions/gitextensions/pull/7198
[7190]:https://github.com/gitextensions/gitextensions/pull/7190
[7189]:https://github.com/gitextensions/gitextensions/pull/7189
[7188]:https://github.com/gitextensions/gitextensions/pull/7188
[7184]:https://github.com/gitextensions/gitextensions/pull/7184
[7172]:https://github.com/gitextensions/gitextensions/pull/7172
[7164]:https://github.com/gitextensions/gitextensions/pull/7164
[7148]:https://github.com/gitextensions/gitextensions/pull/7148
[7144]:https://github.com/gitextensions/gitextensions/pull/7144
[7119]:https://github.com/gitextensions/gitextensions/pull/7119
[7112]:https://github.com/gitextensions/gitextensions/pull/7112
[7109]:https://github.com/gitextensions/gitextensions/pull/7109
[7084]:https://github.com/gitextensions/gitextensions/pull/7084
[7072]:https://github.com/gitextensions/gitextensions/pull/7072
[7047]:https://github.com/gitextensions/gitextensions/pull/7047
[7040]:https://github.com/gitextensions/gitextensions/pull/7040
[7039]:https://github.com/gitextensions/gitextensions/pull/7039
[6997]:https://github.com/gitextensions/gitextensions/pull/6997
[6910]:https://github.com/gitextensions/gitextensions/pull/6910
[6888]:https://github.com/gitextensions/gitextensions/pull/6888



### Version 3.2.1 (2 Sep 2019)

#### Features:
* Fix scrolling for committers label in statistics plugin - PR [7092]
* Display icons in commit templates menu items - PR [7037]
* Settings: Display label to the top to make UI more readable on multiline controls - PR [7036]
* Fix potential bad end of line replacement - PR [7032]
* Pull form items clipped on HiDPI scaled displays  - Issue [7020]
* TortoiseGitMerge.exe old default name - PR [7004]
* Add support for "--rebase-merges" for newest version of git - PR [6920]
* Apply stash to some files - Issue [6902]
* refactor: Move email settings from General to Detailed - PR [6881]
* FormCreateBranch: Improve UX of CheckBox "Checkout after create" - PR [6860]
* Blame: Fix "blame previous revision" feature - PR [6841]
* Can I see commits ordered by author-date instead of commit-date? - Issue [6826]
* Add revision links templates for GitHub and Azure DevOps services - PR [6785]
* artificial context fixes - PR [6770]
* Set recommended Git version to 2.22.0 - PR [6769]
* Refactor "Check for updates" dialog - Issue [6738]
* New script variables for branch and repository name - Issue [6736]
* Too much CPU and RAM usage - Issue [6732]
* ArgumentOutOfRangeException [not] selecting a language on first start - Issue [6726]
* Feature: Download / Install MSI without redirect to Browser - PR [6682]
* Dark theme fixes - PR [6651]
* Move ArtificialCommits from Settings to grid View menu - PR [6638]
* Request more information via NBug submission form - Issue [6607]
* fix: Throws on `git remote` call outside git repo - PR [6586]
* Improve stacktrace readability by using Ben.Demystifier - Issue [6569]
* fix: Invalid URI when open invalid path - PR [6560]
* RevisionGrid: Add branch icons in contextual menu - PR [6534]
* Fix rebase and apply patches displayed patches status - PR [6531]
* [Accessibility] "Default pull action" configuration - Issue [6443]
* Add settings to configure blame display - PR [6430]
* Changed GPG tab setting to show by default - PR [6331]
* Gerrit plugin using deprecated features - Issue [6127]
* Browse --> Diff Tab: Added images are not shown visualized - Issue [1391]

#### Fixes:
* [NBug] CombinedDiff artificial commit cannot be explicitly compare... - Issue [7087]
* Crash on file "save as" - Issue [7059]
* Open jira settings when not configured - PR [7033]
* Overflow error in Commit Dialog - Issue [7023]
* [NBug] Sequence contains no elements - Issue [7011]
* Crash when clicking "Suggest" button next to merge/diff tool paths, when those paths contain forward slashes - Issue [7000]
* Remote Repositories List Empty - Issue [6983]
* [NBug] Illegal characters in path. - Issue [6982]
* Crash on "Create new repository" - Issue [6955]
* Commit message  incorrect height at 150% scale factor - Issue [6898]
* Commit message highlight is one line too high for short commit titles - Issue [6895]
* File history issue in a repository with an enabled sparse checkout - Issue [6892]
* Unable to blame a file - Issue [6815]
* [NBug] Erreur lors de la lecture du répertoire I:\gitLabCloud\clara\. - Issue [6812]
* [NBug] OnActivate must be called on the UI thread. - Issue [6799]
* [NBug] Could not find a part of the path 'H:\.gitconfig19636.tmp'. - Issue [6783]
* [NBug] Object reference not set to an instance of an object. - Issue [6771]
* [NBug] Access to the path 'C:\Users\Michael\AppData\Roaming\GitExt... - Issue [6767]
* [NBug] Object reference not set to an instance of an object. - Issue [6757]
* GE freezes when the avatar column is shown - Issue [6751]
* [NBug] Specified method is not supported. - Issue [6717]
* [NBug] The system cannot find the file specified - Issue [6687]
* [NBug] An item with the same key has already been added. - Issue [6616]
* [NBug] The path is not of a legal form. - Issue [6599]
* Create branch on "working directory" creates revision 11111 instead of last known commit - Issue [6597]
* [NBug] Object reference not set to an instance of an object. - Issue [6583]
* [NBug] Value cannot be null.Parameter name: value - Issue [6549]
* FormCommit incorrect overlay (scale factor 200%) - Issue [6532]
* History not shown for a new file in a different branch - Issue [6458]
* Delete tag not working - Issue [6281]
* Commit message line endings modified by commit dialog - Issue [5908]


[7092]:https://github.com/gitextensions/gitextensions/pull/7092
[7087]:https://github.com/gitextensions/gitextensions/issues/7087
[7059]:https://github.com/gitextensions/gitextensions/issues/7059
[7037]:https://github.com/gitextensions/gitextensions/pull/7037
[7036]:https://github.com/gitextensions/gitextensions/pull/7036
[7033]:https://github.com/gitextensions/gitextensions/pull/7033
[7032]:https://github.com/gitextensions/gitextensions/pull/7032
[7023]:https://github.com/gitextensions/gitextensions/issues/7023
[7020]:https://github.com/gitextensions/gitextensions/issues/7020
[7011]:https://github.com/gitextensions/gitextensions/issues/7011
[7004]:https://github.com/gitextensions/gitextensions/pull/7004
[7000]:https://github.com/gitextensions/gitextensions/issues/7000
[6983]:https://github.com/gitextensions/gitextensions/issues/6983
[6982]:https://github.com/gitextensions/gitextensions/issues/6982
[6955]:https://github.com/gitextensions/gitextensions/issues/6955
[6920]:https://github.com/gitextensions/gitextensions/pull/6920
[6902]:https://github.com/gitextensions/gitextensions/issues/6902
[6898]:https://github.com/gitextensions/gitextensions/issues/6898
[6895]:https://github.com/gitextensions/gitextensions/issues/6895
[6892]:https://github.com/gitextensions/gitextensions/issues/6892
[6881]:https://github.com/gitextensions/gitextensions/pull/6881
[6860]:https://github.com/gitextensions/gitextensions/pull/6860
[6841]:https://github.com/gitextensions/gitextensions/pull/6841
[6826]:https://github.com/gitextensions/gitextensions/issues/6826
[6815]:https://github.com/gitextensions/gitextensions/issues/6815
[6812]:https://github.com/gitextensions/gitextensions/issues/6812
[6799]:https://github.com/gitextensions/gitextensions/issues/6799
[6785]:https://github.com/gitextensions/gitextensions/pull/6785
[6783]:https://github.com/gitextensions/gitextensions/issues/6783
[6771]:https://github.com/gitextensions/gitextensions/issues/6771
[6770]:https://github.com/gitextensions/gitextensions/pull/6770
[6769]:https://github.com/gitextensions/gitextensions/pull/6769
[6767]:https://github.com/gitextensions/gitextensions/issues/6767
[6757]:https://github.com/gitextensions/gitextensions/issues/6757
[6751]:https://github.com/gitextensions/gitextensions/issues/6751
[6738]:https://github.com/gitextensions/gitextensions/issues/6738
[6736]:https://github.com/gitextensions/gitextensions/issues/6736
[6732]:https://github.com/gitextensions/gitextensions/issues/6732
[6726]:https://github.com/gitextensions/gitextensions/issues/6726
[6717]:https://github.com/gitextensions/gitextensions/issues/6717
[6687]:https://github.com/gitextensions/gitextensions/issues/6687
[6682]:https://github.com/gitextensions/gitextensions/pull/6682
[6651]:https://github.com/gitextensions/gitextensions/pull/6651
[6638]:https://github.com/gitextensions/gitextensions/pull/6638
[6616]:https://github.com/gitextensions/gitextensions/issues/6616
[6607]:https://github.com/gitextensions/gitextensions/issues/6607
[6599]:https://github.com/gitextensions/gitextensions/issues/6599
[6597]:https://github.com/gitextensions/gitextensions/issues/6597
[6586]:https://github.com/gitextensions/gitextensions/pull/6586
[6583]:https://github.com/gitextensions/gitextensions/issues/6583
[6569]:https://github.com/gitextensions/gitextensions/issues/6569
[6560]:https://github.com/gitextensions/gitextensions/pull/6560
[6549]:https://github.com/gitextensions/gitextensions/issues/6549
[6534]:https://github.com/gitextensions/gitextensions/pull/6534
[6532]:https://github.com/gitextensions/gitextensions/issues/6532
[6531]:https://github.com/gitextensions/gitextensions/pull/6531
[6458]:https://github.com/gitextensions/gitextensions/issues/6458
[6443]:https://github.com/gitextensions/gitextensions/issues/6443
[6430]:https://github.com/gitextensions/gitextensions/pull/6430
[6331]:https://github.com/gitextensions/gitextensions/pull/6331
[6281]:https://github.com/gitextensions/gitextensions/issues/6281
[6127]:https://github.com/gitextensions/gitextensions/issues/6127
[5908]:https://github.com/gitextensions/gitextensions/issues/5908
[4909]:https://github.com/gitextensions/gitextensions/issues/4909
[1391]:https://github.com/gitextensions/gitextensions/issues/1391



### Version 3.1.1 (2 June 2019)

#### Fixes:
* Ctrl+Backspace affects ReadOnly TextBox - Issue [6667]
* incorrect {sRemotePathFromUrl} parameter evaluation - Issue [6567]
* [NBug] Remote URLs should appear in pairs. - Issue [6562]
* Commit Dialog: New submodule without text if unstaged - Issue [6559]
* Pull dialog stopped "remembering" previous user selection - Issue [6503]
* Commit message not stored after reverting a commit - Issue [6244]
* System.Exception opening an existing repository - Issue [6093]


[6667]:https://github.com/gitextensions/gitextensions/issues/6667
[6567]:https://github.com/gitextensions/gitextensions/issues/6567
[6562]:https://github.com/gitextensions/gitextensions/issues/6562
[6559]:https://github.com/gitextensions/gitextensions/issues/6559
[6503]:https://github.com/gitextensions/gitextensions/issues/6503
[6244]:https://github.com/gitextensions/gitextensions/issues/6244
[6093]:https://github.com/gitextensions/gitextensions/issues/6093


### Version 3.1.0 (13 May 2019)

#### Features:
* Include source branch in link to BitBucket create pull request - PR [6511]
* Generate avatars with author initials as fallback of gravatar - PR [6499]
* Hotkeys for Open with difftool | First/Selected -> Working Directory - Issue [6481]
* FileStatusList: highlight the new name - Issue [6465]
* fix: JiraCommitHintPlugin hidpi layout issues - PR [6464]
* FileStatusList: indicate active filter - Issue [6449]
* FileStatusList filter: mention Regular Expression - Issue [6447]
* Update NBug - PR [6446]
* Support for VS2019 - Issue [6431]
* Text entering over the top of commits tree - Issue [6419]
* Improve blame contextual menu - PR [6400]
* Fix blame control click hold move mouse to the right - PR [6399]
* Left pane reorder trees - PR [6349]
* Being able to navigate to a PR page of an AppVeyor build - PR [6326]
* Author images should support Robohash image service - Issue [6311]
* Automatically load PuTTY SSH key files when needed in left panel - PR [6301]
* Prune remote branches from Remote repositories dialog not working as expected - Issue [6284]
* Display Issue (black history graph area) - Issue [6268]
* Manually handle Ctrl+Backspace on TextBox and ComboBox controls - PR [6256]
* FormCleanupRepository: Add a button to easily add a path to clean up - PR [6250]
* Drag and drop windows folder onto Extensions used to open that repo - Issue [6221]
* Toggle autocomplete commit textbox - PR [6187]
* Fix sorting of commit templates. - PR [6169]
* "There are no staged changes" is not hidden completely - Issue [6149]
* CI integration in revision grid adjustments - PR [6116]
* Commit dialog: Copy selected text from diff directly to commit message - Issue [6115]
* Copy links to clipboard - Issue [6088]
* Ability to disable 'simplify-merges' option - Issue [6066]
* Introduce option to disable build result tab - Issue [6063]
* Left panel - show inactive remotes - Issue [6015]
* Display ahead and behind information in the push button - PR [5994]
* Remove unused setting "Striped branch change" - PR [5991]
* Remove VSIX from installer - Issue [5979]
* Console tab doesn't require translation strings - PR [5951]
* AppVeyor settings enhancements - PR [5941]
* Browse: Sort the Plugin list in alphabetic order - PR [5926]
* Help text for Edit .gitignore not wrapped - PR [5925]
* Git lfs commands deprecated - Issue [5905]
* Plugin GitStatistics: Added calculation of comment lines for assembler files - PR [5846]
* Implement F3 to open difftool for .png - PR [5838]
* Ability to prune remote directly from object tree - Issue [5805]
* Revision Links: Detect GH issues without # - PR [5799]
* Update ManagedExtensibility to vs-mef - PR [5788]
* Revision grid context menu improvements - Issue [5786]
* only update submodule status at changes - PR [5777]
* Issue 3320 - Plugin registration phase - PR [5776]
* Do not open autocomplete when moving in the commit message editor - Issue [5770]
* Improve Submodule Status performance - Issue [5769]
* Browse: Delay status updates when opening/refreshing repos - PR [5747]
* GE documentation update for 3.0 - Issue [5693]
* Pub/sub for the left panel - PR [5669]
* #5125 RevisionGrid Graph: Nearest branch in tooltip - PR [5643]
* Add Submodules to the left pane - Issue [5569]
* Github fork/clone dialog change protocol of remote - Issue [5418]
* Improve overlay of Windows task bar icon - Issue [5336]
* Create an issue from NBug report - Issue [5064]
* Need -depth option on clone github dialog - Issue [3517]

#### Fixes:
* [NBug] Unexpected artificial commit in Git command: 11111111111111... - Issue [6535]
* Format patch: fix patch creation when selection order is invalid (master)  - PR [6526]
* Unhandled RefsWarningException "warning: ignoring broken ref" - Issue [6516]
* Exception if opening a submodule with incorrect superproject reference - Issue [6512]
* Checkout branch: Badly formatted message - Issue [6507]
* Unable to build from cli - Issue [6497]
* ArgumentException on startup - Issue [6442]
* git mergetool should be considered correctly configured even there's no path. - Issue [6438]
* Hex View on Blame Tab Always Shows Working Copy instead of Selected Revision - Issue [6434]
* AppVeyor plugin: Prevent crash when trying to add the same project again - PR [6426]
* Vertical ruler position not correctly read from settings - Issue [6391]
* Date formats used are inconsistent and confusing - Issue [6389]
* FormAbout: fix "Git Extensions is open source. Get involved!" text wa… - PR [6383]
* ExListView not redrawn on expand / collapse - Issue [6375]
* Alt+C in main window has a conflict between menu and commit button - Issue [6368]
* UI freezes when updating submodule status - Issue [6357]
* Unable to save changes for a remote - Issue [6347]
* View History window says "This repository does not yet contain any commits" - Issue [6343]
* "Filter files..." hint overlays the filter - Issue [6336]
* Sorted refs needs to be refreshed after the repository has changed. - PR [6335]
* [Blocker] Thread exhaustion - Issue [6319]
* Copied files in a commit cannot be deleted by 'Reset to parent' - Issue [6297]
* Incorrect context menu items in commit diff tab - Issue [6296]
* Application crashes when selecting "Go to commit" and having big contents in clipboard - Issue [6288]
* NRE on script hotkey unless the revision grid has the focus - Issue [6285]
* Exception on creating worktree - Issue [6277]
* Clear diff in history form when clicked on empty area, just like main form and commit form do - Issue [6271]
* NullReferenceException in commit dialog - Issue [6246]
* Black screen once switching to one of the submodules - Issue [6243]
* Frozen once staging 1000 files with CRLF line break and core.autocrlf = input - Issue [6225]
* Slow startup if never connected to internet - Issue [6223]
* Orphan commit badly displayed in Revision graph - Issue [6210]
* Exception from revision grid - Issue [6193]
* PathTooLongExceptions for file paths shorter than 260 characters in commit window when triggered from VS extension - Issue [6170]
* Jira Commit Hint plugin doesn't work - Issue [6154]
* inverted diffs after refresh - Issue [6147]
* Revision grid: bad behaviour when resizing of commit message column - Issue [6117]
* System.Exception opening an existing repository - Issue [6093]
* View menu icons fail to update - Issue [6086]
* Corrupted Commit Log View after scrolling - Issue [6082]
* Reset selected lines is broken for staged files - Issue [6076]
* Debounce navigating the list of branches - Issue [6072]
* Delete All no longer works - "not selectable" - Issue [6068]
* Fetching origin from the remote branches on first load asks for the ssh key - Issue [6047]
* Copy Commit Info distorts the Commit Info display - Issue [6045]
* Fix ROT selected node ensuring visible without collapsing other nodes… - PR [6013]
* No good way to pass arguments which contain whitespace or special characters to scripts - Issue [5999]
* Fix recent repo dropdown favourites ordering - PR [5995]
* Size dialog "Clone" - Issue [5972]
* Commit button always visible - Issue [5968]
* Option to show tags not stored on restart - Issue [5962]
* Esc key to close the Stash Manager dialog - Issue [5958]
* Esc key to close the File History / Blame dialog from everywhere - Issue [5957]
* Two console tabs if UI is not english - Issue [5916]
* [3.0] Commit status list is sorted by change type - Issue [5909]
* 3.0 Not need branch window which freeze the best super fast application - Issue [5884]
* Commit message is editable in the main UI - Issue [5870]
* Clipping of the search text box in the branches panel - Issue [5866]
* Change working directory dropdown hangs for multiple seconds - Issue [5829]
* Author's avatar settings are not working correctly in Version 3.00.00 RC2 - Issue [5828]
* Exception showing combined diff on merge commit - Issue [5815]
* ArgumentNullException when comparing without checked out branch - Issue [5812]
* Showing "Notes" in FormFileHistory and FormCommitDiff commit info when there are none - Issue [5804]
* Bug: Console tab setting requires restart - Issue [5787]
* Revisions panel empties after double click in blame view - Issue [5678]
* Incorrect layout of the 'Specify branch' dialog - Issue [5676]
* Git Notes don't show in beta 3.00.00.01 - Issue [5653]
* Git extensions crash when using the mouse wheel  - Issue [5621]
* Avatars makes app unresponsive on slow internet connection - Issue [5309]
* "Remote repositories -- Default pull behaviour" tab broken under high dpi - Issue [5028]
* Push dialog remotes not updated - Issue [4853]
* Jira Commit Hint plugin credentials stored in plain text - Issue [4447]
* Plugin loading phase - Issue [3320]


[6535]:https://github.com/gitextensions/gitextensions/issues/6535
[6526]:https://github.com/gitextensions/gitextensions/pull/6526
[6516]:https://github.com/gitextensions/gitextensions/issues/6516
[6512]:https://github.com/gitextensions/gitextensions/issues/6512
[6511]:https://github.com/gitextensions/gitextensions/pull/6511
[6507]:https://github.com/gitextensions/gitextensions/issues/6507
[6499]:https://github.com/gitextensions/gitextensions/pull/6499
[6497]:https://github.com/gitextensions/gitextensions/issues/6497
[6481]:https://github.com/gitextensions/gitextensions/issues/6481
[6465]:https://github.com/gitextensions/gitextensions/issues/6465
[6464]:https://github.com/gitextensions/gitextensions/pull/6464
[6449]:https://github.com/gitextensions/gitextensions/issues/6449
[6447]:https://github.com/gitextensions/gitextensions/issues/6447
[6446]:https://github.com/gitextensions/gitextensions/pull/6446
[6442]:https://github.com/gitextensions/gitextensions/issues/6442
[6438]:https://github.com/gitextensions/gitextensions/issues/6438
[6434]:https://github.com/gitextensions/gitextensions/issues/6434
[6431]:https://github.com/gitextensions/gitextensions/issues/6431
[6426]:https://github.com/gitextensions/gitextensions/pull/6426
[6419]:https://github.com/gitextensions/gitextensions/issues/6419
[6400]:https://github.com/gitextensions/gitextensions/pull/6400
[6399]:https://github.com/gitextensions/gitextensions/pull/6399
[6391]:https://github.com/gitextensions/gitextensions/issues/6391
[6389]:https://github.com/gitextensions/gitextensions/issues/6389
[6383]:https://github.com/gitextensions/gitextensions/pull/6383
[6375]:https://github.com/gitextensions/gitextensions/issues/6375
[6368]:https://github.com/gitextensions/gitextensions/issues/6368
[6357]:https://github.com/gitextensions/gitextensions/issues/6357
[6349]:https://github.com/gitextensions/gitextensions/pull/6349
[6347]:https://github.com/gitextensions/gitextensions/issues/6347
[6343]:https://github.com/gitextensions/gitextensions/issues/6343
[6336]:https://github.com/gitextensions/gitextensions/issues/6336
[6335]:https://github.com/gitextensions/gitextensions/pull/6335
[6326]:https://github.com/gitextensions/gitextensions/pull/6326
[6319]:https://github.com/gitextensions/gitextensions/issues/6319
[6311]:https://github.com/gitextensions/gitextensions/issues/6311
[6301]:https://github.com/gitextensions/gitextensions/pull/6301
[6297]:https://github.com/gitextensions/gitextensions/issues/6297
[6296]:https://github.com/gitextensions/gitextensions/issues/6296
[6288]:https://github.com/gitextensions/gitextensions/issues/6288
[6285]:https://github.com/gitextensions/gitextensions/issues/6285
[6284]:https://github.com/gitextensions/gitextensions/issues/6284
[6277]:https://github.com/gitextensions/gitextensions/issues/6277
[6271]:https://github.com/gitextensions/gitextensions/issues/6271
[6268]:https://github.com/gitextensions/gitextensions/issues/6268
[6256]:https://github.com/gitextensions/gitextensions/pull/6256
[6250]:https://github.com/gitextensions/gitextensions/pull/6250
[6246]:https://github.com/gitextensions/gitextensions/issues/6246
[6243]:https://github.com/gitextensions/gitextensions/issues/6243
[6225]:https://github.com/gitextensions/gitextensions/issues/6225
[6223]:https://github.com/gitextensions/gitextensions/issues/6223
[6221]:https://github.com/gitextensions/gitextensions/issues/6221
[6210]:https://github.com/gitextensions/gitextensions/issues/6210
[6193]:https://github.com/gitextensions/gitextensions/issues/6193
[6187]:https://github.com/gitextensions/gitextensions/pull/6187
[6170]:https://github.com/gitextensions/gitextensions/issues/6170
[6169]:https://github.com/gitextensions/gitextensions/pull/6169
[6154]:https://github.com/gitextensions/gitextensions/issues/6154
[6149]:https://github.com/gitextensions/gitextensions/issues/6149
[6147]:https://github.com/gitextensions/gitextensions/issues/6147
[6117]:https://github.com/gitextensions/gitextensions/issues/6117
[6116]:https://github.com/gitextensions/gitextensions/pull/6116
[6115]:https://github.com/gitextensions/gitextensions/issues/6115
[6093]:https://github.com/gitextensions/gitextensions/issues/6093
[6088]:https://github.com/gitextensions/gitextensions/issues/6088
[6086]:https://github.com/gitextensions/gitextensions/issues/6086
[6082]:https://github.com/gitextensions/gitextensions/issues/6082
[6076]:https://github.com/gitextensions/gitextensions/issues/6076
[6072]:https://github.com/gitextensions/gitextensions/issues/6072
[6068]:https://github.com/gitextensions/gitextensions/issues/6068
[6066]:https://github.com/gitextensions/gitextensions/issues/6066
[6063]:https://github.com/gitextensions/gitextensions/issues/6063
[6047]:https://github.com/gitextensions/gitextensions/issues/6047
[6045]:https://github.com/gitextensions/gitextensions/issues/6045
[6015]:https://github.com/gitextensions/gitextensions/issues/6015
[6013]:https://github.com/gitextensions/gitextensions/pull/6013
[5999]:https://github.com/gitextensions/gitextensions/issues/5999
[5995]:https://github.com/gitextensions/gitextensions/pull/5995
[5994]:https://github.com/gitextensions/gitextensions/pull/5994
[5991]:https://github.com/gitextensions/gitextensions/pull/5991
[5979]:https://github.com/gitextensions/gitextensions/issues/5979
[5972]:https://github.com/gitextensions/gitextensions/issues/5972
[5968]:https://github.com/gitextensions/gitextensions/issues/5968
[5962]:https://github.com/gitextensions/gitextensions/issues/5962
[5958]:https://github.com/gitextensions/gitextensions/issues/5958
[5957]:https://github.com/gitextensions/gitextensions/issues/5957
[5951]:https://github.com/gitextensions/gitextensions/pull/5951
[5941]:https://github.com/gitextensions/gitextensions/pull/5941
[5926]:https://github.com/gitextensions/gitextensions/pull/5926
[5925]:https://github.com/gitextensions/gitextensions/pull/5925
[5916]:https://github.com/gitextensions/gitextensions/issues/5916
[5909]:https://github.com/gitextensions/gitextensions/issues/5909
[5905]:https://github.com/gitextensions/gitextensions/issues/5905
[5884]:https://github.com/gitextensions/gitextensions/issues/5884
[5870]:https://github.com/gitextensions/gitextensions/issues/5870
[5866]:https://github.com/gitextensions/gitextensions/issues/5866
[5846]:https://github.com/gitextensions/gitextensions/pull/5846
[5838]:https://github.com/gitextensions/gitextensions/pull/5838
[5829]:https://github.com/gitextensions/gitextensions/issues/5829
[5828]:https://github.com/gitextensions/gitextensions/issues/5828
[5815]:https://github.com/gitextensions/gitextensions/issues/5815
[5812]:https://github.com/gitextensions/gitextensions/issues/5812
[5805]:https://github.com/gitextensions/gitextensions/issues/5805
[5804]:https://github.com/gitextensions/gitextensions/issues/5804
[5799]:https://github.com/gitextensions/gitextensions/pull/5799
[5788]:https://github.com/gitextensions/gitextensions/pull/5788
[5787]:https://github.com/gitextensions/gitextensions/issues/5787
[5786]:https://github.com/gitextensions/gitextensions/issues/5786
[5777]:https://github.com/gitextensions/gitextensions/pull/5777
[5776]:https://github.com/gitextensions/gitextensions/pull/5776
[5770]:https://github.com/gitextensions/gitextensions/issues/5770
[5769]:https://github.com/gitextensions/gitextensions/issues/5769
[5747]:https://github.com/gitextensions/gitextensions/pull/5747
[5693]:https://github.com/gitextensions/gitextensions/issues/5693
[5678]:https://github.com/gitextensions/gitextensions/issues/5678
[5676]:https://github.com/gitextensions/gitextensions/issues/5676
[5669]:https://github.com/gitextensions/gitextensions/pull/5669
[5653]:https://github.com/gitextensions/gitextensions/issues/5653
[5643]:https://github.com/gitextensions/gitextensions/pull/5643
[5621]:https://github.com/gitextensions/gitextensions/issues/5621
[5569]:https://github.com/gitextensions/gitextensions/issues/5569
[5418]:https://github.com/gitextensions/gitextensions/issues/5418
[5336]:https://github.com/gitextensions/gitextensions/issues/5336
[5309]:https://github.com/gitextensions/gitextensions/issues/5309
[5064]:https://github.com/gitextensions/gitextensions/issues/5064
[5028]:https://github.com/gitextensions/gitextensions/issues/5028
[4853]:https://github.com/gitextensions/gitextensions/issues/4853
[4447]:https://github.com/gitextensions/gitextensions/issues/4447
[3517]:https://github.com/gitextensions/gitextensions/issues/3517
[3320]:https://github.com/gitextensions/gitextensions/issues/3320


### Version 3.0.2 (16 Feb 2019)

#### Fixes:
* QuickPull's hotkey is not working - Issue [6200]
* Can't delete a repository included in the categories of the dashboard - Issue [6192]
* Fix loading of some plugins that failed - PR [6159]
* Regression: Pull Dialog Title when changing merge option - Issue [6150]
* Shell Extension Menu Pull  - Issue [6144]
* gitexe.cmd pull - always opens dialog window in do not merge, only fetch changes - Issue [6060]


[6200]:https://github.com/gitextensions/gitextensions/issues/6200
[6192]:https://github.com/gitextensions/gitextensions/issues/6192
[6159]:https://github.com/gitextensions/gitextensions/pull/6159
[6150]:https://github.com/gitextensions/gitextensions/issues/6150
[6144]:https://github.com/gitextensions/gitextensions/issues/6144
[6060]:https://github.com/gitextensions/gitextensions/issues/6060


### Version 3.0.1 (20 Jan 2019)

#### Fixes:
* Application crashes on repository change - Issue [6094]
* Broken issue templates - Issue [6075]
* Commit Template Settings: Commit Template field one line only - Issue [6029]
* Exception if no current checkout when selecting current revision - PR [6023]
* [Bug]  Custom stash names not working - Issue [6016]
* Update nuget.exe 4.7.1 -> 4.9.2 - PR [6006]
* Empty/exception when resetting to a remote branch, complaining about the Commit Date < min UTC DateTime - Issue [5984]
* Refresh revision grid when superproject branch/tag is completed - PR [5981]
* BitBucket Server plugin has garbage links - Issue [5976]
* New gitignore editor doesn't allow multiple lines - Issue [5975]
* Replace ConcurrentBag in RevisionGraph - PR [5974]
* Crash when clicking on "Help translate" in appearance settings - Issue [5960]
* Crash in patches view - Issue [5959]
* Can no longer reset submodule changes in the super project's commit window. - Issue [5937]
* Commit window left side splitter position not remembered - Issue [5935]
* Script: cDefaultRemotePathFromUrl returns bad information - Issue [5932]
* Reenable vertical ruler - PR [5930]
* Diffs are not displayed properly and cannot be scrolled - Issue [5917]
* Left Panel branches tree view with horizontal scroll defaults to scrolling to center instead of staying on the left - Issue [5907]
* Autolinks from commits hashes broken in version 3.00.00 - Issue [5904]
* QuickPull should behave the same as the default toolbar does. - PR [5897]
* GitFlow plugins Finish button doesn't work fine - Issue [5893]
* Double click in empty TextBox raises ArgumentOutOfRangeException - Issue [5890]
* Revision graph starts from the wrong point - Issue [5880]
* When default Pull button action is set to "Fetch and prune all", the "Open pull dialog..." entry does a fetch and prune all - Issue [5879]
* System.FormatException in diff of submodule add / update - Issue [5873]
* VS2019 initial support - PR [5864]
* UI freeze in 3.0 RC2 while retrieving avatars - Issue [5859]
* "Reset unstaged files" with "deleted added files" should not delete ignored files by default - Issue [5849]
* UI performance on Version 3.00.00 RC2 is very slow - Issue [5836]
* Improve time complexity of IndexOf. - PR [5800]
* Restore revision grid highlighting of commits by author - Issue [5197]
* Plugin distribution - Issue [4885]
* GitExtensions crashes on startup - "There is an error in the XML document" - Issue [3929]


[6094]:https://github.com/gitextensions/gitextensions/issues/6094
[6075]:https://github.com/gitextensions/gitextensions/issues/6075
[6029]:https://github.com/gitextensions/gitextensions/issues/6029
[6023]:https://github.com/gitextensions/gitextensions/pull/6023
[6016]:https://github.com/gitextensions/gitextensions/issues/6016
[6006]:https://github.com/gitextensions/gitextensions/pull/6006
[5984]:https://github.com/gitextensions/gitextensions/issues/5984
[5981]:https://github.com/gitextensions/gitextensions/pull/5981
[5976]:https://github.com/gitextensions/gitextensions/issues/5976
[5975]:https://github.com/gitextensions/gitextensions/issues/5975
[5974]:https://github.com/gitextensions/gitextensions/pull/5974
[5960]:https://github.com/gitextensions/gitextensions/issues/5960
[5959]:https://github.com/gitextensions/gitextensions/issues/5959
[5937]:https://github.com/gitextensions/gitextensions/issues/5937
[5935]:https://github.com/gitextensions/gitextensions/issues/5935
[5932]:https://github.com/gitextensions/gitextensions/issues/5932
[5930]:https://github.com/gitextensions/gitextensions/pull/5930
[5917]:https://github.com/gitextensions/gitextensions/issues/5917
[5907]:https://github.com/gitextensions/gitextensions/issues/5907
[5904]:https://github.com/gitextensions/gitextensions/issues/5904
[5897]:https://github.com/gitextensions/gitextensions/pull/5897
[5893]:https://github.com/gitextensions/gitextensions/issues/5893
[5890]:https://github.com/gitextensions/gitextensions/issues/5890
[5880]:https://github.com/gitextensions/gitextensions/issues/5880
[5879]:https://github.com/gitextensions/gitextensions/issues/5879
[5873]:https://github.com/gitextensions/gitextensions/issues/5873
[5864]:https://github.com/gitextensions/gitextensions/pull/5864
[5859]:https://github.com/gitextensions/gitextensions/issues/5859
[5849]:https://github.com/gitextensions/gitextensions/issues/5849
[5836]:https://github.com/gitextensions/gitextensions/issues/5836
[5800]:https://github.com/gitextensions/gitextensions/pull/5800
[5197]:https://github.com/gitextensions/gitextensions/issues/5197
[4885]:https://github.com/gitextensions/gitextensions/issues/4885
[3929]:https://github.com/gitextensions/gitextensions/issues/3929


### [Version 3.00.00] (09 Dec 2018)

#### Features:
* Fixed #5811: strange resize of the description column when selecting lines with mouse click - PR [5821]
* Commit-graph:  Add option to always display first parent node in left-most column - Issue [5819]
* Corrected git log command. - PR [5816]
* Limits RevisionsSplitContainer.SplitterDistance to a min of 0 - PR [5808]
* Fix clone dialog doesn't pick up destination - PR [5803]
* Issue 4885 - Integration of PluginManager - PR [5775]
* Help / Translate just shows a login screen - Issue [5766]
* Portable build should set IsPortable in config - Issue [5758]
* Refresh reset current branch layout - PR [5748]
* Differentiate between identical rename and modified + renamed - Issue [5744]
* Show correct tooltips for graph lanes - PR [5740]
* CommandLog include workdir - PR [5739]
* Broken AutoPull on rejected push - Issue [5734]
* About->Copy should include DPI scaling - Issue [5732]
* Fixes 5353: scrolling issue caused by refreshing the objecttree - PR [5727]
* EditNetSpell: Double click marks the whole word (including underscores) - PR [5725]
* Fixed null reference when hovering over lane in graph - PR [5724]
* Retrieve worktrees for the current module. - PR [5720]
* Update recommended Git version to 2.19.1 - PR [5696]
* Fix RemoteObjectTree branch folder nodes sometimes becoming bold - PR [5686]
* File names in filestatuslist are rendered too narrow when full screen - Issue [5677]
* Fix incorrect v-position for parent centered windows - PR [5671]
* Performance issue when selecting large diff - Issue [5642]
* Add default action for "Rename branch" context menu item when only one branch selected - Issue [5630]
* Allow navigation to arbitrary commit through a script - Issue [5615]
* Update commit info panel layout - PR [5611]
* Txt change: "Show in folder" - PR [5597]
* When ordering is unspecified use "topo-order" to prevent parents showing before all of their children are shown - PR [5596]
* Setting order revisions by date has no effect - Issue [5594]
* Fix SubmoduleInfoResult having no CurrentSubmoduleName for submodules… - PR [5581]
* Browse: Commands menu for artificial commits - Issue [5557]
* Improved graph rendering performance by reducing locks in settingscache - PR [5540]
* Optimize performance of selecting current revision - PR [5534]
* Delete unused branches one at the time. - PR [5533]
* Feature request: Option to disable automatic search for updates - Issue [5501]
* Performance fixes for large repos (responsive grid during loading) - PR [5473]
* Use diff icon for revision grid context menu item "Compare" - Issue [5432]
* Expand / collapse button of ExListView is not working - Issue [5427]
* GoToSuperproject and GoToSubmodule hotkeys - PR [5425]
* Fetch menu item performs the default pull action - Issue [5420]
* About dialog Add system info  - Issue [5419]
* Leave only two rebase options in RevisionGridControl - PR [5413]
* GUI hangs after loading repo - Issue [5408]
* Submodule dropdown do not contain status - Issue [5386]
* VSIX icons are not updated for 3.0 - Issue [5368]
* Copy multiple commits to clipboard - Issue [5359]
* Reduce number of graph refreshes - PR [5349]
* Hotkey F4 to open file in editor - Issue [5348]
* Browse Window: Ctrl+1, Ctrl+2, ... hotkeys - Issue [5347]
* Add setting that the commit message column fills the remaining width - Issue [5337]
* Diff list: Collapse indicator of headings not clickable and can be invisible - Issue [5332]
* Find a better label for "Fetch all" - Issue [5330]
* Commit dialog: "Go to line" does not work sometimes - Issue [5328]
* Commit dialog: retain the view position when updating the diff - Issue [5327]
* Branch Tree Panel initially empty - Issue [5326]
* Option to show commit info to the left of graph - Issue [5322]
* gitk would not start if Git path was set to cmd/git.exe - PR [5312]
* Add two patterns to C# AutoCompleteRegexes - PR [5285]
* Syntax highlighting for JSM - PR [5250]
* Cleanup: WorkTree/Index should be used rather than Unstaged/Staged - Issue [5230]
* Hide useless buttons for empty bare repo - PR [5229]
* Include process ID in command log - PR [5218]
* Add remote tree icon for VSTS remotes - PR [5214]
* GitHub capitalisation - Issue [5207]
* Revisit logo in dashboard - Issue [5198]
* Feature request: Support TypeScript in Statistics plugin - Issue [5190]
* Ctrl+Backspace doesn't work in `CreateBranch` dialog - Issue [5175]
* Add "Push After Finish" checkbox in GitFlow Panel - Issue [5157]
* Proposal: Delete "Commits per user" feature - Issue [5148]
* Add ability to remove all missing repos from dashboard - Issue [5143]
* Add "Stop tracking file" feature to the Commit Form - PR [5137]
* Inconsistent "GitExt Browse" icon - Issue [5117]
* Increase minimum Git version to 2.11 - Issue [5106]
* Fixing mergetool (setting) status message - PR [5102]
* UI tweaks for v3 - PR [5087]
* Show branches before tags in the revision grid.  - PR [5077]
* Displaying only the abbreviated sha1 in the revision grid - Issue [5032]
* Some default settings should be changed - Issue [5030]
* Remove support for RevisionGrid layouts - Issue [5026]
* FileStatusList filter watermark partially covering border at high DPI - Issue [5023]
* Improve Vsts tfs rest plugin - PR [4998]
* Recheck filter against displayed file name when working in the fileNameOnlyMode mode. - Issue [4991]
* DVCS Graph tweaks - PR [4986]
* Change artificial refs color to "Color other label" setting - PR [4964]
* Settings treeview rendering in High DPI - Issue [4949]
* Context menu for listview groups in Dashboard - Issue [4948]
* Dashboard management - Issue [4947]
* Merge GitPluginShared into GitExtensionsVSIX - PR [4936]
* Remove Git and KDiff3 from installer - PR [4930]
* Check for recommended Git version - PR [4929]
* Fix filter commit label - PR [4926]
* Migrate categorised local repositories - PR [4899]
* Some other improvements/fixes in formverify (Recover lost objects form) - PR [4898]
* Branch graph lines can have invisible colors - Issue [4892]
* Improve FormVerify (Recover lost objects form) - PR [4882]
* [Discussion] Display categorised repository history - Issue [4878]
* Convert to AsyncPackage with background load - Issue [4875]
* Fix case when GitCommonDirectory equals "." - PR [4871]
* Bug in push multiple refs - PR [4850]
* Take WindowsAPICodePack from NuGet - Issue [4820]
* Show FormDiff modeless. - PR [4807]
* Indicate that the "Compare to branch" command needs additional information - PR [4805]
* Delete TAGMESSAGE after tag command completes - PR [4792]
* VSIX plugin: No icon for blame - PR [4790]
* fix: FormFixHome is messed up HiDPI - PR [4780]
* fix: FormRemotes is messed up on HiDPI - PR [4779]
* Icons in buttons are too small under high DPI  - Issue [4778]
* Tab control header is too small under high DPI - Issue [4777]
* Refactor repositories persistence - PR [4766]
* GE skips dashboard if started from a Git repo - Issue [4739]
* Prefer JoinableTaskFactory to other Invoke styles - PR [4727]
* Use MEF to load all plugins - PR [4719]
* fix: FormRemotes is messed up on HiDPI - PR [4714]
* Fix avatar under high DPI - PR [4711]
* Review GitModule.GetSelectedBranchFast - PR [4707]
* Make GitStash immutable and use regex parsing - PR [4706]
* Get remotes - PR [4705]
* Patches 3/3 - PR [4697]
* Environment HOME path dialog is broken under windows - Issue [4690]
*  Blame view/parsing review  - PR [4688]
* Patches 1/3 - PR [4687]
* Multiple enumeration - PR [4682]
* Replace tail call recursion with loop - PR [4681]
* Equality operators - PR [4680]
* Delete unused code - PR [4678]
* Use C# 7 deconstruction - PR [4677]
* Ternary conditionals - PR [4672]
* Introduce WaitCursorScope - PR [4670]
* Fixes for setting of environment variables - PR [4668]
* Questions about GitCommandHelpers.SetEnvironmentVariable - Issue [4664]
* Introduce ArgumentBuilder - PR [4662]
* Replace recursion with iteration - PR [4657]
* Remove GitModule.StreamCopy - PR [4656]
* Use Regex in GitItemParser - PR [4655]
* Use Enumerable.Empty and Array.Empty - PR [4654]
* Defer JIT compile of bulk of Program.Main - PR [4645]
* Commit data - PR [4641]
* String extensions - PR [4640]
* Submodule method review - PR [4639]
* Add Focus filter command to FormBrowse - PR [4638]
* #4554 added a new Commit tab but did not adjust the tab icons - PR [4635]
* Fix invalid usages of ArgumentNullException - PR [4629]
* Add the current process id to the tmpFile name. - PR [4625]
* Remove SVN support - Issue [4592]
* Add button for opening the Commit dialog from the Rebase dialog - Issue [4588]
* TaskCanceledException for ShowSelectedFileDiff() - Issue [4570]
* BrowseDiff should show differences to all selected revisions - Issue [4564]
* FileStatusList handling of parents and selected revision - Issue [4561]
* Commit info in history form - PR [4554]
* Update Git for Windows version to account for recent GitHub breakage - Issue [4523]
* Remove Git and Kdiff3 from installer package - Issue [4515]
* Application font should default to Segoe UI for Windows Vista and above with themes enabled - Issue [4511]
* F3 should open the external diff viewer instead of showing "No string specified to look for!" - Issue [4500]
* Use native ListView and TreeView styles  - Issue [4490]
* Refactor external link definition - PR [4480]
* Replace some lambdas with local functions - PR [4475]
* Find file dialog doesn't work with backslashes and full file path - Issue [4472]
* GitRemoteManager calls GetRefs multiple times - Issue [4453]
* Add a hotkey for toggling viewing of Tags - Issue [4449]
* Update RestSharp dependency to latest  - Issue [4428]
* Update ConEmu dependency to latest - Issue [4427]
* Update references latest available versions - Issue [4426]
* ExternalLinksParser rework - PR [4425]
* Improve jira plugin again - PR [4416]
* Fixes on jira commit hint plugin - PR [4412]
* Installation scripts should allow for IDE build, also in Debug - PR [4411]
* Dialog for selecting branches to view - Issue [4381]
* Add two ScriptEvents, BeforeMerge, AfterMerge - Issue [4380]
* Refactor: Use IFullPathResolver to resolve paths - PR [4359]
* Better display of "Find and replace" dialog on multiple screens computer - PR [4354]
* UI Improvement - Issue [4348]
* Add feature to be able to do a partial stash - PR [4334]
* Add "undo last commit" feature - PR [4333]
* Limit how often GitStatus polls for changes - PR [4327]
* Add form to configure some useful git settings... - PR [4324]
* Git update to 2.16.1 - Issue [4312]
* Visual Studio 2017 as default - Issue [4311]
* Refactoring to use same function to get the short sha - PR [4310]
* Added menu to perform immediate rebase and interactively rebase without the rebase dialog - PR [4309]
* [Discussion] GitExtensions v3.0 proposal - Issue [4308]
* View summary of changes in revision grid - Issue [4281]
* Support build integration support for variables  - PR [4280]
* memory leak & high CPU usage on windows 10 - Issue [4256]
* Activate artificial commits by default #4033 - PR [4247]
* [new feature] Sign commit with GPG - Issue [4238]
* Keep pull/fetch default for all repositories, not just one - Issue [4159]
* Better text highlighter support - Issue [4114]
* GitExtensions UI Update / v3 branch? - Issue [4037]
* Menu to rebase interactively - Issue [3853]
* TFS 2017 Build Server Integration Support - Issue [3779]
* Feature request: use GH API for avatar retrieval when GH private email address is used - Issue [3770]
* feat: Dashboard revisited - PR [3693]
* [Feature Request] Build Server Integration - Visual Studio Online - Issue [3639]
* Add Select All button to the Delete obsolete branches form - Issue [3522]
* File diff window always shows the top most file in a multiple selection - Issue [3510]
* Commit "Author" text box has incorrect descriptive text - Issue [3501]
* There is no way to get to favorite repositories, other than start page. - Issue [3220]
* WinMerge is not supported as a mergetool (only as difftool) - Issue [3163]
* Remember Split View Layout? - Issue [3072]
* Left panel - branch  tree view - PR [3038]
* Edit .gitignore button has display glitch - Issue [2938]
* Add Atom Editor to Git Config section - Issue [2507]
* Keep last used Destination in clone dialog as entered - Issue [2313]
* Add 5MB limit to previews in diff pane - Issue [2272]
* Add a {UserFile} token for scripts - Issue [2000]
* Move stash button more to the right? - Issue [1931]
* (Option to) show committer in the revision grid - Issue [1926]
* Option to disable splash-screen - Issue [1871]
* Favorite Branches - Issue [1511]
* Disable Commit and File Tree Tab if more than one revision selected - Issue [1379]
* Feature request: branch tree view - Issue [538]
* git describe on commit tab - Issue [84]

#### Fixes:
* stack overflow with certain repo - Issue [5853]
* Add try catch around method to guess system encoding. - PR [5820]
* Can't connect to build server of TFS 2017 on premise - Issue [5773]
* Crash when pressing F5 while hovering item in dashboard. - Issue [5764]
* Browse: Inconsistent behavior for Refresh button and F5/Repository-menu - PR [5738]
* Copy SHA via Ctrl+C broken in 3.0 RC1 - Issue [5735]
* Performance problems in repo with lots of submodules - Issue [5733]
* AboutForm shouldn't dance about - Issue [5731]
* CommitInfo panel: Copy commit info doesn't copy the body anymore - Issue [5707]
* System.ArgumentException when loading graph - Issue [5703]
* Dirty status of 3.0RC1 release - Issue [5698]
* Git version check is removed - Issue [5695]
* Commit dialog is not refreshing on form focus anymore - Issue [5684]
* RevisionDiff on merge: ArgumentException double clicking the diff header to collapse it - Issue [5683]
* AOOR in RevisionGraph - Issue [5672]
* Commit Dialog: Commit button not as the default button - Issue [5644]
* Running gitex.cmd from powershell doesn't ever load log panel.  - Issue [5629]
* Exception opening commit window due to bad WindowPositions.xml - Issue [5627]
* Long branch name overflows in dashboard - Issue [5625]
* Commit Tab does not clear on init of new repo - Issue [5624]
* Filter revision view for changes on different files/directories - Issue [5623]
* Statistics plugin Broken - Issue [5605]
* Commit graph continuity issue in 3.0 - Issue [5593]
* Ctrl+T for Create tag form picks checked out revision rather than active one - Issue [5564]
* Crash on launch when git not installed - Issue [5562]
* Copy author should include email - Issue [5558]
* Prevent showing async loaded data for a revision it was not loaded for. - PR [5537]
* Open fails when a recent repository doesn't exists anymore - Issue [5515]
* FormCommit shows as unable to stage - Issue [5514]
* Jira Commit Hint plugin doesn't work 3.00.00 beta1 - Issue [5500]
* Unexpected artificial commit in Git command: 1111111111111111111111111111111111111111 - Issue [5493]
* There seems to be no way to turn on "Drop Stash Confirmation" once it's been turned off - Issue [5488]
* Blame options mismatch names and function - Issue [5485]
* Use GitArgumentBuilder to generate git commands in all git calls - Issue [5479]
* Submodules dialog and parenthesis in path - Issue [5469]
* Duplicate line breaks when copying from individual file history - Issue [5440]
* Refreshing the repo status takes too long. - Issue [5439]
* Diff tab: can't select file in the file list - Issue [5438]
* Diff tab: file list resize bug - Issue [5437]
* Bug: remote urls are mangled to lowercase in ExternalLinkRevisionParser::ParseRemotes - Issue [5409]
* FormDeleteTag incorrect height in HiDPI - Issue [5380]
* Check AppData\Local\Programs for editors and merge tools - Issue [5343]
* Search settings should expand the whole tree - Issue [5341]
* FormTagDelete isn't scaled correctly - Issue [5300]
* Part of the current branch graph is colored in grey instead with the good color - Issue [5243]
* FileList: Space between path and file - Issue [5206]
* Git Extension crashes when trying to access settings page in Ubuntu 16.04 - Issue [5187]
* Git config log.showSignature breaks revision grid - Issue [5179]
* Graph column width sometimes appears too narrow - Issue [5167]
* Browsing submodule repo can introduce UI pauses - Issue [5166]
* AppVeyor build is broken: ValueTuple DLL is missing - Issue [5165]
* JTF+Rx Stack overflow - Issue [5134]
* Error on push deleting a remote branch when a tag with the same name exists - Issue [5119]
* Repository init dialog shows incorrect location - Issue [5107]
* Dashboard refinements  - Issue [5084]
* Git-status in the background should use --no-optional-locks - Issue [5066]
* Typing 'b' or 'h' into the Diff Filter Files text box doesn't insert the character - Issue [5065]
* RevisionGrid: tooltip with multi-line body is not shown if the multi-line indicator column is hidden - Issue [5036]
* Customized window size is not kept - Issue [5021]
* RTF round-trip bug causes System.ArgumentException: File format is not valid - Issue [5005]
* NBug scaling issues - Issue [4989]
* Diff view options are positioned wrong, cannot be selected - Issue [4978]
* crash on copy to clipboard operation if repo history is still loading - Issue [4966]
* Crash when you click on the Commit tab - Issue [4956]
* "Reset all changes" not working when "Refresh dialog on form focus" true - Issue [4907]
* Column widths not adjustable - Issue [4902]
* Can't collapse diff with parent in diff panel - Issue [4886]
* CommitPickerSmallControl broken under high DPI - Issue [4862]
* Init wrong folder - Issue [4855]
* LeftPanel - inconsistent remotes management - Issue [4832]
* LeftPanel - inconsistent branch ordering - Issue [4830]
* LeftPanel - IOE "Cannot rename a non-existing remote" - Issue [4829]
* LeftPanel - RepoObjectsTree._branchCriterionAutoCompletionSrc - Issue [4828]
* Invalid patch header: diff --combined GitUI/CommandsDialogs/FormBrowse.cs - Issue [4827]
* Errors loading TeamFoundation assemblies - Issue [4817]
* "Clone repository" form layout broken at high DPI - Issue [4776]
* Fix separator in file name within File History form - PR [4770]
* Add missing word to dialog text - PR [4695]
* List of submodule changes in CommitForm broken - Issue [4684]
* Wrong diff for initial commit - Issue [4580]
* Empty Diff form shows up on revisions grid double click - Issue [4579]
* Crash on opening repository in Recent list. - Issue [4549]
* Find in the Diff tab shows differences reversed after wrapping around the files - Issue [4546]
* Crash during copy to clipboard - Issue [4542]
* "Restore to selected version" function is removed from 2.51 - Issue [4535]
* AsyncLoader doesn't cancel on dispose - Issue [4517]
* Is there a bug in RevisionGraph.ProcessGitLog? - Issue [4516]
* GitK is not launched with newer Git - Issue [4510]
* PuTTY Installer Is Outdated - Issue [4509]
* GitEx selects the top revision after creating a new tag - Issue [4495]
* 2.51: App crashes upon pressing "Script"  - Issue [4488]
* Find in the Diff tab fails to find text - Issue [4485]
* Commit index count disappears on refresh - Issue [4483]
* Branch input isn't focused on Merge window - Issue [4464]
* Unable to solution build in VS2017 - Issue [4458]
* Option to set language in - Dictionary For Spelling Check - not working - Issue [4443]
* Remotes not persisted - Issue [4441]
* GitHub HTTPS push behind proxy - API auth fails (connection not using proxy) - Issue [4422]
* "BuildInstallers.VS2015.cmd : The system cannot find the file specified" in AppVeyor build - Issue [4407]
* MenuItems for untracked should be limited for difftool - Issue [4396]
* Git config diff.noprefix true option breaks most of GitExtensions functionality - Issue [4392]
* Browse Diff Reset: Multiple parents are not handled - Issue [4387]
* Branch filtering not working - Issue [4370]
* git log is called for artificial commits - Issue [4361]
* When creating an annotated tag, a TAGMESSAGE file is left in the repository - Issue [4358]
* Command Log: logs may be lost - Issue [4231]
* Stash List is called to often - Issue [4230]
* "Do you want to add a tracking reference?" dialog appears on different screen - Issue [4205]
* Cycling through Layouts should re-layout everything - Issue [4195]
* Cannot revert or cherry-pick selected lines - Issue [4190]
* Feature: Remove or change old credential.helper setting - Issue [4179]
* "Open local repository" layout broken at high DPI - Issue [4174]
* Long filenames in commit dialog do now show correctly - Issue [4104]
* High HDPI is really messing up the display of screens - Issue [4099]
* Constant repository polling for changes - Issue [4069]
* Stash dialog can't be closed by Escape key - Issue [4066]
* White Text on Light background - Issue [3978]
* "Reword Commit" doesn't refresh the UI - Issue [3935]
* Start page race condition bug when saving settings - Issue [3932]
* "Clean working directory" form layout broken at high DPI - Issue [3828]
* Font selection lets you select font style but then ignores it - Issue [3795]
* git-credential-winstore.exe missing - Issue [3732]
* Commit Screen disappears after clicking on the drop down buttons on it - Issue [3593]
* Console window cannot be closed with the Escape key - Issue [3531]
* GitCredentialWinStore missing - Issue [3511]
* GetSubmoduleStatusAsync CancellationTokenSource Disposed - Issue [3278]
* Not able to issue "git flow release publish" in git-flow plugin  - Issue [2838]
* Fields order in "create tag" window - Issue [2515]
* Blame showing incorrect commit - Issue [2342]
* Submodule dialog: "Name" textbox too small for long names - Issue [1858]


### [Version 2.51.05] (2 September 2018)

#### Fixes:
* Git config log.showSignature breaks revision grid - Issue [5179]

#### Fixes (Mono specific):
* Settings causes crash under linux/mono - Issue [5311]
* Git Extension crashes when trying to access settings page in Ubuntu 16.04 - Issue [5187]
* Diff view options are positioned wrong, cannot be selected under linux/mono - Issue [4978]


### [Version 2.51.04] (8 July 2018)

#### Fixes:
* A number of changed files on Commit button is always '2' - Issue [5127]
* Error on push deleting a remote branch when a tag with the same name exists - Issue [5119]


### [Version 2.51.03] (26 June 2018)

#### Features:
* Set recommended version Git version to 2.17.1 - PR [5095]
* RTF round-trip bug causes System.ArgumentException: File format is not valid - Issue [5005]
* Recheck filter against displayed file name when working in the fileNameOnlyMode mode. - Issue [4991]
* Crash on opening repository in Recent list. - Issue [4549]
* Add Atom Editor to Git Config section - Issue [2507]

#### Fixes:
* Repository init dialog shows incorrect location - Issue [5107]
* Git-status in the background should use --no-optional-locks - Issue [5066]
* Typing 'b' or 'h' into the Diff Filter Files text box doesn't insert the character - Issue [5065]
* Customized window size is not kept - Issue [5021]
* Commit index count disappears on refresh - Issue [4483]


### [Version 2.51.02] (24 May 2018)

#### Highlights:
* Simplified installer - Git and KDiff3 installers are no longer installed as part of GitExtensions's installation. The user will have to install external apps and tools separately.
* Fixes to address a number of layout issues for high DPI monitors.

#### Features:
* Remove Git and KDiff3 from installer 2.51 - PR [4933]
* Check for recommended Git version 2.51 - PR [4932]
* Check for recommended Git version - PR [4929]
* Delete TAGMESSAGE after tag command completes - PR [4792]
* fix: FormFixHome is messed up HiDPI - PR [4780]
* fix: FormRemotes is messed up on HiDPI - PR [4779]
* Fix avatar under high DPI - PR [4711]
* Remove Git from installer package - Issue [4515]

#### Fixes:
* Shell start script failing if run from a different directory - Issue [4975]
* Crash when you click on the Commit tab - Issue [4956]
* "Reset all changes" not working when "Refresh dialog on form focus" true - Issue [4907]
* CommitPickerSmallControl broken under high DPI - Issue [4862]
* Init wrong folder - Issue [4855]
* "Clone repository" form layout broken at high DPI - Issue [4776]
* Git config diff.noprefix true option breaks most of GitExtensions functionality - Issue [4392]
* When creating an annotated tag, a TAGMESSAGE file is left in the repository - Issue [4358]
* "Open local repository" layout broken at high DPI - Issue [4174]
* High HDPI is really messing up the display of screens - Issue [4099]
* "Clean working directory" form layout broken at high DPI - Issue [3828]




### [Version 2.51.01] (11 Mar 2018)

#### Highlights
* Updated bundled Git to 2.16.2
* Updated Putty to 0.70

#### Features:
* TaskCanceledException for ShowSelectedFileDiff() - Issue [4570]
* Add a hotkey for toggling viewing of Tags - Issue [4449]
* Fixes on jira commit hint plugin - PR [4412]
* Add two ScriptEvents, BeforeMerge, AfterMerge - Issue [4380]
* Better display of "Find and replace" dialog on multiple screens computer - PR [4354]
* Commit "Author" text box has incorrect descriptive text - Issue [3501]
* Keep last used Destination in clone dialog as entered - Issue [2313]

#### Fixes:
* Wrong diff for initial commit - Issue [4580]
* Find in the Diff tab shows differences reversed after wrapping around the files - Issue [4546]
* "Restore to selected version" function is removed from 2.51 - Issue [4535]
* Update Git for Windows version to account for recent GitHub breakage [4523]
* GitK is not launched with newer Git - Issue [4510]
* PuTTY Installer Is Outdated - Issue [4509]
* GitEx selects the top revision after creating a new tag - Issue [4495]
* 2.51: App crashes upon pressing "Script"  - Issue [4488]
* Find in the Diff tab fails to find text - Issue [4485]
* Branch input isn't focused on Merge window - Issue [4464]
* Option to set language in - Dictionary For Spelling Check - not working - Issue [4443]
* Remotes not persisted - Issue [4441]
* git log is called for artificial commits - Issue [4361]
* "Reword Commit" doesn't refresh the UI - Issue [3935]
* Not able to issue "git flow release publish" in git-flow plugin  - Issue [2838]
* Fields order in "create tag" window - Issue [2515]
* Submodule dialog: "Name" textbox too small for long names - Issue [1858]




### [Version 2.51.00] (28 Jan 2018)

#### Features:
* Commandline difftool raised Assert - PR [4386]
* Replace lightbulb images - PR [4351]
* Rename arguments related to diff to firstRevision, secondRevision to … - PR [4344]
* Use built-in stream.CopyTo method in SynchronizedProcessReader - PR [4343]
* Add icons in the browse form command menu - PR [4331]
* Display some missing shortcuts in Browse form menus - PR [4330]
* FormBrowse: Add option to display reflog references - PR [4321]
* Display branch name in bold only when it is the one checked out - PR [4320]
* Create branch modal buttons under linux/mono - Issue [4319]
* Browse Diff Untracked: Delete and Edit menu items are not enabled - PR [4318]
* Commit & Push (forced with lease) when Amend is checked - Issue [4296]
* FileHistory: Show Blame tab also for artificial commits - PR [4293]
* Artificial commit changed count should be dynamic - PR [4209]
* Jenkins build server integration: support for multi pipeline and wildcards  - Issue [4202]
* GitEx does not remember splitter position - Issue [4058]
* Enhanced view of uncommitted changes in Browse Repository - Issue [4031]
* Support github-mac:// protocol - Issue [4276]
* Add --simplify-merges when showing file full history - Issue [4264]
* Change text in settings for artificial commits - PR [4246]
* refactor: Split state and behavior of CommitInformation - PR [4241]
* RevisionHeader work follow up - Issue [4237]
* Scroll commit list during rebase conflict so the next to apply commit is visible - Issue [4233]
* Bitbucket basic functionality #4204 - PR [4228]
* Change the display name for Bitbucket Server plugin - PR [4227]
* Pad fields in RevisionHeader with spaces instead of tabs - PR [4218]
* Bitbucket plugin: Exception if not initialized - PR [4211]
* Rename "Atlassian Stash" to Bitbucket - PR [4210]
* Browse Difftool Menu Items cleanup - PR [4207]
* Jenkins integration - show more interesting data first. - PR [4197]
* Provide GPG tab layout for Mono - Issue [4196]
* Commit Form: Display current branch name - PR [4189]
* Better naming of archives done through a filetree directory - PR [4188]
* BrowseDiff Hotkey support: DEL to delete unstaged files - PR [4168]
* Browse Diff Menu Items should be disabled when no item is SelectedDiff - PR [4167]
* FormBrowse Commands in toolbar menu raised exceptions for artificial … - PR [4166]
* RevisionFileTree context menu gave exceptions if no items were Selected - PR [4165]
* Commit menus raised exceptions if no items were Selected - PR [4163]
* Compare artificial commits to all other commits - PR [4157]
* Always show artificial commits in RevisionGrid - PR [4147]
* View stash - names are cut off and selectfield is not resizable - Issue [4120]
* Fix FormCommit file list filter input getting treated as hotkeys - PR [4115]
* Fix potential copy-paste bug - PR [4109]
* FileHistory: Show DiffTab when opening, not ViewFile - PR [4105]
* Hide CommitInfo panel for virtual commits - PR [4096]
* (A lot of) filetree improvements - PR [4093]
* Browse Diff Submodule menu options for unstaged commit - PR [4092]
* Remove "(slow!)" for showing staged/unstaged as commits in Settings - PR [4088]
* Stage/unstage in browse - PR [4087]
* Show count for artificial commits - PR [4086]
* CA2202 CA2213 suppression - PR [4085]
* Various forms: limit menu options for artificial and submodule - PR [4084]
* Feature/n4031 refactoring anon icon menu - PR [4079]
* Use GitExt icon for menu items that open a new instance for Submodule - PR [4076]
* Try to find ssh.exe in git installation directory - PR [4074]
* Change "reset all changes" button position in commit dialog - Issue [4057]
* FileStatusList had useless horizontal scrollbar - PR [4052]
* Enhanced view of uncommitted changes in Browse Repository - Issue [4031]
* Enhance file tree control - PR [4022]
* Extract "File Tree" control from FormBrowse - PR [4020]
* Opening up the search dropdown list on focus - Issue [4016]
* Remove RSS feeds functionality - PR [4008]
* "Check for update" window appears behind other open windows - Issue [3999]
* Change Pull dialog title and menu entry to Pull/Fetch - Issue [3970]
* added *.m for Matlab files - PR [3955]
* Reflog: display also reflog for remotes - PR [3953]
* Convert user-supplied relative path to absolute path - Issue [3947]
* Fix tab order in the FileStatusList component - PR [3930]
* Skip worktree feature - PR [3921]
* Form commit workflow improvements - PR [3920]
* Add (and refactor) diff and merge tools - PR [3919]
* Delete index.lock should delete in submodules - Issue [3915]
* Allow to configure the number of Recent repositories - Issue [3908]
* Feature request: Add option to choose branch name ordering preference - Issue [3907]
* Update Reactive Extensions to 3.1 - PR [3900]
* Creating new local branch triggers updating submodules? - Issue [3899]
* On "Commit dialog" configuration page, raise the "previous messages" limit - Issue [3892]
* Feature Req.: Commit button should indicate if file in repos. changed - Issue [3887]
* [Feature] Tag dialog allow to sign the tag - Issue [3842]
* Annoying closing of menu from tool bar buttons - Issue [3832]
* Turn off zebra striping in 2.50 browser? - Issue [3810]
* Make autocomplete for files starting with a dot available in commit message field (in UI of executable) - Issue [3760]
* FormOpenDirectory: Add a button to go (easily) to the parent directory - PR [3733]
* [feature request] More descriptive diffs for merge commits - Issue [3709]
* Add Visual Studio Code to the editor list - Issue [3652]
* Implement support for --skip-worktree - Issue [3525]
* Support signing commits via GPG - Issue [3161]
* Diff window: configurable column for "ruler" or "gutter mark" - Issue [2868]
* Visual Studio 2008: File history/blame shows the current line  - Issue [2839]
* Jira Commit Hint Plugin - PR [2495]
* Context menu for commit with remote branch doesn't offer `Delete branch` option - Issue [1583]

#### Fixes:
* Commit view shows inverted diff output - Issue [4374]
* Wrong diff for stashed untracked files - Issue [4373]
* Branch filtering not working - Issue [4370]
* Unable to add remote having selected deactivated item - Issue [4349]
* Bitbucket Server: XSRF error when approving - Issue [4345]
* fix: AE when starting app without a repository - PR [4340]
* BuildReport: Exception for WebBrowserCtrl.Navigate - Issue [4322]
* Browse Diff Untracked: Delete and Edit menu items are not enabled - Issue [4316]
* FormFileHistory: DiffToLocal hidden also when relevant - Issue [4315]
* Browse Diff Garbage and exception for untracked files - Issue [4301]
* Number of changed files isn't displayed in Commit button - Issue [4295]
* File rename events are not detected by filewatcher - Issue [4292]
* Exception occurs when trying load the delete tag form - Issue [4283]
* "View Stash" triggers System.ArgumentOutOfRangeException - Issue [4263]
* NRE in FormPull when running TranslationApp - Issue [4258]
* Commit tab: _commitInformationProvider was null. - Issue [4255]
* System.IO.IOException "Unable to remove the file to be replaced." - Issue [4250]
* Diff tab selected when GE starts up - Issue [4242]
* puttykeyfile option is not written to config during clone - Issue [4235]
* Commands are duplicated in GE Gitcommand log - Issue [4213]
* BitBucket plugin is broken - Issue [4204]
* Jenkins integration does not refresh "in progress" builds info. - Issue [4185]
* NPE on closing settings dialog - Issue [4160]
* Invisible "Browse" button in "Open repository" dialog - Issue [4132]
* diff.submodules=log raises exception - Issue [4130]
* AOORE in "Open local repository" dialog under Mono - Issue [4126]
* Some commands throws NullReferenceException for a new empty repo (2.50.02) - Issue [4098]
* Debug builds fails at commit if reallocated - PR [4075]
* Unable to filter file in the commit dialog - Issue [4062]
* DiffMerge should be sgdm.exe - Issue [4049]
* Release Notes Generator breaks under git version 2.14.1 - Issue [4028]
* Not able to read TAGMESSAGE file - Issue [4025]
* Github > View pull requests... > Close throws "Object reference not set to an instance of an object." - Issue [4024]
* Cancelling checking if shell extension is registered crashes GitExtensions - PR [4019]
* Filter branch combobox is case sensitive  - Issue [4014]
* NRE on open gitextensions - Issue [4012]
* Scripts with On event setting "AfterCheckout" or "BeforeCheckout" do not activate on revision checkout - Issue [4006]
* Weird tab field order - Issue [3990]
* Commit window title does not reflect newly created branch - Issue [3982]
* Panel layouts are unstable now - Issue [3966]
* NRE when jira plugin not configured - Issue [3962]
* OutOfMemoryException on startup after Changing Commit View Layout - Issue [3959]
* Commit dialog shows unsupported file for sub-modules with diff.mnemonicprefix=true - Issue [3948]
* commit message template file not found in root folder of repo - Issue [3897]
* File tree no longer working - Issue [3875]
* Apply patch / Select patch file should filter for lowercase *.patch - Issue [3867]
* Text strings for `.git/info/exclude` modals need adjusting - Issue [3860]
* "Existing worktrees" window does not handle worktrees with a space in the path - Issue [3849]
* GitFlow plugin is missing since GitExtensions v2.49.03 - Issue [3839]
* Height of bottom tab control (Commit-Info, File-Tree, Diff) gets smaller on GitExtensions start - Issue [3822]
* Fix spelling in UI: "mergeconflict*" -> "merge conflict*" - PR [3772]
* Exception shown instead of error message for locked file in commit dialog - Issue [3759]
* 'Show remote branches' check state does not toggle after click - Issue [3730]
* Multiple GUI regressions on mono - Issue [3725]
* GitCommands: Avoid creating a fake remote ref on pull - PR [3484]
* Format patch creates a file with a lower case p in .patch. Filter uses upper case p in .Patch - Issue [2870]
* Scripts not asking for confirmation even if configured to do so - Issue [1608]
* Show an error message when cloning without specifying a destination - Issue [1605]
* Error while resetting files - Issue [1307]




### [Version 2.51.RC2] (14 Jan 2018)

#### Features:
* Replace lightbulb images - PR [4351]
* Rename arguments related to diff to firstRevision, secondRevision to … - PR [4344]
* Use built-in stream.CopyTo method in SynchronizedProcessReader - PR [4343]
* Add icons in the browse form command menu - PR [4331]
* Display some missing shortcuts in Browse form menus - PR [4330]
* FormBrowse: Add option to display reflog references - PR [4321]
* Display branch name in bold only when it is the one checked out - PR [4320]
* Create branch modal buttons under linux/mono - Issue [4319]
* Browse Diff Untracked: Delete and Edit menu items are not enabled - PR [4318]
* Commit & Push (forced with lease) when Amend is checked - Issue [4296]
* FileHistory: Show Blame tab also for artificial commits - PR [4293]
* Artificial commit changed count should be dynamic - PR [4209]
* Jenkins build server integration: support for multi pipeline and wildcards  - Issue [4202]
* GitEx does not remember splitter position - Issue [4058]
* Enhanced view of uncommitted changes in Browse Repository - Issue [4031]

#### Fixes:
* Unable to add remote having selected deactivated item - Issue [4349]
* Bitbucket Server: XSRF error when approving - Issue [4345]
* fix: AE when starting app without a repository - PR [4340]
* BuildReport: Exception for WebBrowserCtrl.Navigate - Issue [4322]
* Browse Diff Untracked: Delete and Edit menu items are not enabled - Issue [4316]
* FormFileHistory: DiffToLocal hidden also when relevant - Issue [4315]
* Browse Diff Garbage and exception for untracked files - Issue [4301]
* Number of changed files isn't displayed in Commit button - Issue [4295]




### [Version 2.51.RC1] (31 Dec 2017)

#### Features:
* Support github-mac:// protocol - Issue [4276]
* Add --simplify-merges when showing file full history - Issue [4264]
* Change text in settings for artificial commits - PR [4246]
* refactor: Split state and behavior of CommitInformation - PR [4241]
* RevisionHeader work follow up - Issue [4237]
* Scroll commit list during rebase conflict so the next to apply commit is visible - Issue [4233]
* Bitbucket basic functionality #4204 - PR [4228]
* Change the display name for Bitbucket Server plugin - PR [4227]
* Pad fields in RevisionHeader with spaces instead of tabs - PR [4218]
* Bitbucket plugin: Exception if not initialized - PR [4211]
* Rename "Atlassian Stash" to Bitbucket - PR [4210]
* Browse Difftool Menu Items cleanup - PR [4207]
* Jenkins integration - show more interesting data first. - PR [4197]
* Provide GPG tab layout for Mono - Issue [4196]
* Commit Form: Display current branch name - PR [4189]
* Better naming of archives done through a filetree directory - PR [4188]
* BrowseDiff Hotkey support: DEL to delete unstaged files - PR [4168]
* Browse Diff Menu Items should be disabled when no item is SelectedDiff - PR [4167]
* FormBrowse Commands in toolbar menu raised exceptions for artificial … - PR [4166]
* RevisionFileTree context menu gave exceptions if no items were Selected - PR [4165]
* Commit menus raised exceptions if no items were Selected - PR [4163]
* Compare artificial commits to all other commits - PR [4157]
* Always show artificial commits in RevisionGrid - PR [4147]
* View stash - names are cut off and selectfield is not resizable - Issue [4120]
* Fix FormCommit file list filter input getting treated as hotkeys - PR [4115]
* Fix potential copy-paste bug - PR [4109]
* FileHistory: Show DiffTab when opening, not ViewFile - PR [4105]
* Hide CommitInfo panel for virtual commits - PR [4096]
* (A lot of) filetree improvements - PR [4093]
* Browse Diff Submodule menu options for unstaged commit - PR [4092]
* Remove "(slow!)" for showing staged/unstaged as commits in Settings - PR [4088]
* Stage/unstage in browse - PR [4087]
* Show count for artificial commits - PR [4086]
* CA2202 CA2213 suppression - PR [4085]
* Various forms: limit menu options for artificial and submodule - PR [4084]
* Feature/n4031 refactoring anon icon menu - PR [4079]
* Use GitExt icon for menu items that open a new instance for Submodule - PR [4076]
* Try to find ssh.exe in git installation directory - PR [4074]
* Change "reset all changes" button position in commit dialog - Issue [4057]
* FileStatusList had useless horizontal scrollbar - PR [4052]
* Enhanced view of uncommitted changes in Browse Repository - Issue [4031]
* Enhance file tree control - PR [4022]
* Extract "File Tree" control from FormBrowse - PR [4020]
* Opening up the search dropdown list on focus - Issue [4016]
* Remove RSS feeds functionality - PR [4008]
* "Check for update" window appears behind other open windows - Issue [3999]
* Change Pull dialog title and menu entry to Pull/Fetch - Issue [3970]
* added *.m for Matlab files - PR [3955]
* Reflog: display also reflog for remotes - PR [3953]
* Convert user-supplied relative path to absolute path - Issue [3947]
* Fix tab order in the FileStatusList component - PR [3930]
* Skip worktree feature - PR [3921]
* Form commit workflow improvements - PR [3920]
* Add (and refactor) diff and merge tools - PR [3919]
* Delete index.lock should delete in submodules - Issue [3915]
* Allow to configure the number of Recent repositories - Issue [3908]
* Feature request: Add option to choose branch name ordering preference - Issue [3907]
* Update Reactive Extensions to 3.1 - PR [3900]
* Creating new local branch triggers updating submodules? - Issue [3899]
* On "Commit dialog" configuration page, raise the "previous messages" limit - Issue [3892]
* Feature Req.: Commit button should indicate if file in repos. changed - Issue [3887]
* [Feature] Tag dialog allow to sign the tag - Issue [3842]
* Annoying closing of menu from tool bar buttons - Issue [3832]
* Turn off zebra striping in 2.50 browser? - Issue [3810]
* Make autocomplete for files starting with a dot available in commit message field (in UI of executable) - Issue [3760]
* FormOpenDirectory: Add a button to go (easily) to the parent directory - PR [3733]
* [feature request] More descriptive diffs for merge commits - Issue [3709]
* Add Visual Studio Code to the editor list - Issue [3652]
* Implement support for --skip-worktree - Issue [3525]
* Support signing commits via GPG - Issue [3161]
* Diff window: configurable column for "ruler" or "gutter mark" - Issue [2868]
* Visual Studio 2008: File history/blame shows the current line  - Issue [2839]
* Jira Commit Hint Plugin - PR [2495]
* Context menu for commit with remote branch doesn't offer `Delete branch` option - Issue [1583]

#### Fixes:
* File rename events are not detected by filewatcher - Issue [4292]
* Exception occurs when trying load the delete tag form - Issue [4283]
* "View Stash" triggers System.ArgumentOutOfRangeException - Issue [4263]
* NRE in FormPull when running TranslationApp - Issue [4258]
* Commit tab: _commitInformationProvider was null. - Issue [4255]
* System.IO.IOException "Unable to remove the file to be replaced." - Issue [4250]
* Diff tab selected when GE starts up - Issue [4242]
* puttykeyfile option is not written to config during clone - Issue [4235]
* Commands are duplicated in GE Gitcommand log - Issue [4213]
* BitBucket plugin is broken - Issue [4204]
* Jenkins integration does not refresh "in progress" builds info. - Issue [4185]
* NPE on closing settings dialog - Issue [4160]
* Invisible "Browse" button in "Open repository" dialog - Issue [4132]
* diff.submodules=log raises exception - Issue [4130]
* AOORE in "Open local repository" dialog under Mono - Issue [4126]
* Some commands throws NullReferenceException for a new empty repo (2.50.02) - Issue [4098]
* Debug builds fails at commit if reallocated - PR [4075]
* Unable to filter file in the commit dialog - Issue [4062]
* DiffMerge should be sgdm.exe - Issue [4049]
* Release Notes Generator breaks under git version 2.14.1 - Issue [4028]
* Not able to read TAGMESSAGE file - Issue [4025]
* Github > View pull requests... > Close throws "Object reference not set to an instance of an object." - Issue [4024]
* Cancelling checking if shell extension is registered crashes GitExtensions - PR [4019]
* Filter branch combobox is case sensitive  - Issue [4014]
* NRE on open gitextensions - Issue [4012]
* Scripts with On event setting "AfterCheckout" or "BeforeCheckout" do not activate on revision checkout - Issue [4006]
* Weird tab field order - Issue [3990]
* Commit window title does not reflect newly created branch - Issue [3982]
* Panel layouts are unstable now - Issue [3966]
* NRE when jira plugin not configured - Issue [3962]
* OutOfMemoryException on startup after Changing Commit View Layout - Issue [3959]
* Commit dialog shows unsupported file for sub-modules with diff.mnemonicprefix=true - Issue [3948]
* commit message template file not found in root folder of repo - Issue [3897]
* File tree no longer working - Issue [3875]
* Apply patch / Select patch file should filter for lowercase *.patch - Issue [3867]
* Text strings for `.git/info/exclude` modals need adjusting - Issue [3860]
* "Existing worktrees" window does not handle worktrees with a space in the path - Issue [3849]
* GitFlow plugin is missing since GitExtensions v2.49.03 - Issue [3839]
* Height of bottom tab control (Commit-Info, File-Tree, Diff) gets smaller on GitExtensions start - Issue [3822]
* Fix spelling in UI: "mergeconflict*" -> "merge conflict*" - PR [3772]
* Exception shown instead of error message for locked file in commit dialog - Issue [3759]
* 'Show remote branches' check state does not toggle after click - Issue [3730]
* Multiple GUI regressions on mono - Issue [3725]
* GitCommands: Avoid creating a fake remote ref on pull - PR [3484]
* Format patch creates a file with a lower case p in .patch. Filter uses upper case p in .Patch - Issue [2870]
* Scripts not asking for confirmation even if configured to do so - Issue [1608]
* Show an error message when cloning without specifying a destination - Issue [1605]
* Error while resetting files - Issue [1307]




### [Version 2.50.02] (06 September 2017)

#### Features:
* Remote repositories modal defaults to inactive repo - Issue [3861]
* Allow cherry-picking multiple commits from FormBrowse menu - PR [3852]

#### Fixes:
* Clicking Commit causes a crash - Issue [3827]
* 2.50.01 Quoting issues on git checkout command - Issue [3969]
* Can't delete index.lock because it is being used by another process. - Issue [3902]
* Missing Newtonsoft.Json on startup exception - Issue [3879]
* 2.50.00/2.50.01 introduced issue parsing " in scripts - Issue [3864]
* System.NullReferenceException when trying to push - Issue [3862]
* GitExtensions 2.50.01 gives System.NullReferenceException at start - Issue [3855]
* Application crashing on startup after upgrading to 2.50.01 - Issue [3845]
* DirectoryNotFoundException in Remote Repositories dialog - Issue [3844]
* help picture disappeared when opening pull dialog - Issue [3829]
* Committing fails: could not read log file, Invalid argument - Issue [3800]
* NRE when attempting to push with no remote configured - Issue [3794]
* Exception "Illegal characters in path" when invoking "browse" via command line parameter - Issue [3489]
* Blows up when I click on Help in Branch dialog - Issue [3011]

### [Version 2.50.01] (07 July 2017)

#### Fixes:
* Push branch with no upstream defaults to first remote and not to "origin" - Issue [3821]
* [Bug] after 2.50 unable to use "Revert selected lines" from commit window - Issue [3819]
* [Bug / Regression] It is not possible to run an external difftool (F3) in the diff view of "Commit index / Current unstaged changes" - Issue [3814]
* Merge Conflict contextmenu items are disabled in 2.5.0 - Issue [3809]
* GitExtensions gives continuous System.ArgumentNullException - Issue [3806]
* SEG FAULT on every git operation. - Issue [3804]
* Amend Commit is not working in Version 2.5 - Issue [3786]

### [Version 2.50] (23 June 2017)

#### Features:
* Hotkey for "Stage All" button - Issue [3756]
* Increased width for 'Open local repository' dialog. - PR [3644]
* Shortcut key for "Create fixup commit" - Issue [3616]
* Open diff form not in modal - PR [3598]
* Git Worktree Support (Git 2.5) - Issue [3590]
* File diff window always shows the top most file in a multiple selection - Issue [3510]
* Feature request: assign F2 key to "Rename branch" - Issue [3503]
* Feature proposal - turn remotes on/off - Issue [3456]
* Autonormalise local branch at checkout - Issue [3450]
* Warn the user if resetting a local branch is non fast forward. - Issue [3438]
* AppVeyor CI plugin - PR [3426]
* 2.49 RC2: Branch name not showing in VS 2015  - Issue [3393]
* ConEmu settings - Issue [3392]
* Atlassian Stash is now called Bitbucket Server - Issue [3334]
* Feature: Support multi Selection in diff tab - Issue [3293]
* Feature: Allow to run plugins from the scripts - Issue [3248]
* Better ergonomic around commit form - PR [3245]
* Feature/tfs2015 build integration - PR [3219]
* Improve UI in 'merge branches'  - Issue [3208]
* Add Build server integration for TFS-Build 2015 - Issue [3177]
* Add Edit functionality in Advanced menu - Issue [3166]
* "Add to gitignore" should include "/" prefix - Issue [3162]
* Revert selected lines context option - Issue [3159]
* Teamcity - log onto teamcity using HTTP NTLM authentication  - Issue [3119]
* Need staged file count - Issue [3073]
* Some fixes for a filter in "Commit" dialog - PR [3000]
* Allow to change merge message - PR [2997]
* Support for powershell scripts. - PR [2917]
* Visual Studio 2008: File history/blame shows the current line  - Issue [2839]
* Localizable phrases of ProxySwitcher's settings - PR [2802]
* [Feature request] Improve usability for "revision links" feature - Issue [2768]
* Detect hashes in commit messages and convert them to hyper links - Issue [2714]
* Add option to open submodule from diff lists - Issue [2706]
* Add --log option to merge window - Issue [2688]
* Add ability to reveal certain commit from command line - Issue [2675]
* Revision header's height is short if Japanese font is selected - Issue [2670]
* Option to disable detached head dialogs - Issue [2460]
* better ordering of branch names in "checkout branch" dialog - Issue [2455]
* Expose the "repository excludes" file for configuration - Issue [2194]
* Simplify calling "Prune remote branches" - Issue [2141]
* Allow creating a new branch in the commit dialog - Issue [2016]
* Add a checkbox to enable the --no-verify flag on commit - Issue [1982]
* Support Meld as a mergetool - Issue [1975]
* allow multi-select in resolve conflict window - Issue [1845]
* Ability to clear recent repositories. - Issue [1064]


#### Fixes:
* "Recover lost object" sometime doesn't show result - Issue [3777]
* Very slow if "Show first parent" enabled - Issue [3767]
* Win32Exception when clicking on Revision Link - Issue [3763]
* Open Transifex website for translation - PR [3746]
* mono: crash on an attempt to "Copy commit info" - Issue [3729]
* The found text is not highlighted when searching in DiffView - Issue [3719]
* Commit messages are corrupted if there are "mixed" encodings in log - Issue [3707]
* Bug in Scripts - Issue [3691]
* Portable mode & Gravatar Cache path - Issue [3594]
* Commit dialog diff panel resizes inconsistently - Issue [3592]
* Revision grid suddenly change selection after finish loading revisions - Issue [3583]
* Don't use ResetMixed for "Unstage All" button when the commit is a merge - Issue [3565]
* View Stash - window layout - Issue [3564]
* Ctrl+R without Ctrl+L raise NullReferenceException - Issue [3534]
* ConfigFile parser crashed if section contains ']' - Issue [3532]
* WBEM_E_NOT_FOUND when clicking on matched Revision link - Issue [3515]
* Checkout branch window doesn't rescale - Issue [3490]
* Crash in main window after filenames are misaligned - Issue [3467]
* Hotkeys lost in 2.49 - Issue [3432]
* Branch rename does not autonormalise - Issue [3424]
* Visual Studio 2008 AddIn isn't working anymore - Issue [3423]
* 2.49RC2 Clean working directory single line log  - Issue [3383]
* GitExtensions Merge ODS - Hangs Process - Issue [3218]
* when merge ms-office-word file,it opened two local version. - Issue [3192]
* Saving new remote clears URL entry - Issue [3154]
* Delete selected, poor usability = lost changes - Issue [3127]
* Revisiongrid doesn't keep the selection - Issue [2956]
* Cannot paste into commit window - Issue [2926]
* On Diff/File List pane got exception ArgumentOutOfRangeException InvalidArgument Value of -1 is not valid for index - Issue [2759]
* Strip any ANSI escape codes from git command output - PR [2689]
* Remote Repositories > Separate Push URL not working - Issue [2550]
* In the statistics plugin, in the "Code by type" tab, allocation of space on the pie chart was wrong. - Issue [2530]
* Commit selected is a random commit after deleting a branch - Issue [2446]
* 2.47.x IME error with Spell checker - Issue [2301]

### Version 2.50RC2 (15 June 2017)
#### Fixes:
* fix Teamcity build chooser [3762]
* Crash after saving settings - Issue [3755]
* System.ArgumentException on start - Issue [3761]

### Version 2.50RC1 (10 June 2017)
#### Features:
* Increased width for 'Open local repository' dialog. - PR [3644]
* Shortcut key for "Create fixup commit" - Issue [3616]
* Open diff form not in modal - PR [3598]
* Git Worktree Support (Git 2.5) - Issue [3590]
* File diff window always shows the top most file in a multiple selection - Issue [3510]
* Feature request: assign F2 key to "Rename branch" - Issue [3503]
* Feature proposal - turn remotes on/off - Issue [3456]
* Autonormalise local branch at checkout - Issue [3450]
* Warn the user if resetting a local branch is non fast forward. - Issue [3438]
* AppVeyor CI plugin - PR [3426]
* 2.49 RC2: Branch name not showing in VS 2015  - Issue [3393]
* ConEmu settings - Issue [3392]
* Atlassian Stash is now called Bitbucket Server - Issue [3334]
* Feature: Support multi Selection in diff tab - Issue [3293]
* Feature: Allow to run plugins from the scripts - Issue [3248]
* Better ergonomic around commit form - PR [3245]
* Feature/tfs2015 build integration - PR [3219]
* Improve UI in 'merge branches'  - Issue [3208]
* Add Build server integration for TFS-Build 2015 - Issue [3177]
* Add Edit functionality in Advanced menu - Issue [3166]
* "Add to gitignore" should include "/" prefix - Issue [3162]
* Revert selected lines context option - Issue [3159]
* Teamcity - log onto teamcity using HTTP NTLM authentication  - Issue [3119]
* Need staged file count - Issue [3073]
* Some fixes for a filter in "Commit" dialog - PR [3000]
* Allow to change merge message - PR [2997]
* Support for powershell scripts. - PR [2917]
* Visual Studio 2008: File history/blame shows the current line  - Issue [2839]
* Localizable phrases of ProxySwitcher's settings - PR [2802]
* [Feature request] Improve usability for "revision links" feature - Issue [2768]
* Detect hashes in commit messages and convert them to hyper links - Issue [2714]
* Add option to open submodule from diff lists - Issue [2706]
* Add --log option to merge window - Issue [2688]
* Add ability to reveal certain commit from command line - Issue [2675]
* Revision header's height is short if Japanese font is selected - Issue [2670]
* Option to disable detached head dialogs - Issue [2460]
* better ordering of branch names in "checkout branch" dialog - Issue [2455]
* Expose the "repository excludes" file for configuration - Issue [2194]
* Simplify calling "Prune remote branches" - Issue [2141]
* Allow creating a new branch in the commit dialog - Issue [2016]
* Add a checkbox to enable the --no-verify flag on commit - Issue [1982]
* Support Meld as a mergetool - Issue [1975]
* allow multi-select in resolve conflict window - Issue [1845]
* Ability to clear recent repositories. - Issue [1064]

#### Fixes:
* The found text is not highlighted when searching in DiffView - Issue [3719]
* Commit messages are corrupted if there are "mixed" encodings in log - Issue [3707]
* Bug in Scripts - Issue [3691]
* Portable mode & Gravatar Cache path - Issue [3594]
* Commit dialog diff panel resizes inconsistently - Issue [3592]
* Revision grid suddenly change selection after finish loading revisions - Issue [3583]
* Don't use ResetMixed for "Unstage All" button when the commit is a merge - Issue [3565]
* View Stash - window layout - Issue [3564]
* Ctrl+R without Ctrl+L raise NullReferenceException - Issue [3534]
* ConfigFile parser crashed if section contains ']' - Issue [3532]
* WBEM_E_NOT_FOUND when clicking on matched Revision link - Issue [3515]
* Checkout branch window doesn't rescale - Issue [3490]
* Crash in main window after filenames are misaligned - Issue [3467]
* Hotkeys lost in 2.49 - Issue [3432]
* Branch rename does not autonormalise - Issue [3424]
* Visual Studio 2008 AddIn isn't working anymore - Issue [3423]
* 2.49RC2 Clean working directory single line log  - Issue [3383]
* GitExtensions Merge ODS - Hangs Process - Issue [3218]
* when merge ms-office-word file,it opened two local version. - Issue [3192]
* Saving new remote clears URL entry - Issue [3154]
* Delete selected, poor usability = lost changes - Issue [3127]
* Revisiongrid doesn't keep the selection - Issue [2956]
* Cannot paste into commit window - Issue [2926]
* On Diff/File List pane got exception ArgumentOutOfRangeException InvalidArgument Value of -1 is not valid for index - Issue [2759]
* Strip any ANSI escape codes from git command output - PR [2689]
* Remote Repositories > Separate Push URL not working - Issue [2550]
* In the statistics plugin, in the "Code by type" tab, allocation of space on the pie chart was wrong. - Issue [2530]
* Commit selected is a random commit after deleting a branch - Issue [2446]
* 2.47.x IME error with Spell checker - Issue [2301]

### Version 2.49.03 (26 March 2017)
* Fixed issue #3605. NullReferenceException from CanBeGitUrl when trying to clone git repository from dashboard.
* Fixed issue #3578. File history and blame not show when path to file contain Cyrillic chars bug reproducible.

### Version 2.49.02 (22 March 2017)
* Fixed issue #3464. Background fetch plugin - Not working.
* Fixed issue #3491. 'Enter commit message' does not disappear when you start typing your message bug reproducible.
* Fixed issue #3394. Gitext Clone in Explorer Context Menu try to clone in the wrong folder bug.
* Fixed issue #3142. Comparing A/B/Working directory with diff tool UX.
* Fixed issue #3589. Option 'Close dialog when all changes are committed' does not work properly bug
* Fixed issue #3569. Windows OS version string is incorrect for 8, 8.1, and 10.
* Fixed issue #3539. Non-sequential tab ordering on Settings -> Advanced -> Confirmations page.
* Fixed issue #3554. Comparing branch form didn't honor the ShowMoreContext toolbar button.
* Fix problem with incorrect building command line arguments. PR #3551

### Version 2.49.01 (12 March 2017)
* Fixed issue #3587. Diff view is blank if git configuration is diff.color=always
* Fixed issue #3427. Pushing to a different remote wrongly ask about new branch bug.
* Fixed issue #3560. GitExtensions doesn't respect core.commentChar setting in interactive rebase.
* Fixed issue #3507. Branch normalization strips valid characters from the branch name.
* Fixed issue #3372. Create tag: set focus to tag name first feature-request UX.
* Fixed issue #3441. Prompt to commit after resolving merge conflicts even when "Do not commit" is checked bug.
* Fixed issue #3445. "Reset file(s) to" functionality doesn't work bug.
* Fixed issue #2679. "Unsupported file" error with submodules
* Fixed issue #3434. Filtering commits by Unicode search text fails bug
* Fixed issue #3412. Fix reset file menu item text.
* Fix "Host Fingerprint not registered" Plink handling on clone. PR #3405
* Fix crash when one of the stash is an autostash. PR #3410

### Version 2.49 (9 November 2016)
* Updated Git for Windows to version 2.10.1. PR #3353
* Updated Putty to version 0.67
* KDiff3 rolled back to version 0.9.97
* VSIX extension for VS2015. PR #2885, #3331
* ConEmu Console Emulator Control for Running Git Commands in the Real Terminal. PR #3152
* Highlighting of authored commits. PR #2672
* Support sparse checkout. PR #2918
* Support shallow clone. PR #2911
* Support no checkout on Clone. PR #2921
* Stash before rebase. PR #2770
* Draw smoother Bezier curves in Revision Grid. PR #2662
* Support for --assume-unchanged. PR #2889
* Colorful diffs for Linux. PR #2969
* Comparing to another branch or Commit. PR #3039
* Support for “View Tag annotations”. PR #2836
* Support showing the real line number for diff. PR #2988
* Add force with lease to the advanced push options. PR #2991
* Diff filtering. PR #3198
* Cherry pick selected file/selected lines
* Added preset for VisualStudio Diff Tool. PR #3034
* Polish translation and dictionary added
* Czech translation added
* Disabled "Traditional Chinese" translation
* Romanian dictionary added. PR #2979

### Version 2.49RC2 (22 October 2016)
* Updated Git for Windows to version 2.10.1. PR #3353
* Updated Putty to version 0.67
* Fixed issue #3356, #3370, #3357: ConEmu integration issues
* Fixed issue #2532: Fix encoding of gitext.sh
* Fixed issue #3364: Prevent crash when encountering a 'tag' in 'recover lost objects' form
* Fixed issue #3365: Exception when search 'diff contains' has a special character

### Version 2.49RC1 (12 October 2016)
* Updated Git for Windows to version 2.10.0
* KDiff3 rolled back to version 0.9.97
* VSIX extension for VS2015. PR #2885, #3331
* ConEmu Console Emulator Control for Running Git Commands in the Real Terminal. PR #3152
* Highlighting of authored commits. PR #2672
* Support sparse checkout. PR #2918
* Support shallow clone. PR #2911
* Support no checkout on Clone. PR #2921
* Stash before rebase. PR #2770
* Draw smoother Bezier curves in Revision Grid. PR #2662
* Support for --assume-unchanged. PR #2889
* Colorful diffs for Linux. PR #2969
* Comparing to another branch or Commit. PR #3039
* Support for “View Tag annotations”. PR #2836
* Support showing the real line number for diff. PR #2988
* Add force with lease to the advanced push options. PR #2991
* Diff filtering. PR #3198
* Cherry pick selected file/selected lines
* Added preset for VisualStudio Diff Tool. PR #3034
* Polish translation and dictionary added
* Czech translation added
* Disabled "Traditional Chinese" translation
* Romanian dictionary added. PR #2979



* Added an option to remember the ignore-white-spaces preference for all the diff viewers
* Option to check for release candidate versions
* Make dictionary setting configurable for each repository separately
* Use complete name of the merged/rebased branch to avoid conflict with a remote branch (if named the same).
* Remember the IgnoreWhitespaceChanges settings for FileViewer. PR #2844
* Specify Git and PuTTY locations with environment variables. PR #2367
* Tags in branch list are visible when "Local" is selected. PR #2543, #2545
* Follow only exact renames setting added. PR #2627
* Update preview list of ignored files in background. PR #2557
* Statistics plugin improvements. PR #2707
* DOS Codepage 852 added into supported encodings. PR #2913
* {WorkingDir} Parameter for Scripts. PR #2914
* Multiple tfs build defs. PR #2916
* Show commit SHA1 in log. PR #2659
* Allow auto-normalisation of branch name. PR #3233
* Using Common Item Dialog to select folders. PR #2788
* Ability to run scripts with several selected commits: {sHashes} argument. PR #2578
* Use the repository URL from the clipboard if available. PR #2586
* Enter/Return in file tree acts as double click. PR #2785
* Add script events: BeforeCheckout, AfterCheckout. PR #3211
* Clarified wording for Pull dropdown menu, now matches tooltips. PR #2830
* Manage new format to detect detached HEAD (Git >v2.4). PR #3010
* Support combined diff for merge commit
* Per repository plugins settings
* TeamCity: Add an option to try to display build report logged as a guest. PR #3224
* Add a popup to easy finding a TeamCity build. PR #3241
* Improve fixup commits. PR #3264
* Some changes around .gitignore. PR #3283
* Search graph row index by commit hash using dictionary. PR #3295
* FormChooseCommit: Add links helper to find parent(s) of current selected commit. PR #3246
* --first-parent filtration added, --full-history fixed, hotkeys exposed, context menu in file history window unification. PR #3250
* Added setting to enable/disable autocompletion in commit dialog. PR #2799
* Menu entries for improved accessibility. PR #3234
* Fixed an intermittent bug where ObjectDisposedException occurs on launch
* Fixed a bug where branch filter throws null reference exception when no repository selected
* Fixed issue #2977, #2566, #2712, #2972, #2959, #2958, #2904: Fixes for mono build
* Fixed issue #3093: Fix height calculation for RevisionHeader in the CommitInfo panel on Linux
* Fixed issue #3094: Change the default application font for Linux
* Fixed issue #3267: Linux aware paths
* Fixed issue #3100: Don't show Putty toolstrip menu item when not running on Windows
* Fixed issue #2769: GitExtensions slow with many Submodules
* Fixed issue #3207: Checkout branch dialog is too narrow
* Fixed issue #3069: "Unsupported commit message encoding" in conemu repo
* Fixed issue #3274: Honour the AutoSetupMerge git config
* Fixed issue #2924: "Illegal Characters in Path" if %PATH% Contains Quotes
* Fixed issue #3262: Fix p4merge diff settings
* Fixed issue #2759: On Diff/File List pane got exception ArgumentOutOfRangeException InvalidArgument Value of -1 is not valid for index
* Fixed issue #3297: Unable to use '/' or '.' in branch name when creating a new branch
* Fixed issue #3271: No need to close the "create branch" dialog if failed to create
* Fixed issue #3054: Changes to global gitignore do not properly propagate to the Commit button
* Fixed issue #573: Push Multiple Branches hang if using OpenSSH for key mgmt
* Fixed issue #3079: Hidden expandable column between message and author
* Fixed issue #2813: Bisect labels size problems on French version
* Fixed issue #2965: Config Settings written with upper case True and False
* Fixed issue #2292: Don't auto-remember the desired action 'Local changes' when checking out a branch
* Fixed issue #3136: No option to pull request if host is stash/bit bucket but repo (or project) contains
* Fixed issue #3231: Shouldn't popup the "not on a branch" warning when editing a commit during rebasing
* Fixed issue #3063: Application crash caused by hotkey CTRL+P
* Fixed issue #3221: Prevent crash due to invalid Build CI project name regex saved
* Fixed issue #3014: Ignore web browser script error for TeamCity and GitHub
* Fixed issue #2654: Jenkins plugin: Incorrectly detects Internet Explorer 'Document Mode'
* Fixed issue #3055: "Don't set as default" option in Pull menu is ignored for three of the five items in the Pull menu
* Fixed issue #3006: CryptographicException when attempting to open a repository
* Fixed issue #3111: Creating branch from empty repo leads to ArgumentOutOfRangeException
* Fixed issue #2790: Can not assign null value to UICommandsSource
* Fixed issue #3067: Diff for the root commit is broken in master branch
* Fixed issue #2993: Do not show password in plain text in Stash plugin options
* Fixed issue #2860: Unable to open 'periodic background fetch' settings
* Fixed issue #2954: Git Flow plugin: Fix some combo boxes overlap with labels
* Fixed issue #2614: Fix a problem with the directory name detected for certain url
* Fixed issue #2887: Jenkins integration not requesting credentials
* Fixed issue #3015: Mono: Unable to locate plugins folder
* Fixed issue #2902: Ampersand (&) in Revision Link URI breaks revision message
* Fixed issue #2846: Remember check-box states in "Cherry pick commit" dialog
* Fixed issue #2874: Fix infinite loop bug during undo with auto-wrap
* Fixed issue #2857: Fails to reset selected lines on new files
* Fixed issue #2840: Commit filter by number doesn't work
* Fixed issue #2700, #2822, #2854: Filtering by branch name or commit id doesn't work
* Fixed issue #2786: Clicking on the branch dropdown before selecting a repository triggers a NPE
* Fixed issue #2692: Intermittent error "cannot access a disposed object" on launch
* Fixed issue #2821: Additional hotkeys in Commit Dialog
* Fixed issue #2822: File history and blame dialogs are empty
* Fixed issue #2847: Application crashes when an empty string is pasted as a commit message
* Fixed issue #2739: Turn on Treat Warnings As Errors for all projects
* Fixed issue #2809: Jenkins plugin: Login with default credentials (single sign on)
* Fixed issue #2731: Move Microsoft.TeamFoundation.Client references from GAC to NuGet
* Fixed issue #2761: ArgumentOutOfRangeException is thrown when right clicking of the table header of 'Resolve merge conflicts' window
* Fixed issue #2154, #2645: Branches with comma in their name cannot be deleted
* Fixed issue #2686: ssh:// with port not working with Putty
* Fixed issue #2493: After Clone, GitExt shows "Starting a second message loop on a single thread is not a valid operation. Use Form.ShowDialog instead"
* Fixed issue #2694: typos in .gitignore template text
* Fixed issue #2453: Autocomplete Display-Bug
* Fixed issue #2488: AutoCRLF correction when copying a text from the FileViewer
* Fixed issue #2301: IME error with Spell checker
* Fixed issue #2617: Fixed calls of plink for host key caching with invalid urls
* Fixed issue #2473: Fixes in GitStatistics Plugin
* Fixed issue #2480: Wrong Gource URL and settings
* Fixed issue #2584: Cloning from Explorer in a drive root crashes GitExtensions
* Fixed issue #2597: [VS Plugin] Don't try highlight node when it is not found
* Fixed issue #2590: [VS Plugin] Allow some commands on all targets
* Fixed issue #2601, #2587, #2559, #2560: Fix issues with VS Plugin
* Fixed issue #2591: NullReferenceException in GitPlugin
* Fixed issue #2620: Fix a couple of exceptions thrown when processing is incorrectly done on error messages
* Fixed issue #2565: Fix for "init" command line command
* Fixed issue #2501: Fix for "fatal: Not a valid object name" when displaying a nonexistent blob
* Fixed issue #2440: Fixed parsing quoted printable for Author field
* Fixed issue #2422: Fix refresh issue with the branches filter textbox
* Fixed issue #2409: Display correctly windows end of line in git commit message

### Version 2.48.05 (16 May 2015)
* Fixed issue #2493: StartBrowseDialog failed after clone
* Fixed issue #2783: Fixed crash when right click on blank line in 'File Tree'
* Enter/Return in file tree acts as double click
* Support Git for Windows path for Linux tools

### Version 2.48.04 (8 May 2015)
* Fixed issue #1643: Do stage of 16506 files and GUI becomes Not Responding
* Fixed issue #2591: VSAddin solutionItem.ProjectItem == null when selected 'References' item in C# project
* Fixed issue #2587, #2601: VSAddin fixed StackOverflowException
* Fixed issue #2584: Escape the last backslash from paths before running GitExtensions to avoid escaping the double-quote
* Fixed issue #2574: MSysGit updated to version 1.9.5-preview20141217
* Fixed issue #2649: Refreshing the ignored files set every 10 minutes instead of every 500 milliseconds
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
* Support integration with TeamCity and Jenkins build server
* Support pull request for Atlassian Stash
* GitExt suggest update submodules after changing revision. PR #2176
* Show commit changes (i.e: -1+5) on Checkout Branch, CheckoutR revision, Create Branch and Create Tag dialogs
* Separate windows to merge submodules
* Increased performance and lowered memory footprint of DvcsGraph
* Allow Create branch in Commit dialog
* Added support for remote branches to the DeleteUnusedBranches plugin
* Revision grid will show superproject tags/branches/remote branches and conflict Base/Remote/Local
* Added Sublime Text 3 to editor list
* Added p4merge to the list of difftools
* Added BeyondCompare4 to the list of diff and merge tools
* Added SemanticMerge to the list of diff and merge tools
* Added hotkey to close repository via CTRL+W
* Open .git/config fixed
* "Back" button and history
* Disabled by default: include untracked files in stash
* Committer name added to commit dialog status bar. PR #1812
* Check ValidSvnWorkindDir before do svn commands. Method GitSvnCommandHelpers.ValidSvnWorkindDir work not correct on submodule repo
* Fixed undetected working directory in root directory (the additional "dir.rfind" in the while condition stopped the loop **before** e.g. "C:" has been reached)
* "Initialize repository" renamed to "Create repository"
* "working dir" and "working tree" renamed to "working directory" to simplify translation
* Prefer Putty from GitExtensions
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
* Added BackgroundFetch plugin in order to allow periodic fetching of all remotes automatically
* Putty updated to version beta 0.63 (released 2013-08-06)
* Display diff files list for each parent in separate collapsible group
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
* Close commit dialog when all changes are committed - now considers new file as a change
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
* Fixed issue #1585: IsBinaryAccordingToGitAttribute() rewritten using "git check-attr"
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
* Fixed issue #1428: Uncheck "Amend Commit" checkbox after committing
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
* Fixed issue #1173: Integrated NBug
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
* Fixed issue #1128: Removed buttons from Visual Studio Xml Editor toolbar
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
* Fixed issue 951: install Git Extensions into 'All Programs' instead of 'All Programs\Git Extensions'
* Fixed issue 954: improve RSS feed deletion
* Fixed issue 955: GitHub plugin fixed for GitHub api 3
* Fixed issue 965: integrated text editor usability improvements
* Fixed issue 995: support github-windows and git URL link protocol
* Fixed issue 1000: added option to sign-off commits
* Added French translation
* Separate commit button status icon if dirty only submodules
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
* Fixed issue: merge conflict dialog crash when "Diff-Scripts" folder not exist
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
* Added support for staging/unstaging files with non-ASCII characters
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
* German transaction updated
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
* Fixed issue: remove '.git' from target directory if the original repository ends with '.git'
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
* Fixed issue 318: when pushing new branch, track it automatically
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
* Fixed bug: installing MSysGit or KDiff3 using complete setup doesn't work when UAC is enabled
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
* Fixed bug: exception when deleting repository form dashboard using dashboardeditor
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
* Fixed issue 131: Add a blame function (commandline: GitExtensions blame [filename])
* Fixed issue 135: settings not saved when closing application
* Gravatars are not longer stored in the IsolatedStorage, but use the ApplicationData path
* Default windows font is used instead of Segoe UI
* Add search file function to file tree in browse dialog (ctrl+f)

### Version 1.98
* Fixed issue 105: Allow to open "gitex browse" with a given filter.
* Fixed issue 106: Show all branches which "contain" a given commit in their history.
* Fixed issue 107: Alt+f4 and other function keys not working when revision graph has focus.
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
* Added the ability to move to the prev/next quicksearch string by hitting alt+arrowup/alt+arrowdown
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
* The scroll position in file viewer is saved when switching revision.
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
* Delete commit message after successful commit.
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
* Added progress dialog to "Check for updates" plugin

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
* Fixed a bug in default .gitignore file, it would show the [Db]ebug directory. It should be [Dd]ebug directory of course.
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
* Fixed crash when loading some repositories (git://git.kernel.org/pub/scm/git/git.git)

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
* Better Visual Studio integration.

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
* Small changes to improve usability

### Version 1.30
* Added support for custom mergetools
* Fixed settings for git 1.6.1.xxx
* Improved patch and rebase features
* Removed a lot of annoying mergeconflict popups
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
* OpenSSH passphrase are not needed anymore.
* PuTTY private keys can be configured per remote, so key is automatically loaded.

### Version 1.14
* Improved rebase features a bit.
* Minor bug fixes.

### Version 1.13
* I'm still focusing on the push and pull features, because I use this a lot myself.
* Improved auto-settings-correct features
* Added rebase features
* Improved merge conflict handling a bit.

### Version 1.12
* Fixed lots of remote feature mistakes and added some missing features.
* Push/pull/fetch should work as supposed to.
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
* Blame function added to file history
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

* For this version I also added a non-installer version. This is just a zip file that contains the binairy files.
* Please note that this is just the standalone application without shell extensions!

### Version 0.91
* Rewritten most of commit logic. This works better now.
* Colors added on tag/branch/stash labels
* I also added a directory history on open/push/pull/clone, just to increase usabillity

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
* VS2008 doesn't crash on errors anymore (which was very annoying!)

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




[Version 2.50]:https://github.com/gitextensions/gitextensions/releases/tag/v2.50
[3777]:https://github.com/gitextensions/gitextensions/issues/3777
[3767]:https://github.com/gitextensions/gitextensions/issues/3767
[3763]:https://github.com/gitextensions/gitextensions/issues/3763
[3756]:https://github.com/gitextensions/gitextensions/issues/3756
[3746]:https://github.com/gitextensions/gitextensions/pull/3746
[3729]:https://github.com/gitextensions/gitextensions/issues/3729
[3719]:https://github.com/gitextensions/gitextensions/issues/3719
[3707]:https://github.com/gitextensions/gitextensions/issues/3707
[3691]:https://github.com/gitextensions/gitextensions/issues/3691
[3644]:https://github.com/gitextensions/gitextensions/pull/3644
[3616]:https://github.com/gitextensions/gitextensions/issues/3616
[3598]:https://github.com/gitextensions/gitextensions/pull/3598
[3594]:https://github.com/gitextensions/gitextensions/issues/3594
[3592]:https://github.com/gitextensions/gitextensions/issues/3592
[3590]:https://github.com/gitextensions/gitextensions/issues/3590
[3583]:https://github.com/gitextensions/gitextensions/issues/3583
[3565]:https://github.com/gitextensions/gitextensions/issues/3565
[3564]:https://github.com/gitextensions/gitextensions/issues/3564
[3534]:https://github.com/gitextensions/gitextensions/issues/3534
[3532]:https://github.com/gitextensions/gitextensions/issues/3532
[3515]:https://github.com/gitextensions/gitextensions/issues/3515
[3510]:https://github.com/gitextensions/gitextensions/issues/3510
[3503]:https://github.com/gitextensions/gitextensions/issues/3503
[3490]:https://github.com/gitextensions/gitextensions/issues/3490
[3467]:https://github.com/gitextensions/gitextensions/issues/3467
[3456]:https://github.com/gitextensions/gitextensions/issues/3456
[3450]:https://github.com/gitextensions/gitextensions/issues/3450
[3438]:https://github.com/gitextensions/gitextensions/issues/3438
[3432]:https://github.com/gitextensions/gitextensions/issues/3432
[3426]:https://github.com/gitextensions/gitextensions/pull/3426
[3424]:https://github.com/gitextensions/gitextensions/issues/3424
[3423]:https://github.com/gitextensions/gitextensions/issues/3423
[3393]:https://github.com/gitextensions/gitextensions/issues/3393
[3392]:https://github.com/gitextensions/gitextensions/issues/3392
[3383]:https://github.com/gitextensions/gitextensions/issues/3383
[3334]:https://github.com/gitextensions/gitextensions/issues/3334
[3293]:https://github.com/gitextensions/gitextensions/issues/3293
[3248]:https://github.com/gitextensions/gitextensions/issues/3248
[3245]:https://github.com/gitextensions/gitextensions/pull/3245
[3219]:https://github.com/gitextensions/gitextensions/pull/3219
[3218]:https://github.com/gitextensions/gitextensions/issues/3218
[3208]:https://github.com/gitextensions/gitextensions/issues/3208
[3192]:https://github.com/gitextensions/gitextensions/issues/3192
[3177]:https://github.com/gitextensions/gitextensions/issues/3177
[3166]:https://github.com/gitextensions/gitextensions/issues/3166
[3162]:https://github.com/gitextensions/gitextensions/issues/3162
[3159]:https://github.com/gitextensions/gitextensions/issues/3159
[3154]:https://github.com/gitextensions/gitextensions/issues/3154
[3127]:https://github.com/gitextensions/gitextensions/issues/3127
[3119]:https://github.com/gitextensions/gitextensions/issues/3119
[3073]:https://github.com/gitextensions/gitextensions/issues/3073
[3000]:https://github.com/gitextensions/gitextensions/pull/3000
[2997]:https://github.com/gitextensions/gitextensions/pull/2997
[2956]:https://github.com/gitextensions/gitextensions/issues/2956
[2926]:https://github.com/gitextensions/gitextensions/issues/2926
[2917]:https://github.com/gitextensions/gitextensions/pull/2917
[2839]:https://github.com/gitextensions/gitextensions/issues/2839
[2802]:https://github.com/gitextensions/gitextensions/pull/2802
[2768]:https://github.com/gitextensions/gitextensions/issues/2768
[2759]:https://github.com/gitextensions/gitextensions/issues/2759
[2714]:https://github.com/gitextensions/gitextensions/issues/2714
[2706]:https://github.com/gitextensions/gitextensions/issues/2706
[2689]:https://github.com/gitextensions/gitextensions/pull/2689
[2688]:https://github.com/gitextensions/gitextensions/issues/2688
[2675]:https://github.com/gitextensions/gitextensions/issues/2675
[2670]:https://github.com/gitextensions/gitextensions/issues/2670
[2550]:https://github.com/gitextensions/gitextensions/issues/2550
[2530]:https://github.com/gitextensions/gitextensions/issues/2530
[2460]:https://github.com/gitextensions/gitextensions/issues/2460
[2455]:https://github.com/gitextensions/gitextensions/issues/2455
[2446]:https://github.com/gitextensions/gitextensions/issues/2446
[2301]:https://github.com/gitextensions/gitextensions/issues/2301
[2194]:https://github.com/gitextensions/gitextensions/issues/2194
[2141]:https://github.com/gitextensions/gitextensions/issues/2141
[2016]:https://github.com/gitextensions/gitextensions/issues/2016
[1982]:https://github.com/gitextensions/gitextensions/issues/1982
[1975]:https://github.com/gitextensions/gitextensions/issues/1975
[1845]:https://github.com/gitextensions/gitextensions/issues/1845
[1064]:https://github.com/gitextensions/gitextensions/issues/1064

[3761]:https://github.com/gitextensions/gitextensions/issues/3761
[3755]:https://github.com/gitextensions/gitextensions/issues/3755
[3762]:https://github.com/gitextensions/gitextensions/pull/3762

[Version 2.50.01]:https://github.com/gitextensions/gitextensions/releases/tag/v2.50.01
[3821]:https://github.com/gitextensions/gitextensions/issues/3821
[3819]:https://github.com/gitextensions/gitextensions/issues/3819
[3814]:https://github.com/gitextensions/gitextensions/issues/3814
[3809]:https://github.com/gitextensions/gitextensions/issues/3809
[3806]:https://github.com/gitextensions/gitextensions/issues/3806
[3804]:https://github.com/gitextensions/gitextensions/issues/3804
[3786]:https://github.com/gitextensions/gitextensions/issues/3786


[Version 2.50.02]:https://github.com/gitextensions/gitextensions/releases/tag/v2.50.02
[3969]:https://github.com/gitextensions/gitextensions/issues/3969
[3902]:https://github.com/gitextensions/gitextensions/issues/3902
[3879]:https://github.com/gitextensions/gitextensions/issues/3879
[3864]:https://github.com/gitextensions/gitextensions/issues/3864
[3862]:https://github.com/gitextensions/gitextensions/issues/3862
[3861]:https://github.com/gitextensions/gitextensions/issues/3861
[3855]:https://github.com/gitextensions/gitextensions/issues/3855
[3852]:https://github.com/gitextensions/gitextensions/pull/3852
[3845]:https://github.com/gitextensions/gitextensions/issues/3845
[3844]:https://github.com/gitextensions/gitextensions/issues/3844
[3829]:https://github.com/gitextensions/gitextensions/issues/3829
[3827]:https://github.com/gitextensions/gitextensions/issues/3827
[3800]:https://github.com/gitextensions/gitextensions/issues/3800
[3794]:https://github.com/gitextensions/gitextensions/issues/3794
[3489]:https://github.com/gitextensions/gitextensions/issues/3489
[3011]:https://github.com/gitextensions/gitextensions/issues/3011


[Version 2.51.RC1]:https://github.com/gitextensions/gitextensions/releases/tag/v2.51.RC1
[4292]:https://github.com/gitextensions/gitextensions/issues/4292
[4283]:https://github.com/gitextensions/gitextensions/issues/4283
[4276]:https://github.com/gitextensions/gitextensions/issues/4276
[4264]:https://github.com/gitextensions/gitextensions/issues/4264
[4263]:https://github.com/gitextensions/gitextensions/issues/4263
[4258]:https://github.com/gitextensions/gitextensions/issues/4258
[4255]:https://github.com/gitextensions/gitextensions/issues/4255
[4250]:https://github.com/gitextensions/gitextensions/issues/4250
[4246]:https://github.com/gitextensions/gitextensions/pull/4246
[4242]:https://github.com/gitextensions/gitextensions/issues/4242
[4241]:https://github.com/gitextensions/gitextensions/pull/4241
[4237]:https://github.com/gitextensions/gitextensions/issues/4237
[4235]:https://github.com/gitextensions/gitextensions/issues/4235
[4233]:https://github.com/gitextensions/gitextensions/issues/4233
[4228]:https://github.com/gitextensions/gitextensions/pull/4228
[4227]:https://github.com/gitextensions/gitextensions/pull/4227
[4218]:https://github.com/gitextensions/gitextensions/pull/4218
[4213]:https://github.com/gitextensions/gitextensions/issues/4213
[4211]:https://github.com/gitextensions/gitextensions/pull/4211
[4210]:https://github.com/gitextensions/gitextensions/pull/4210
[4207]:https://github.com/gitextensions/gitextensions/pull/4207
[4204]:https://github.com/gitextensions/gitextensions/issues/4204
[4197]:https://github.com/gitextensions/gitextensions/pull/4197
[4196]:https://github.com/gitextensions/gitextensions/issues/4196
[4189]:https://github.com/gitextensions/gitextensions/pull/4189
[4188]:https://github.com/gitextensions/gitextensions/pull/4188
[4185]:https://github.com/gitextensions/gitextensions/issues/4185
[4168]:https://github.com/gitextensions/gitextensions/pull/4168
[4167]:https://github.com/gitextensions/gitextensions/pull/4167
[4166]:https://github.com/gitextensions/gitextensions/pull/4166
[4165]:https://github.com/gitextensions/gitextensions/pull/4165
[4163]:https://github.com/gitextensions/gitextensions/pull/4163
[4160]:https://github.com/gitextensions/gitextensions/issues/4160
[4157]:https://github.com/gitextensions/gitextensions/pull/4157
[4147]:https://github.com/gitextensions/gitextensions/pull/4147
[4132]:https://github.com/gitextensions/gitextensions/issues/4132
[4130]:https://github.com/gitextensions/gitextensions/issues/4130
[4126]:https://github.com/gitextensions/gitextensions/issues/4126
[4120]:https://github.com/gitextensions/gitextensions/issues/4120
[4115]:https://github.com/gitextensions/gitextensions/pull/4115
[4109]:https://github.com/gitextensions/gitextensions/pull/4109
[4105]:https://github.com/gitextensions/gitextensions/pull/4105
[4098]:https://github.com/gitextensions/gitextensions/issues/4098
[4096]:https://github.com/gitextensions/gitextensions/pull/4096
[4093]:https://github.com/gitextensions/gitextensions/pull/4093
[4092]:https://github.com/gitextensions/gitextensions/pull/4092
[4088]:https://github.com/gitextensions/gitextensions/pull/4088
[4087]:https://github.com/gitextensions/gitextensions/pull/4087
[4086]:https://github.com/gitextensions/gitextensions/pull/4086
[4085]:https://github.com/gitextensions/gitextensions/pull/4085
[4084]:https://github.com/gitextensions/gitextensions/pull/4084
[4079]:https://github.com/gitextensions/gitextensions/pull/4079
[4076]:https://github.com/gitextensions/gitextensions/pull/4076
[4075]:https://github.com/gitextensions/gitextensions/pull/4075
[4074]:https://github.com/gitextensions/gitextensions/pull/4074
[4062]:https://github.com/gitextensions/gitextensions/issues/4062
[4057]:https://github.com/gitextensions/gitextensions/issues/4057
[4052]:https://github.com/gitextensions/gitextensions/pull/4052
[4049]:https://github.com/gitextensions/gitextensions/issues/4049
[4031]:https://github.com/gitextensions/gitextensions/issues/4031
[4028]:https://github.com/gitextensions/gitextensions/issues/4028
[4025]:https://github.com/gitextensions/gitextensions/issues/4025
[4024]:https://github.com/gitextensions/gitextensions/issues/4024
[4022]:https://github.com/gitextensions/gitextensions/pull/4022
[4020]:https://github.com/gitextensions/gitextensions/pull/4020
[4019]:https://github.com/gitextensions/gitextensions/pull/4019
[4016]:https://github.com/gitextensions/gitextensions/issues/4016
[4014]:https://github.com/gitextensions/gitextensions/issues/4014
[4012]:https://github.com/gitextensions/gitextensions/issues/4012
[4008]:https://github.com/gitextensions/gitextensions/pull/4008
[4006]:https://github.com/gitextensions/gitextensions/issues/4006
[3999]:https://github.com/gitextensions/gitextensions/issues/3999
[3990]:https://github.com/gitextensions/gitextensions/issues/3990
[3982]:https://github.com/gitextensions/gitextensions/issues/3982
[3970]:https://github.com/gitextensions/gitextensions/issues/3970
[3966]:https://github.com/gitextensions/gitextensions/issues/3966
[3962]:https://github.com/gitextensions/gitextensions/issues/3962
[3959]:https://github.com/gitextensions/gitextensions/issues/3959
[3955]:https://github.com/gitextensions/gitextensions/pull/3955
[3953]:https://github.com/gitextensions/gitextensions/pull/3953
[3948]:https://github.com/gitextensions/gitextensions/issues/3948
[3947]:https://github.com/gitextensions/gitextensions/issues/3947
[3930]:https://github.com/gitextensions/gitextensions/pull/3930
[3921]:https://github.com/gitextensions/gitextensions/pull/3921
[3920]:https://github.com/gitextensions/gitextensions/pull/3920
[3919]:https://github.com/gitextensions/gitextensions/pull/3919
[3915]:https://github.com/gitextensions/gitextensions/issues/3915
[3908]:https://github.com/gitextensions/gitextensions/issues/3908
[3907]:https://github.com/gitextensions/gitextensions/issues/3907
[3900]:https://github.com/gitextensions/gitextensions/pull/3900
[3899]:https://github.com/gitextensions/gitextensions/issues/3899
[3897]:https://github.com/gitextensions/gitextensions/issues/3897
[3892]:https://github.com/gitextensions/gitextensions/issues/3892
[3887]:https://github.com/gitextensions/gitextensions/issues/3887
[3875]:https://github.com/gitextensions/gitextensions/issues/3875
[3867]:https://github.com/gitextensions/gitextensions/issues/3867
[3860]:https://github.com/gitextensions/gitextensions/issues/3860
[3849]:https://github.com/gitextensions/gitextensions/issues/3849
[3842]:https://github.com/gitextensions/gitextensions/issues/3842
[3839]:https://github.com/gitextensions/gitextensions/issues/3839
[3832]:https://github.com/gitextensions/gitextensions/issues/3832
[3822]:https://github.com/gitextensions/gitextensions/issues/3822
[3810]:https://github.com/gitextensions/gitextensions/issues/3810
[3772]:https://github.com/gitextensions/gitextensions/pull/3772
[3760]:https://github.com/gitextensions/gitextensions/issues/3760
[3759]:https://github.com/gitextensions/gitextensions/issues/3759
[3733]:https://github.com/gitextensions/gitextensions/pull/3733
[3730]:https://github.com/gitextensions/gitextensions/issues/3730
[3725]:https://github.com/gitextensions/gitextensions/issues/3725
[3709]:https://github.com/gitextensions/gitextensions/issues/3709
[3652]:https://github.com/gitextensions/gitextensions/issues/3652
[3525]:https://github.com/gitextensions/gitextensions/issues/3525
[3484]:https://github.com/gitextensions/gitextensions/pull/3484
[3161]:https://github.com/gitextensions/gitextensions/issues/3161
[2870]:https://github.com/gitextensions/gitextensions/issues/2870
[2868]:https://github.com/gitextensions/gitextensions/issues/2868
[2839]:https://github.com/gitextensions/gitextensions/issues/2839
[2495]:https://github.com/gitextensions/gitextensions/pull/2495
[1608]:https://github.com/gitextensions/gitextensions/issues/1608
[1605]:https://github.com/gitextensions/gitextensions/issues/1605
[1583]:https://github.com/gitextensions/gitextensions/issues/1583
[1307]:https://github.com/gitextensions/gitextensions/issues/1307

[Version 2.51.RC2]:https://github.com/gitextensions/gitextensions/releases/tag/v2.51.RC2
[4351]:https://github.com/gitextensions/gitextensions/pull/4351
[4349]:https://github.com/gitextensions/gitextensions/issues/4349
[4345]:https://github.com/gitextensions/gitextensions/issues/4345
[4344]:https://github.com/gitextensions/gitextensions/pull/4344
[4343]:https://github.com/gitextensions/gitextensions/pull/4343
[4340]:https://github.com/gitextensions/gitextensions/pull/4340
[4331]:https://github.com/gitextensions/gitextensions/pull/4331
[4330]:https://github.com/gitextensions/gitextensions/pull/4330
[4322]:https://github.com/gitextensions/gitextensions/issues/4322
[4321]:https://github.com/gitextensions/gitextensions/pull/4321
[4320]:https://github.com/gitextensions/gitextensions/pull/4320
[4319]:https://github.com/gitextensions/gitextensions/issues/4319
[4318]:https://github.com/gitextensions/gitextensions/pull/4318
[4316]:https://github.com/gitextensions/gitextensions/issues/4316
[4315]:https://github.com/gitextensions/gitextensions/issues/4315
[4301]:https://github.com/gitextensions/gitextensions/issues/4301
[4296]:https://github.com/gitextensions/gitextensions/issues/4296
[4295]:https://github.com/gitextensions/gitextensions/issues/4295
[4293]:https://github.com/gitextensions/gitextensions/pull/4293
[4209]:https://github.com/gitextensions/gitextensions/pull/4209
[4202]:https://github.com/gitextensions/gitextensions/issues/4202
[4058]:https://github.com/gitextensions/gitextensions/issues/4058
[4031]:https://github.com/gitextensions/gitextensions/issues/4031

[Version 2.51.00]:https://github.com/gitextensions/gitextensions/releases/tag/v2.51
[4386]:https://github.com/gitextensions/gitextensions/pull/4386
[4374]:https://github.com/gitextensions/gitextensions/issues/4374
[4373]:https://github.com/gitextensions/gitextensions/issues/4373
[4370]:https://github.com/gitextensions/gitextensions/issues/4370

[Version 2.51.01]:https://github.com/gitextensions/gitextensions/releases/tag/v2.51.01
[4580]:https://github.com/gitextensions/gitextensions/issues/4580
[4570]:https://github.com/gitextensions/gitextensions/issues/4570
[4546]:https://github.com/gitextensions/gitextensions/issues/4546
[4535]:https://github.com/gitextensions/gitextensions/issues/4535
[4523]:https://github.com/gitextensions/gitextensions/issues/4523
[4510]:https://github.com/gitextensions/gitextensions/issues/4510
[4509]:https://github.com/gitextensions/gitextensions/issues/4509
[4495]:https://github.com/gitextensions/gitextensions/issues/4495
[4488]:https://github.com/gitextensions/gitextensions/issues/4488
[4485]:https://github.com/gitextensions/gitextensions/issues/4485
[4464]:https://github.com/gitextensions/gitextensions/issues/4464
[4449]:https://github.com/gitextensions/gitextensions/issues/4449
[4443]:https://github.com/gitextensions/gitextensions/issues/4443
[4441]:https://github.com/gitextensions/gitextensions/issues/4441
[4412]:https://github.com/gitextensions/gitextensions/pull/4412
[4380]:https://github.com/gitextensions/gitextensions/issues/4380
[4361]:https://github.com/gitextensions/gitextensions/issues/4361
[4354]:https://github.com/gitextensions/gitextensions/pull/4354
[3935]:https://github.com/gitextensions/gitextensions/issues/3935
[3501]:https://github.com/gitextensions/gitextensions/issues/3501
[2838]:https://github.com/gitextensions/gitextensions/issues/2838
[2515]:https://github.com/gitextensions/gitextensions/issues/2515
[2313]:https://github.com/gitextensions/gitextensions/issues/2313
[1858]:https://github.com/gitextensions/gitextensions/issues/1858

[Version 2.51.02]:https://github.com/gitextensions/gitextensions/releases/tag/v2.51.02
[4975]:https://github.com/gitextensions/gitextensions/issues/4975
[4956]:https://github.com/gitextensions/gitextensions/issues/4956
[4933]:https://github.com/gitextensions/gitextensions/pull/4933
[4932]:https://github.com/gitextensions/gitextensions/pull/4932
[4929]:https://github.com/gitextensions/gitextensions/pull/4929
[4907]:https://github.com/gitextensions/gitextensions/issues/4907
[4862]:https://github.com/gitextensions/gitextensions/issues/4862
[4855]:https://github.com/gitextensions/gitextensions/issues/4855
[4792]:https://github.com/gitextensions/gitextensions/pull/4792
[4780]:https://github.com/gitextensions/gitextensions/pull/4780
[4779]:https://github.com/gitextensions/gitextensions/pull/4779
[4776]:https://github.com/gitextensions/gitextensions/issues/4776
[4711]:https://github.com/gitextensions/gitextensions/pull/4711
[4515]:https://github.com/gitextensions/gitextensions/issues/4515
[4392]:https://github.com/gitextensions/gitextensions/issues/4392
[4358]:https://github.com/gitextensions/gitextensions/issues/4358
[4174]:https://github.com/gitextensions/gitextensions/issues/4174
[4099]:https://github.com/gitextensions/gitextensions/issues/4099
[3828]:https://github.com/gitextensions/gitextensions/issues/3828


[Version 2.51.03]:https://github.com/gitextensions/gitextensions/releases/tag/v2.51.03
[5107]:https://github.com/gitextensions/gitextensions/issues/5107
[5095]:https://github.com/gitextensions/gitextensions/pull/5095
[5066]:https://github.com/gitextensions/gitextensions/issues/5066
[5065]:https://github.com/gitextensions/gitextensions/issues/5065
[5021]:https://github.com/gitextensions/gitextensions/issues/5021
[5005]:https://github.com/gitextensions/gitextensions/issues/5005
[4991]:https://github.com/gitextensions/gitextensions/issues/4991
[4549]:https://github.com/gitextensions/gitextensions/issues/4549
[4483]:https://github.com/gitextensions/gitextensions/issues/4483
[2507]:https://github.com/gitextensions/gitextensions/issues/2507


[Version 2.51.04]:https://github.com/gitextensions/gitextensions/releases/tag/v2.51.04
[5127]:https://github.com/gitextensions/gitextensions/issues/5127
[5119]:https://github.com/gitextensions/gitextensions/issues/5119


[Version 2.51.05]:https://github.com/gitextensions/gitextensions/releases/tag/v2.51.05
[5311]:https://github.com/gitextensions/gitextensions/issues/5311
[5187]:https://github.com/gitextensions/gitextensions/issues/5187
[5179]:https://github.com/gitextensions/gitextensions/issues/5179
[4978]:https://github.com/gitextensions/gitextensions/issues/4978


[Version 3.00.00]:https://github.com/gitextensions/gitextensions/releases/tag/v3.00.00
[5853]:https://github.com/gitextensions/gitextensions/issues/5853
[5821]:https://github.com/gitextensions/gitextensions/pull/5821
[5820]:https://github.com/gitextensions/gitextensions/pull/5820
[5819]:https://github.com/gitextensions/gitextensions/issues/5819
[5816]:https://github.com/gitextensions/gitextensions/pull/5816
[5808]:https://github.com/gitextensions/gitextensions/pull/5808
[5803]:https://github.com/gitextensions/gitextensions/pull/5803
[5775]:https://github.com/gitextensions/gitextensions/pull/5775
[5773]:https://github.com/gitextensions/gitextensions/issues/5773
[5766]:https://github.com/gitextensions/gitextensions/issues/5766
[5764]:https://github.com/gitextensions/gitextensions/issues/5764
[5758]:https://github.com/gitextensions/gitextensions/issues/5758
[5748]:https://github.com/gitextensions/gitextensions/pull/5748
[5744]:https://github.com/gitextensions/gitextensions/issues/5744
[5740]:https://github.com/gitextensions/gitextensions/pull/5740
[5739]:https://github.com/gitextensions/gitextensions/pull/5739
[5738]:https://github.com/gitextensions/gitextensions/pull/5738
[5735]:https://github.com/gitextensions/gitextensions/issues/5735
[5734]:https://github.com/gitextensions/gitextensions/issues/5734
[5733]:https://github.com/gitextensions/gitextensions/issues/5733
[5732]:https://github.com/gitextensions/gitextensions/issues/5732
[5731]:https://github.com/gitextensions/gitextensions/issues/5731
[5727]:https://github.com/gitextensions/gitextensions/pull/5727
[5725]:https://github.com/gitextensions/gitextensions/pull/5725
[5724]:https://github.com/gitextensions/gitextensions/pull/5724
[5720]:https://github.com/gitextensions/gitextensions/pull/5720
[5707]:https://github.com/gitextensions/gitextensions/issues/5707
[5703]:https://github.com/gitextensions/gitextensions/issues/5703
[5698]:https://github.com/gitextensions/gitextensions/issues/5698
[5696]:https://github.com/gitextensions/gitextensions/pull/5696
[5695]:https://github.com/gitextensions/gitextensions/issues/5695
[5686]:https://github.com/gitextensions/gitextensions/pull/5686
[5684]:https://github.com/gitextensions/gitextensions/issues/5684
[5683]:https://github.com/gitextensions/gitextensions/issues/5683
[5677]:https://github.com/gitextensions/gitextensions/issues/5677
[5672]:https://github.com/gitextensions/gitextensions/issues/5672
[5671]:https://github.com/gitextensions/gitextensions/pull/5671
[5644]:https://github.com/gitextensions/gitextensions/issues/5644
[5642]:https://github.com/gitextensions/gitextensions/issues/5642
[5630]:https://github.com/gitextensions/gitextensions/issues/5630
[5629]:https://github.com/gitextensions/gitextensions/issues/5629
[5627]:https://github.com/gitextensions/gitextensions/issues/5627
[5625]:https://github.com/gitextensions/gitextensions/issues/5625
[5624]:https://github.com/gitextensions/gitextensions/issues/5624
[5623]:https://github.com/gitextensions/gitextensions/issues/5623
[5615]:https://github.com/gitextensions/gitextensions/issues/5615
[5611]:https://github.com/gitextensions/gitextensions/pull/5611
[5605]:https://github.com/gitextensions/gitextensions/issues/5605
[5597]:https://github.com/gitextensions/gitextensions/pull/5597
[5596]:https://github.com/gitextensions/gitextensions/pull/5596
[5594]:https://github.com/gitextensions/gitextensions/issues/5594
[5593]:https://github.com/gitextensions/gitextensions/issues/5593
[5581]:https://github.com/gitextensions/gitextensions/pull/5581
[5564]:https://github.com/gitextensions/gitextensions/issues/5564
[5562]:https://github.com/gitextensions/gitextensions/issues/5562
[5558]:https://github.com/gitextensions/gitextensions/issues/5558
[5557]:https://github.com/gitextensions/gitextensions/issues/5557
[5540]:https://github.com/gitextensions/gitextensions/pull/5540
[5537]:https://github.com/gitextensions/gitextensions/pull/5537
[5534]:https://github.com/gitextensions/gitextensions/pull/5534
[5533]:https://github.com/gitextensions/gitextensions/pull/5533
[5515]:https://github.com/gitextensions/gitextensions/issues/5515
[5514]:https://github.com/gitextensions/gitextensions/issues/5514
[5501]:https://github.com/gitextensions/gitextensions/issues/5501
[5500]:https://github.com/gitextensions/gitextensions/issues/5500
[5493]:https://github.com/gitextensions/gitextensions/issues/5493
[5488]:https://github.com/gitextensions/gitextensions/issues/5488
[5485]:https://github.com/gitextensions/gitextensions/issues/5485
[5479]:https://github.com/gitextensions/gitextensions/issues/5479
[5473]:https://github.com/gitextensions/gitextensions/pull/5473
[5469]:https://github.com/gitextensions/gitextensions/issues/5469
[5440]:https://github.com/gitextensions/gitextensions/issues/5440
[5439]:https://github.com/gitextensions/gitextensions/issues/5439
[5438]:https://github.com/gitextensions/gitextensions/issues/5438
[5437]:https://github.com/gitextensions/gitextensions/issues/5437
[5432]:https://github.com/gitextensions/gitextensions/issues/5432
[5427]:https://github.com/gitextensions/gitextensions/issues/5427
[5425]:https://github.com/gitextensions/gitextensions/pull/5425
[5420]:https://github.com/gitextensions/gitextensions/issues/5420
[5419]:https://github.com/gitextensions/gitextensions/issues/5419
[5413]:https://github.com/gitextensions/gitextensions/pull/5413
[5409]:https://github.com/gitextensions/gitextensions/issues/5409
[5408]:https://github.com/gitextensions/gitextensions/issues/5408
[5386]:https://github.com/gitextensions/gitextensions/issues/5386
[5380]:https://github.com/gitextensions/gitextensions/issues/5380
[5368]:https://github.com/gitextensions/gitextensions/issues/5368
[5359]:https://github.com/gitextensions/gitextensions/issues/5359
[5349]:https://github.com/gitextensions/gitextensions/pull/5349
[5348]:https://github.com/gitextensions/gitextensions/issues/5348
[5347]:https://github.com/gitextensions/gitextensions/issues/5347
[5343]:https://github.com/gitextensions/gitextensions/issues/5343
[5341]:https://github.com/gitextensions/gitextensions/issues/5341
[5337]:https://github.com/gitextensions/gitextensions/issues/5337
[5332]:https://github.com/gitextensions/gitextensions/issues/5332
[5330]:https://github.com/gitextensions/gitextensions/issues/5330
[5328]:https://github.com/gitextensions/gitextensions/issues/5328
[5327]:https://github.com/gitextensions/gitextensions/issues/5327
[5326]:https://github.com/gitextensions/gitextensions/issues/5326
[5322]:https://github.com/gitextensions/gitextensions/issues/5322
[5312]:https://github.com/gitextensions/gitextensions/pull/5312
[5300]:https://github.com/gitextensions/gitextensions/issues/5300
[5285]:https://github.com/gitextensions/gitextensions/pull/5285
[5250]:https://github.com/gitextensions/gitextensions/pull/5250
[5243]:https://github.com/gitextensions/gitextensions/issues/5243
[5230]:https://github.com/gitextensions/gitextensions/issues/5230
[5229]:https://github.com/gitextensions/gitextensions/pull/5229
[5218]:https://github.com/gitextensions/gitextensions/pull/5218
[5214]:https://github.com/gitextensions/gitextensions/pull/5214
[5207]:https://github.com/gitextensions/gitextensions/issues/5207
[5206]:https://github.com/gitextensions/gitextensions/issues/5206
[5198]:https://github.com/gitextensions/gitextensions/issues/5198
[5190]:https://github.com/gitextensions/gitextensions/issues/5190
[5187]:https://github.com/gitextensions/gitextensions/issues/5187
[5179]:https://github.com/gitextensions/gitextensions/issues/5179
[5175]:https://github.com/gitextensions/gitextensions/issues/5175
[5167]:https://github.com/gitextensions/gitextensions/issues/5167
[5166]:https://github.com/gitextensions/gitextensions/issues/5166
[5165]:https://github.com/gitextensions/gitextensions/issues/5165
[5157]:https://github.com/gitextensions/gitextensions/issues/5157
[5148]:https://github.com/gitextensions/gitextensions/issues/5148
[5143]:https://github.com/gitextensions/gitextensions/issues/5143
[5137]:https://github.com/gitextensions/gitextensions/pull/5137
[5134]:https://github.com/gitextensions/gitextensions/issues/5134
[5119]:https://github.com/gitextensions/gitextensions/issues/5119
[5117]:https://github.com/gitextensions/gitextensions/issues/5117
[5107]:https://github.com/gitextensions/gitextensions/issues/5107
[5106]:https://github.com/gitextensions/gitextensions/issues/5106
[5102]:https://github.com/gitextensions/gitextensions/pull/5102
[5087]:https://github.com/gitextensions/gitextensions/pull/5087
[5084]:https://github.com/gitextensions/gitextensions/issues/5084
[5077]:https://github.com/gitextensions/gitextensions/pull/5077
[5066]:https://github.com/gitextensions/gitextensions/issues/5066
[5065]:https://github.com/gitextensions/gitextensions/issues/5065
[5036]:https://github.com/gitextensions/gitextensions/issues/5036
[5032]:https://github.com/gitextensions/gitextensions/issues/5032
[5030]:https://github.com/gitextensions/gitextensions/issues/5030
[5026]:https://github.com/gitextensions/gitextensions/issues/5026
[5023]:https://github.com/gitextensions/gitextensions/issues/5023
[5021]:https://github.com/gitextensions/gitextensions/issues/5021
[5005]:https://github.com/gitextensions/gitextensions/issues/5005
[4998]:https://github.com/gitextensions/gitextensions/pull/4998
[4991]:https://github.com/gitextensions/gitextensions/issues/4991
[4989]:https://github.com/gitextensions/gitextensions/issues/4989
[4986]:https://github.com/gitextensions/gitextensions/pull/4986
[4978]:https://github.com/gitextensions/gitextensions/issues/4978
[4966]:https://github.com/gitextensions/gitextensions/issues/4966
[4964]:https://github.com/gitextensions/gitextensions/pull/4964
[4956]:https://github.com/gitextensions/gitextensions/issues/4956
[4949]:https://github.com/gitextensions/gitextensions/issues/4949
[4948]:https://github.com/gitextensions/gitextensions/issues/4948
[4947]:https://github.com/gitextensions/gitextensions/issues/4947
[4936]:https://github.com/gitextensions/gitextensions/pull/4936
[4930]:https://github.com/gitextensions/gitextensions/pull/4930
[4929]:https://github.com/gitextensions/gitextensions/pull/4929
[4926]:https://github.com/gitextensions/gitextensions/pull/4926
[4907]:https://github.com/gitextensions/gitextensions/issues/4907
[4902]:https://github.com/gitextensions/gitextensions/issues/4902
[4899]:https://github.com/gitextensions/gitextensions/pull/4899
[4898]:https://github.com/gitextensions/gitextensions/pull/4898
[4892]:https://github.com/gitextensions/gitextensions/issues/4892
[4886]:https://github.com/gitextensions/gitextensions/issues/4886
[4882]:https://github.com/gitextensions/gitextensions/pull/4882
[4878]:https://github.com/gitextensions/gitextensions/issues/4878
[4875]:https://github.com/gitextensions/gitextensions/issues/4875
[4871]:https://github.com/gitextensions/gitextensions/pull/4871
[4862]:https://github.com/gitextensions/gitextensions/issues/4862
[4855]:https://github.com/gitextensions/gitextensions/issues/4855
[4850]:https://github.com/gitextensions/gitextensions/pull/4850
[4832]:https://github.com/gitextensions/gitextensions/issues/4832
[4830]:https://github.com/gitextensions/gitextensions/issues/4830
[4829]:https://github.com/gitextensions/gitextensions/issues/4829
[4828]:https://github.com/gitextensions/gitextensions/issues/4828
[4827]:https://github.com/gitextensions/gitextensions/issues/4827
[4820]:https://github.com/gitextensions/gitextensions/issues/4820
[4817]:https://github.com/gitextensions/gitextensions/issues/4817
[4807]:https://github.com/gitextensions/gitextensions/pull/4807
[4805]:https://github.com/gitextensions/gitextensions/pull/4805
[4792]:https://github.com/gitextensions/gitextensions/pull/4792
[4790]:https://github.com/gitextensions/gitextensions/pull/4790
[4780]:https://github.com/gitextensions/gitextensions/pull/4780
[4779]:https://github.com/gitextensions/gitextensions/pull/4779
[4778]:https://github.com/gitextensions/gitextensions/issues/4778
[4777]:https://github.com/gitextensions/gitextensions/issues/4777
[4776]:https://github.com/gitextensions/gitextensions/issues/4776
[4770]:https://github.com/gitextensions/gitextensions/pull/4770
[4766]:https://github.com/gitextensions/gitextensions/pull/4766
[4739]:https://github.com/gitextensions/gitextensions/issues/4739
[4727]:https://github.com/gitextensions/gitextensions/pull/4727
[4719]:https://github.com/gitextensions/gitextensions/pull/4719
[4714]:https://github.com/gitextensions/gitextensions/pull/4714
[4711]:https://github.com/gitextensions/gitextensions/pull/4711
[4707]:https://github.com/gitextensions/gitextensions/pull/4707
[4706]:https://github.com/gitextensions/gitextensions/pull/4706
[4705]:https://github.com/gitextensions/gitextensions/pull/4705
[4697]:https://github.com/gitextensions/gitextensions/pull/4697
[4695]:https://github.com/gitextensions/gitextensions/pull/4695
[4690]:https://github.com/gitextensions/gitextensions/issues/4690
[4688]:https://github.com/gitextensions/gitextensions/pull/4688
[4687]:https://github.com/gitextensions/gitextensions/pull/4687
[4684]:https://github.com/gitextensions/gitextensions/issues/4684
[4682]:https://github.com/gitextensions/gitextensions/pull/4682
[4681]:https://github.com/gitextensions/gitextensions/pull/4681
[4680]:https://github.com/gitextensions/gitextensions/pull/4680
[4678]:https://github.com/gitextensions/gitextensions/pull/4678
[4677]:https://github.com/gitextensions/gitextensions/pull/4677
[4672]:https://github.com/gitextensions/gitextensions/pull/4672
[4670]:https://github.com/gitextensions/gitextensions/pull/4670
[4668]:https://github.com/gitextensions/gitextensions/pull/4668
[4664]:https://github.com/gitextensions/gitextensions/issues/4664
[4662]:https://github.com/gitextensions/gitextensions/pull/4662
[4657]:https://github.com/gitextensions/gitextensions/pull/4657
[4656]:https://github.com/gitextensions/gitextensions/pull/4656
[4655]:https://github.com/gitextensions/gitextensions/pull/4655
[4654]:https://github.com/gitextensions/gitextensions/pull/4654
[4645]:https://github.com/gitextensions/gitextensions/pull/4645
[4641]:https://github.com/gitextensions/gitextensions/pull/4641
[4640]:https://github.com/gitextensions/gitextensions/pull/4640
[4639]:https://github.com/gitextensions/gitextensions/pull/4639
[4638]:https://github.com/gitextensions/gitextensions/pull/4638
[4635]:https://github.com/gitextensions/gitextensions/pull/4635
[4629]:https://github.com/gitextensions/gitextensions/pull/4629
[4625]:https://github.com/gitextensions/gitextensions/pull/4625
[4592]:https://github.com/gitextensions/gitextensions/issues/4592
[4588]:https://github.com/gitextensions/gitextensions/issues/4588
[4580]:https://github.com/gitextensions/gitextensions/issues/4580
[4579]:https://github.com/gitextensions/gitextensions/issues/4579
[4570]:https://github.com/gitextensions/gitextensions/issues/4570
[4564]:https://github.com/gitextensions/gitextensions/issues/4564
[4561]:https://github.com/gitextensions/gitextensions/issues/4561
[4554]:https://github.com/gitextensions/gitextensions/pull/4554
[4549]:https://github.com/gitextensions/gitextensions/issues/4549
[4546]:https://github.com/gitextensions/gitextensions/issues/4546
[4542]:https://github.com/gitextensions/gitextensions/issues/4542
[4535]:https://github.com/gitextensions/gitextensions/issues/4535
[4523]:https://github.com/gitextensions/gitextensions/issues/4523
[4517]:https://github.com/gitextensions/gitextensions/issues/4517
[4516]:https://github.com/gitextensions/gitextensions/issues/4516
[4515]:https://github.com/gitextensions/gitextensions/issues/4515
[4511]:https://github.com/gitextensions/gitextensions/issues/4511
[4510]:https://github.com/gitextensions/gitextensions/issues/4510
[4509]:https://github.com/gitextensions/gitextensions/issues/4509
[4500]:https://github.com/gitextensions/gitextensions/issues/4500
[4495]:https://github.com/gitextensions/gitextensions/issues/4495
[4490]:https://github.com/gitextensions/gitextensions/issues/4490
[4488]:https://github.com/gitextensions/gitextensions/issues/4488
[4485]:https://github.com/gitextensions/gitextensions/issues/4485
[4483]:https://github.com/gitextensions/gitextensions/issues/4483
[4480]:https://github.com/gitextensions/gitextensions/pull/4480
[4475]:https://github.com/gitextensions/gitextensions/pull/4475
[4472]:https://github.com/gitextensions/gitextensions/issues/4472
[4464]:https://github.com/gitextensions/gitextensions/issues/4464
[4458]:https://github.com/gitextensions/gitextensions/issues/4458
[4453]:https://github.com/gitextensions/gitextensions/issues/4453
[4449]:https://github.com/gitextensions/gitextensions/issues/4449
[4443]:https://github.com/gitextensions/gitextensions/issues/4443
[4441]:https://github.com/gitextensions/gitextensions/issues/4441
[4428]:https://github.com/gitextensions/gitextensions/issues/4428
[4427]:https://github.com/gitextensions/gitextensions/issues/4427
[4426]:https://github.com/gitextensions/gitextensions/issues/4426
[4425]:https://github.com/gitextensions/gitextensions/pull/4425
[4422]:https://github.com/gitextensions/gitextensions/issues/4422
[4416]:https://github.com/gitextensions/gitextensions/pull/4416
[4412]:https://github.com/gitextensions/gitextensions/pull/4412
[4411]:https://github.com/gitextensions/gitextensions/pull/4411
[4407]:https://github.com/gitextensions/gitextensions/issues/4407
[4396]:https://github.com/gitextensions/gitextensions/issues/4396
[4392]:https://github.com/gitextensions/gitextensions/issues/4392
[4387]:https://github.com/gitextensions/gitextensions/issues/4387
[4381]:https://github.com/gitextensions/gitextensions/issues/4381
[4380]:https://github.com/gitextensions/gitextensions/issues/4380
[4370]:https://github.com/gitextensions/gitextensions/issues/4370
[4361]:https://github.com/gitextensions/gitextensions/issues/4361
[4359]:https://github.com/gitextensions/gitextensions/pull/4359
[4358]:https://github.com/gitextensions/gitextensions/issues/4358
[4354]:https://github.com/gitextensions/gitextensions/pull/4354
[4348]:https://github.com/gitextensions/gitextensions/issues/4348
[4334]:https://github.com/gitextensions/gitextensions/pull/4334
[4333]:https://github.com/gitextensions/gitextensions/pull/4333
[4327]:https://github.com/gitextensions/gitextensions/pull/4327
[4324]:https://github.com/gitextensions/gitextensions/pull/4324
[4312]:https://github.com/gitextensions/gitextensions/issues/4312
[4311]:https://github.com/gitextensions/gitextensions/issues/4311
[4310]:https://github.com/gitextensions/gitextensions/pull/4310
[4309]:https://github.com/gitextensions/gitextensions/pull/4309
[4308]:https://github.com/gitextensions/gitextensions/issues/4308
[4281]:https://github.com/gitextensions/gitextensions/issues/4281
[4280]:https://github.com/gitextensions/gitextensions/pull/4280
[4256]:https://github.com/gitextensions/gitextensions/issues/4256
[4247]:https://github.com/gitextensions/gitextensions/pull/4247
[4238]:https://github.com/gitextensions/gitextensions/issues/4238
[4231]:https://github.com/gitextensions/gitextensions/issues/4231
[4230]:https://github.com/gitextensions/gitextensions/issues/4230
[4205]:https://github.com/gitextensions/gitextensions/issues/4205
[4195]:https://github.com/gitextensions/gitextensions/issues/4195
[4190]:https://github.com/gitextensions/gitextensions/issues/4190
[4179]:https://github.com/gitextensions/gitextensions/issues/4179
[4174]:https://github.com/gitextensions/gitextensions/issues/4174
[4159]:https://github.com/gitextensions/gitextensions/issues/4159
[4114]:https://github.com/gitextensions/gitextensions/issues/4114
[4104]:https://github.com/gitextensions/gitextensions/issues/4104
[4099]:https://github.com/gitextensions/gitextensions/issues/4099
[4069]:https://github.com/gitextensions/gitextensions/issues/4069
[4066]:https://github.com/gitextensions/gitextensions/issues/4066
[4037]:https://github.com/gitextensions/gitextensions/issues/4037
[3978]:https://github.com/gitextensions/gitextensions/issues/3978
[3935]:https://github.com/gitextensions/gitextensions/issues/3935
[3932]:https://github.com/gitextensions/gitextensions/issues/3932
[3853]:https://github.com/gitextensions/gitextensions/issues/3853
[3828]:https://github.com/gitextensions/gitextensions/issues/3828
[3795]:https://github.com/gitextensions/gitextensions/issues/3795
[3779]:https://github.com/gitextensions/gitextensions/issues/3779
[3770]:https://github.com/gitextensions/gitextensions/issues/3770
[3732]:https://github.com/gitextensions/gitextensions/issues/3732
[3693]:https://github.com/gitextensions/gitextensions/pull/3693
[3639]:https://github.com/gitextensions/gitextensions/issues/3639
[3593]:https://github.com/gitextensions/gitextensions/issues/3593
[3531]:https://github.com/gitextensions/gitextensions/issues/3531
[3522]:https://github.com/gitextensions/gitextensions/issues/3522
[3511]:https://github.com/gitextensions/gitextensions/issues/3511
[3510]:https://github.com/gitextensions/gitextensions/issues/3510
[3501]:https://github.com/gitextensions/gitextensions/issues/3501
[3278]:https://github.com/gitextensions/gitextensions/issues/3278
[3220]:https://github.com/gitextensions/gitextensions/issues/3220
[3163]:https://github.com/gitextensions/gitextensions/issues/3163
[3072]:https://github.com/gitextensions/gitextensions/issues/3072
[3038]:https://github.com/gitextensions/gitextensions/pull/3038
[2938]:https://github.com/gitextensions/gitextensions/issues/2938
[2838]:https://github.com/gitextensions/gitextensions/issues/2838
[2515]:https://github.com/gitextensions/gitextensions/issues/2515
[2507]:https://github.com/gitextensions/gitextensions/issues/2507
[2342]:https://github.com/gitextensions/gitextensions/issues/2342
[2313]:https://github.com/gitextensions/gitextensions/issues/2313
[2272]:https://github.com/gitextensions/gitextensions/issues/2272
[2000]:https://github.com/gitextensions/gitextensions/issues/2000
[1931]:https://github.com/gitextensions/gitextensions/issues/1931
[1926]:https://github.com/gitextensions/gitextensions/issues/1926
[1871]:https://github.com/gitextensions/gitextensions/issues/1871
[1858]:https://github.com/gitextensions/gitextensions/issues/1858
[1511]:https://github.com/gitextensions/gitextensions/issues/1511
[1379]:https://github.com/gitextensions/gitextensions/issues/1379
[538]:https://github.com/gitextensions/gitextensions/issues/538
[84]:https://github.com/gitextensions/gitextensions/issues/84
