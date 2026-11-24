using System;
using Game.Core.Level;

namespace Game.Core.UI
{
    /// <summary>
    /// Handles the end of a level: it records the stars the player earned, then opens the win or lose popup. The
    /// result is recorded first, so the popup already sees the new stars and the unlocked next level.
    /// </summary>
    public sealed class LevelEndPresenter
    {
        private readonly LevelProgress _progress;
        private readonly PopupStack _stack;
        private readonly PlayerProgress _player;
        private readonly int _levelIndex;
        private bool _enabled;

        public LevelEndPresenter(LevelProgress progress, PopupStack stack, PlayerProgress player, int levelIndex)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _stack = stack ?? throw new ArgumentNullException(nameof(stack));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _levelIndex = levelIndex;
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
            {
                _player.RecordResult(_levelIndex, StarRating.Calculate(_progress));
                _stack.Open(PopupId.Win);
            }
            else if (state == LevelState.Lost)
            {
                _stack.Open(PopupId.Lose);
            }
        }
    }
}
