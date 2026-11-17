using Game.Core.Board;
using UnityEngine;

namespace Game.Runtime.Board
{
    /// <summary>One gem on screen. It only shows a colour; the board model decides everything else.</summary>
    public sealed class PieceView : MonoBehaviour, IPoolable
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _renderer;

        public PieceColor Color { get; private set; }

        private void Awake()
        {
            if (_renderer == null)
            {
                GameLog.Error(nameof(PieceView), $"Missing SpriteRenderer on {name}");
                enabled = false;
            }
        }

        public void SetPiece(PieceColor color, Sprite sprite)
        {
            Color = color;
            _renderer.sprite = sprite;
        }

        public void OnSpawned()
        {
            // A reused gem may have been shrunk by the clear animation, so reset everything visual.
            transform.localScale = Vector3.one;
            _renderer.color = UnityEngine.Color.white;
        }

        public void OnDespawned()
        {
            transform.localScale = Vector3.one;
            Color = PieceColor.None;
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
