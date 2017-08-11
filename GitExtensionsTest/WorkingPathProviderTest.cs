using FluentAssertions;
using GitCommands;
using GitExtensions;
using GitUIPluginInterfaces;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
            NSubstituteHelper.RegisterAsInstanceFactory();
            _ext = new WorkingPathProvider.Exterior();
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
            //act
            string workingDir = _workingPathProvider.GetWorkingDir(new string[0]);
            //assert
            workingDir.Should().Be(unitTestRecentWorkingDir);
        }

        [Test]
        public void ThrowOnUnconfiguredCall()
        {
            Action act = () =>
            {
                //arange
                _ext.StartWithRecentWorkingDir = false;
                string unitTestRecentWorkingDir = "unitTestRecentWorkingDir";
                _ext.RecentWorkingDir = unitTestRecentWorkingDir;
                _ext.IsValidGitWorkingDir = (dirPath) => unitTestRecentWorkingDir.Equals(dirPath);
                NSubstituteHelper.ThrowOnUnconfiguredCall();
                //act
                string workingDir = _workingPathProvider.GetWorkingDir(new string[0]);
                //assert
                workingDir.Should().Be(string.Empty);
            };

            act.ShouldThrow<UnconfiguredCallException>();
        }

        [Test]
        public void DontThrowOnUnconfiguredCall()
        {
            //arange
            _ext.StartWithRecentWorkingDir = false;
            string unitTestRecentWorkingDir = "unitTestRecentWorkingDir";
            _ext.RecentWorkingDir = unitTestRecentWorkingDir;
            _ext.IsValidGitWorkingDir = (dirPath) => unitTestRecentWorkingDir.Equals(dirPath);
            //act
            string workingDir = _workingPathProvider.GetWorkingDir(new string[0]);
            //assert
            workingDir.Should().Be(string.Empty);
        }



    }

    [Serializable()]
    public class UnconfiguredCallException : Exception
    { }

    class ThrowIfUnconfigured : ICallHandler
    {
        public RouteAction Handle(ICall call)
        {            
            throw new UnconfiguredCallException();
        }
    }

    public class NSubstituteHelper : IInstanceFactory
    {
        private static List<object> substitutes = new List<object>();

        public static void RegisterAsInstanceFactory()
        {
            ClearStaticInstances();
            StaticDI.InstanceFactory = new NSubstituteHelper();
        }

        public T CreateInstance<T>() where T : class, new()
        {
            T sut = Substitute.For<T>();
            substitutes.Add(sut);

            return sut;
        }

        private static void ClearStaticInstances()
        {
            substitutes.Clear();
            StaticDI.ClearInstances();
        }

        public static void ThrowOnUnconfiguredCall()
        {
            CallHandlerFactory throwingFactory = s => new ThrowIfUnconfigured();
            foreach (object sut in substitutes)
            {
                var sub = SubstitutionContext.Current.GetCallRouterFor(sut);
                sub.RegisterCustomCallHandlerFactory(throwingFactory);
            }
        }
    }
}

