﻿using System;
using GitUI.UserManual;
using NUnit.Framework;
using FluentAssertions;
using GitCommands;

namespace GitExtensionsTest.GitUI
{
    [TestFixture]
    public class SingleHtmlUserManualFixture
    {
        [TestCase((string)null)]
        [TestCase("merge-conflicts")]
        public void GetUrl(string anchor)
        {
            var sut = new SingleHtmlUserManual(anchor);

            var expected = SingleHtmlUserManual.Location + "/index.html".Combine("#", anchor); 

            sut.GetUrl().Should().Be(expected);
        }
    }
}
