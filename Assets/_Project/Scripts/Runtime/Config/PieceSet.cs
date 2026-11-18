using Game.Core.Board;
using UnityEngine;

namespace Game.Runtime.Config
{
    /// <summary>Which sprite shows each gem colour. Read-only at runtime.</summary>
    [CreateAssetMenu(menuName = "Match3/Piece Set", fileName = "SO_PieceSet")]
    public sealed class PieceSet : ScriptableObject
    {
        [Header("Sprites")]
        [Tooltip("One sprite per colour, in this order: Green, Blue, Red, Purple, Yellow, White, Pink. Unused colours can stay empty.")]
        [SerializeField] private Sprite[] _sprites = new Sprite[BoardModel.MaxColorCount];

        [Tooltip("Arrow drawn on a line gem. It points left and right; a vertical line gem shows it turned a quarter.")]
        [SerializeField] private Sprite _lineOverlay;

        /// <summary>The overlay drawn on a special gem of this kind, or null for a normal gem.</summary>
        public Sprite GetOverlay(PieceKind kind)
        {
            return kind == PieceKind.LineHorizontal || kind == PieceKind.LineVertical ? _lineOverlay : null;
        }

        /// <summary>The sprite for a colour, or null if none is assigned.</summary>
        public Sprite GetSprite(PieceColor color)
        {
            var index = (int)color - 1;
            if (index < 0 || index >= _sprites.Length)
                return null;

            return _sprites[index];
        }
    }
}
