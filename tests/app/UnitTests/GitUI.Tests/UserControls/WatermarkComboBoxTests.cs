using FluentAssertions;
using GitUI.UserControls;

namespace GitUITests.UserControls;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class WatermarkComboBoxTests
{
    private const string Watermark = "Enter text here...";
    private Form _form = null!;
    private TabControl _tabControl = null!;
    private TabPage _tab1 = null!;
    private TabPage _tab2 = null!;
    private WatermarkComboBox _comboBox1 = null!;
    private TextBox _textBox = null!;
    private Font _originalFont = null!;
    private Color _originalForeColor;
    private Font _watermarkFont = null!;
    private Color _watermarkColor;

    [SetUp]
    public void SetUp()
    {
        _form = new Form();
        _tabControl = new TabControl { Parent = _form, Dock = DockStyle.Fill };
        _tab1 = new TabPage("Tab 1") { Parent = _tabControl };
        _tab2 = new TabPage("Tab 2") { Parent = _tabControl };
        _tabControl.SelectedTab = _tab1;
        _comboBox1 = new WatermarkComboBox { Parent = _tab1, Name = "ComboBox1", Watermark = Watermark, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
        _textBox = new TextBox { Parent = _tab1, Name = "TextBox", Top = _comboBox1.Bottom + 10 };

        _originalFont = (Font)_comboBox1.Font.Clone();
        _originalForeColor = _comboBox1.ForeColor;
        _watermarkFont = new Font((Font)_originalFont.Clone(), FontStyle.Italic);
        _watermarkColor = SystemColors.GrayText;

        _form.Show();

        FocusAway();

        Application.DoEvents();
    }

    [TearDown]
    public void TearDown()
    {
        _form.Dispose();
        _watermarkFont.Dispose();
        _originalFont.Dispose();
    }

    [Test]
    public void Initialized_empty_should_show_watermark()
    {
        AssertWatermarkVisible(_comboBox1, Watermark);
    }

    [Test]
    public void Initialized_with_text_should_not_show_watermark()
    {
        using Form form = new();
        Font font = new(Control.DefaultFont, FontStyle.Bold);
        using WatermarkComboBox comboBox = new() { Parent = form, Text = "Initial Text", Font = font };
        form.Show();

        AssertWatermarkHidden(comboBox, "Initial Text", font);
    }

    [Test]
    public void Got_focus_should_hide_watermark()
    {
        AssertWatermarkVisible(_comboBox1, Watermark);

        _comboBox1.Focus();

        AssertWatermarkHidden(_comboBox1, string.Empty);
    }

    [Test]
    public void Focus_and_blur_should_not_produce_events()
    {
        AssertWatermarkVisible(_comboBox1, Watermark);

        int eventOccurred = 0;
        _comboBox1.TextChanged += (sender, e) => eventOccurred++;

        _comboBox1.Focus();
        FocusAway();
        _comboBox1.Focus();
        FocusAway();

        AssertWatermarkVisible(_comboBox1, Watermark);
        Assert.That(eventOccurred, Is.Zero, "No events should have been triggered during focus and blur.");
    }

    [Test]
    public void Change_of_text_should_produce_text_change_events()
    {
        AssertWatermarkVisible(_comboBox1, Watermark);

        int textEventOccurred = 0;
        _comboBox1.TextChanged += (sender, e) => textEventOccurred++;

        _comboBox1.Text = "New Text";

        AssertWatermarkHidden(_comboBox1, "New Text");
        Assert.That(textEventOccurred, Is.EqualTo(1), "Event should have been triggered once.");
    }

    [Test]
    public void Change_of_text_to_empty_should_produce_text_change_events()
    {
        _comboBox1.Text = "User text";
        AssertWatermarkHidden(_comboBox1, "User text");

        int textEventOccurred = 0;
        _comboBox1.TextChanged += (sender, e) => textEventOccurred++;

        _comboBox1.Text = "";

        AssertWatermarkVisible(_comboBox1, Watermark);
        Assert.That(textEventOccurred, Is.EqualTo(1), "Event should have been triggered once.");
    }

    [Test]
    public void Lost_focus_should_show_watermark_when_text_is_empty()
    {
        _comboBox1.Focus();
        AssertWatermarkHidden(_comboBox1, string.Empty);

        FocusAway();

        AssertWatermarkVisible(_comboBox1, Watermark);
    }

    [Test]
    public void Lost_focus_should_not_show_watermark_when_text_is_not_empty()
    {
        _comboBox1.Focus();
        Application.DoEvents();
        _comboBox1.Text = "User Text";
        AssertWatermarkHidden(_comboBox1, "User Text");

        FocusAway();

        AssertWatermarkHidden(_comboBox1, "User Text");
    }

    [TestCase("")]
    [TestCase(null)]
    public void Text_changed_to_empty_should_show_watermark_when_not_focused(string? emptyValue)
    {
        _comboBox1.Focus();
        Application.DoEvents();
        _comboBox1.Text = "Some Text";
        Application.DoEvents();
        FocusAway();
        AssertWatermarkHidden(_comboBox1, "Some Text");

        _comboBox1.Text = emptyValue;

        AssertWatermarkVisible(_comboBox1, Watermark);
    }

    [Test]
    public void Text_changed_to_non_empty_should_hide_watermark_when_watermark_active_and_not_focused()
    {
        AssertWatermarkVisible(_comboBox1, Watermark);

        _comboBox1.Text = "Programmatic Text";

        AssertWatermarkHidden(_comboBox1, "Programmatic Text");
    }

    [Test]
    public void Watermark_property_should_update_displayed_watermark()
    {
        AssertWatermarkVisible(_comboBox1, Watermark);

        const string newWatermark = "New watermark text";
        _comboBox1.Watermark = newWatermark;

        AssertWatermarkVisible(_comboBox1, newWatermark);
    }

    [Test]
    public void Text_changed_to_watermark_text_should_not_hide_watermark()
    {
        _comboBox1.Focus();
        Application.DoEvents();
        _comboBox1.Text = Watermark;
        Application.DoEvents();

        FocusAway();

        AssertWatermarkHidden(_comboBox1, Watermark);
    }

    [Test]
    public void Font_changed_should_update_original_font_when_watermark_inactive()
    {
        _comboBox1.Focus();
        AssertWatermarkHidden(_comboBox1, string.Empty);

        using Font newFont = new("Arial", 12);
        _comboBox1.Font = newFont;
        Application.DoEvents();

        FocusAway();
        Font expectedWatermarkFont = new(newFont, FontStyle.Italic);
        AssertWatermarkVisible(_comboBox1, Watermark, expectedWatermarkFont);

        _comboBox1.Focus();
        AssertWatermarkHidden(_comboBox1, string.Empty, newFont);
    }

    [Test]
    public void ForeColor_changed_should_update_original_color_when_watermark_inactive()
    {
        _comboBox1.Focus();
        AssertWatermarkHidden(_comboBox1, string.Empty);

        Color newColor = Color.Blue;
        _comboBox1.ForeColor = newColor;
        Application.DoEvents();

        FocusAway();
        AssertWatermarkVisible(_comboBox1, Watermark);

        _comboBox1.Text = "Some Text";
        AssertWatermarkHidden(_comboBox1, "Some Text", foreColor: newColor);
    }

    [Test]
    public void Lost_focus_by_switching_tabs_should_show_watermark_when_text_is_empty()
    {
        // When switching tabs and the OnLeave is called, the Focused property is still true
        // this test ensures that the watermark is shown correctly in this scenario.

        _comboBox1.Focus();
        Application.DoEvents();
        _comboBox1.Text = string.Empty;
        Application.DoEvents();

        AssertWatermarkHidden(_comboBox1, string.Empty);

        _tabControl.SelectedTab = _tab2;
        Application.DoEvents();

        AssertWatermarkVisible(_comboBox1, Watermark);
    }

    private void AssertWatermarkVisible(WatermarkComboBox comboBox, string expectedWatermark, Font? font = null)
    {
        Application.DoEvents();
        comboBox.IsWatermarkVisible.Should().BeTrue();
        comboBox.Text.Should().BeEmpty();
        comboBox.Font.Should().Be(font ?? _watermarkFont);
        comboBox.ForeColor.Should().Be(_watermarkColor);
        comboBox.GetTestAccessor().BaseText.Should().Be(expectedWatermark);
    }

    private void AssertWatermarkHidden(WatermarkComboBox comboBox, string expectedText, Font? font = null, Color? foreColor = null)
    {
        Application.DoEvents();
        comboBox.IsWatermarkVisible.Should().BeFalse();
        comboBox.Text.Should().Be(expectedText);
        comboBox.Font.Should().Be(font ?? _originalFont);
        comboBox.ForeColor.Should().Be(foreColor ?? _originalForeColor);
    }

    private void FocusAway() => _textBox.Focus();
}
