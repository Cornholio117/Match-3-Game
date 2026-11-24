using System;

namespace Game.Core.UI
{
    /// <summary>What the presenter needs from the pause popup: a way to hear the Resume button.</summary>
    public interface IPausePopupView
    {
        event Action ResumeClicked;
    }

    /// <summary>Decides what the pause popup's buttons do. It knows the view only through its interface.</summary>
    public sealed class PausePopupPresenter
    {
        private readonly IPausePopupView _view;
        private readonly PopupStack _stack;

        public PausePopupPresenter(IPausePopupView view, PopupStack stack)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _stack = stack ?? throw new ArgumentNullException(nameof(stack));
        }

        public void Enable()
        {
            _view.ResumeClicked += HandleResumeClicked;
        }

        public void Disable()
        {
            _view.ResumeClicked -= HandleResumeClicked;
        }

        private void HandleResumeClicked()
        {
            _stack.Close(PopupId.Pause);
        }
    }
}
