using FluentAssertions;
using GitCommands;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace ResourceManagerTests.CommitDataRenders
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class CommitDataHeaderRendererTests
    {
        private IHeaderLabelFormatter _labelFormatter;
        private IHeaderRenderStyleProvider _headerRendererStyleProvider;
        private ILinkFactory _linkFactory;
        private IDateFormatter _dateFormatter;
        private CommitDataHeaderRenderer _renderer;

        private readonly IReadOnlyList<ObjectId> _childrenHashes = new[]
        {
            ObjectId.Parse("3b6ce324e30ed7fda24483fd56a180c34a262202"),
            ObjectId.Parse("2a8788ff15071a202505a96f80796dbff5750ddf"),
            ObjectId.Parse("8e66fa8095a86138a7c7fb22318d2f819669831e")
        };

        private readonly IReadOnlyList<ObjectId> _parentHashes = new[]
        {
            ObjectId.Parse("5542334ab518b329426783d74c8f4204c2d75a43"),
            ObjectId.Parse("92bc4ad5e509f7dbe87dc4e679fcb879c3235788"),
            ObjectId.Parse("bc911920838c15bcf86808904ecb897595b9ef5f")
        };

        [SetUp]
        public void Setup()
        {
            _labelFormatter = Substitute.For<IHeaderLabelFormatter>();
            _labelFormatter.FormatLabel(TranslatedStrings.Author, Arg.Any<int>()).Returns(x => "Author:        ");
            _labelFormatter.FormatLabel(TranslatedStrings.Committer, Arg.Any<int>()).Returns(x => "Committer:     ");
            _labelFormatter.FormatLabel(TranslatedStrings.Date, Arg.Any<int>()).Returns(x => "Date:          ");
            _labelFormatter.FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>()).Returns(x => "Author date:   ");
            _labelFormatter.FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>()).Returns(x => "Commit date:   ");
            _labelFormatter.FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>()).Returns(x => "Commit hash:   ");
            _labelFormatter.FormatLabel(TranslatedStrings.GetParents(1), Arg.Any<int>()).Returns(x => "Parent:        ");
            _labelFormatter.FormatLabel(TranslatedStrings.GetParents(3), Arg.Any<int>()).Returns(x => "Parents:       ");
            _labelFormatter.FormatLabel(TranslatedStrings.GetChildren(1), Arg.Any<int>()).Returns(x => "Child:         ");
            _labelFormatter.FormatLabel(TranslatedStrings.GetChildren(3), Arg.Any<int>()).Returns(x => "Children:      ");

            _headerRendererStyleProvider = Substitute.For<IHeaderRenderStyleProvider>();
            _linkFactory = Substitute.For<ILinkFactory>();
            _dateFormatter = Substitute.For<IDateFormatter>();

            _renderer = new CommitDataHeaderRenderer(_labelFormatter, _dateFormatter, _headerRendererStyleProvider, _linkFactory);
        }

        [Test]
        public void GetFont_should_get_font_from_style_provider()
        {
            using Control c = new();
            using var g = c.CreateGraphics();
            _renderer.GetFont(g);

            _headerRendererStyleProvider.Received().GetFont(g);
        }

        [Test]
        public void GetTabStops_should_get_stops_from_style_provider()
        {
            _renderer.GetTabStops();

            _headerRendererStyleProvider.Received().GetTabStops();
        }

        [Test]
        public void Render_should_throw_if_data_null()
        {
            ((Action)(() => _renderer.Render(null, true))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Render_should_not_render_committer_info_if_same_as_author_info()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            CommitData data = new(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_committer_if_different_from_author()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = "John Doe <John.Doe@test.com>";
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            CommitData data = new(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _linkFactory.CreateLink(committer, Arg.Any<string>()).Returns(x => committer);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Committer:     John Doe <John.Doe@test.com>{Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Committer, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_commit_date_if_different_from_author_date()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = DateTime.Parse("2017-10-23T06:17:11+05");
            CommitData data = new(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");
            _dateFormatter.FormatDateAsRelativeLocal(commitDate).Returns("2 months ago (10/23/2017 12:17:11)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Author date:   6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit date:   2 months ago (10/23/2017 12:17:11){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Date, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Committer, Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_commit_children()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            CommitData data = new(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");
            data.ChildIds = _childrenHashes;

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c{Environment.NewLine}" +
                $"Children:      {_childrenHashes[0].ToShortString()} " +
                $"{_childrenHashes[1].ToShortString()} " +
                $"{_childrenHashes[2].ToShortString()}");
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_commit_parents()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            CommitData data = new(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                _parentHashes,
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c{Environment.NewLine}" +
                $"Parents:       {_parentHashes[0].ToShortString()} {_parentHashes[1].ToShortString()} {_parentHashes[2].ToShortString()}");
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>());
        }

        [TestCase(GitRevision.IndexGuid)]
        [TestCase(GitRevision.WorkTreeGuid)]
        public void Render_should_render_minimal_info_for_artificial_commits(string artificialGuid)
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            CommitData data = new(
                ObjectId.Parse(artificialGuid),
                _parentHashes,
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}" +
                               $"Parents:       {_parentHashes[0].ToShortString()} {_parentHashes[1].ToShortString()} {_parentHashes[2].ToShortString()}");
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Author, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Date, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void RenderPlain_should_throw_if_data_null()
        {
            ((Action)(() => _renderer.RenderPlain(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void RenderPlain_should_not_render_committer_info_if_same_as_author_info()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            CommitData data = new(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.RenderPlain(data);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void RenderPlain_should_render_committer_if_different_from_author()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = "John Doe <John.Doe@test.com>";
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            CommitData data = new(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _linkFactory.CreateLink(committer, Arg.Any<string>()).Returns(x => committer);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.RenderPlain(data);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Committer:     John Doe <John.Doe@test.com>{Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Committer, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void RenderPlain_should_render_commit_date_if_different_from_author_date()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = DateTime.Parse("2017-10-23T06:17:11+05");
            CommitData data = new(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");
            _dateFormatter.FormatDateAsRelativeLocal(commitDate).Returns("2 months ago (10/23/2017 12:17:11)");

            var result = _renderer.RenderPlain(data);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Author date:   6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit date:   2 months ago (10/23/2017 12:17:11){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.AuthorDate, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitDate, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(TranslatedStrings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Date, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(TranslatedStrings.Committer, Arg.Any<int>());
        }

        [Test]
        public void GetPlainText_should_repace_multiple_spaces_and_tabs()
        {
            string header = "label 1:  value 1\nlabel 2:\tvalue 2\nlabel 3:\t value 3\nlabel 4: value 4";
            string expected = "label 1: value 1\nlabel 2: value 2\nlabel 3: value 3\nlabel 4: value 4";
            _renderer.GetPlainText(header).Should().Be(expected);
        }

        [Test]
        public void GetPlainText_should_remove_relative_time([Values("1 minute", "3 years")] string relativeTime)
        {
            string header = $"\ndate: {relativeTime} ago (time)\nlabel 2: value 2";
            string expected = "\ndate: time\nlabel 2: value 2";
            _renderer.GetPlainText(header).Should().Be(expected);
        }

        [Test]
        public void GetPlainText_should_not_remove_ago_from_author([Values("mago@x.y", "x@blagoq.org", "ago <x@y.z>")] string author)
        {
            string header = $"\nAuthor: {author}\nlabel 2: value 2";
            _renderer.GetPlainText(header).Should().Be(header);
        }

        [Test]
        public void GetPlainText_should_remove_child_and_parent_commits([Values(
            "Child: 123bcdef",
            "Children: 123bcdef, 234abcd",
            "Parent: 123bcdef",
            "Parents: 123bcdef, 234abcd",
            "Children: 123bcdef, 234abcd\nParent: 345bcdef")]
            string relatives)
        {
            string header = $"label 1: value 1\n{relatives}\nlabel 3: value 3";
            string expected = $"label 1: value 1\nlabel 3: value 3";
            _renderer.GetPlainText(header).Should().Be(expected);
        }
    }
}
