using System;
using System.IO;
using FluentAssertions;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class ExceptionUtilsTests
    {
        [Test]
        public void HasInnerOfType()
        {
            var ex1 = new DivideByZeroException("boom");
            var ex2 = new ArgumentException("far", ex1);
            var ex3 = new FileNotFoundException("opps", ex2);

            ex3.HasInnerOfType<ArgumentException>().Should().BeTrue();
            ex3.HasInnerOfType<DivideByZeroException>().Should().BeTrue();
        }

        [Test]
        public void InnerOfType()
        {
            var ex1 = new DivideByZeroException("boom");
            var ex2 = new ArgumentException("far", ex1);
            var ex3 = new FileNotFoundException("opps", ex2);

            ex3.InnerOfType<ArgumentException>().Should().Be(ex2);
            ex3.InnerOfType<DivideByZeroException>().Should().Be(ex1);
        }
    }
}