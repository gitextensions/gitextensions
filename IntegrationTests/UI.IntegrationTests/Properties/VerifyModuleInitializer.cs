using System.Runtime.CompilerServices;

namespace UI.IntegrationTests.Properties
{
    internal class VerifyModuleInitializer
    {
        [ModuleInitializer]
        public static void Init() =>
            VerifierSettings.UseStrictJson();
    }
}
