using System;
using UnityEngine;

namespace Game.Runtime.Config
{
    /// <summary>The levels of the game in play order. Read-only at runtime.</summary>
    [CreateAssetMenu(menuName = "Match3/Level Catalog", fileName = "SO_LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [Header("Levels")]
        [Tooltip("Every level, first to last. A level unlocks when the one before it is won.")]
        [SerializeField] private LevelConfig[] _levels = new LevelConfig[0];

        public int Count => _levels.Length;

        public LevelConfig Get(int index)
        {
            if (index < 0 || index >= _levels.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _levels[index];
        }
    }
}
