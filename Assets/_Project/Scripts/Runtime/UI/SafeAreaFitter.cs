using Game.Core.UI;
using UnityEngine;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Keeps a UI container inside the part of the screen that is safe to draw on, so the HUD is not hidden behind a
    /// camera cut-out or a gesture bar. Put it on a full-stretch child of a Canvas and place the HUD inside it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Apply();
        }

        // Checked every frame because it is two cheap comparisons: the screen can rotate or resize (split screen,
        // foldables) and there is no event for it. It allocates nothing.
        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea || Screen.width != _lastScreenSize.x || Screen.height != _lastScreenSize.y)
                Apply();
        }

        private void Apply()
        {
            var safe = Screen.safeArea;
            _lastSafeArea = safe;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            var anchors = SafeAreaMath.ToAnchors(Screen.width, Screen.height, safe.x, safe.y, safe.width, safe.height);
            _rect.anchorMin = new Vector2(anchors.MinX, anchors.MinY);
            _rect.anchorMax = new Vector2(anchors.MaxX, anchors.MaxY);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;

            if (_debugMode)
                GameLog.Info(nameof(SafeAreaFitter), $"safe area {safe} on screen {Screen.width}x{Screen.height}");
        }
    }
}
