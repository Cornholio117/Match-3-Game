using System;

namespace Game.Core.UI
{
    /// <summary>What the presenter needs from the pause popup: its buttons.</summary>
    public interface IPausePopupView
    {
        event Action ResumeClicked;
        event Action LevelSelectClicked;
    }

    /// <summary>Decides what the pause popup's buttons do. It knows the view only through its interface.</summary>
    public sealed class PausePopupPresenter
    {
        private readonly IPausePopupView _view;
        private readonly PopupStack _stack;
        private readonly ILevelFlow _flow;
        private bool _enabled;

        public PausePopupPresenter(IPausePopupView view, PopupStack stack, ILevelFlow flow)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _stack = stack ?? throw new ArgumentNullException(nameof(stack));
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        }

        public void Enable()
        {
            if (_enabled)
                return;

            _enabled = true;
            _view.ResumeClicked += HandleResumeClicked;
            _view.LevelSelectClicked += HandleLevelSelectClicked;
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            _enabled = false;
            _view.ResumeClicked -= HandleResumeClicked;
            _view.LevelSelectClicked -= HandleLevelSelectClicked;
        }

        private void HandleResumeClicked()
        {
            _stack.Close(PopupId.Pause);
        }

        // Leaving the level: the popup closes first so it is not left open on the next screen.
        private void HandleLevelSelectClicked()
        {
            _stack.Close(PopupId.Pause);
            _flow.OpenLevelSelect();
        }
    }
}
