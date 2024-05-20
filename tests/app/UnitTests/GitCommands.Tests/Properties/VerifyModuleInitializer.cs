using System.Runtime.CompilerServices;

namespace GitCommandsTests.Properties
{
    internal class VerifyModuleInitializer
    {
        [ModuleInitializer]
        public static void Init() =>
            VerifierSettings.UseStrictJson();
    }
}
