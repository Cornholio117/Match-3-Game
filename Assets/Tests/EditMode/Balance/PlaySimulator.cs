using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using Game.Core.Level;
using Game.Runtime.Config;

namespace Game.Tests.EditMode.Balance
{
    /// <summary>The kind of virtual player: a weak one that picks any valid swap, and a good one that picks the biggest match.</summary>
    public enum PlayerKind
    {
        Random,
        Greedy
    }

    public readonly struct PlayResult
    {
        public bool Won { get; }
        public int MovesUsed { get; }
        public int Score { get; }
        public bool Stuck { get; }

        /// <summary>The score for a score goal, the gems collected for a collect goal.</summary>
        public int GoalProgress { get; }

        public PlayResult(bool won, int movesUsed, int score, bool stuck, int goalProgress)
        {
            Won = won;
            MovesUsed = movesUsed;
            Score = score;
            Stuck = stuck;
            GoalProgress = goalProgress;
        }
    }

    /// <summary>
    /// Plays one whole level with a virtual player through the real board, move finder and level tracker, so level
    /// numbers can be tuned by measuring win rates instead of guessing. Same seed, same game.
    /// </summary>
    public static class PlaySimulator
    {
        // A swap of two special gems (a combo) is worth about a big match to the greedy player.
        private const int ComboValue = 15;

        public static PlayResult Play(LevelConfig level, PlayerKind kind, int seed, ScoreRules rules = null)
        {
            return Play(level.Width, level.Height, level.ColorCount, level.Moves, level.GoalType, level.GoalAmount, level.GoalColor, kind, seed, rules);
        }

        public static PlayResult Play(
            int width, int height, int colorCount, int moves, LevelGoalType goalType, int goalAmount, PieceColor goalColor,
            PlayerKind kind, int seed, ScoreRules rules = null)
        {
            var game = BoardGame.CreateFilled(width, height, colorCount, new SeededRandom(seed));
            var progress = new LevelProgress(goalType, goalAmount, goalColor, moves, rules ?? new ScoreRules());
            var chooser = new SeededRandom(seed + 7919);
            var options = new List<SwapMove>();
            var steps = new List<BoardStep>();
            var runs = new List<Match>();
            var used = 0;
            var stuck = false;

            while (progress.State == LevelState.InProgress)
            {
                MoveFinder.FindValidSwaps(game.Board, options);
                if (options.Count == 0)
                {
                    stuck = true;
                    break;
                }

                var chosen = kind == PlayerKind.Random
                    ? options[chooser.Next(options.Count)]
                    : PickGreedy(game.Board, options, runs, goalType, goalColor);

                game.TrySwap(chosen.A, chosen.B, steps);
                progress.ApplyTurn(steps);
                used++;
            }

            return new PlayResult(progress.State == LevelState.Won, used, progress.Score, stuck, progress.GoalProgress);
        }

        // Tries each swap, counts the gems in the runs it would make, undoes it, and keeps the best. On a collect
        // level, runs of the goal colour count double, as a sensible player would aim for them.
        private static SwapMove PickGreedy(BoardModel board, List<SwapMove> options, List<Match> runs, LevelGoalType goalType, PieceColor goalColor)
        {
            var best = options[0];
            var bestValue = -1;

            for (var i = 0; i < options.Count; i++)
            {
                var move = options[i];
                var value = 0;

                if (board.GetKind(move.A) != PieceKind.Normal && board.GetKind(move.B) != PieceKind.Normal)
                {
                    value = ComboValue;
                }
                else
                {
                    board.Swap(move.A, move.B);
                    MatchFinder.Find(board, runs);
                    for (var r = 0; r < runs.Count; r++)
                    {
                        var weight = goalType == LevelGoalType.CollectColor && runs[r].Color == goalColor ? 2 : 1;
                        value += runs[r].Length * weight;
                    }

                    board.Swap(move.A, move.B);
                }

                if (value > bestValue)
                {
                    bestValue = value;
                    best = move;
                }
            }

            return best;
        }
    }
}
