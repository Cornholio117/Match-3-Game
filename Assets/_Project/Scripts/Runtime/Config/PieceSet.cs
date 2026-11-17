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
