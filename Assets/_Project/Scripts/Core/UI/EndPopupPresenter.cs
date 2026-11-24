using System;
using Game.Core.Level;

namespace Game.Core.UI
{
    /// <summary>What the presenter needs from a win or lose popup: the Retry button and a place to show the result.</summary>
    public interface IEndPopupView
    {
        event Action RetryClicked;
        void ShowResult(int score, int goalProgress, int goalAmount);
    }

    /// <summary>Something that can start the current level over.</summary>
    public interface ILevelRestarter
    {
        void Restart();
    }

    /// <summary>
    /// Decides what a win or lose popup does: it fills in the result when its popup opens, and Retry closes the popup
    /// and restarts the level. One class serves both popups; the popup id tells them apart.
    /// </summary>
    public sealed class EndPopupPresenter
    {
        private readonly IEndPopupView _view;
        private readonly PopupStack _stack;
        private readonly PopupId _popupId;
        private readonly LevelProgress _progress;
        private readonly ILevelRestarter _restarter;
        private bool _enabled;

        public EndPopupPresenter(
            IEndPopupView view, PopupStack stack, PopupId popupId, LevelProgress progress, ILevelRestarter restarter)
        {
            if (popupId == PopupId.None)
                throw new ArgumentException("None is not a popup.", nameof(popupId));

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _stack = stack ?? throw new ArgumentNullException(nameof(stack));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _restarter = restarter ?? throw new ArgumentNullException(nameof(restarter));
            _popupId = popupId;
        }

        public void Enable()
        {
            if (_enabled)
                return;

            _enabled = true;
            _stack.Opened += HandleOpened;
            _view.RetryClicked += HandleRetryClicked;
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            _enabled = false;
            _stack.Opened -= HandleOpened;
            _view.RetryClicked -= HandleRetryClicked;
        }

        private void HandleOpened(PopupId id)
        {
            if (id == _popupId)
                _view.ShowResult(_progress.Score, _progress.GoalProgress, _progress.GoalAmount);
        }

        // The popup is closed first so the restart starts from a clean screen.
        private void HandleRetryClicked()
        {
            _stack.Close(_popupId);
            _restarter.Restart();
        }
    }
}
