using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GitUI.CommitInfo;
using NUnit.Framework;
using ResourceManager;

namespace GitUITests.UserControls.CommitInfo
{
    public class RefsFormatterTests
    {
        private readonly IReadOnlyList<string> _refs
            = new List<string> { "r1", null, "r3", "r4", "r5", "r6", "r7", "r8", "r9", "r10", "r11", "r12", "r13" };

        // Created once for each test
        private ILinkFactory _linkFactory;
        private RefsFormatter _refsFormatter;

        [SetUp]
        public void SetUp()
        {
            _linkFactory = new LinkFactory();
            _refsFormatter = new RefsFormatter(_linkFactory);
        }

        [Test]
        public void LinkFactoryNull()
        {
            ((Action)(() => new RefsFormatter(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void RefsNull()
        {
            _refsFormatter.FormatBranches(branches: null, showAsLinks: true, limit: true)
                .Should().Be(string.Empty);

            _refsFormatter.FormatTags(tags: null, showAsLinks: true, limit: true)
                .Should().Be(string.Empty);
        }

        [Test]
        public void Empty([Values(true, false)] bool showAsLinks, [Values(true, false)] bool limit)
        {
            IReadOnlyList<string> refs = new List<string>();

            _refsFormatter.FormatBranches(refs, showAsLinks, limit)
                .Should().Be(GitUI.Strings.ContainedInNoBranch);

            _refsFormatter.FormatTags(refs, showAsLinks, limit)
                .Should().Be(GitUI.Strings.ContainedInNoTag);
        }

        [Test]
        public void LimitNotExceededOrNotLimited([Values(1, 2, 3, 9, 10, 11, 12, 13)] int count, [Values(true, false)] bool showAsLinks)
        {
            bool limit = count <= 12;
            IReadOnlyList<string> refs = _refs.Take(count).ToList();
            IEnumerable<string> formattedBranches = refs.Select(r => FormatRef(r, "branch", showAsLinks));
            IEnumerable<string> formattedTags = refs.Select(r => FormatRef(r, "tag", showAsLinks));

            _refsFormatter.FormatBranches(refs, showAsLinks, limit)
                .Should().Be(GitUI.Strings.ContainedInBranches
                             + Environment.NewLine
                             + formattedBranches.Join(Environment.NewLine));

            _refsFormatter.FormatTags(refs, showAsLinks, limit)
                .Should().Be(GitUI.Strings.ContainedInTags
                             + Environment.NewLine
                             + formattedTags.Join(Environment.NewLine));
        }

        [Test]
        public void LimitExceeded([Values(true, false)] bool showAsLinks)
        {
            IEnumerable<string> formattedBranches = _refs.Take(10).Select(r => FormatRef(r, "branch", showAsLinks));
            IEnumerable<string> formattedTags = _refs.Take(10).Select(r => FormatRef(r, "tag", showAsLinks));

            _refsFormatter.FormatBranches(_refs, showAsLinks, limit: true)
                .Should().Be(GitUI.Strings.ContainedInBranches
                             + Environment.NewLine
                             + formattedBranches.Join(Environment.NewLine)
                             + GetShowAllLink("branches"));

            _refsFormatter.FormatTags(_refs, showAsLinks, limit: true)
                .Should().Be(GitUI.Strings.ContainedInTags
                             + Environment.NewLine
                             + formattedTags.Join(Environment.NewLine)
                             + GetShowAllLink("tags"));
        }

        private string FormatRef(string r, string type, bool showAsLinks)
        {
            return showAsLinks ? $"<a href='gitext://goto{type}/{r}'>{r}</a>" : r;
        }

        private string GetShowAllLink(string type)
        {
            return $"{Environment.NewLine}<a href='gitext://showall/{type}'>[ {Strings.ShowAll} ]</a>{Environment.NewLine}";
        }
    }
}
