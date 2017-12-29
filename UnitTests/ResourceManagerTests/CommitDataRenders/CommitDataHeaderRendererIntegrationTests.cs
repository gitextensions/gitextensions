using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using GitCommands;
using NUnit.Framework;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace ResourceManagerTests.CommitDataRenders
{
    [SetCulture("")]
    [SetUICulture("")]
    [TestFixture]
    public class CommitDataHeaderRendererIntegrationTests
    {
        private CommitData _data;
        private CommitDataHeaderRenderer _rendererTabs;
        private CommitDataHeaderRenderer _rendererSpaces;

        [SetUp]
        public void Setup()
        {
            var commitGuid = Guid.NewGuid().ToString("N");
            var treeGuid = Guid.NewGuid().ToString("N");
            var parentGuid1 = Guid.NewGuid().ToString("N");
            var parentGuid2 = Guid.NewGuid().ToString("N");
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);

            _data = new CommitData(commitGuid, treeGuid,
                new ReadOnlyCollection<string>(new List<string> { parentGuid1, parentGuid2 }),
                "John Doe (Acme Inc) <John.Doe@test.com>", authorTime,
                "Jane Doe <Jane.Doe@test.com>", commitTime,
                "\tI made a really neato change.\n\nNotes (p4notes):\n\tP4@547123");
            _data.ChildrenGuids = new List<string> { "3b6ce324e30ed7fda24483fd56a180c34a262202", "2a8788ff15071a202505a96f80796dbff5750ddf", "8e66fa8095a86138a7c7fb22318d2f819669831e" };

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
                                 "Commit hash:	" + _data.Guid + Environment.NewLine +
                                 "Children:		<a href='gitext://gotocommit/" + _data.ChildrenGuids[0] + "'>" + _data.ChildrenGuids[0].Substring(0, 10) + "</a> <a href='gitext://gotocommit/" + _data.ChildrenGuids[1] + "'>" + _data.ChildrenGuids[1].Substring(0, 10) + "</a> <a href='gitext://gotocommit/" + _data.ChildrenGuids[2] + "'>" + _data.ChildrenGuids[2].Substring(0, 10) + "</a>" + Environment.NewLine +
                                 "Parent(s):		<a href='gitext://gotocommit/" + _data.ParentGuids[0] + "'>" + _data.ParentGuids[0].Substring(0, 10) + "</a> <a href='gitext://gotocommit/" + _data.ParentGuids[1] + "'>" + _data.ParentGuids[1].Substring(0, 10) + "</a>";

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
                                 "Commit hash:	" + _data.Guid + Environment.NewLine +
                                 "Children:		" + _data.ChildrenGuids[0].Substring(0, 10) + " " + _data.ChildrenGuids[1].Substring(0, 10) + " " + _data.ChildrenGuids[2].Substring(0, 10) + Environment.NewLine +
                                 "Parent(s):		" + _data.ParentGuids[0].Substring(0, 10) + " " + _data.ParentGuids[1].Substring(0, 10);

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
                                 "Commit hash: " + _data.Guid + Environment.NewLine +
                                 "Children:    <a href='gitext://gotocommit/" + _data.ChildrenGuids[0] + "'>" + _data.ChildrenGuids[0].Substring(0, 10) + "</a> <a href='gitext://gotocommit/" + _data.ChildrenGuids[1] + "'>" + _data.ChildrenGuids[1].Substring(0, 10) + "</a> <a href='gitext://gotocommit/" + _data.ChildrenGuids[2] + "'>" + _data.ChildrenGuids[2].Substring(0, 10) + "</a>" + Environment.NewLine +
                                 "Parent(s):   <a href='gitext://gotocommit/" + _data.ParentGuids[0] + "'>" + _data.ParentGuids[0].Substring(0, 10) + "</a> <a href='gitext://gotocommit/" + _data.ParentGuids[1] + "'>" + _data.ParentGuids[1].Substring(0, 10) + "</a>";

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
                                 "Commit hash: " + _data.Guid + Environment.NewLine +
                                 "Children:    " + _data.ChildrenGuids[0].Substring(0, 10) + " " + _data.ChildrenGuids[1].Substring(0, 10) + " " +_data.ChildrenGuids[2].Substring(0, 10) + Environment.NewLine +
                                 "Parent(s):   " + _data.ParentGuids[0].Substring(0, 10) + " " + _data.ParentGuids[1].Substring(0, 10);

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
                                 "Commit hash:	" + _data.Guid;

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
                                 "Commit hash: " + _data.Guid;

            var result = _rendererSpaces.RenderPlain(_data);

            result.Should().Be(expectedHeader);
        }
    }
}
