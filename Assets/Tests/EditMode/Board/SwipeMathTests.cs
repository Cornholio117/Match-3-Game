using Game.Core.Board;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class SwipeMathTests
    {
        [TestCase(1f, 0f, Direction.Right)]
        [TestCase(-1f, 0f, Direction.Left)]
        [TestCase(0f, 1f, Direction.Up)]
        [TestCase(0f, -1f, Direction.Down)]
        public void TryGetDirection_StraightSwipe_ReturnsThatDirection(float dx, float dy, Direction expected)
        {
            var found = SwipeMath.TryGetDirection(dx, dy, 0.5f, out var direction);

            Assert.That(found, Is.True);
            Assert.That(direction, Is.EqualTo(expected));
        }

        [Test]
        public void TryGetDirection_TooShort_ReturnsFalse()
        {
            Assert.That(SwipeMath.TryGetDirection(0.2f, 0.1f, 0.5f, out _), Is.False);
        }

        [Test]
        public void TryGetDirection_DiagonalSwipe_LongerAxisWins()
        {
            SwipeMath.TryGetDirection(0.4f, 1.0f, 0.5f, out var direction);

            Assert.That(direction, Is.EqualTo(Direction.Up));
        }

        [Test]
        public void TryGetDirection_ExactTie_CountsAsHorizontal()
        {
            SwipeMath.TryGetDirection(-1f, 1f, 0.5f, out var direction);

            Assert.That(direction, Is.EqualTo(Direction.Left));
        }

        [Test]
        public void TryGetDirection_ExactlyTheMinimum_Counts()
        {
            Assert.That(SwipeMath.TryGetDirection(0.5f, 0f, 0.5f, out _), Is.True);
        }
    }
}
