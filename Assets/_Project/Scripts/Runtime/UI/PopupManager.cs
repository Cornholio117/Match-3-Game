using System;
using Game.Core.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

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
        private bool _loggedKeyboardMissing = true;
        private bool _loggedEscapeDown;

        /// <summary>Raised when the back button is pressed and no popup is open.</summary>
        public event Action BackPressedWithoutPopup;

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
            }
        }

        private void OnEnable()
        {
            _stack.Opened += HandleOpened;
            _stack.Closed += HandleClosed;

            if (_debugMode)
            {
                InputSystem.onDeviceChange += HandleDeviceChange;
                InputSystem.onEvent += HandleInputEvent;
                for (var i = 0; i < InputSystem.devices.Count; i++)
                    GameLog.Info(nameof(PopupManager), $"input device at start: {InputSystem.devices[i].layout} ({InputSystem.devices[i].displayName})");
            }
        }

        private void OnDisable()
        {
            _stack.Opened -= HandleOpened;
            _stack.Closed -= HandleClosed;

            if (_debugMode)
            {
                InputSystem.onDeviceChange -= HandleDeviceChange;
                InputSystem.onEvent -= HandleInputEvent;
            }
        }

        // The Android back button reaches the Input System as the Escape key. Phones report several keyboard-type
        // devices (this one has six: hardware keys, the on-screen keyboard, a virtual one), and the back press does not
        // always arrive on Keyboard.current, so every keyboard is checked. Polling a few keys each frame is input
        // polling, which R5.4 allows, and it allocates nothing.
        private void Update()
        {
            if (_debugMode)
                LogBackKeyState(Keyboard.current);

            if (BackPressedThisFrame())
                HandleBackPress();
        }

        /// <summary>
        /// What a back press does: a popup gets it first (a finished level's popup refuses it and stays). With none
        /// open, the press belongs to the screen underneath, which the app flow decides. Public so it can be tested
        /// without a real key press.
        /// </summary>
        public void HandleBackPress()
        {
            if (_stack.Count > 0)
                _stack.CloseTopOnBack();
            else
                BackPressedWithoutPopup?.Invoke();
        }

        private static bool BackPressedThisFrame()
        {
            var devices = InputSystem.devices;
            for (var i = 0; i < devices.Count; i++)
            {
                if (devices[i] is Keyboard keyboard && keyboard.escapeKey.wasPressedThisFrame)
                    return true;
            }

            return false;
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
                GameLog.Info(nameof(PopupManager), $"escape wasPressedThisFrame on Keyboard.current ({keyboard.displayName}) at frame {Time.frameCount}");

            var devices = InputSystem.devices;
            for (var i = 0; i < devices.Count; i++)
            {
                if (devices[i] is Keyboard other && other != keyboard && other.escapeKey.wasPressedThisFrame)
                    GameLog.Info(nameof(PopupManager), $"escape wasPressedThisFrame on ANOTHER keyboard ({other.displayName}) at frame {Time.frameCount}");
            }
        }

        private static void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            GameLog.Info(nameof(PopupManager), $"input device {change}: {device.layout} ({device.displayName}) at frame {Time.frameCount}");
        }

        // Debug only: every raw input event that targets a keyboard, before the Input System applies it.
        private static void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (device is Keyboard)
                GameLog.Info(nameof(PopupManager), $"raw event {eventPtr.type} on keyboard ({device.displayName}) at frame {Time.frameCount}");
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
