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
