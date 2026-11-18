using Game.Core.Board;
using UnityEngine;

namespace Game.Runtime.Board
{
    /// <summary>One gem on screen. It only shows a colour and, for a special gem, an overlay; the model decides the rest.</summary>
    public sealed class PieceView : MonoBehaviour, IPoolable
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _renderer;

        [Tooltip("Child renderer drawn on top of the gem to mark a special gem (the line arrow).")]
        [SerializeField] private SpriteRenderer _overlayRenderer;

        public PieceColor Color { get; private set; }
        public PieceKind Kind { get; private set; }

        private void Awake()
        {
            if (_renderer == null || _overlayRenderer == null)
            {
                GameLog.Error(nameof(PieceView), $"Missing renderer or overlay renderer on {name}");
                enabled = false;
            }
        }

        public void SetPiece(PieceColor color, Sprite sprite)
        {
            Color = color;
            _renderer.sprite = sprite;
        }

        /// <summary>Marks the gem as a special kind (or back to normal). The overlay is turned for a vertical gem.</summary>
        public void SetKind(PieceKind kind, Sprite overlay)
        {
            Kind = kind;

            var show = kind != PieceKind.Normal && overlay != null;
            _overlayRenderer.enabled = show;
            _overlayRenderer.sprite = show ? overlay : null;
            _overlayRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, kind == PieceKind.LineVertical ? 90f : 0f);
        }

        public void OnSpawned()
        {
            // A reused gem may have been shrunk by the clear animation or marked special, so reset everything visual.
            transform.localScale = Vector3.one;
            _renderer.color = UnityEngine.Color.white;
            SetKind(PieceKind.Normal, null);
        }

        public void OnDespawned()
        {
            transform.localScale = Vector3.one;
            Color = PieceColor.None;
            SetKind(PieceKind.Normal, null);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
        }
#endif
    }
}
