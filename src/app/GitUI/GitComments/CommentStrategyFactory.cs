using GitCommands;

namespace GitUI.GitComments
{
    public static class CommentStrategyFactory
    {
        // Static readonly array: Instantiated once at class load; add new strategies here
        private static readonly ICommentStrategy[] AllStrategies =
        {
            new DefaultCommentStrategy(),
            new ExtendedCommentStrategy(),
            new GitDefaultCommentStrategy()
        };

        /// <summary>
        /// Gets the existing or default comment strategy.
        /// </summary>
        /// <param name="strategyId">The strategy identifier.</param>
        /// <returns>ICommentStrategy.</returns>
        public static ICommentStrategy GetExistingOrDefault(int strategyId)
        {
            return AllStrategies.FirstOrDefault(s => s.Id == strategyId)
                   ?? AllStrategies[0]; // Fallback to first
        }

        /// <summary>
        /// Gets the application selected comment strategy.
        /// </summary>
        /// <returns>ICommentStrategy.</returns>
        public static ICommentStrategy GetSelected()
        {
            var commentStrategyId = AppSettings.CommentStrategyId;
            return GetExistingOrDefault(commentStrategyId);
        }

        public static List<ICommentStrategy> GetAll()
        {
            return AllStrategies.OrderBy(s => s.Id).ToList(); // Sorted by ID for consistency
        }
    }
}
