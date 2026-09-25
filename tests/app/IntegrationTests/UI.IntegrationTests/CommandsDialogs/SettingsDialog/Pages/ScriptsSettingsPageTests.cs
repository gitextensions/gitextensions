using System.ComponentModel.Design;
using System.Reflection;
using CommonTestUtils;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtensions.UITests;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using NSubstitute;

namespace UITests.CommandsDialogs.SettingsDialog.Pages;

[Apartment(ApartmentState.STA)]
public class ScriptsSettingsPageTests
{
    private ReferenceRepository _referenceRepository = null!;
    private ServiceContainer _serviceContainer = null!;
    private MockHost _form = null!;
    private ScriptsSettingsPage _settingsPage = null!;

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();

        IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
        scriptsManager.GetScripts().Returns(
        [
            new ScriptInfo { HotkeyCommandIdentifier = 9001, Name = "first", Enabled = true, Icon = "StatusBadgeSuccess" },
            new ScriptInfo { HotkeyCommandIdentifier = 9002, Name = "second", Enabled = true, Icon = "StatusBadgeSuccess" },
        ]);

        _serviceContainer = GlobalServiceContainer.CreateDefaultMockServiceContainer();
        _serviceContainer.RemoveService<IScriptsManager>();
        _serviceContainer.AddService(scriptsManager);
    }

    [TearDown]
    public void TearDown()
    {
        _settingsPage.Dispose();
        _form.Dispose();
        _serviceContainer.Dispose();
        _referenceRepository.Dispose();
    }

    [Test]
    public void Committing_a_pending_grid_edit_while_rebinding_after_an_Icon_change_should_not_crash()
    {
        RunFormTest(
            async form =>
            {
                _settingsPage.LoadSettings();
                _settingsPage.OnPageShown();
                await UITest.WaitForIdleAsync();

                ScriptsSettingsPage.TestAccessor accessor = _settingsPage.GetTestAccessor();
                PropertyGrid propertyGrid = accessor.propertyGrid1;
                propertyGrid.SelectedObject.Should().NotBeNull();

                GridItem root = propertyGrid.SelectedGridItem!;
                while (root.Parent is not null)
                {
                    root = root.Parent;
                }

                GridItem iconItem = FindProperty(root, "Icon");
                GridItem iconFilePathItem = FindProperty(root, "IconFilePath");

                // The user has typed into the grid, but the edit is not committed yet.
                // (In #12353 the pending edit was the item picked in the open Icon drop-down list.)
                iconFilePathItem.Select().Should().BeTrue();
                Control gridView = propertyGrid.Controls.Cast<Control>().Single(c => c.GetType().Name == "PropertyGridView");
                TextBox editTextBox = gridView.Controls.OfType<TextBox>().Single();
                editTextBox.Visible.Should().BeTrue();
                editTextBox.Text = "pending.ico";

                // The grid commits a new Icon value and raises PropertyValueChanged, as it does on a cursor key in the drop-down list.
                // BindScripts must not make the grid commit the pending edit (which rebinds again) while it selects a list item.
                typeof(PropertyGrid)
                    .GetMethod("OnPropertyValueChanged", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(propertyGrid, [new PropertyValueChangedEventArgs(iconItem, oldValue: null)]);

                await UITest.WaitForIdleAsync();

                accessor.lvScripts.Items.Count.Should().Be(2);
                accessor.lvScripts.SelectedItems.Count.Should().Be(1);
            });
    }

    private static GridItem FindProperty(GridItem root, string name)
        => Flatten(root).First(item => item.GridItemType == GridItemType.Property && item.PropertyDescriptor?.Name == name);

    private static IEnumerable<GridItem> Flatten(GridItem item)
    {
        yield return item;

        foreach (GridItem child in item.GridItems)
        {
            foreach (GridItem descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private void RunFormTest(Func<MockHost, Task> testDriverAsync)
    {
        UITest.RunForm(
            () =>
            {
                _form = new MockHost(_referenceRepository.Module)
                {
                    Size = new(800, 600)
                };

                _settingsPage = SettingsPageBase.Create<ScriptsSettingsPage>(_form, _serviceContainer);
                _settingsPage.Dock = DockStyle.Fill;

                _form.Controls.Add(_settingsPage);

                _form.ShowDialog(owner: null);
            },
            testDriverAsync);
    }

    private sealed class MockHost : Form, ISettingsPageHost
    {
        public MockHost(GitModule module)
        {
            CheckSettingsLogic = new(new(module));
        }

        public CheckSettingsLogic CheckSettingsLogic { get; }

        public void GotoPage(SettingsPageReference settingsPageReference)
        {
            throw new NotImplementedException();
        }

        public void LoadAll()
        {
            throw new NotImplementedException();
        }

        public void SaveAll()
        {
            throw new NotImplementedException();
        }
    }
}
