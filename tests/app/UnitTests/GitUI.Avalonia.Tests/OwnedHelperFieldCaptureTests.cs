using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitExtensions.ParityCapture;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class OwnedHelperFieldCaptureTests
{
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void ReadPrimary_should_discover_owned_nested_fields_and_preserve_source_order_aliases_without_following_services(bool directAlias)
    {
        HelperOwner form = new(directAlias);
        CaptureNode first = new AvaloniaControlTreeReader(form, renderScale: 1)
            .ReadPrimary(form, new PixelSize(300, 200)).Root;
        CaptureNode second = new AvaloniaControlTreeReader(form, renderScale: 1)
            .ReadPrimary(form, new PixelSize(300, 200)).Root;
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

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void ReadPrimary_should_discover_helpers_declared_by_inherited_and_constructed_generic_owners(bool generic)
    {
        Window form = generic ? new GenericOwner<int>() : new DerivedOwner();

        CaptureNode button = new AvaloniaControlTreeReader(form, renderScale: 1)
            .ReadPrimary(form, new PixelSize(300, 200)).Root.Children.Single();

        button.FieldName.Should().Be("_btnInheritedOrGeneric");
        button.FieldAliases.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void ReadPrimary_should_keep_actual_WorkingDir_fixed_field_identity()
    {
        WorkingDirectoryToolStripSplitButton selector = new();
        selector.GetTestAccessor().FillDropDown([], []);
        Window window = new() { Content = selector };

        CaptureNode root = new AvaloniaControlTreeReader(window, renderScale: 1)
            .ReadPrimary(window, new PixelSize(400, 200)).Root;
        foreach (string fieldName in new[] { "_tsmiOpenLocalRepository", "_tsmiCloseRepo" })
        {
            CaptureNode node = Flatten(root).Single(candidate => candidate.FieldName == fieldName);
            node.Type.Should().Be(typeof(NativeToolStripDropDownMenuItem).FullName);
            node.Id.Should().EndWith("/" + fieldName);
        }
    }

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

    private sealed class HelperOwner : Window
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
            Button button = new() { Name = "live-action", Content = "Owned action" };
            _implementation = new(this, button);
            _directAlias = directAlias ? button : null;
            _service = new(button);
            _collection = new(button);
            _provider = new(button);
            _component = new(button);
            _callback = () => GC.KeepAlive(button);
            _closure = _callback.Target;
            Content = button;
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

    private class InheritedOwner : Window
    {
        private readonly Implementation _implementation;

        protected InheritedOwner()
        {
            Button button = new();
            _implementation = new(button);
            Content = button;
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

    private sealed class GenericOwner<T> : Window
    {
        private readonly Implementation _implementation;

        public GenericOwner()
        {
            Button button = new();
            _implementation = new(button);
            Content = button;
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
