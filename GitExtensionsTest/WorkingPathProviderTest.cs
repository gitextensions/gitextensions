using FluentAssertions;
using GitCommands;
using GitExtensions;
using GitUIPluginInterfaces;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using System;

namespace GitExtensionsTest
{
    [TestFixture]
    public class WorkingPathProviderTests
    {
        private WorkingPathProvider.Exterior _ext;
        private WorkingPathProvider _workingPathProvider;

        [SetUp]
        public void Setup()
        {
            _ext = new WorkingPathProvider.Exterior();
            _ext.Directory = Substitute.For<DirectoryGateway>();
            _workingPathProvider = new WorkingPathProvider(_ext);
        }

        [Test]
        public void ReturnsRecentDirectory_if_RecentDirectory_IsValidGitWorkingDir()
        {
            //arange
            _ext.Directory.CurrentDirectory.Returns(string.Empty);
            _ext.StartWithRecentWorkingDir = true;
            string unitTestRecentWorkingDir = "unitTestRecentWorkingDir";
            _ext.RecentWorkingDir = unitTestRecentWorkingDir;
            _ext.IsValidGitWorkingDir = (dirPath) => unitTestRecentWorkingDir.Equals(dirPath);
            //act
            string workingDir = _workingPathProvider.GetWorkingDir(new string[0]);
            //assert
            workingDir.Should().Be(unitTestRecentWorkingDir);
        }

        [Test]
        public void ReturnsRecentDirectory_if_RecentDirectory_is_not_ValidGitWorkingDir()
        {
            //arange
            _ext.Directory.CurrentDirectory.Returns(string.Empty);
            _ext.StartWithRecentWorkingDir = true;
            string unitTestRecentWorkingDir = "unitTestRecentWorkingDir";
            _ext.RecentWorkingDir = unitTestRecentWorkingDir;
            _ext.IsValidGitWorkingDir = (dirPath) => unitTestRecentWorkingDir.Equals(dirPath);

            var sub = SubstitutionContext.Current.GetCallRouterFor(_ext.Directory);
            sub.RegisterCustomCallHandlerFactory(s => new ThrowIfUnconfigured());
            //act
            string workingDir = _workingPathProvider.GetWorkingDir(new string[0]);
           // _ext.Directory.DirectoryExists("");
            //assert
            workingDir.Should().Be(unitTestRecentWorkingDir);
        }


    }

    class ThrowIfUnconfigured : ICallHandler
    {
        public RouteAction Handle(ICall call)
        {            
            throw new Exception("Unconfigured call");
        }
    }

//    public static class NSubstituteExt
  //  {
   //     public static void 
    //}
}

