using System;

namespace Game.Core.Level
{
    /// <summary>
    /// What the player has achieved: the best stars on each level, and so which levels are unlocked. Level 1 is always
    /// open; a level opens when the one before it has at least one star. Kept in memory for now; the save system
    /// will store and restore it.
    /// </summary>
    public sealed class PlayerProgress
    {
        private readonly int[] _stars;

        /// <summary>Raised with the level index when its best stars went up.</summary>
        public event Action<int> LevelStarsChanged;

        public int LevelCount => _stars.Length;

        public int TotalStars
        {
            get
            {
                var total = 0;
                for (var i = 0; i < _stars.Length; i++)
                    total += _stars[i];

                return total;
            }
        }

        public PlayerProgress(int levelCount)
        {
            if (levelCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(levelCount));

            _stars = new int[levelCount];
        }

        public int GetStars(int levelIndex)
        {
            CheckIndex(levelIndex);
            return _stars[levelIndex];
        }

        public bool IsUnlocked(int levelIndex)
        {
            CheckIndex(levelIndex);
            return levelIndex == 0 || _stars[levelIndex - 1] >= 1;
        }

        public bool HasNextLevel(int levelIndex)
        {
            CheckIndex(levelIndex);
            return levelIndex + 1 < _stars.Length;
        }

        /// <summary>Keeps the best result. Returns true when the stars on that level went up.</summary>
        public bool RecordResult(int levelIndex, int stars)
        {
            CheckIndex(levelIndex);
            if (stars < 0 || stars > StarRating.MaxStars)
                throw new ArgumentOutOfRangeException(nameof(stars));

            if (stars <= _stars[levelIndex])
                return false;

            _stars[levelIndex] = stars;
            LevelStarsChanged?.Invoke(levelIndex);
            return true;
        }

        private void CheckIndex(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _stars.Length)
                throw new ArgumentOutOfRangeException(nameof(levelIndex));
        }
    }
}
