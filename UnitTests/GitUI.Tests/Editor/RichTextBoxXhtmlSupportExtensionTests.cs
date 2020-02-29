using System;
using System.Net;
using System.Text;
using System.Windows.Forms;
using FluentAssertions;
using GitUI.Editor.RichTextBoxExtension;
using NUnit.Framework;
using ResourceManager;

namespace GitUITests.Editor
{
    [TestFixture]
    public class RichTextBoxXhtmlSupportExtensionTests
    {
        private const string _defaultLinkText = "link";
        private const string _defaultLinkUri = "https://uri.org";
        private const string _defaultPrefix = "pre ";
        private const string _defaultSuffix = " suf";

        private RichTextBox _rtb;
        private ILinkFactory _linkFactory;

        [SetUp]
        public void Setup()
        {
            _rtb = new RichTextBox();
            _linkFactory = new LinkFactory();
        }

        private void SetupLink(string prefix, string linkText, string uri, string suffix)
        {
            var text = new StringBuilder();

            if (prefix != null)
            {
                text.Append(prefix);
            }

            if (uri != null)
            {
                if (linkText == null)
                {
                    text.Append(WebUtility.HtmlEncode(uri));
                }
                else
                {
                    text.Append(_linkFactory.CreateLink(linkText, uri));
                }
            }

            if (suffix != null)
            {
                text.Append(suffix);
            }

            _rtb.SetXHTMLText(text.ToString());
        }

        private void SetupDefaultLink()
        {
            SetupLink(_defaultPrefix, _defaultLinkText, _defaultLinkUri, _defaultSuffix);
        }

        [Test]
        public void GetLink_should_return_null_if_index_is_invalid()
        {
            SetupLink(prefix: "", linkText: null, uri: null, suffix: "");
            _rtb.GetLink(-1).Should().BeNull();
            _rtb.GetLink(0).Should().BeNull();

            SetupDefaultLink();
            _rtb.GetLink(-1).Should().BeNull();
            _rtb.GetLink(_rtb.Text.Length).Should().BeNull();
        }

        [Test]
        public void GetLink_should_return_null_if_left_of_link()
        {
            SetupDefaultLink();
            for (int index = 0; index < _defaultPrefix.Length; ++index)
            {
                _rtb.GetLink(index).Should().BeNull();
            }
        }

        [Test]
        public void GetLink_should_return_null_if_right_of_link()
        {
            SetupDefaultLink();
            for (int index = _defaultPrefix.Length + _defaultLinkText.Length; index < _rtb.Text.Length; ++index)
            {
                _rtb.GetLink(index).Should().BeNull();
            }
        }

        [Test]
        public void GetLink_should_return_uri_if_at_link()
        {
            SetupDefaultLink();
            for (int index = _defaultPrefix.Length; index < _defaultPrefix.Length + _defaultLinkText.Length; ++index)
            {
                _rtb.GetLink(index).Should().Be(_defaultLinkUri);
            }
        }

        [Test]
        public void GetLink_should_return_uri_if_begins_with_link()
        {
            SetupLink(prefix: null, _defaultLinkText, _defaultLinkUri, _defaultSuffix);
            _rtb.GetLink(0).Should().Be(_defaultLinkUri);
        }

        [Test]
        public void GetLink_should_return_uri_if_link_at_line_begin()
        {
            SetupLink(_defaultPrefix + "\n", _defaultLinkText, _defaultLinkUri, _defaultSuffix);
            _rtb.GetLink(_defaultPrefix.Length + 1).Should().Be(_defaultLinkUri);
        }

        [Test]
        public void GetLink_should_return_uri_if_ends_with_link()
        {
            SetupLink(_defaultPrefix, _defaultLinkText, _defaultLinkUri, suffix: null);
            _rtb.GetLink(_defaultPrefix.Length).Should().Be(_defaultLinkUri);
        }

        [Test]
        public void GetLink_should_return_uri_if_link_at_line_end()
        {
            SetupLink(_defaultPrefix, _defaultLinkText, _defaultLinkUri, "\n" + _defaultSuffix);
            _rtb.GetLink(_defaultPrefix.Length).Should().Be(_defaultLinkUri);
        }

        [Test]
        public void GetLink_should_return_uri_if_comma_after_link()
        {
            SetupLink(_defaultPrefix, _defaultLinkText, _defaultLinkUri, "," + _defaultSuffix);
            _rtb.GetLink(_defaultPrefix.Length).Should().Be(_defaultLinkUri);
        }

        [Test]
        public void GetLink_should_return_uri_if_without_link_text()
        {
            SetupLink(_defaultPrefix, null, _defaultLinkUri, _defaultSuffix);
            _rtb.GetLink(_defaultPrefix.Length).Should().Be(_defaultLinkUri);
        }

        [Test]
        public void GetLink_should_return_uri_if_ends_with_link_without_link_text()
        {
            SetupLink(_defaultPrefix, null, _defaultLinkUri, suffix: null);
            _rtb.GetLink(_defaultPrefix.Length).Should().Be(_defaultLinkUri);
        }
    }
}