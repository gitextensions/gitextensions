using System;
using FluentAssertions;
using GitCommands;
using GitUIPluginInterfaces;
using NUnit.Framework;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace ResourceManagerTests.CommitDataRenders
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class CommitDataHeaderRendererIntegrationTests
    {
        private CommitData _data;
        private CommitDataHeaderRenderer _rendererTabs;
        private CommitDataHeaderRenderer _rendererSpaces;

        [SetUp]
        public void Setup()
        {
            var commitGuid = ObjectId.Random();
            var treeGuid = ObjectId.Random();
            var parentGuid1 = ObjectId.Random();
            var parentGuid2 = ObjectId.Random();
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);

            _data = new CommitData(
                commitGuid, treeGuid,
                new[] { parentGuid1, parentGuid2 },
                "John Doe (Acme Inc) <John.Doe@test.com>", authorTime,
                "Jane Doe <Jane.Doe@test.com>", commitTime,
                "\tI made a really neat change.\n\nNotes (p4notes):\n\tP4@547123")
            {
                ChildIds = new[]
                {
                    ObjectId.Parse("3b6ce324e30ed7fda24483fd56a180c34a262202"),
                    ObjectId.Parse("2a8788ff15071a202505a96f80796dbff5750ddf"),
                    ObjectId.Parse("8e66fa8095a86138a7c7fb22318d2f819669831e")
                }
            };

            _rendererTabs = new CommitDataHeaderRenderer(new TabbedHeaderLabelFormatter(), new DateFormatter(), new TabbedHeaderRenderStyleProvider(), new LinkFactory());
            _rendererSpaces = new CommitDataHeaderRenderer(new MonospacedHeaderLabelFormatter(), new DateFormatter(), new MonospacedHeaderRenderStyleProvider(), new LinkFactory());
        }

        [Test]
        public void Render_with_tabs_and_links()
        {
            var expectedHeader = "Author:			<a href='mailto:John.Doe@test.com'>John Doe (Acme Inc) &lt;John.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date:	3 days ago (" + LocalizationHelpers.GetFullDateString(_data.AuthorDate) + ")" + Environment.NewLine +
                                 "Committer:		<a href='mailto:Jane.Doe@test.com'>Jane Doe &lt;Jane.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date:	2 days ago (" + LocalizationHelpers.GetFullDateString(_data.CommitDate) + ")" + Environment.NewLine +
                                 "Commit hash:	" + _data.ObjectId + Environment.NewLine +
                                 "Children:		" +
                                   "<a href='gitext://gotocommit/" + _data.ChildIds[0] + "'>" + _data.ChildIds[0].ToShortString() + "</a> " +
                                   "<a href='gitext://gotocommit/" + _data.ChildIds[1] + "'>" + _data.ChildIds[1].ToShortString() + "</a> " +
                                   "<a href='gitext://gotocommit/" + _data.ChildIds[2] + "'>" + _data.ChildIds[2].ToShortString() + "</a>" + Environment.NewLine +
                                 "Parents:		" +
                                   "<a href='gitext://gotocommit/" + _data.ParentGuids[0] + "'>" + _data.ParentGuids[0].ToShortString() + "</a> " +
                                   "<a href='gitext://gotocommit/" + _data.ParentGuids[1] + "'>" + _data.ParentGuids[1].ToShortString() + "</a>";

            var result = _rendererTabs.Render(_data, true);

            result.Should().Be(expectedHeader);
        }

        [Test]
        public void Render_with_tabs_no_links()
        {
            var expectedHeader = "Author:			<a href='mailto:John.Doe@test.com'>John Doe (Acme Inc) &lt;John.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date:	3 days ago (" + LocalizationHelpers.GetFullDateString(_data.AuthorDate) + ")" + Environment.NewLine +
                                 "Committer:		<a href='mailto:Jane.Doe@test.com'>Jane Doe &lt;Jane.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date:	2 days ago (" + LocalizationHelpers.GetFullDateString(_data.CommitDate) + ")" + Environment.NewLine +
                                 "Commit hash:	" + _data.ObjectId + Environment.NewLine +
                                 "Children:		" +
                                   _data.ChildIds[0].ToShortString() + " " +
                                   _data.ChildIds[1].ToShortString() + " " +
                                   _data.ChildIds[2].ToShortString() + Environment.NewLine +
                                 "Parents:		" +
                                   _data.ParentGuids[0].ToShortString() + " " +
                                   _data.ParentGuids[1].ToShortString();

            var result = _rendererTabs.Render(_data, false);

            result.Should().Be(expectedHeader);
        }

        [Test]
        public void Render_with_spaces_with_links()
        {
            var expectedHeader = "Author:      <a href='mailto:John.Doe@test.com'>John Doe (Acme Inc) &lt;John.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date: 3 days ago (" + LocalizationHelpers.GetFullDateString(_data.AuthorDate) + ")" + Environment.NewLine +
                                 "Committer:   <a href='mailto:Jane.Doe@test.com'>Jane Doe &lt;Jane.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date: 2 days ago (" + LocalizationHelpers.GetFullDateString(_data.CommitDate) + ")" + Environment.NewLine +
                                 "Commit hash: " + _data.ObjectId + Environment.NewLine +
                                 "Children:    " +
                                   "<a href='gitext://gotocommit/" + _data.ChildIds[0] + "'>" + _data.ChildIds[0].ToShortString() + "</a> " +
                                   "<a href='gitext://gotocommit/" + _data.ChildIds[1] + "'>" + _data.ChildIds[1].ToShortString() + "</a> " +
                                   "<a href='gitext://gotocommit/" + _data.ChildIds[2] + "'>" + _data.ChildIds[2].ToShortString() + "</a>" + Environment.NewLine +
                                 "Parents:     " +
                                   "<a href='gitext://gotocommit/" + _data.ParentGuids[0] + "'>" + _data.ParentGuids[0].ToShortString() + "</a> " +
                                   "<a href='gitext://gotocommit/" + _data.ParentGuids[1] + "'>" + _data.ParentGuids[1].ToShortString() + "</a>";

            var result = _rendererSpaces.Render(_data, true);

            result.Should().Be(expectedHeader);
        }

        [Test]
        public void Render_with_spaces_no_links()
        {
            var expectedHeader = "Author:      <a href='mailto:John.Doe@test.com'>John Doe (Acme Inc) &lt;John.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date: 3 days ago (" + LocalizationHelpers.GetFullDateString(_data.AuthorDate) + ")" + Environment.NewLine +
                                 "Committer:   <a href='mailto:Jane.Doe@test.com'>Jane Doe &lt;Jane.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date: 2 days ago (" + LocalizationHelpers.GetFullDateString(_data.CommitDate) + ")" + Environment.NewLine +
                                 "Commit hash: " + _data.ObjectId + Environment.NewLine +
                                 "Children:    " +
                                   _data.ChildIds[0].ToShortString() + " " +
                                   _data.ChildIds[1].ToShortString() + " " +
                                   _data.ChildIds[2].ToShortString() + Environment.NewLine +
                                 "Parents:     " +
                                   _data.ParentGuids[0].ToShortString() + " " +
                                   _data.ParentGuids[1].ToShortString();

            var result = _rendererSpaces.Render(_data, false);

            result.Should().Be(expectedHeader);
        }

        [Test]
        public void RenderPlain_with_tabs()
        {
            var expectedHeader = "Author:			John Doe (Acme Inc) <John.Doe@test.com>" + Environment.NewLine +
                                 "Author date:	3 days ago (" + LocalizationHelpers.GetFullDateString(_data.AuthorDate) + ")" + Environment.NewLine +
                                 "Committer:		Jane Doe <Jane.Doe@test.com>" + Environment.NewLine +
                                 "Commit date:	2 days ago (" + LocalizationHelpers.GetFullDateString(_data.CommitDate) + ")" + Environment.NewLine +
                                 "Commit hash:	" + _data.ObjectId;

            var result = _rendererTabs.RenderPlain(_data);

            result.Should().Be(expectedHeader);
        }

        [Test]
        public void RenderPlain_with_spaces()
        {
            var expectedHeader = "Author:      John Doe (Acme Inc) <John.Doe@test.com>" + Environment.NewLine +
                                 "Author date: 3 days ago (" + LocalizationHelpers.GetFullDateString(_data.AuthorDate) + ")" + Environment.NewLine +
                                 "Committer:   Jane Doe <Jane.Doe@test.com>" + Environment.NewLine +
                                 "Commit date: 2 days ago (" + LocalizationHelpers.GetFullDateString(_data.CommitDate) + ")" + Environment.NewLine +
                                 "Commit hash: " + _data.ObjectId;

            var result = _rendererSpaces.RenderPlain(_data);

            result.Should().Be(expectedHeader);
        }
    }
}
