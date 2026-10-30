using System;

namespace Game.Core.Board
{
    /// <summary>A cell on the board. X is the column, Y is the row; (0,0) is the bottom-left cell.</summary>
    // A readonly struct lives inline (no heap allocation), so passing positions around a
    // 64-cell board every frame creates no garbage for the GC (R5.1).
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public int X { get; }
        public int Y { get; }

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Returns the neighbouring cell in the given direction. Does not check board bounds.</summary>
        public GridPosition Move(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return new GridPosition(X, Y + 1);
                case Direction.Down: return new GridPosition(X, Y - 1);
                case Direction.Left: return new GridPosition(X - 1, Y);
                case Direction.Right: return new GridPosition(X + 1, Y);
                default: throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        /// <summary>True when the cell lies within a board of the given width and height.</summary>
        public bool IsInside(int width, int height) => X >= 0 && X < width && Y >= 0 && Y < height;

        /// <summary>True when the other cell is directly above, below, left or right (a legal swap partner).</summary>
        public bool IsAdjacent(GridPosition other)
        {
            var distance = Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
            return distance == 1;
        }

        // IEquatable<T> lets Dictionary/HashSet compare without boxing the struct.
        public bool Equals(GridPosition other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(GridPosition left, GridPosition right) => left.Equals(right);

        public static bool operator !=(GridPosition left, GridPosition right) => !left.Equals(right);

        public override string ToString() => $"({X},{Y})";
    }
}
