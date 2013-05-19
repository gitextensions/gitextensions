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
            ctrl.GetUrl().Should().Be("https://gitextensions.readthedocs.org/en/latest/");

            ctrl.ManualSectionSubfolder = "aaa";
            ctrl.ManualSectionAnchorName = null;
            ctrl.GetUrl().Should().Be("https://gitextensions.readthedocs.org/en/latest/aaa");

            ctrl.ManualSectionSubfolder = "bbb";
            ctrl.ManualSectionAnchorName = "ccc";
            ctrl.GetUrl().Should().Be("https://gitextensions.readthedocs.org/en/latest/bbb#ccc");
        }
    }
}
