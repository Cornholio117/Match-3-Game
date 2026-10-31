using System;

namespace Game.Core.Level
{
    /// <summary>Tracks the remaining moves for the current level.</summary>
    public sealed class MovesModel
    {
        public event Action<int> MovesChanged;
        public event Action OutOfMoves;

        public int Remaining { get; private set; }

        public MovesModel(int startingMoves)
        {
            if (startingMoves <= 0)
                throw new ArgumentOutOfRangeException(nameof(startingMoves));

            Remaining = startingMoves;
        }

        public void Consume()
        {
            if (Remaining == 0) return;

            Remaining--;
            MovesChanged?.Invoke(Remaining);

            if (Remaining == 0)
                OutOfMoves?.Invoke();
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;

            Remaining += amount;
            MovesChanged?.Invoke(Remaining);
        }
    }
}
