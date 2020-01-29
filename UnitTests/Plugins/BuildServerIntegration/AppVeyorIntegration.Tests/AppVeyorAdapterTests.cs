using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ApprovalTests;
using AppVeyorIntegration;
using CommonTestUtils;
using FluentAssertions;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using NSubstitute;
using NUnit.Framework;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace AppVeyorIntegrationTests
{
    [TestFixture]
    public class AppVeyorAdapterTests
    {
        private AppVeyorAdapter.Project _project = new AppVeyorAdapter.Project
            { Id = "ProjectId", Name = "ProjectName", QueryUrl = "ProjectQueryUrl" };

        [Test]
        public void Should_return_no_build_Info_When_Api_Json_is_empty()
        {
            var buildInfo = new AppVeyorAdapter().ExtractBuildInfo(_project, string.Empty);

            buildInfo.Should().HaveCount(0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Test]
        public void Should_return_a_build_Info_When_Json_content_is_the_one_of_a_pull_request_build()
        {
            Approvals.Verify(BuildBuildInfoForFile("AppVeyorResult_pull_request_build.json"));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Test]
        public void Should_return_a_build_Info_When_Json_content_is_the_one_of_a_master_build()
        {
            Approvals.Verify(BuildBuildInfoForFile("AppVeyorResult_master.json"));
        }

        private string BuildBuildInfoForFile(string filename)
        {
            var resultString = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(),
                $"{GetType().Namespace}.MockData.{filename}");
            var appVeyorAdapter = new AppVeyorAdapter();
            appVeyorAdapter.Initialize(Substitute.For<IBuildServerWatcher>(), Substitute.For<ISettingsSource>(), id => true);

            var buildInfo = appVeyorAdapter.ExtractBuildInfo(_project, resultString).ToList();
            return YamlSerialize(buildInfo);
        }

        private string YamlSerialize(List<AppVeyorBuildInfo> buildInfo)
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new CommitsYamlTypeConverter())
                .Build();

            return serializer.Serialize(buildInfo);
        }
    }

    public class CommitsYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            if (type == typeof(ObjectId)
            || type == typeof(ObjectId[])
            || type == typeof(DateTime))
            {
                return true;
            }

            return false;
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (type == typeof(DateTime))
            {
                emitter.Emit(new Scalar(null, ((DateTime)value).ToUniversalTime().ToString("O")));
            }

            if (type == typeof(ObjectId))
            {
                emitter.Emit(new Scalar(null, value.ToString()));
            }

            if (type == typeof(ObjectId[]))
            {
                var commits = (ObjectId[])value;
                emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

                foreach (ObjectId commit in commits)
                {
                    emitter.Emit(new Scalar(null, commit.ToString()));
                }

                emitter.Emit(new SequenceEnd());
            }
        }
    }
}
