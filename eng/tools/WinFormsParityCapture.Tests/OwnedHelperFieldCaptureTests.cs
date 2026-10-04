using System.Reflection;
using System.Runtime.ExceptionServices;
using AwesomeAssertions;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class OwnedHelperFieldCaptureTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void ReadPrimary_should_discover_owned_nested_fields_and_preserve_source_order_aliases_without_following_services(bool directAlias)
    {
        using HelperOwner form = new(directAlias);
        CaptureNode first = new ControlTreeReader(form, dpi: 96)
            .ReadPrimary(form, new Rectangle(0, 0, 300, 200)).Root;
        CaptureNode second = new ControlTreeReader(form, dpi: 96)
            .ReadPrimary(form, new Rectangle(0, 0, 300, 200)).Root;
        CaptureNode button = first.Children.Single();

        first.FieldName.Should().BeNull("the helper's owner backlink is not an authored control alias");
        button.FieldName.Should().Be(directAlias ? "_directAlias" : "_zOwnedButton");
        string[] expectedAliases = directAlias
            ? ["_zOwnedButton", "_aAlias", "_nestedAlias"]
            : ["_aAlias", "_nestedAlias"];
        button.FieldAliases.Should().Equal(expectedAliases);
        button.Name.Should().Be("live-action");
        button.Id.Should().Be(second.Children.Single().Id);
        button.FieldAliases.Should().Equal(second.Children.Single().FieldAliases);
        first.Children.Should().ContainSingle("helpers, closures, collections and services are not semantic controls");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ReadPrimary_should_discover_helpers_declared_by_inherited_and_constructed_generic_owners(bool generic)
    {
        using Form form = generic ? new GenericOwner<int>() : new DerivedOwner();

        CaptureNode button = new ControlTreeReader(form, dpi: 96)
            .ReadPrimary(form, new Rectangle(0, 0, 300, 200)).Root.Children.Single();

        button.FieldName.Should().Be("_btnInheritedOrGeneric");
        button.FieldAliases.Should().BeEmpty();
    }

    [Test]
    public void ReadPrimary_should_discover_actual_source_WorkingDir_fixed_fields_without_loading_Browse_or_executing_actions()
    {
        MethodInfo isolate = typeof(ToolStripOwnerOverflowTests).GetMethod(
            "WithIsolatedSettings", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(typeof(ToolStripOwnerOverflowTests).FullName, "WithIsolatedSettings");
        Invoke(isolate, null, (Action<string>)(settingsDirectory =>
        {
            string settingsPath = Path.GetFullPath(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath);
            Path.GetRelativePath(settingsDirectory, settingsPath).Should().Be("GitExtensions.settings");

            // Only the actual source toolbar item is initialized. There is no source
            // Browse, OnLoad, repository, command click, or SessionEnding subscription.
            Assembly source = typeof(FormBrowse).Assembly;
            using ToolStripSplitButton selector = Create<ToolStripSplitButton>(
                source, "GitUI.CommandsDialogs.Menus.WorkingDirectoryToolStripSplitButton");
            using ToolStripMenuItem start = Create<ToolStripMenuItem>(
                source, "GitUI.CommandsDialogs.Menus.StartToolStripMenuItem");
            using ToolStripMenuItem close = new("Close");
            IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
            Func<IGitUICommands> commands = () => throw new InvalidOperationException("No source action may execute.");
            MethodInfo initialize = selector.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public)
                ?? throw new MissingMethodException(selector.GetType().FullName, "Initialize");
            Invoke(initialize, selector, commands, history, start, close);
            object implementation = selector.GetType().GetField("_implementation", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(selector) ?? throw new MissingFieldException(selector.GetType().FullName, "_implementation");
            MethodInfo populate = implementation.GetType().GetMethod("FillDropDown", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingMethodException(implementation.GetType().FullName, "FillDropDown");
            Invoke(populate, implementation, selector);
            using Form host = new();
            using ToolStrip strip = new();
            strip.Items.Add(selector);
            host.Controls.Add(strip);

            CaptureNode root = new ControlTreeReader(host, dpi: 96)
                .ReadPrimary(host, new Rectangle(0, 0, 400, 200)).Root;
            foreach (string fieldName in new[] { "_tsmiOpenLocalRepository", "_tsmiCloseRepo", "_tsmiRecentReposSettings", "_txtFilter" })
            {
                object sourceItem = implementation.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(implementation) ?? throw new MissingFieldException(implementation.GetType().FullName, fieldName);
                CaptureNode node = Flatten(root).Single(candidate => candidate.FieldName == fieldName);
                node.Type.Should().Be(sourceItem.GetType().FullName);
                node.Id.Should().EndWith("/" + fieldName);
            }

            history.Received(1).PopulateFavouriteRepositoriesMenu(Arg.Any<ToolStripDropDownItem>());
            history.Received(1).PopulateRecentRepositoriesMenu(selector);
        }));
    }

    private static T Create<T>(Assembly source, string typeName)
        where T : class
        => Activator.CreateInstance(source.GetType(typeName, throwOnError: true)
            ?? throw new TypeLoadException(typeName)) as T ?? throw new InvalidOperationException(typeName);

    private static IEnumerable<CaptureNode> Flatten(CaptureNode node)
    {
        yield return node;
        foreach (CaptureNode child in node.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private static void Invoke(MethodInfo method, object? target, params object?[] arguments)
    {
        try
        {
            method.Invoke(target, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
        }
    }

    private sealed class HelperOwner : Form
    {
        private readonly ColumnImplementation _implementation;
        private readonly Button? _directAlias;
        private readonly UnrelatedService _service;
        private readonly CollectionImplementation _collection;
        private readonly ProviderImplementation _provider;
        private readonly ComponentImplementation _component;
        private readonly Action _callback;
        private readonly object? _closure;

        public HelperOwner(bool directAlias)
        {
            Button button = new() { Name = "live-action", Text = "Owned action" };
            _implementation = new(this, button);
            _directAlias = directAlias ? button : null;
            _service = new(button);
            _collection = new(button);
            _provider = new(button);
            _component = new(button);
            _callback = () => GC.KeepAlive(button);
            _closure = _callback.Target;
            Controls.Add(button);
            GC.KeepAlive(new object?[] { _service, _collection, _provider, _component, _closure, _directAlias, _implementation });
        }

        public object UnreadProperty => throw new InvalidOperationException("Field indexing must never execute properties.");

        private sealed class ColumnImplementation
        {
            private readonly Button _zOwnedButton;
            private readonly Button _aAlias;
            private readonly NestedAlias _nested;
            private readonly HelperOwner _owner;

            public ColumnImplementation(HelperOwner owner, Button button)
            {
                _zOwnedButton = button;
                _aAlias = button;
                _nested = new(button, this);
                _owner = owner;
                GC.KeepAlive(new object[] { _zOwnedButton, _aAlias, _nested, _owner });
            }

            private sealed class NestedAlias
            {
                private readonly Button _nestedAlias;
                private readonly ColumnImplementation _cycle;

                public NestedAlias(Button button, ColumnImplementation cycle)
                {
                    _nestedAlias = button;
                    _cycle = cycle;
                    GC.KeepAlive(new object[] { _nestedAlias, _cycle });
                }
            }
        }

        private sealed class CollectionImplementation : List<object>
        {
            private readonly Button _collectionAlias;

            public CollectionImplementation(Button button)
            {
                _collectionAlias = button;
                Add(button);
                GC.KeepAlive(_collectionAlias);
            }
        }

        private sealed class ProviderImplementation : IServiceProvider
        {
            private readonly Button _providerAlias;

            public ProviderImplementation(Button button)
            {
                _providerAlias = button;
                GC.KeepAlive(_providerAlias);
            }

            public object? GetService(Type serviceType) => throw new InvalidOperationException("Services must not be visited.");
        }

        private sealed class ComponentImplementation : System.ComponentModel.Component
        {
            private readonly Button _componentAlias;

            public ComponentImplementation(Button button)
            {
                _componentAlias = button;
                GC.KeepAlive(_componentAlias);
            }
        }
    }

    private class InheritedOwner : Form
    {
        private readonly Implementation _implementation;

        protected InheritedOwner()
        {
            Button button = new();
            _implementation = new(button);
            Controls.Add(button);
            GC.KeepAlive(_implementation);
        }

        private sealed class Implementation
        {
            private readonly Button _btnInheritedOrGeneric;

            public Implementation(Button button)
            {
                _btnInheritedOrGeneric = button;
                GC.KeepAlive(_btnInheritedOrGeneric);
            }
        }
    }

    private sealed class DerivedOwner : InheritedOwner
    {
    }

    private sealed class GenericOwner<T> : Form
    {
        private readonly Implementation _implementation;

        public GenericOwner()
        {
            Button button = new();
            _implementation = new(button);
            Controls.Add(button);
            GC.KeepAlive(_implementation);
        }

        private sealed class Implementation
        {
            private readonly Button _btnInheritedOrGeneric;

            public Implementation(Button button)
            {
                _btnInheritedOrGeneric = button;
                GC.KeepAlive(_btnInheritedOrGeneric);
            }
        }
    }

    private sealed class UnrelatedService
    {
        private readonly Button _serviceAlias;

        public UnrelatedService(Button button)
        {
            _serviceAlias = button;
            GC.KeepAlive(_serviceAlias);
        }
    }
}
