// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the
// Code Analysis results, point to "Suppress Message", and click
// "In Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1824:MarkAssembliesWithNeutralResourcesLanguage")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Westhuis", Scope = "member", Target = "GitUI.AboutBox.#InitializeComponent()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Henk", Scope = "member", Target = "GitUI.AboutBox.#InitializeComponent()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "henkwesthuis", Scope = "member", Target = "GitUI.AboutBox.#InitializeComponent()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ppk", Scope = "member", Target = "GitUI.BrowseForPrivateKey.#Browse(System.Windows.Forms.IWin32Window)")]
[assembly: SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "<Pending>", Scope = "member", Target = "~M:GitUI.BuildServerIntegration.BuildServerWatcher.ShowBuildServerCredentialsFormAsync(System.String,GitUIPluginInterfaces.BuildServerIntegration.IBuildServerCredentials)~System.Threading.Tasks.Task{GitUIPluginInterfaces.BuildServerIntegration.IBuildServerCredentials}")]
[assembly: SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "<Pending>", Scope = "member", Target = "~M:GitUI.CommandsDialogs.BrowseDialog.FormUpdates.Done")]
[assembly: SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "<Pending>", Scope = "member", Target = "~M:GitUI.CommandsDialogs.RepoHosting.CreatePullRequestForm.PopulateBranchesComboAndEnableCreateButton(GitExtensions.Extensibility.Plugins.IHostedRemote,System.Windows.Forms.ComboBox)")]
