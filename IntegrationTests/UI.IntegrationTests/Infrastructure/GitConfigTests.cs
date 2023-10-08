using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitExtUtils;

namespace UITests.Infrastructure
{
    internal class GitConfigTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private string[] _allConfigs;

        [SetUp]
        public void SetUp()
        {
            bool first = _referenceRepository is null;
            ReferenceRepository.ResetRepo(ref _referenceRepository);

            if (first)
            {
                string cmdPath = (Environment.GetEnvironmentVariable("COMSPEC") ?? "C:/WINDOWS/system32/cmd.exe").ToPosixPath().QuoteNE();
                _referenceRepository.Module.GitExecutable.RunCommand($"config --local difftool.cmd.path {cmdPath}").Should().BeTrue();
                _referenceRepository.Module.GitExecutable.RunCommand($"config --local mergetool.cmd.path {cmdPath}").Should().BeTrue();
                _referenceRepository.Module.GitExecutable.RunCommand("config --local diff.guitool cmd").Should().BeTrue();
                _referenceRepository.Module.GitExecutable.RunCommand("config --local merge.guitool cmd").Should().BeTrue();
                var result = _referenceRepository.Module.GitExecutable.Execute(new GitArgumentBuilder("help", gitOptions: (ArgumentString)"--no-pager") { "--config" });
                _allConfigs = result.StandardOutput.Split('\n').Where(cfg => !string.IsNullOrWhiteSpace(cfg) && !cfg.Contains("git help config"))
                    .Concat(new string[] { "a.a.C", "$#@@#$%&#@", "1234", "~x.Y" })
                    .ToArray();
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            AppSettings.SetDocumentationBaseUrl("master");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void ValidateAllConfigOptionsWork()
        {
            var settings = _allConfigs.Select(cfg => new { Setting = _referenceRepository.Module.GetEffectiveGitSetting(cfg, false), ConfigName = cfg }).ToArray();
            settings.Should().NotContainNulls();
            settings.Select(s => s.Setting).Should().NotContainNulls();

            foreach (var g in settings.GroupBy(s => s.Setting.Status))
            {
                Console.WriteLine(g.Key?.ToString() ?? "NULL");
                foreach (var s in g)
                {
                    Console.WriteLine($"\t{s.ConfigName}");
                }
            }

            var nullResults = settings.Where(v => v.Setting.Status is null);
            foreach (var nr in nullResults)
            {
                Console.WriteLine(nr.ConfigName);
            }
        }
    }
}
