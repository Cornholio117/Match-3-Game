using Game.Core.UI;
using Game.Runtime.Board;
using Game.Runtime.Level;
using Game.Runtime.UI;
using UnityEngine;

namespace Game.Runtime.App
{
    /// <summary>
    /// The flow of the whole app: which screen is showing (Main Menu, Level Select, Gameplay) and how the player gets
    /// from one to the next. It owns the screen navigator, switches the screen views, starts levels through the level
    /// session, and decides what the back button does when no popup is open. It is a composition point and holds no
    /// game rules.
    /// </summary>
    public sealed class AppFlow : MonoBehaviour, IMenuFlow
    {
        [Header("References")]
        [SerializeField] private LevelSession _session;
        [SerializeField] private PopupManager _popups;
        [SerializeField] private BoardController _board;
        [SerializeField] private MainMenuView _mainMenu;
        [SerializeField] private LevelSelectView _levelSelect;

        [Tooltip("One per screen, in any order.")]
        [SerializeField] private ScreenView[] _screens = new ScreenView[0];

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private readonly ScreenNavigator _navigator = new ScreenNavigator(AppScreen.MainMenu);
        private MainMenuPresenter _mainMenuPresenter;
        private LevelSelectPresenter _levelSelectPresenter;

        /// <summary>The screen that is showing now.</summary>
        public AppScreen CurrentScreen => _navigator.Current;

        private void Awake()
        {
            if (_session == null || _popups == null || _board == null || _mainMenu == null || _levelSelect == null || _screens.Length == 0)
            {
                GameLog.Error(nameof(AppFlow), $"Missing session, popups, board, menu views or screens on {name}");
                enabled = false;
                return;
            }

            _mainMenuPresenter = new MainMenuPresenter(_mainMenu, this);
            _levelSelectPresenter = new LevelSelectPresenter(_session.Player, _levelSelect, this);
        }

        private void OnEnable()
        {
            _navigator.ScreenChanged += HandleScreenChanged;
            _session.LevelSelectRequested += HandleLevelSelectRequested;
            _popups.BackPressedWithoutPopup += HandleBackWithoutPopup;
            _mainMenuPresenter.Enable();
            _levelSelectPresenter.Enable();
        }

        private void OnDisable()
        {
            _navigator.ScreenChanged -= HandleScreenChanged;
            _session.LevelSelectRequested -= HandleLevelSelectRequested;
            _popups.BackPressedWithoutPopup -= HandleBackWithoutPopup;
            _mainMenuPresenter.Disable();
            _levelSelectPresenter.Disable();
        }

        // Start, not Awake: every screen's objects have woken up, so hiding them is safe.
        private void Start()
        {
            ApplyScreen(_navigator.Current);
        }

        public void OpenLevelSelect()
        {
            _navigator.Go(AppScreen.LevelSelect);
        }

        // The gameplay screen is shown first so the board is built on an active board.
        public void PlayLevel(int levelIndex)
        {
            _navigator.Go(AppScreen.Gameplay);
            _session.StartLevel(levelIndex);
        }

        public void GoBack()
        {
            _navigator.Back();
        }

        private void HandleScreenChanged(AppScreen from, AppScreen to)
        {
            ApplyScreen(to);

            if (_debugMode)
                GameLog.Info(nameof(AppFlow), $"screen {from} -> {to}");
        }

        private void ApplyScreen(AppScreen screen)
        {
            for (var i = 0; i < _screens.Length; i++)
            {
                if (_screens[i].Screen == screen)
                    _screens[i].Show();
                else
                    _screens[i].Hide();
            }

            // The board is hidden off the gameplay screen, so a stray touch must not play a turn on it.
            _board.SetInputEnabled(screen == AppScreen.Gameplay);

            // Leaving gameplay leaves no popup behind.
            if (screen != AppScreen.Gameplay)
            {
                _popups.Stack.Close(PopupId.Pause);
                _popups.Stack.Close(PopupId.Win);
                _popups.Stack.Close(PopupId.Lose);
            }
        }

        private void HandleLevelSelectRequested()
        {
            OpenLevelSelect();
        }

        // Back with no popup open: on the level select screen it goes back to the menu; in a level it opens the pause
        // popup (the usual Android behaviour); on the main menu there is nowhere to go yet.
        private void HandleBackWithoutPopup()
        {
            switch (_navigator.Current)
            {
                case AppScreen.LevelSelect:
                    _navigator.Back();
                    break;
                case AppScreen.Gameplay:
                    _popups.OpenPause();
                    break;
            }
        }
    }
}
