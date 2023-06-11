using System.Reflection;
using System.Runtime.CompilerServices;
using AppVeyorIntegration;
using CommonTestUtils;
using FluentAssertions;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using NSubstitute;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace AppVeyorIntegrationTests
{
    [TestFixture]
    public class AppVeyorAdapterTests
    {
        private const string _projectId = "account/repo";

        [Test]
        public void Should_return_no_build_Info_When_Api_Json_is_empty()
        {
            var buildInfo = new AppVeyorAdapter().ExtractBuildInfo(_projectId, string.Empty);

            buildInfo.Should().HaveCount(0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Test]
        public async Task Should_return_a_build_Info_When_Json_content_is_the_one_of_a_pull_request_build()
        {
            await Verifier.Verify(BuildBuildInfoForFile("AppVeyorResult_pull_request_build.json")).UseDirectory("ApprovedFiles");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Test]
        public async Task Should_return_a_build_Info_When_Json_content_is_the_one_of_a_master_build()
        {
            await Verifier.Verify(BuildBuildInfoForFile("AppVeyorResult_master.json")).UseDirectory("ApprovedFiles");
        }

        private string BuildBuildInfoForFile(string filename)
        {
            var resultString = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(),
                $"{GetType().Namespace}.MockData.{filename}");
            AppVeyorAdapter appVeyorAdapter = new();
            appVeyorAdapter.Initialize(Substitute.For<IBuildServerWatcher>(), Substitute.For<ISettingsSource>(), () => { },
                id => true);

            var buildInfo = appVeyorAdapter.ExtractBuildInfo(_projectId, resultString).ToList();
            return YamlSerialize(buildInfo);
        }

        private static string YamlSerialize(List<AppVeyorBuildInfo> buildInfo)
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
