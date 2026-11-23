using CommonTestUtils;
using GitUI;
using GitUI.Blame;

namespace GitUITests.UserControls;

[Apartment(ApartmentState.STA)]
public class BlameControlSplitterTests
{
    [Test]
    public void InitSplitterManager_ShouldRegisterBothSplitters_WithNamespacedSettingNames()
    {
        // arrange
        using BlameControl blameControl = new();
        SplitterManager splitterManager = new(new MemorySettings());

        // act
        blameControl.InitSplitterManager(new NestedSplitterManager(splitterManager, "FormBlame"));

        // assert
        List<SplitterManager.SplitterData> splitters = splitterManager.GetTestAccessor().Splitters;
        splitters.Should().HaveCount(2);
        splitters[0].SizeSettingsKey.Should().Be("FormBlame.BlameControl.splitContainer1_Size");
        splitters[1].SizeSettingsKey.Should().Be("FormBlame.BlameControl.splitContainer2_Size");
    }

    [Test]
    public void RestoreSplitters_WhenNoSavedSettings_ShouldUseDefaultDistanceOfPanel1MinSizePlus1()
    {
        // arrange
        MemorySettings settings = new();
        using BlameControl blameControl = new();
        SplitterManager splitterManager = new(settings);
        blameControl.InitSplitterManager(new NestedSplitterManager(splitterManager, "FormBlame"));

        // act
        splitterManager.RestoreSplitters();

        // assert
        SplitContainer splitContainer1 = GetSplitContainer(blameControl, "splitContainer1")!;
        splitContainer1.SplitterDistance.Should().Be(splitContainer1.Panel1MinSize + 1);
        foreach (SplitterManager.SplitterData splitter in splitterManager.GetTestAccessor().Splitters)
        {
            settings.GetInt(splitter.SizeSettingsKey).Should().BeNull();
            settings.GetInt(splitter.DistanceSettingsKey).Should().BeNull();
        }
    }

    [Test]
    public void SaveAndRestoreSplitters_ShouldRestoreSavedSplitterDistance()
    {
        // arrange
        MemorySettings settings = new();
        using BlameControl blameControl = new();
        SplitterManager splitterManager = new(settings);
        blameControl.InitSplitterManager(new NestedSplitterManager(splitterManager, "FormBlame"));
        splitterManager.RestoreSplitters();
        SplitContainer splitContainer1 = GetSplitContainer(blameControl, "splitContainer1")!;
        const int newDistance = 300;
        splitContainer1.SplitterDistance = newDistance;
        splitterManager.SaveSplitters();

        // act
        using BlameControl restoredControl = new();
        SplitterManager restoredManager = new(settings);
        restoredControl.InitSplitterManager(new NestedSplitterManager(restoredManager, "FormBlame"));
        restoredManager.RestoreSplitters();

        // assert
        GetSplitContainer(restoredControl, "splitContainer1")!.SplitterDistance.Should().Be(newDistance);
    }

    private static SplitContainer? GetSplitContainer(Control control, string name)
    {
        foreach (Control child in control.Controls)
        {
            if (child.Name == name)
            {
                return (SplitContainer)child;
            }

            if (child is SplitContainer splitContainer)
            {
                SplitContainer? found = GetSplitContainer(splitContainer.Panel1, name) ?? GetSplitContainer(splitContainer.Panel2, name);
                if (found is not null)
                {
                    return found;
                }
            }
        }

        return null;
    }
}
