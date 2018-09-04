using System;
using System.Collections.Generic;
using System.Windows.Forms;
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
            _labelFormatter.FormatLabel(Strings.Author, Arg.Any<int>()).Returns(x => "Author:        ");
            _labelFormatter.FormatLabel(Strings.Committer, Arg.Any<int>()).Returns(x => "Committer:     ");
            _labelFormatter.FormatLabel(Strings.Date, Arg.Any<int>()).Returns(x => "Date:          ");
            _labelFormatter.FormatLabel(Strings.AuthorDate, Arg.Any<int>()).Returns(x => "Author date:   ");
            _labelFormatter.FormatLabel(Strings.CommitDate, Arg.Any<int>()).Returns(x => "Commit date:   ");
            _labelFormatter.FormatLabel(Strings.CommitHash, Arg.Any<int>()).Returns(x => "Commit hash:   ");
            _labelFormatter.FormatLabel(Strings.GetParents(1), Arg.Any<int>()).Returns(x => "Parent:        ");
            _labelFormatter.FormatLabel(Strings.GetParents(Arg.Any<int>()), Arg.Any<int>()).Returns(x => "Parents:       ");
            _labelFormatter.FormatLabel(Strings.GetChildren(1), Arg.Any<int>()).Returns(x => "Child:         ");
            _labelFormatter.FormatLabel(Strings.GetChildren(Arg.Any<int>()), Arg.Any<int>()).Returns(x => "Children:      ");

            _headerRendererStyleProvider = Substitute.For<IHeaderRenderStyleProvider>();
            _linkFactory = Substitute.For<ILinkFactory>();
            _dateFormatter = Substitute.For<IDateFormatter>();

            _renderer = new CommitDataHeaderRenderer(_labelFormatter, _dateFormatter, _headerRendererStyleProvider, _linkFactory);
        }

        [Test]
        public void GetFont_should_get_font_from_style_provider()
        {
            using (var c = new Control())
            {
                using (var g = c.CreateGraphics())
                {
                    _renderer.GetFont(g);

                    _headerRendererStyleProvider.Received().GetFont(g);
                }
            }
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
            var data = new CommitData(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                ObjectId.Random(),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(Strings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_committer_if_different_from_author()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = "John Doe <John.Doe@test.com>";
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                ObjectId.Random(),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _linkFactory.CreateLink(committer, Arg.Any<string>()).Returns(x => committer);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Committer:     John Doe <John.Doe@test.com>{Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(Strings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.Committer, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_commit_date_if_different_from_author_date()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = DateTime.Parse("2017-10-23T06:17:11+05");
            var data = new CommitData(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                ObjectId.Random(),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");
            _dateFormatter.FormatDateAsRelativeLocal(commitDate).Returns("2 months ago (10/23/2017 12:17:11)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Author date:   6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit date:   2 months ago (10/23/2017 12:17:11){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(Strings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.AuthorDate, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitDate, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Date, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Committer, Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_commit_children()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                ObjectId.Random(),
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
            _labelFormatter.Received(1).FormatLabel(Strings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_commit_parents()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                ObjectId.Random(),
                _parentHashes,
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c{Environment.NewLine}" +
                $"Parents:       {_parentHashes[0].ToShortString()} {_parentHashes[1].ToShortString()} {_parentHashes[2].ToShortString()}");
            _labelFormatter.Received(1).FormatLabel(Strings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.CommitDate, Arg.Any<int>());
        }

        [TestCase(GitRevision.IndexGuid)]
        [TestCase(GitRevision.WorkTreeGuid)]
        public void Render_should_render_minimal_info_for_artificial_commits(string artificialGuid)
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData(
                ObjectId.Parse(artificialGuid),
                ObjectId.Random(),
                _childrenHashes,
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Parents:       3b6ce324e3 2a8788ff15 8e66fa8095");
            _labelFormatter.Received(1).FormatLabel(Strings.Author, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Date, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.CommitDate, Arg.Any<int>());
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
            var data = new CommitData(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                ObjectId.Random(),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.RenderPlain(data);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(Strings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Committer, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void RenderPlain_should_render_committer_if_different_from_author()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = "John Doe <John.Doe@test.com>";
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                ObjectId.Random(),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _linkFactory.CreateLink(committer, Arg.Any<string>()).Returns(x => committer);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.RenderPlain(data);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Committer:     John Doe <John.Doe@test.com>{Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(Strings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.Date, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.Committer, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.AuthorDate, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.CommitDate, Arg.Any<int>());
        }

        [Test]
        public void RenderPlain_should_render_commit_date_if_different_from_author_date()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = DateTime.Parse("2017-10-23T06:17:11+05");
            var data = new CommitData(
                ObjectId.Parse("7fa3109989e0523aeacb178995a2a3aa6c302a2c"),
                ObjectId.Random(),
                Array.Empty<ObjectId>(),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");
            _dateFormatter.FormatDateAsRelativeLocal(commitDate).Returns("2 months ago (10/23/2017 12:17:11)");

            var result = _renderer.RenderPlain(data);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Author date:   6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit date:   2 months ago (10/23/2017 12:17:11){Environment.NewLine}Commit hash:   7fa3109989e0523aeacb178995a2a3aa6c302a2c");
            _labelFormatter.Received(1).FormatLabel(Strings.Author, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.AuthorDate, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitDate, Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.CommitHash, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Date, Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.Committer, Arg.Any<int>());
        }
    }
}
