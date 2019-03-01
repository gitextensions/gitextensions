using System;
using System.Threading;
using GitCommands;
using GitCommands.Submodules;
using GitUI;

namespace CommonTestUtils
{
    public class SubmoduleTestHelpers
    {
        public static SubmoduleInfoResult UpdateSubmoduleStatusAndWaitForResult(ISubmoduleStatusProvider provider, GitModule module, bool updateStatus = false)
        {
            SubmoduleInfoResult result = null;
            provider.StatusUpdated += Provider_StatusUpdated;

            provider.UpdateSubmodulesStatus(
                updateStatus: updateStatus,
                workingDirectory: module.WorkingDir,
                noBranchText: string.Empty);

            AsyncTestHelper.WaitForPendingOperations();

            provider.StatusUpdated -= Provider_StatusUpdated;

            return result;

            void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
                result = e.Info;
            }
        }
    }
}
