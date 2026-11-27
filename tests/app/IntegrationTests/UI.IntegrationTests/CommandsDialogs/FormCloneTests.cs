using CommonTestUtils;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormCloneTests
{
    // Created once for the fixture
    private ReferenceRepository _referenceRepository = null!;

    // Created once for each test
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
    public void Should_fit_its_content_at_larger_text_sizes(float fontSizeInPoints)
    {
        AppSettings.Font = new Font(SystemFonts.MessageBoxFont!.FontFamily, fontSizeInPoints);

        RunFormTest(
            form =>
            {
                FormClone.TestAccessor accessor = form.GetTestAccessor();

                TestContext.Out.WriteLine($"form {form.Bounds}, minimum {form.MinimumSize}, maximum {form.MaximumSize}");
                LayoutAssert.Report(accessor.RepositoryFields);
                LayoutAssert.Report(accessor.RepositoryType);
                LayoutAssert.Report(accessor.RepositoryTypeOptions);
                TestContext.Out.WriteLine($"Info {accessor.Info.Bounds}");

                LayoutAssert.ChildrenDoNotOverlap(accessor.RepositoryFields);
                LayoutAssert.ChildrenFitIntoContainer(accessor.RepositoryFields);
                LayoutAssert.CaptionFits(accessor.FromBrowse);
                LayoutAssert.CaptionFits(accessor.ToBrowse);

                LayoutAssert.ChildrenFitIntoContainer(accessor.RepositoryType);
                LayoutAssert.ChildrenDoNotOverlap(accessor.RepositoryTypeOptions);
                LayoutAssert.ChildrenFitIntoContainer(accessor.RepositoryTypeOptions);

                // The banner always renders two lines of text.
                accessor.Info.Height.Should().BeGreaterThanOrEqualTo(2 * accessor.Info.Font.Height);
            });
    }

    [TestCase(null, false, "")]
    [TestCase("", false, "")]
    [TestCase(" ", false, "")]
    [TestCase("blah", false, "")]
    [TestCase("git clone https://github.com/gitextensions/gitextensions && cd gitextensions", true, "https://github.com/gitextensions/gitextensions")]
    [TestCase("git clone ssh://username@gerrit-server:/PROJECT", true, "ssh://username@gerrit-server:/PROJECT")]
    [TestCase("git clone https://github.com/gitextensions/gitextensions && git clone https://github.com/gitextensions/git.hub", true, "https://github.com/gitextensions/gitextensions")]
    public void Test_Url_extract_from_string(
        string? text, bool expected, string expectedUrl)
    {
        RunFormTest(
            form =>
            {
                FormClone.TestAccessor accessor = form.GetTestAccessor();

                accessor.TryExtractUrl(text!, out string url).Should().Be(expected);

                // No need to compare URL if the result was expected to be false
                if (expected)
                {
                    url.Should().Be(expectedUrl);
                }
            });
    }

    private void RunFormTest(Action<FormClone> testDriver)
    {
        RunFormTest(
            form =>
            {
                testDriver(form);
                return Task.CompletedTask;
            });
    }

    private void RunFormTest(Func<FormClone, Task> testDriverAsync)
    {
        UITest.RunForm(
            () =>
            {
                _commands.StartCloneDialog(owner: null, url: null);
            },
            testDriverAsync);
    }
}
