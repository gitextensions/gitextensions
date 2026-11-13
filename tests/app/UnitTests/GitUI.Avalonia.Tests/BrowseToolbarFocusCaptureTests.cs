using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class BrowseToolbarFocusCaptureTests
{
    [AvaloniaTest]
    [TestCase("_NO_TRANSLATE_WorkingDir")]
    [TestCase("branchSelect")]
    [TestCase("toolStripButtonPull")]
    [TestCase("tsbtnAdvancedFilter")]
    [TestCase("tsddbtnBranchFilter")]
    [TestCase("tsddbtnRevisionFilter")]
    public void Tree_should_release_native_toolbar_owner_selection_while_its_real_flyout_is_open(string field)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using FormBrowse form = CreateForm();
        try
        {
            Control owner = form.GetLogicalDescendants().OfType<Control>().Single(control => control.Name == field);
            using (AvaloniaControlStateDriver hover = AvaloniaControlStateDriver.Apply(form,
                       new CaptureStatePlan { Id = "owner.hover", Kind = CaptureStateKind.Hover, TargetField = field }))
            {
                owner.IsPointerOver.Should().BeTrue();
                CaptureNode node = Read(form, field, hover.PopupFocusOwner);
                node.Focused.Should().BeTrue();
                node.Selected.Should().BeTrue();
            }

            using (AvaloniaControlStateDriver opened = AvaloniaControlStateDriver.Apply(form,
                       new CaptureStatePlan { Id = "owner.open-hovered", Kind = CaptureStateKind.MenuOpenHoveredOwner, TargetField = field }))
            {
                FlyoutBase flyout = owner switch
                {
                    SplitButton split => split.Flyout ?? throw new AssertionException("The source toolbar selector must retain its flyout."),
                    DropDownButton dropDown => dropDown.Flyout ?? throw new AssertionException("The source filter must retain its flyout."),
                    _ => throw new AssertionException("The source toolbar owner must expose its real dropdown."),
                };
                flyout.IsOpen.Should().BeTrue();
                owner.IsPointerOver.Should().BeTrue("the declared capture keeps the pointer over the owner, unlike its native selection role");
                CaptureNode node = Read(form, field, opened.PopupFocusOwner);
                node.Focused.Should().BeFalse("the source ToolStripItem.Selected releases its owner role when this dropdown opens");
                node.Selected.Should().BeFalse();
                flyout.IsOpen.Should().BeTrue("reading the tree must not change product popup or keyboard state");
            }
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Tree_should_retain_native_main_menu_owner_selection_while_its_real_submenu_is_open()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using FormBrowse form = CreateForm();
        try
        {
            MenuItem owner = form.mainMenuStrip.Items.OfType<MenuItem>().Single(item => item.Name == "commandsToolStripMenuItem");
            using AvaloniaControlStateDriver opened = AvaloniaControlStateDriver.Apply(form,
                new CaptureStatePlan
                {
                    Id = "commands.open-hovered",
                    Kind = CaptureStateKind.MenuOpenHoveredOwner,
                    TargetField = "commandsToolStripMenuItem",
                });
            owner.IsSubMenuOpen.Should().BeTrue();
            CaptureNode node = Read(form, "commandsToolStripMenuItem", opened.PopupFocusOwner);
            node.Focused.Should().BeTrue("the original MenuStrip owner remains selected while its submenu is open");
            node.Selected.Should().BeTrue();
            owner.IsSubMenuOpen.Should().BeTrue();
            using AvaloniaControlStateDriver childHovered = AvaloniaControlStateDriver.Apply(form,
                new CaptureStatePlan
                {
                    Id = "commands.commit.hover",
                    Kind = CaptureStateKind.Hover,
                    TargetField = "commitToolStripMenuItem",
                });
            owner.IsSubMenuOpen.Should().BeTrue("hovering a child must preserve the owner's open paint state");
            owner.IsPointerOver.Should().BeFalse();
            CaptureNode childHoverOwner = Read(form, "commandsToolStripMenuItem", opened.PopupFocusOwner);
            childHoverOwner.Focused.Should().BeFalse("the source MenuStrip owner releases raw selection when its popup child is hovered");
            childHoverOwner.Selected.Should().BeFalse();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("tsmiShowBranchesCurrent")]
    [TestCase("tsmiBranchRemote")]
    [TestCase("tsmiCommitFilter")]
    [TestCase("commitToolStripMenuItem")]
    public void Tree_should_preserve_native_grid_focus_ownership_while_hovering_a_real_owned_popup_row(string field)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using FormBrowse form = CreateForm();
        try
        {
            using AvaloniaControlStateDriver hovered = AvaloniaControlStateDriver.Apply(form,
                new CaptureStatePlan { Id = "row.hover", Kind = CaptureStateKind.Hover, TargetField = field });
            hovered.PopupSurfaceRoots.Should().NotBeEmpty();
            hovered.PopupFocusOwner.Should().BeSameAs(form.RevisionGridControl.GetTestAccessor().Revisions,
                "the capture must retain the actual focused grid before entering its popup, not infer a default owner afterward");
            CaptureNode node = Read(form, "_gridView", hovered.PopupFocusOwner);
            node.Focused.Should().BeTrue("native ToolStrip popups keep the existing grid HWND focused even when Avalonia focuses an owned popup scope");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("branchSelect")]
    [TestCase("tsddbtnBranchFilter")]
    [TestCase("commandsToolStripMenuItem")]
    public void Tree_should_not_invent_grid_focus_when_an_editor_owned_focus_before_a_real_popup_opened(string field)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using FormBrowse form = CreateForm();
        FlyoutBase? flyout = null;
        MenuItem? menuOwner = null;
        try
        {
            ToolbarComboBox filter = form.GetLogicalDescendants().OfType<ToolbarComboBox>()
                .Single(control => control.Name == "tstxtRevisionFilter");
            TextBox editor = filter.GetVisualDescendants().OfType<TextBox>().Single();
            editor.Focus().Should().BeTrue();
            Control focusedBefore = form.FocusManager?.GetFocusedElement() as Control
                ?? throw new AssertionException("The real editor must supply the observed pre-popup focus owner.");
            focusedBefore.Should().BeSameAs(editor);
            Control owner = form.GetLogicalDescendants().OfType<Control>().Single(control => control.Name == field);
            MenuItem row;
            if (owner is MenuItem menu)
            {
                menuOwner = menu;
                menu.IsSubMenuOpen = true;
                Dispatcher.UIThread.RunJobs();
                row = menu.Items.OfType<MenuItem>().First(item => item.IsVisible && item.IsEnabled);
            }
            else
            {
                flyout = owner switch
                {
                    SplitButton split => split.Flyout,
                    DropDownButton dropDown => dropDown.Flyout,
                    _ => null,
                };
                MenuFlyout ownedMenu = flyout as MenuFlyout
                    ?? throw new AssertionException("The real source selector must expose its owned menu.");
                ownedMenu.ShowAt(owner);
                Dispatcher.UIThread.RunJobs();
                row = ownedMenu.Items.OfType<MenuItem>().First(item => item.IsVisible && item.IsEnabled);
            }

            row.Focus().Should().BeTrue();
            Dispatcher.UIThread.RunJobs();
            Read(form, "_gridView", focusedBefore).Focused.Should().BeFalse(
                "an unrelated popup cannot manufacture pre-existing grid focus");
            Read(form, "tstxtRevisionFilter", focusedBefore).Focused.Should().BeTrue(
                "the actual observed editor, not an assumed default grid, supplies the native HWND ownership role");
            CaptureNode primary = new AvaloniaControlTreeReader(form, renderScale: 1, popupFocusOwner: focusedBefore)
                .ReadPrimary(form, new PixelSize(1800, 950)).Root;
            primary.Focused.Should().BeFalse("native Form.Focused excludes its focused child HWND");
            Enumerate(primary).Single(node => node.FieldName == "ToolStripFilters").Focused.Should().BeFalse(
                "the captured editor's ordinary ancestors must not become focused merely because they contain it");
            Read(form, "_gridView").Focused.Should().BeFalse(
                "without observed pre-popup ownership, an open menu alone is insufficient evidence of native grid focus");
        }
        finally
        {
            flyout?.Hide();
            if (menuOwner is not null)
            {
                menuOwner.IsSubMenuOpen = false;
            }

            form.Close();
        }
    }

    private static FormBrowse CreateForm()
    {
        FormBrowse form = new() { Width = 1800, Height = 950 };
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(Substitute.For<IGitModule>());
        (typeof(GitModuleForm).GetProperty(nameof(GitModuleForm.UICommands), BindingFlags.Instance | BindingFlags.Public)
            ?? throw new AssertionException("The original-shaped UICommands boundary must remain available."))
            .SetValue(form, commands);
        IconSplitButton branch = form.FindControl<IconSplitButton>("branchSelect")
            ?? throw new AssertionException("The original-shaped branch selector must remain available.");
        MenuFlyout branchMenu = branch.Flyout as MenuFlyout
            ?? throw new AssertionException("The real branch selector must retain its owned menu.");
        branchMenu.Items.Add(new MenuItem { Header = "Chec_kout branch..." });
        form.Show();
        Dispatcher.UIThread.RunJobs();
        form.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        form.RevisionGridControl.GetTestAccessor().Revisions.Focus().Should().BeTrue();
        return form;
    }

    private static CaptureNode Read(FormBrowse form, string field, Control? popupFocusOwner = null)
    {
        CaptureNode root = new AvaloniaControlTreeReader(form, renderScale: 1, popupFocusOwner: popupFocusOwner)
            .ReadPrimary(form, new PixelSize(1800, 950)).Root;
        return Enumerate(root).Single(node => node.FieldName == field);
    }

    private static IEnumerable<CaptureNode> Enumerate(CaptureNode node)
    {
        yield return node;
        foreach (CaptureNode child in node.Children)
        {
            foreach (CaptureNode descendant in Enumerate(child))
            {
                yield return descendant;
            }
        }
    }
}
