using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using NSubstitute;
using NUnit.Framework;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace ResourceManagerTests.CommitDataRenders
{
    [SetCulture("")]
    [SetUICulture("")]
    [TestFixture]
    public class CommitDataHeaderRendererTests
    {
        private IHeaderLabelFormatter _labelFormatter;
        private IHeaderRenderStyleProvider _headerRendererStyleProvider;
        private ILinkFactory _linkFactory;
        private IDateFormatter _dateFormatter;
        private CommitDataHeaderRenderer _renderer;

        [SetUp]
        public void Setup()
        {
            _labelFormatter = Substitute.For<IHeaderLabelFormatter>();
            _labelFormatter.FormatLabel(Strings.GetAuthorText(), Arg.Any<int>()).Returns(x => "Author:        ");
            _labelFormatter.FormatLabel(Strings.GetCommitterText(), Arg.Any<int>()).Returns(x => "Committer:     ");
            _labelFormatter.FormatLabel(Strings.GetDateText(), Arg.Any<int>()).Returns(x => "Date:          ");
            _labelFormatter.FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>()).Returns(x => "Author date:   ");
            _labelFormatter.FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>()).Returns(x => "Commit date:   ");
            _labelFormatter.FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>()).Returns(x => "Commit hash:   ");
            _labelFormatter.FormatLabel(Strings.GetParentsText(), Arg.Any<int>()).Returns(x => "Parent(s):     ");
            _labelFormatter.FormatLabel(Strings.GetChildrenText(), Arg.Any<int>()).Returns(x => "Children:      ");

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
            ((Action)(() => _renderer.Render(null, true))).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Render_should_not_render_committer_info_if_same_as_author_info()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData("8ea78df688ec4719a9756c1199a515d1",
                Guid.NewGuid().ToString("N"),
                new ReadOnlyCollection<string>(new List<string>()),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   8ea78df688ec4719a9756c1199a515d1");
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitterText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_committer_if_different_from_author()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = "John Doe <John.Doe@test.com>";
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData("8ea78df688ec4719a9756c1199a515d1",
                Guid.NewGuid().ToString("N"),
                new ReadOnlyCollection<string>(new List<string>()),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _linkFactory.CreateLink(committer, Arg.Any<string>()).Returns(x => committer);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Committer:     John Doe <John.Doe@test.com>{Environment.NewLine}Commit hash:   8ea78df688ec4719a9756c1199a515d1");
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitterText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_commit_date_if_different_from_author_date()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = DateTime.Parse("2017-10-23T06:17:11+05");
            var data = new CommitData("8ea78df688ec4719a9756c1199a515d1",
                Guid.NewGuid().ToString("N"),
                new ReadOnlyCollection<string>(new List<string>()),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");
            _dateFormatter.FormatDateAsRelativeLocal(commitDate).Returns("2 months ago (10/23/2017 12:17:11)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Author date:   6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit date:   2 months ago (10/23/2017 12:17:11){Environment.NewLine}Commit hash:   8ea78df688ec4719a9756c1199a515d1");
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitterText(), Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_commit_children()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData("8ea78df688ec4719a9756c1199a515d1",
                Guid.NewGuid().ToString("N"),
                new ReadOnlyCollection<string>(new List<string>()),
                author, authorDate,
                committer, commitDate, "");
            data.ChildrenGuids = new List<string> { "3b6ce324e30ed7fda24483fd56a180c34a262202", "2a8788ff15071a202505a96f80796dbff5750ddf", "8e66fa8095a86138a7c7fb22318d2f819669831e" };

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   8ea78df688ec4719a9756c1199a515d1{Environment.NewLine}Children:      3b6ce324e3 2a8788ff15 8e66fa8095");
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitterText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>());
        }

        [Test]
        public void Render_should_render_commit_parents()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData("8ea78df688ec4719a9756c1199a515d1",
                Guid.NewGuid().ToString("N"),
                new ReadOnlyCollection<string>(new List<string> { "3b6ce324e30ed7fda24483fd56a180c34a262202", "2a8788ff15071a202505a96f80796dbff5750ddf", "8e66fa8095a86138a7c7fb22318d2f819669831e" }),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   8ea78df688ec4719a9756c1199a515d1{Environment.NewLine}Parent(s):     3b6ce324e3 2a8788ff15 8e66fa8095");
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitterText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>());
        }

        [TestCase(GitRevision.IndexGuid)]
        [TestCase(GitRevision.UnstagedGuid)]
        public void Render_should_render_minimal_info_for_artificial_commits(string artificialGuid)
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData(artificialGuid,
                Guid.NewGuid().ToString("N"),
                new ReadOnlyCollection<string>(new List<string> { "3b6ce324e30ed7fda24483fd56a180c34a262202", "2a8788ff15071a202505a96f80796dbff5750ddf", "8e66fa8095a86138a7c7fb22318d2f819669831e" }),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);

            var result = _renderer.Render(data, false);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Parent(s):     3b6ce324e3 2a8788ff15 8e66fa8095");
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitterText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>());
        }

        [Test]
        public void RenderPlain_should_throw_if_data_null()
        {
            ((Action)(() => _renderer.RenderPlain(null))).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void RenderPlain_should_not_render_committer_info_if_same_as_author_info()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData("8ea78df688ec4719a9756c1199a515d1",
                Guid.NewGuid().ToString("N"),
                new ReadOnlyCollection<string>(new List<string>()),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.RenderPlain(data);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit hash:   8ea78df688ec4719a9756c1199a515d1");
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitterText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>());
        }

        [Test]
        public void RenderPlain_should_render_committer_if_different_from_author()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = "John Doe <John.Doe@test.com>";
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = authorDate;
            var data = new CommitData("8ea78df688ec4719a9756c1199a515d1",
                Guid.NewGuid().ToString("N"),
                new ReadOnlyCollection<string>(new List<string>()),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _linkFactory.CreateLink(committer, Arg.Any<string>()).Returns(x => committer);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");

            var result = _renderer.RenderPlain(data);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Date:          6 months ago (06/17/2017 23:38:40){Environment.NewLine}Committer:     John Doe <John.Doe@test.com>{Environment.NewLine}Commit hash:   8ea78df688ec4719a9756c1199a515d1");
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitterText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>());
        }

        [Test]
        public void RenderPlain_should_render_commit_date_if_different_from_author_date()
        {
            var author = "John Doe (Acme Inc) <John.Doe@test.com>";
            var committer = author;
            var authorDate = DateTime.Parse("2017-06-17T16:38:40+03");
            var commitDate = DateTime.Parse("2017-10-23T06:17:11+05");
            var data = new CommitData("8ea78df688ec4719a9756c1199a515d1",
                Guid.NewGuid().ToString("N"),
                new ReadOnlyCollection<string>(new List<string>()),
                author, authorDate,
                committer, commitDate, "");

            _linkFactory.CreateLink(author, Arg.Any<string>()).Returns(x => author);
            _dateFormatter.FormatDateAsRelativeLocal(authorDate).Returns("6 months ago (06/17/2017 23:38:40)");
            _dateFormatter.FormatDateAsRelativeLocal(commitDate).Returns("2 months ago (10/23/2017 12:17:11)");

            var result = _renderer.RenderPlain(data);

            result.Should().Be($"Author:        John Doe (Acme Inc) <John.Doe@test.com>{Environment.NewLine}Author date:   6 months ago (06/17/2017 23:38:40){Environment.NewLine}Commit date:   2 months ago (10/23/2017 12:17:11){Environment.NewLine}Commit hash:   8ea78df688ec4719a9756c1199a515d1");
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetAuthorDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitDateText(), Arg.Any<int>());
            _labelFormatter.Received(1).FormatLabel(Strings.GetCommitHashText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetDateText(), Arg.Any<int>());
            _labelFormatter.DidNotReceive().FormatLabel(Strings.GetCommitterText(), Arg.Any<int>());
        }
    }
}
