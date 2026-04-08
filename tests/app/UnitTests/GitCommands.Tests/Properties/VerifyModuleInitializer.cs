using System.Runtime.CompilerServices;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Properties;

internal class VerifyModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.UseStrictJson();
        VerifierSettings.IgnoreMembers(nameof(ObjectId.IsZeroOrArtificial));
    }
}
