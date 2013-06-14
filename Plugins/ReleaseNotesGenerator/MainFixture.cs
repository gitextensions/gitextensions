using NUnit.Framework;
using ReleaseNotesGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;

namespace GitExtensionsTest.GitUI
{
    [TestFixture]
    public class ReleaseNoteGeneratorPluginFixture
    {
        [Test]
        public void FormatStringTest()
        {
            HtmlFragment.To8DigitString(15).Should().Be("00000015");
        }

        [Test]
        [Ignore("Modifies clipboard content / Thread need STA mode")]
        public void CopyToClipboard()
        {
            HtmlFragment.CopyToClipboard("<p>Hallo</p>");
        }
    }
}
