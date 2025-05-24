using FluentAssertions;
using GitUI.Editor;

namespace GitUITests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class ComboBoxWatermarkManagerTests
{
    private const string Watermark = "Enter text here...";
    private Form _form = null!;
    private ComboBox _comboBox1 = null!;
    private ComboBox _comboBox2 = null!; // For focus change tests
    private TextBox _textBox = null!; // For focus change tests and for initial focus
    private ComboBoxWatermarkManager _manager1 = null!;
    private ComboBoxWatermarkManager _manager2 = null!;
    private Font _originalFont = null!;
    private Color _originalForeColor;
    private Font _watermarkFont = null!;
    private Color _watermarkColor;

    [SetUp]
    public void SetUp()
    {
        _form = new Form();
        _comboBox1 = new ComboBox { Parent = _form, Name = "ComboBox1" };
        _comboBox2 = new ComboBox { Parent = _form, Name = "ComboBox2", Left = _comboBox1.Right + 10 };
        _textBox = new TextBox { Parent = _form, Name = "TextBox", Top = _comboBox1.Bottom + 10 };

        // Store initial styles before manager potentially changes them
        _originalFont = (Font)_comboBox1.Font.Clone();
        _originalForeColor = _comboBox1.ForeColor;
        _watermarkFont = new Font(_originalFont, FontStyle.Italic);
        _watermarkColor = SystemColors.GrayText;

        _form.Show();

        // Initialize managers AFTER showing the form and storing original styles
        _manager1 = new ComboBoxWatermarkManager(_comboBox1, Watermark);
        _manager2 = new ComboBoxWatermarkManager(_comboBox2, "Another watermark");

        _textBox.Focus();

        Application.DoEvents();
    }

    [TearDown]
    public void TearDown()
    {
        _manager1.Dispose();
        _manager2.Dispose();

        _comboBox1.Dispose();
        _comboBox2.Dispose();
        _form.Dispose();
        _watermarkFont.Dispose();
    }

