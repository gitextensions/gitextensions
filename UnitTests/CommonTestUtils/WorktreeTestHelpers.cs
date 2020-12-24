using System.Threading.Tasks;
using GitCommands;
using GitCommands.Worktrees;

namespace CommonTestUtils
{
    public class WorktreeTestHelpers
    {
        public static async Task<WorktreeInfoResult> UpdateWorktreeStructureAndWaitForResultAsync(IWorktreeStatusProvider provider, GitModule module)
        {
            WorktreeInfoResult result = null;
            provider.StatusUpdated += ProviderStatusUpdated;
            try
            {
                await provider.UpdateWorktreesAsync(module.WorkingDir);
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
            }
            finally
            {
                provider.StatusUpdated -= ProviderStatusUpdated;
            }

            return result;

            void ProviderStatusUpdated(object sender, WorktreeStatusEventArgs e)
            {
                result = e.Info;
            }
        }
    }
}
