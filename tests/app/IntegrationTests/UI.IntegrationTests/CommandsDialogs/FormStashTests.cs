using CommonTestUtils;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormStashTests
{
    private ReferenceRepository _referenceRepository = null!;
    private GitUICommands _commands = null!;
    private Font _originalFont = null!;

    [SetUp]
    public void SetUp()
    {
        _originalFont = AppSettings.Font;
        _referenceRepository = new ReferenceRepository();
        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.Font = _originalFont;
        _referenceRepository.Dispose();
    }

    /// <summary>
    ///  The Windows "Text size" accessibility setting enlarges <see cref="SystemFonts.MessageBoxFont"/>,
    ///  which <see cref="AppSettings.Font"/> defaults to, without changing the DPI - so nothing scales
    ///  the sizes recorded by the designer. 12pt stands for the 134% text size of issue #13113.
    /// </summary>
    [TestCase(9F)]
    [TestCase(12F)]
    public void Should_fit_the_buttons_at_larger_text_sizes(float fontSizeInPoints)
    {
        AppSettings.Font = new Font(SystemFonts.MessageBoxFont!.FontFamily, fontSizeInPoints);

        UITest.RunForm<FormStash>(
            () => _commands.StartStashDialog(owner: null),
            form =>
            {
                FormStash.TestAccessor accessor = form.GetTestAccessor();

                LayoutAssert.Report(accessor.ButtonsAndOptions);

                LayoutAssert.ChildrenDoNotOverlap(accessor.ButtonsAndOptions);
                LayoutAssert.ChildrenFitIntoContainer(accessor.ButtonsAndOptions);
                LayoutAssert.CaptionFits(accessor.StashAll);
                LayoutAssert.CaptionFits(accessor.StashSelected);
                LayoutAssert.CaptionFits(accessor.Drop);
                LayoutAssert.CaptionFits(accessor.ApplySelected);

                return Task.CompletedTask;
            });
    }
}
