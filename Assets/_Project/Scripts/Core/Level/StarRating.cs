namespace Game.Core.Level
{
    /// <summary>Turns how well a level was won into 0 to 3 stars.</summary>
    public static class StarRating
    {
        public const int MaxStars = 3;

        /// <summary>The stars for a level that has just ended; 0 unless it was won.</summary>
        public static int Calculate(LevelProgress progress)
        {
            if (progress.State != LevelState.Won)
                return 0;

            return Calculate(progress.GoalType, progress.GoalAmount, progress.GoalProgress, progress.Moves.Remaining, progress.StartingMoves);
        }

        /// <summary>
        /// 0 stars when the goal was not reached. A score goal earns 1 star at the goal, 2 at one and a half times it
        /// and 3 at twice it. A collect goal has no score to compare, so it is rated by the moves left: 1 star for
        /// winning, 2 with at least a quarter of the moves left, 3 with at least half left. Whole-number maths only.
        /// </summary>
        public static int Calculate(LevelGoalType goalType, int goalAmount, int goalProgress, int movesRemaining, int startingMoves)
        {
            if (goalProgress < goalAmount)
                return 0;

            if (goalType == LevelGoalType.Score)
            {
                if (goalProgress >= 2 * goalAmount)
                    return 3;

                return goalProgress * 2 >= goalAmount * 3 ? 2 : 1;
            }

            if (movesRemaining * 100 >= startingMoves * 50)
                return 3;

            return movesRemaining * 100 >= startingMoves * 25 ? 2 : 1;
        }
    }
}
