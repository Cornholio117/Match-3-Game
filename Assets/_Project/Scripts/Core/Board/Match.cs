using System;

namespace Game.Core.Board
{
    /// <summary>A run of 3 or more gems of one colour in a row (Direction.Right) or a column (Direction.Up).</summary>
    public readonly struct Match
    {
        public PieceColor Color { get; }
        public GridPosition Start { get; }
        public Direction Direction { get; }
        public int Length { get; }

        public Match(PieceColor color, GridPosition start, Direction direction, int length)
        {
            if (direction != Direction.Right && direction != Direction.Up)
                throw new ArgumentException("A match runs Right or Up.", nameof(direction));

            Color = color;
            Start = start;
            Direction = direction;
            Length = length;
        }

        /// <summary>The cell at the given index along the run, starting from Start.</summary>
        public GridPosition GetPosition(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Direction == Direction.Right
                ? new GridPosition(Start.X + index, Start.Y)
                : new GridPosition(Start.X, Start.Y + index);
        }
    }
}
