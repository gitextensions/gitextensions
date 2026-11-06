using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitCommands.Settings;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Settings;
using GitUI;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[Category("P8.6i.126")]
public sealed class SplitterManagerTests
{
    [AvaloniaTest]
    public void Grid_splitter_should_retain_a_distance_larger_than_its_unrealized_bounds()
    {
        IConfigValueStore store = Substitute.For<IConfigValueStore>();
        store.GetValue("Main_Distance").Returns("700");
        Grid grid = new() { ColumnDefinitions = new ColumnDefinitions("*,6,*") };
        GridSplitter splitter = new() { ResizeDirection = GridResizeDirection.Columns };
        Grid.SetColumn(splitter, 1);
        grid.Children.Add(splitter);
        SplitterManager manager = new(new SettingsSource<IConfigValueStore>(store));
        manager.AddSplitter(grid, "Main");

        manager.RestoreSplitters();

        grid.ColumnDefinitions[0].Width.Should().Be(new GridLength(700));
        grid.Measure(new Size(1000, 500));
        grid.Arrange(new Rect(0, 0, 1000, 500));
        grid.ColumnDefinitions[0].ActualWidth.Should().Be(700);
    }

    [Test]
    public void Delegating_splitter_target_should_round_trip_distance_and_size()
    {
        Dictionary<string, string?> values = [];
        IConfigValueStore store = Substitute.For<IConfigValueStore>();
        store.GetValue(Arg.Any<string>()).Returns(call => values.GetValueOrDefault(call.Arg<string>()));
        store.When(config => config.SetValue(Arg.Any<string>(), Arg.Any<string?>()))
            .Do(call => values[call.ArgAt<string>(0)] = call.ArgAt<string?>(1));
        SettingsSource settings = new SettingsSource<IConfigValueStore>(store);
        double distance = 275;
        double size = 6;

        SplitterManager manager = new(settings);
        manager.AddSplitter("MainSplitContainer", () => distance, value => distance = value, () => size);
        manager.SaveSplitters();

        values["MainSplitContainer_Distance"].Should().Be("275");
        values["MainSplitContainer_Size"].Should().Be("6");

        distance = 100;
        size = 4;
        SplitterManager restoredManager = new(settings);
        restoredManager.AddSplitter("MainSplitContainer", () => distance, value => distance = value, () => size);
        restoredManager.RestoreSplitters();

        distance.Should().Be(275);
    }
}
