using System;
using System.Collections.Generic;
using System.Threading;
using GitCommands;
using GitCommands.Submodules;
using GitUI;

namespace CommonTestUtils
{
    public class SubmoduleTestHelpers
    {
        public static SubmoduleInfoResult UpdateSubmoduleStructureAndWaitForResult(ISubmoduleStatusProvider provider, GitModule module, bool updateStatus = false)
        {
            SubmoduleInfoResult result = null;
            provider.StatusUpdated += Provider_StatusUpdated;

            provider.UpdateSubmodulesStructure(
                workingDirectory: module.WorkingDir,
                noBranchText: string.Empty,
                updateStatus: updateStatus);

            AsyncTestHelper.WaitForPendingOperations();

            provider.StatusUpdated -= Provider_StatusUpdated;

            return result;

            void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
                result = e.Info;
            }
        }

        public static void UpdateSubmoduleStatusAndWaitForResult(ISubmoduleStatusProvider provider, GitModule module, IReadOnlyList<GitItemStatus> gitStatus)
        {
            List<DetailedSubmoduleInfo> result = new List<DetailedSubmoduleInfo>();
            provider.StatusUpdated += Provider_StatusUpdated;

            provider.UpdateSubmodulesStatus(
                workingDirectory: module.WorkingDir,
                gitStatus: gitStatus);

            AsyncTestHelper.WaitForPendingOperations();

            provider.StatusUpdated -= Provider_StatusUpdated;

            return;

            void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
            }
        }
    }
}
