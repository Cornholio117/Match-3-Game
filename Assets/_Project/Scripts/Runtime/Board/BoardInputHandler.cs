using System;
using Game.Core.Board;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Game.Runtime.Board
{
    /// <summary>
    /// Reads finger swipes on the board and turns each into "swap this cell with its neighbour". It only reports the
    /// request; whether the swap is allowed is the game's decision. In the Editor the mouse acts as a finger.
    /// </summary>
    public sealed class BoardInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private Camera _camera;

        [Header("Settings")]
        [Tooltip("How far a finger must move, in gem widths, before it counts as a swipe.")]
        [SerializeField, Range(0.1f, 1f)] private float _minSwipeCells = 0.3f;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private int _activeFinger = -1;
        private bool _hasStart;
        private GridPosition _startCell;
        private Vector2 _startWorld;

        public event Action<GridPosition, GridPosition> SwapRequested;

        private void Awake()
        {
            if (_boardView == null || _camera == null)
            {
                GameLog.Error(nameof(BoardInputHandler), $"Missing board view or camera on {name}");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
            TouchSimulation.Enable();
#endif
            Touch.onFingerDown += HandleFingerDown;
            Touch.onFingerMove += HandleFingerMove;
            Touch.onFingerUp += HandleFingerUp;
        }

        private void OnDisable()
        {
            Touch.onFingerDown -= HandleFingerDown;
            Touch.onFingerMove -= HandleFingerMove;
            Touch.onFingerUp -= HandleFingerUp;
#if UNITY_EDITOR
            TouchSimulation.Disable();
#endif
            EnhancedTouchSupport.Disable();
        }

        private void HandleFingerDown(Finger finger)
        {
            // Only the first finger counts; extra fingers are ignored until it lifts.
            if (_activeFinger != -1)
                return;

            _activeFinger = finger.index;
            var world = ScreenToBoardPlane(finger.screenPosition);
            _hasStart = _boardView.TryGetCell(world, out _startCell);
            _startWorld = world;

            if (_debugMode)
                GameLog.Info(nameof(BoardInputHandler), $"down on cell {_startCell} (on board: {_hasStart})");
        }

        private void HandleFingerMove(Finger finger)
        {
            if (finger.index != _activeFinger || !_hasStart)
                return;

            var world = ScreenToBoardPlane(finger.screenPosition);
            var delta = world - _startWorld;

            // One swap per touch: once a swipe is recognised, wait for the finger to lift.
            if (!SwipeMath.TryGetDirection(delta.x, delta.y, _minSwipeCells, out var direction))
                return;

            _hasStart = false;
            SwapRequested?.Invoke(_startCell, _startCell.Move(direction));
        }

        private void HandleFingerUp(Finger finger)
        {
            if (finger.index != _activeFinger)
                return;

            _activeFinger = -1;
            _hasStart = false;
        }

        // The board lies on the z = 0 plane; for an orthographic camera this maps a screen point onto it.
        private Vector2 ScreenToBoardPlane(Vector2 screenPosition)
        {
            var point = new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z);
            return _camera.ScreenToWorldPoint(point);
        }
    }
}
