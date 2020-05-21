using System.Collections.Generic;
using System.Threading.Tasks;
using GitCommands;
using GitCommands.Submodules;

namespace CommonTestUtils
{
    public class SubmoduleTestHelpers
    {
        public static async Task<SubmoduleInfoResult> UpdateSubmoduleStructureAndWaitForResultAsync(ISubmoduleStatusProvider provider, GitModule module, bool updateStatus = false)
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

        public static async Task UpdateSubmoduleStatusAndWaitForResultAsync(ISubmoduleStatusProvider provider, GitModule module, IReadOnlyList<GitItemStatus> gitStatus)
        {
            provider.StatusUpdated += Provider_StatusUpdated;

            await provider.UpdateSubmodulesStatusAsync(
                    workingDirectory: module.WorkingDir,
                    gitStatus: gitStatus,
                    forceUpdate: true);

            await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

            provider.StatusUpdated -= Provider_StatusUpdated;

            return;

            void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
            }
        }
    }
}