    [Test]
    public void Constructor_should_show_watermark_when_combobox_is_empty()
    {
        // Assert
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);
    }

    [Test]
    public void Constructor_should_not_show_watermark_when_combobox_has_text()
    {
        // Arrange
        using Form form = new();
        using ComboBox comboBox = new() { Parent = form, Text = "Initial Text" };
        form.Show();

        // Act
        using ComboBoxWatermarkManager manager = new(comboBox, Watermark);

        // Assert
        AssertWatermarkInactive(comboBox, manager, "Initial Text");
    }

    [Test]
    public void Got_focus_should_hide_watermark()
    {
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);

        // Act
        _comboBox1.Focus();
        Application.DoEvents();

        // Assert
        AssertWatermarkInactive(_comboBox1, _manager1, string.Empty);
    }

    [Test]
    public void Lost_focus_should_show_watermark_when_text_is_empty()
    {
        // Arrange
        _comboBox1.Focus(); // Hide watermark
        Application.DoEvents();
        AssertWatermarkInactive(_comboBox1, _manager1, string.Empty);

        // Act
        _textBox.Focus(); // Change focus to another control
        Application.DoEvents();

        // Assert
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);
    }

    [Test]
    public void Lost_focus_should_not_show_watermark_when_text_is_not_empty()
    {
        // Arrange
        _comboBox1.Focus(); // Hide watermark
        Application.DoEvents();
        _comboBox1.Text = "User Text";
        Application.DoEvents();
        AssertWatermarkInactive(_comboBox1, _manager1, "User Text");

        // Act
        _textBox.Focus(); // Change focus
        Application.DoEvents();

        // Assert
        AssertWatermarkInactive(_comboBox1, _manager1, "User Text");
    }

    [Test]
    public void Text_changed_to_empty_should_show_watermark_when_not_focused()
    {
        // Arrange
        _comboBox1.Focus();
        Application.DoEvents();
        _comboBox1.Text = "Some Text";
        Application.DoEvents();
        _textBox.Focus(); // Lose focus
        Application.DoEvents();
        AssertWatermarkInactive(_comboBox1, _manager1, "Some Text");

        // Act
        _comboBox1.Text = ""; // Programmatically clear text while not focused
        Application.DoEvents();

        // Assert
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);
    }

    [Test]
    public void Text_changed_to_non_empty_should_hide_watermark_when_watermark_active_and_not_focused()
    {
        // Arrange
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);

        // Act
        _comboBox1.Text = "Programmatic Text"; // Set text while watermark is active
        Application.DoEvents();

        // Assert
        AssertWatermarkInactive(_comboBox1, _manager1, "Programmatic Text");
    }

    [Test]
    public void Text_changed_to_non_empty_should_hide_watermark_when_watermark_active_and_focused()
    {
        // Arrange
        _comboBox1.Focus(); // Regain focus, watermark hides, text becomes ""
        Application.DoEvents();
        AssertWatermarkInactive(_comboBox1, _manager1, "");

        // Act
        // Simulate typing - setting text while focused and previously empty
        _comboBox1.Text = "User Typing";
        Application.DoEvents();

        // Assert
        AssertWatermarkInactive(_comboBox1, _manager1, "User Typing");
    }

    [Test]
    public void Text_changed_to_watermark_text_should_not_hide_watermark()
    {
        // Arrange (Watermark is active initially)
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);

        // Act
        // Programmatically set text to the exact watermark string
        // Let's test setting it while focused, then losing focus.
        _comboBox1.Focus();
        Application.DoEvents(); // Hides watermark
        _comboBox1.Text = Watermark; // Set text to watermark value
        Application.DoEvents();
        _textBox.Focus(); // Lose focus
        Application.DoEvents();

        // Assert
        // Even though the text *is* the watermark string, because it was explicitly set,
        // it should NOT be treated as a watermark.
        AssertWatermarkInactive(_comboBox1, _manager1, Watermark);
    }

    [Test]
    public void Text_set_programmatically_to_null_should_show_watermark_when_not_focused()
    {
        // Arrange
        _comboBox1.Text = "Some Text";
        Application.DoEvents();
        _textBox.Focus(); // Lose focus
        Application.DoEvents();
        AssertWatermarkInactive(_comboBox1, _manager1, "Some Text");

        // Act
        _comboBox1.Text = null; // Programmatically set text to null while not focused
        Application.DoEvents();

        // Assert
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);
    }

    [Test]
    public void Font_changed_should_update_original_font_when_watermark_inactive()
    {
        // Arrange
        _comboBox1.Focus(); // Hide watermark
        Application.DoEvents();
        AssertWatermarkInactive(_comboBox1, _manager1, string.Empty);

        // Act
        using Font newFont = new("Arial", 12);
        _comboBox1.Font = newFont;
        Application.DoEvents();

        // Assert manager picked up the change
        // Lose focus to trigger potential watermark application with the *new* original font
        _textBox.Focus();
        Application.DoEvents(); // Should show watermark
        Font expectedWatermarkFont = new(newFont, FontStyle.Italic);
        AssertWatermarkActive(_comboBox1, _manager1, Watermark, expectedWatermarkFont);

        _comboBox1.Focus(); // Hide watermark again
        Application.DoEvents();
        AssertWatermarkInactive(_comboBox1, _manager1, string.Empty, newFont);
    }

    [Test]
    public void Forecolor_changed_should_update_original_color_when_watermark_inactive()
    {
        // Arrange
        _comboBox1.Focus(); // Hide watermark
        Application.DoEvents();
        AssertWatermarkInactive(_comboBox1, _manager1, string.Empty);

        // Act
        Color newColor = Color.Blue;
        _comboBox1.ForeColor = newColor;
        Application.DoEvents();

        // Assert manager picked up the change
        // Lose focus to trigger potential watermark application with the *new* original color
        _textBox.Focus();
        Application.DoEvents(); // Should show watermark
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);
        _comboBox1.ForeColor.Should().Be(_watermarkColor); // Watermark color is fixed

        // Set text programatically to show it with the *new* original color
        _comboBox1.Text = "Some Text";
        Application.DoEvents(); // Should show watermark
        AssertWatermarkInactive(_comboBox1, _manager1, "Some Text", foreColor: newColor);
    }

    [Test]
    public void Dispose_should_unsubscribe_events_and_restore_state_if_watermark_active()
    {
        // Arrange
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);

        // Act
        _manager1.Dispose();

        // Assert: State should be restored to original (empty text)
        _comboBox1.Text.Should().BeEmpty();
        _comboBox1.Font.Should().Be(_originalFont);
        _comboBox1.ForeColor.Should().Be(_originalForeColor);

        // Assert: Events should no longer trigger manager logic
        _comboBox1.Focus();
        Application.DoEvents();
        _comboBox1.Text = "After Dispose";
        Application.DoEvents();
        _comboBox1.Text = "";
        Application.DoEvents();
        _textBox.Focus(); // Lose focus
        Application.DoEvents();

        // State should remain as set, watermark logic is detached
        _comboBox1.Text.Should().BeEmpty();
    }

    [Test]
    public void Dispose_should_unsubscribe_events_and_keep_state_if_watermark_inactive()
    {
        // Arrange
        _comboBox1.Focus();
        Application.DoEvents();
        _comboBox1.Text = "User Text";
        Application.DoEvents();
        AssertWatermarkInactive(_comboBox1, _manager1, "User Text");

        // Act
        _manager1.Dispose();

        // Assert: State should remain as it was
        _comboBox1.Text.Should().Be("User Text");
        _comboBox1.Font.Should().Be(_originalFont);
        _comboBox1.ForeColor.Should().Be(_originalForeColor);
    }

    [Test]
    public void Focus_change_between_two_managed_comboboxes()
    {
        // Arrange (Both start with watermark)
        AssertWatermarkActive(_comboBox1, _manager1, Watermark);
        AssertWatermarkActive(_comboBox2, _manager2, "Another watermark");

        // Act 1: Focus ComboBox1
        _comboBox1.Focus();
        Application.DoEvents();

        // Assert 1: CB1 watermark hidden, CB2 unchanged
        AssertWatermarkInactive(_comboBox1, _manager1, string.Empty);
        AssertWatermarkActive(_comboBox2, _manager2, "Another watermark");

        // Act 2: Enter text in ComboBox1
        _comboBox1.Text = "Text in CB1";
        Application.DoEvents();

        // Assert 2: CB1 has text, CB2 unchanged
        AssertWatermarkInactive(_comboBox1, _manager1, "Text in CB1");
        AssertWatermarkActive(_comboBox2, _manager2, "Another watermark");

        // Act 3: Focus ComboBox2
        _comboBox2.Focus();
        Application.DoEvents();

        // Assert 3: CB1 keeps text (lost focus), CB2 watermark hidden
        AssertWatermarkInactive(_comboBox1, _manager1, "Text in CB1");
        AssertWatermarkInactive(_comboBox2, _manager2, string.Empty);

        // Act 4: Focus back to ComboBox1 (leaving CB2 empty)
        _comboBox1.Focus();
        Application.DoEvents();

        // Assert 4: CB1 keeps text and focus, CB2 shows watermark (lost focus while empty)
        AssertWatermarkInactive(_comboBox1, _manager1, "Text in CB1");
        AssertWatermarkActive(_comboBox2, _manager2, "Another watermark");
    }

    private void AssertWatermarkActive(ComboBox comboBox, ComboBoxWatermarkManager manager, string expectedWatermark, Font? font = null)
    {
        manager.WatermarkActive.Should().BeTrue();
        comboBox.Text.Should().Be(expectedWatermark);
        comboBox.Font.Should().Be(font ?? _watermarkFont); // Assumes watermark font is italic version of original
        comboBox.ForeColor.Should().Be(_watermarkColor);
        manager.ComboBoxText.Should().BeEmpty();
    }

    private void AssertWatermarkInactive(ComboBox comboBox, ComboBoxWatermarkManager manager, string expectedText, Font? font = null, Color? foreColor = null)
    {
        manager.WatermarkActive.Should().BeFalse();
        comboBox.Text.Should().Be(expectedText);
        comboBox.Font.Should().Be(font ?? _originalFont);
        comboBox.ForeColor.Should().Be(foreColor ?? _originalForeColor);
        manager.ComboBoxText.Should().Be(expectedText);
    }
}
