using System;
using Game.Core.Board;
using Game.Core.Level;

namespace Game.Core.UI
{
    /// <summary>What the presenter needs from the HUD: three things it can be told to show.</summary>
    public interface IHudView
    {
        void ShowMoves(int remaining);
        void ShowScore(int score);
        void ShowGoal(LevelGoalType type, PieceColor color, int current, int target);
    }

    /// <summary>Keeps the HUD in step with the level: it listens to LevelProgress and tells the view what to show.</summary>
    public sealed class HudPresenter
    {
        private readonly LevelProgress _progress;
        private readonly IHudView _view;
        private bool _enabled;

        public HudPresenter(LevelProgress progress, IHudView view)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        /// <summary>Starts listening and shows the current values straight away. Calling it twice does nothing extra.</summary>
        public void Enable()
        {
            if (_enabled)
                return;

            _enabled = true;
            _progress.Moves.MovesChanged += HandleMovesChanged;
            _progress.ScoreChanged += HandleScoreChanged;
            _progress.GoalProgressChanged += HandleGoalProgressChanged;

            HandleMovesChanged(_progress.Moves.Remaining);
            HandleScoreChanged(_progress.Score);
            HandleGoalProgressChanged(_progress.GoalProgress);
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            _enabled = false;
            _progress.Moves.MovesChanged -= HandleMovesChanged;
            _progress.ScoreChanged -= HandleScoreChanged;
            _progress.GoalProgressChanged -= HandleGoalProgressChanged;
        }

        private void HandleMovesChanged(int remaining)
        {
            _view.ShowMoves(remaining);
        }

        private void HandleScoreChanged(int score)
        {
            _view.ShowScore(score);
        }

        private void HandleGoalProgressChanged(int current)
        {
            _view.ShowGoal(_progress.GoalType, _progress.GoalColor, current, _progress.GoalAmount);
        }
    }
}
