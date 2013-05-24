using GitUI.UserControls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;

namespace GitExtensionsTest.GitUI
{
    [TestFixture]
    public class GotoUserManualControlFixture
    {
        [Test]
        public void GetUrlTest()
        {
            var ctrl = new GotoUserManualControl();

            ctrl.ManualLocation = "https://gitextensions.readthedocs.org/en/latest";
            ctrl.ManualType = ManualType.StandardHtml;

            ctrl.ManualSectionSubfolder = null;
            ctrl.ManualSectionAnchorName = null;
            ctrl.GetUrl().Should().Be("https://gitextensions.readthedocs.org/en/latest//"); // both null makes no sense atm
            
            ctrl.ManualSectionSubfolder = "merge_conflicts";
            ctrl.ManualSectionAnchorName = null;
            ctrl.GetUrl().Should().Be("https://gitextensions.readthedocs.org/en/latest/merge_conflicts/");

            ctrl.ManualSectionSubfolder = "merge_conflicts";
            ctrl.ManualSectionAnchorName = "merge-conflicts";
            ctrl.GetUrl().Should().Be("https://gitextensions.readthedocs.org/en/latest/merge_conflicts/#merge-conflicts");


            ctrl.ManualLocation = "file:///D:/data2/projects/gitextensions/GitExtensionsDoc/build/singlehtml";
            ctrl.ManualType = ManualType.SingleHtml;

            ctrl.ManualSectionSubfolder = null;
            ctrl.ManualSectionAnchorName = null;
            ctrl.GetUrl().Should().Be("file:///D:/data2/projects/gitextensions/GitExtensionsDoc/build/singlehtml/index.html");

            ctrl.ManualSectionSubfolder = "merge_conflicts";
            ctrl.ManualSectionAnchorName = null;
            ctrl.GetUrl().Should().Be("file:///D:/data2/projects/gitextensions/GitExtensionsDoc/build/singlehtml/index.html");

            ctrl.ManualSectionSubfolder = "merge_conflicts";
            ctrl.ManualSectionAnchorName = "merge-conflicts";
            ctrl.GetUrl().Should().Be("file:///D:/data2/projects/gitextensions/GitExtensionsDoc/build/singlehtml/index.html#merge-conflicts");
        }
    }
}
