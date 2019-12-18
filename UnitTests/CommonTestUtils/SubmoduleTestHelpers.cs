using System;
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
            provider.StatusUpdated += Provider_StatusUpdated;

            await provider.UpdateSubmodulesStructureAsync(
                    workingDirectory: module.WorkingDir,
                    noBranchText: string.Empty,
                    updateStatus: updateStatus).ConfigureAwait(false);

            AsyncTestHelper.WaitForPendingOperations(AsyncTestHelper.UnexpectedTimeout);

            provider.StatusUpdated -= Provider_StatusUpdated;

            return result;

            void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
                result = e.Info;
            }
        }

        public static async void UpdateSubmoduleStatusAndWaitForResultAsync(ISubmoduleStatusProvider provider, GitModule module, IReadOnlyList<GitItemStatus> gitStatus)
        {
            List<DetailedSubmoduleInfo> result = new List<DetailedSubmoduleInfo>();
            provider.StatusUpdated += Provider_StatusUpdated;

            await provider.UpdateSubmodulesStatusAsync(
                    workingDirectory: module.WorkingDir,
                    gitStatus: gitStatus);

            AsyncTestHelper.WaitForPendingOperations(AsyncTestHelper.UnexpectedTimeout);

            provider.StatusUpdated -= Provider_StatusUpdated;

            return;

            void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
            }
        }
    }
}
