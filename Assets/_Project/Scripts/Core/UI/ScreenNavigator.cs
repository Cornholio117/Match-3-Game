using System;
using System.Collections.Generic;

namespace Game.Core.UI
{
    /// <summary>The full-screen pages of the game. None means "no screen".</summary>
    public enum AppScreen
    {
        None = 0,
        MainMenu = 1,
        LevelSelect = 2,
        Gameplay = 3
    }

    /// <summary>
    /// Which screen is showing and how to get back: a history of the screens the player came from. Pure logic; the
    /// scene side listens to ScreenChanged and shows or hides the matching page.
    /// </summary>
    public sealed class ScreenNavigator
    {
        private readonly List<AppScreen> _history = new List<AppScreen>();

        /// <summary>Raised with the screen that was left and the screen that is now showing.</summary>
        public event Action<AppScreen, AppScreen> ScreenChanged;

        public AppScreen Current { get; private set; }

        /// <summary>How many screens are behind the current one.</summary>
        public int Depth => _history.Count;

        public ScreenNavigator(AppScreen start)
        {
            if (start == AppScreen.None)
                throw new ArgumentException("None is not a screen.", nameof(start));

            Current = start;
        }

        /// <summary>
        /// Goes to a screen. If that screen is already in the history, it goes back to it (dropping the screens in
        /// between) instead of stacking a second copy; asking for the current screen does nothing.
        /// </summary>
        public void Go(AppScreen screen)
        {
            if (screen == AppScreen.None)
                throw new ArgumentException("None is not a screen.", nameof(screen));

            if (screen == Current)
                return;

            var from = Current;
            var index = _history.LastIndexOf(screen);
            if (index >= 0)
            {
                _history.RemoveRange(index, _history.Count - index);
            }
            else
            {
                _history.Add(Current);
            }

            Current = screen;
            ScreenChanged?.Invoke(from, Current);
        }

        /// <summary>Goes back one screen. Returns false on the first screen, where there is nowhere to go.</summary>
        public bool Back()
        {
            if (_history.Count == 0)
                return false;

            var from = Current;
            Current = _history[_history.Count - 1];
            _history.RemoveAt(_history.Count - 1);
            ScreenChanged?.Invoke(from, Current);
            return true;
        }
    }
}
