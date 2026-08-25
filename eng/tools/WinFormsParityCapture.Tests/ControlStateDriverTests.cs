using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Category("P0_1")]
public sealed class ControlStateDriverTests
{
    [Test]
    public void Apply_should_resize_and_restore_the_client_surface()
    {
        using Form form = new() { ClientSize = new Size(240, 140) };
        form.Show();
        CaptureStatePlan state = new()
        {
            Id = "resized",
            Kind = CaptureStateKind.Normal,
            WidthDip = 340,
            HeightDip = 220
        };

        using (ControlStateDriver.Apply(form, state))
        {
            form.ClientSize.Should().Be(new Size(340, 220));
        }

        form.ClientSize.Should().Be(new Size(240, 140));
    }
}
