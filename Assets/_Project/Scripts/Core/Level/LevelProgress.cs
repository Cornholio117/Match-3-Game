using System;
using System.Collections.Generic;
using Game.Core.Board;

namespace Game.Core.Level
{
    /// <summary>
    /// The live state of one level: score, gems collected, moves left, and whether it is won or lost. It is fed the
    /// recorded steps of each accepted swap and raises events that the HUD and popups listen to.
    /// </summary>
    public sealed class LevelProgress
    {
        private readonly ScoreRules _rules;

        public event Action<int> ScoreChanged;
        public event Action<int> GoalProgressChanged;
        public event Action<LevelState> StateChanged;

        public MovesModel Moves { get; }
        public LevelGoalType GoalType { get; }

        /// <summary>The colour to collect; None for a score goal.</summary>
        public PieceColor GoalColor { get; }

        public int GoalAmount { get; }
        public int Score { get; private set; }
        public int Collected { get; private set; }
        public LevelState State { get; private set; }

        /// <summary>How far along the goal is: the score for a score goal, the gems collected for a collect goal.</summary>
        public int GoalProgress => GoalType == LevelGoalType.Score ? Score : Collected;

        public LevelProgress(LevelGoalType goalType, int goalAmount, PieceColor goalColor, int moves, ScoreRules rules)
        {
            if (goalAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(goalAmount));

            if (goalType == LevelGoalType.CollectColor && goalColor == PieceColor.None)
                throw new ArgumentException("A collect goal needs a colour.", nameof(goalColor));

            GoalType = goalType;
            GoalColor = goalColor;
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            GoalAmount = goalAmount;
            Moves = new MovesModel(moves);
            State = LevelState.InProgress;
        }

        /// <summary>
        /// Counts one accepted swap: uses a move, adds the score and the collected gems, then checks the result. The
        /// whole turn, cascades included, is counted before the check, and reaching the goal on the last move wins.
        /// Turns after the level has ended are ignored.
        /// </summary>
        public void ApplyTurn(List<BoardStep> steps)
        {
            if (State != LevelState.InProgress)
                return;

            var progressBefore = GoalProgress;

            Moves.Consume();

            var points = _rules.Calculate(steps);
            if (points > 0)
            {
                Score += points;
                ScoreChanged?.Invoke(Score);
            }

            if (GoalType == LevelGoalType.CollectColor)
                Collected += CountCleared(steps, GoalColor);

            if (GoalProgress != progressBefore)
                GoalProgressChanged?.Invoke(GoalProgress);

            if (GoalProgress >= GoalAmount)
                SetState(LevelState.Won);
            else if (Moves.Remaining == 0)
                SetState(LevelState.Lost);
        }

        private static int CountCleared(List<BoardStep> steps, PieceColor color)
        {
            var count = 0;
            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i].Kind == BoardStepKind.Clear && steps[i].Color == color)
                    count++;
            }

            return count;
        }

        private void SetState(LevelState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
