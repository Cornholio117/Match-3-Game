using System;

namespace Game.Core.Board
{
    /// <summary>Turns a finger movement into a swap direction.</summary>
    public static class SwipeMath
    {
        /// <summary>
        /// Returns true and the direction when the finger moved at least minDistance. The longer axis wins,
        /// and an exact tie counts as horizontal. Up is positive y, as on the screen and on the board.
        /// </summary>
        public static bool TryGetDirection(float dx, float dy, float minDistance, out Direction direction)
        {
            var absX = Math.Abs(dx);
            var absY = Math.Abs(dy);

            if (Math.Max(absX, absY) < minDistance)
            {
                direction = default;
                return false;
            }

            if (absX >= absY)
                direction = dx > 0f ? Direction.Right : Direction.Left;
            else
                direction = dy > 0f ? Direction.Up : Direction.Down;

            return true;
        }
    }
}
