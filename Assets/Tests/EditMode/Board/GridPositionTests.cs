using System.Collections.Generic;
using Game.Core.Board;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class GridPositionTests
    {
        [TestCase(Direction.Up, 2, 4)]
        [TestCase(Direction.Down, 2, 2)]
        [TestCase(Direction.Left, 1, 3)]
        [TestCase(Direction.Right, 3, 3)]
        public void Move_EachDirection_ShiftsByOneCell(Direction direction, int expectedX, int expectedY)
        {
            var start = new GridPosition(2, 3);

            var moved = start.Move(direction);

            Assert.That(moved, Is.EqualTo(new GridPosition(expectedX, expectedY)));
        }

        [Test]
        public void Move_DoesNotChangeOriginal()
        {
            var start = new GridPosition(2, 3);

            start.Move(Direction.Up);

            Assert.That(start, Is.EqualTo(new GridPosition(2, 3)));
        }

        [TestCase(0, 0, true)]
        [TestCase(7, 7, true)]
        [TestCase(-1, 0, false)]
        [TestCase(0, -1, false)]
        [TestCase(8, 0, false)]
        [TestCase(0, 8, false)]
        public void IsInside_8x8Board_ReturnsExpected(int x, int y, bool expected)
        {
            Assert.That(new GridPosition(x, y).IsInside(8, 8), Is.EqualTo(expected));
        }

        [TestCase(2, 2, 3, 2, true)]
        [TestCase(2, 2, 2, 1, true)]
        [TestCase(2, 2, 3, 3, false)]
        [TestCase(2, 2, 4, 2, false)]
        [TestCase(2, 2, 2, 2, false)]
        public void IsAdjacent_VariousPairs_OnlyOrthogonalNeighboursCount(int ax, int ay, int bx, int by, bool expected)
        {
            var a = new GridPosition(ax, ay);
            var b = new GridPosition(bx, by);

            Assert.That(a.IsAdjacent(b), Is.EqualTo(expected));
            Assert.That(b.IsAdjacent(a), Is.EqualTo(expected));
        }

        [Test]
        public void Equals_SameCoordinates_AreEqualAndShareHashCode()
        {
            var a = new GridPosition(4, 5);
            var b = new GridPosition(4, 5);

            Assert.That(a == b, Is.True);
            Assert.That(a != b, Is.False);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentCoordinates_AreNotEqual()
        {
            Assert.That(new GridPosition(1, 2) == new GridPosition(2, 1), Is.False);
        }

        [Test]
        public void HashSet_SameCoordinates_StoredOnce()
        {
            var set = new HashSet<GridPosition> { new GridPosition(1, 1), new GridPosition(1, 1) };

            Assert.That(set.Count, Is.EqualTo(1));
        }
    }
}
