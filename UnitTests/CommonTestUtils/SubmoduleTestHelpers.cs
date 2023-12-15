using GitCommands.Submodules;
using GitExtensions.Extensibility.Git;

namespace CommonTestUtils
{
    public class SubmoduleTestHelpers
    {
        public static async Task<SubmoduleInfoResult> UpdateSubmoduleStructureAndWaitForResultAsync(ISubmoduleStatusProvider provider, IGitModule module, bool updateStatus = true)
        {
            SubmoduleInfoResult result = null;
            provider.StatusUpdated += ProviderStatusUpdated;
            try
            {
                await provider.UpdateSubmodulesStructureAsync(
                        workingDirectory: module.WorkingDir,
                        noBranchText: string.Empty,
                        updateStatus: updateStatus);

                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
            }
            finally
            {
                provider.StatusUpdated -= ProviderStatusUpdated;
            }

            return result;

            void ProviderStatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
                result = e.Info;
            }
        }

        public static async Task UpdateSubmoduleStatusAndWaitForResultAsync(ISubmoduleStatusProvider provider, IGitModule module, IReadOnlyList<GitItemStatus> gitStatus, bool forceUpdate = true)
        {
            provider.StatusUpdated += Provider_StatusUpdated;

            await provider.UpdateSubmodulesStatusAsync(
                    workingDirectory: module.WorkingDir,
                    gitStatus: gitStatus,
                    forceUpdate: forceUpdate);

            await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

            provider.StatusUpdated -= Provider_StatusUpdated;

            return;

            void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
            }
        }
    }
}
