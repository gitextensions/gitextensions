using System.Collections.Generic;
using GitCommands;
using GitCommands.Submodules;

namespace CommonTestUtils
{
    public class SubmoduleTestHelpers
    {
        public static SubmoduleInfoResult UpdateSubmoduleStructureAndWaitForResult(ISubmoduleStatusProvider provider, GitModule module, bool updateStatus = false)
        {
            SubmoduleInfoResult result = null;
            provider.StatusUpdated += ProviderStatusUpdated;
            try
            {
                provider.UpdateSubmodulesStructure(
                    workingDirectory: module.WorkingDir,
                    noBranchText: string.Empty,
                    updateStatus: updateStatus);

                AsyncTestHelper.WaitForPendingOperations();
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

        public static void UpdateSubmoduleStatusAndWaitForResult(ISubmoduleStatusProvider provider, GitModule module, IReadOnlyList<GitItemStatus> gitStatus)
        {
            provider.UpdateSubmodulesStatus(workingDirectory: module.WorkingDir, gitStatus: gitStatus, forceUpdate: true);

            AsyncTestHelper.WaitForPendingOperations();
        }
    }
}