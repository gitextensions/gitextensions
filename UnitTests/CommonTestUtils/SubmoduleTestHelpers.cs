using System.Collections.Generic;
using System.Threading.Tasks;
using GitCommands;
using GitCommands.Submodules;
using GitUI;

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
                provider.UpdateSubmodulesStructure(
                    workingDirectory: module.WorkingDir,
                    noBranchText: string.Empty,
                    updateStatus: updateStatus);

                await ThreadHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
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
            provider.UpdateSubmodulesStatus(workingDirectory: module.WorkingDir, gitStatus: gitStatus, forceUpdate: true);

            await ThreadHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
        }
    }
}
