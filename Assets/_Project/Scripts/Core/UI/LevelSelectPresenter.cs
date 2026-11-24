using System;
using Game.Core.Level;

namespace Game.Core.UI
{
    /// <summary>What the presenter needs from the level select screen: its buttons, and a way to show one level.</summary>
    public interface ILevelSelectView
    {
        event Action<int> LevelClicked;
        event Action BackClicked;

        /// <summary>Shows one level button: its earned stars (0 to 3) and whether the player may play it.</summary>
        void ShowLevel(int levelIndex, int stars, bool unlocked);
    }

    /// <summary>
    /// Keeps the level select screen in step with the player's progress and decides what a tap does: an unlocked level
    /// starts, a locked one does nothing. The buttons refresh whenever stars change, so a newly unlocked level is
    /// ready when the player returns to the screen.
    /// </summary>
    public sealed class LevelSelectPresenter
    {
        private readonly PlayerProgress _player;
        private readonly ILevelSelectView _view;
        private readonly IMenuFlow _flow;
        private bool _enabled;

        public LevelSelectPresenter(PlayerProgress player, ILevelSelectView view, IMenuFlow flow)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        }

        public void Enable()
        {
            if (_enabled)
                return;

            _enabled = true;
            _view.LevelClicked += HandleLevelClicked;
            _view.BackClicked += HandleBackClicked;
            _player.LevelStarsChanged += HandleStarsChanged;
            RefreshAll();
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            _enabled = false;
            _view.LevelClicked -= HandleLevelClicked;
            _view.BackClicked -= HandleBackClicked;
            _player.LevelStarsChanged -= HandleStarsChanged;
        }

        private void RefreshAll()
        {
            for (var i = 0; i < _player.LevelCount; i++)
                _view.ShowLevel(i, _player.GetStars(i), _player.IsUnlocked(i));
        }

        private void HandleStarsChanged(int levelIndex)
        {
            RefreshAll();
        }

        private void HandleLevelClicked(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _player.LevelCount)
                return;

            if (_player.IsUnlocked(levelIndex))
                _flow.PlayLevel(levelIndex);
        }

        private void HandleBackClicked()
        {
            _flow.GoBack();
        }
    }
}
