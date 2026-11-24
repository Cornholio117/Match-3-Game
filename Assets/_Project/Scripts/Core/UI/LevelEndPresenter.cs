using System;
using Game.Core.Level;

namespace Game.Core.UI
{
    /// <summary>Opens the win or lose popup when the level ends. It decides; the popup views only display.</summary>
    public sealed class LevelEndPresenter
    {
        private readonly LevelProgress _progress;
        private readonly PopupStack _stack;
        private bool _enabled;

        public LevelEndPresenter(LevelProgress progress, PopupStack stack)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _stack = stack ?? throw new ArgumentNullException(nameof(stack));
        }

        public void Enable()
        {
            if (_enabled)
                return;

            _enabled = true;
            _progress.StateChanged += HandleStateChanged;
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            _enabled = false;
            _progress.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(LevelState state)
        {
            if (state == LevelState.Won)
                _stack.Open(PopupId.Win);
            else if (state == LevelState.Lost)
                _stack.Open(PopupId.Lose);
        }
    }
}
