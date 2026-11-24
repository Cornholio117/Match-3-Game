using Game.Core.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Owns the popup stack, shows and hides the matching popup views, and closes the top popup on the Android
    /// back button. It is the composition point for popup presenters: it builds each one from its view and the stack.
    /// </summary>
    public sealed class PopupManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Every popup in the scene. They are hidden at start.")]
        [SerializeField] private PopupView[] _popups;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private PopupStack _stack;
        private PausePopupPresenter _pausePresenter;

        /// <summary>True while any popup is open; board input is ignored then.</summary>
        public bool IsAnyOpen => _stack != null && _stack.Count > 0;

        private void Awake()
        {
            _stack = new PopupStack();

            for (var i = 0; i < _popups.Length; i++)
            {
                if (_popups[i] == null)
                {
                    GameLog.Error(nameof(PopupManager), $"Empty popup slot {i} on {name}");
                    enabled = false;
                    return;
                }

                _popups[i].Hide();

                if (_popups[i] is PausePopupView pausePopup)
                    _pausePresenter = new PausePopupPresenter(pausePopup, _stack);
            }
        }

        private void OnEnable()
        {
            _stack.Opened += HandleOpened;
            _stack.Closed += HandleClosed;
            _pausePresenter?.Enable();
        }

        private void OnDisable()
        {
            _stack.Opened -= HandleOpened;
            _stack.Closed -= HandleClosed;
            _pausePresenter?.Disable();
        }

        // The Android back button reaches the Input System as the Escape key. Checking one key each frame
        // is input polling, which R5.4 allows, and it allocates nothing.
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                _stack.CloseTop();
        }

        /// <summary>Opens the pause popup. Called by the HUD pause button.</summary>
        public void OpenPause()
        {
            _stack.Open(PopupId.Pause);
        }

        private void HandleOpened(PopupId id)
        {
            FindView(id)?.Show();

            if (_debugMode)
                GameLog.Info(nameof(PopupManager), $"opened {id}, {_stack.Count} open");
        }

        private void HandleClosed(PopupId id)
        {
            FindView(id)?.Hide();

            if (_debugMode)
                GameLog.Info(nameof(PopupManager), $"closed {id}, {_stack.Count} open");
        }

        private PopupView FindView(PopupId id)
        {
            for (var i = 0; i < _popups.Length; i++)
            {
                if (_popups[i].Id == id)
                    return _popups[i];
            }

            GameLog.Error(nameof(PopupManager), $"No popup view for {id}");
            return null;
        }
    }
}
