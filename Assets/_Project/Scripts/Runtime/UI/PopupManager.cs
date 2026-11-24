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

        // Created here, not in Awake, so other scene objects can use Stack in their own Awake or OnEnable without
        // depending on which object Unity wakes first.
        private readonly PopupStack _stack = new PopupStack();
        private PausePopupPresenter _pausePresenter;
        private bool _loggedKeyboardMissing = true;
        private bool _loggedEscapeDown;

        /// <summary>The stack of open popups, for the code that opens popups.</summary>

        /// <summary>True while any popup is open; board input is ignored then.</summary>
        public bool IsAnyOpen => _stack.Count > 0;

        public PopupStack Stack => _stack;

        private void Awake()
        {
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

            if (_debugMode)
            {
                InputSystem.onDeviceChange += HandleDeviceChange;
                for (var i = 0; i < InputSystem.devices.Count; i++)
                    GameLog.Info(nameof(PopupManager), $"input device at start: {InputSystem.devices[i].layout} ({InputSystem.devices[i].displayName})");
            }
        }

        private void OnDisable()
        {
            _stack.Opened -= HandleOpened;
            _stack.Closed -= HandleClosed;
            _pausePresenter?.Disable();

            if (_debugMode)
                InputSystem.onDeviceChange -= HandleDeviceChange;
        }

        // The Android back button reaches the Input System as the Escape key. Checking one key each frame
        // is input polling, which R5.4 allows, and it allocates nothing.
        private void Update()
        {
            var keyboard = Keyboard.current;

            if (_debugMode)
                LogBackKeyState(keyboard);

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                _stack.CloseTopOnBack();
        }

        // Debug only (R8): logs when the keyboard device appears and every change of the Escape key (the Android
        // back button), with the frame number, to find out why a back press is sometimes ignored on a phone.
        private void LogBackKeyState(Keyboard keyboard)
        {
            var missing = keyboard == null;
            if (missing != _loggedKeyboardMissing)
            {
                _loggedKeyboardMissing = missing;
                GameLog.Info(nameof(PopupManager), $"Keyboard.current is {(missing ? "missing" : "present")} at frame {Time.frameCount}");
            }

            if (missing)
                return;

            var down = keyboard.escapeKey.isPressed;
            if (down != _loggedEscapeDown)
            {
                _loggedEscapeDown = down;
                GameLog.Info(nameof(PopupManager), $"escape isPressed={down} at frame {Time.frameCount}");
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
                GameLog.Info(nameof(PopupManager), $"escape wasPressedThisFrame at frame {Time.frameCount}");
        }

        private static void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            GameLog.Info(nameof(PopupManager), $"input device {change}: {device.layout} ({device.displayName}) at frame {Time.frameCount}");
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
