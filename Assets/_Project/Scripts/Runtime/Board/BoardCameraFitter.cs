using Game.Core.Board;
using UnityEngine;

namespace Game.Runtime.Board
{
    /// <summary>Sizes and positions the camera so the whole board sits inside the phone's safe area.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class BoardCameraFitter : MonoBehaviour
    {
        [Header("Board")]
        [Tooltip("Board width in world units. One gem is 1 unit (256 pixels per unit).")]
        [SerializeField, Min(1)] private int _boardWidth = 8;

        [Tooltip("Board height in world units.")]
        [SerializeField, Min(1)] private int _boardHeight = 8;

        [Tooltip("World position of the board's centre. Cell (0,0) is centred on (0,0), so an 8x8 board is centred on (3.5, 3.5).")]
        [SerializeField] private Vector2 _boardCenter = new Vector2(3.5f, 3.5f);

        [Tooltip("Extra empty space around the board, in world units, so gems don't touch the screen edge.")]
        [SerializeField, Min(0f)] private float _margin = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            Fit();
        }

        // Called once at start: the game is locked to portrait, so the screen does not change while playing.
        public void Fit()
        {
            var safe = Screen.safeArea;
            var screen = new ScreenArea(Screen.width, Screen.height, safe.x, safe.y, safe.width, safe.height);
            var result = CameraFit.Calculate(_boardWidth, _boardHeight, _boardCenter.x, _boardCenter.y, _margin, screen);

            _camera.orthographic = true;
            _camera.orthographicSize = result.OrthographicSize;
            var position = transform.position;
            transform.position = new Vector3(result.CameraX, result.CameraY, position.z);

            if (_debugMode)
                GameLog.Info(nameof(BoardCameraFitter), $"size={result.OrthographicSize:F2} at ({result.CameraX:F2}, {result.CameraY:F2})");
        }

        [ContextMenu("Debug/Fit Now")]
        private void FitNow()
        {
            _camera = GetComponent<Camera>();
            Fit();
        }

        private void OnDrawGizmos()
        {
            if (!_debugMode) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_boardCenter, new Vector3(_boardWidth, _boardHeight, 0f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_boardCenter, new Vector3(_boardWidth + 2f * _margin, _boardHeight + 2f * _margin, 0f));
        }
    }
}
