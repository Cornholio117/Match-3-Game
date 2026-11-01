using System.Collections.Generic;
using NUnit.Framework;

namespace Game.Tests.EditMode.Study
{
    // Study exercises (week 1, Tuesday): one small puzzle per collection type.
    public static class CollectionsExercises
    {
        // HashSet: remember what was seen, and stop as soon as a number repeats.
        public static bool ContainsDuplicate(int[] values)
        {
            var seen = new HashSet<int>();
            for (var i = 0; i < values.Length; i++)
            {
                if (seen.Contains(values[i]))
                    return true;

                seen.Add(values[i]);
            }

            return false;
        }
    }

    public sealed class CollectionsExercisesTests
    {
        private enum PieceColor
        {
            Red,
            Blue,
            Green,
            Yellow
        }

        [Test]
        public void Dictionary_WriteToExistingKey_ReplacesValue()
        {
            var scoreFor = new Dictionary<PieceColor, int>();
            scoreFor[PieceColor.Red] = 10;

            scoreFor[PieceColor.Red] = 50;

            Assert.That(scoreFor[PieceColor.Red], Is.EqualTo(50));
        }

        [Test]
        public void Dictionary_ReadMissingKey_Throws()
        {
            var scoreFor = new Dictionary<PieceColor, int>();
            scoreFor[PieceColor.Red] = 10;

            Assert.Throws<KeyNotFoundException>(() => { var points = scoreFor[PieceColor.Yellow]; });
        }

        [Test]
        public void TryGetValue_MissingKey_ReturnsFalse()
        {
            var scoreFor = new Dictionary<PieceColor, int>();
            scoreFor[PieceColor.Red] = 10;

            var found = scoreFor.TryGetValue(PieceColor.Yellow, out var points);

            Assert.That(found, Is.False);
            Assert.That(points, Is.EqualTo(0));
        }

        [Test]
        public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
        {
            var scoreFor = new Dictionary<PieceColor, int>();
            scoreFor[PieceColor.Red] = 10;

            var found = scoreFor.TryGetValue(PieceColor.Red, out var points);

            Assert.That(found, Is.True);
            Assert.That(points, Is.EqualTo(10));
        }

        [Test]
        public void Stack_PopTwice_ReturnsNewestMovesFirst()
        {
            var moves = new Stack<string>();
            moves.Push("swap A");
            moves.Push("swap B");
            moves.Push("swap C");

            var first = moves.Pop();
            var second = moves.Pop();

            Assert.That(first, Is.EqualTo("swap C"));
            Assert.That(second, Is.EqualTo("swap B"));
            Assert.That(moves.Count, Is.EqualTo(1));
        }

        [Test]
        public void ContainsDuplicate_NumberRepeats_ReturnsTrue()
        {
            Assert.That(CollectionsExercises.ContainsDuplicate(new[] { 1, 2, 3, 1 }), Is.True);
        }

        [Test]
        public void ContainsDuplicate_AllDifferent_ReturnsFalse()
        {
            Assert.That(CollectionsExercises.ContainsDuplicate(new[] { 1, 2, 3, 4 }), Is.False);
        }

        [Test]
        public void ContainsDuplicate_EmptyArray_ReturnsFalse()
        {
            Assert.That(CollectionsExercises.ContainsDuplicate(new int[0]), Is.False);
        }
    }
}
