using System;
using Game.Core.Level;

namespace Game.Core.UI
{
    /// <summary>What the presenter needs from a win or lose popup: its buttons, and a place to show the result.</summary>
    public interface IEndPopupView
    {
        event Action RetryClicked;
        event Action NextLevelClicked;
        event Action LevelSelectClicked;
        void ShowResult(int score, int goalProgress, int goalAmount, int stars, bool canGoNext);
    }

    /// <summary>What a popup button can ask the game to do with the level.</summary>
    public interface ILevelFlow
    {
        /// <summary>True when there is a next level and the player may play it.</summary>
        bool CanStartNextLevel { get; }

        /// <summary>Starts the current level over.</summary>
        void Restart();

        /// <summary>Starts the next level. Does nothing when CanStartNextLevel is false.</summary>
        void StartNextLevel();

        /// <summary>Leaves the level and shows the level select screen.</summary>
        void OpenLevelSelect();
    }

    /// <summary>
    /// Decides what a win or lose popup does: it fills in the result (score, stars, whether Next Level is offered)
    /// when its popup opens, and its buttons close the popup and then change the level. One class serves both
    /// popups; the popup id tells them apart.
    /// </summary>
    public sealed class EndPopupPresenter
    {
        private readonly IEndPopupView _view;
        private readonly PopupStack _stack;
        private readonly PopupId _popupId;
        private readonly LevelProgress _progress;
        private readonly ILevelFlow _flow;
        private bool _enabled;

        public EndPopupPresenter(
            IEndPopupView view, PopupStack stack, PopupId popupId, LevelProgress progress, ILevelFlow flow)
        {
            if (popupId == PopupId.None)
                throw new ArgumentException("None is not a popup.", nameof(popupId));

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _stack = stack ?? throw new ArgumentNullException(nameof(stack));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
            _popupId = popupId;
        }

        public void Enable()
        {
            if (_enabled)
                return;

            _enabled = true;
            _stack.Opened += HandleOpened;
            _view.RetryClicked += HandleRetryClicked;
            _view.NextLevelClicked += HandleNextLevelClicked;
            _view.LevelSelectClicked += HandleLevelSelectClicked;
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            _enabled = false;
            _stack.Opened -= HandleOpened;
            _view.RetryClicked -= HandleRetryClicked;
            _view.NextLevelClicked -= HandleNextLevelClicked;
            _view.LevelSelectClicked -= HandleLevelSelectClicked;
        }

        private void HandleOpened(PopupId id)
        {
            if (id != _popupId)
                return;

            var won = _progress.State == LevelState.Won;
            _view.ShowResult(
                _progress.Score,
                _progress.GoalProgress,
                _progress.GoalAmount,
                StarRating.Calculate(_progress),
                won && _flow.CanStartNextLevel);
        }

        // The popup is closed first so the new level starts from a clean screen.
        private void HandleRetryClicked()
        {
            _stack.Close(_popupId);
            _flow.Restart();
        }

        private void HandleNextLevelClicked()
        {
            if (!_flow.CanStartNextLevel)
                return;

            _stack.Close(_popupId);
            _flow.StartNextLevel();
        }

        private void HandleLevelSelectClicked()
        {
            _stack.Close(_popupId);
            _flow.OpenLevelSelect();
        }
    }
}
