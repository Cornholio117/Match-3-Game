using NUnit.Framework;

namespace Game.Tests.EditMode.Study
{
    // Study exercise: struct = photocopy, class = two names for one object.
    public sealed class ValueVsReferenceTests
    {
        private struct PointS
        {
            public int X;
        }

        private sealed class PointC
        {
            public int X;
        }

        [Test]
        public void CopyStruct_ChangeCopy_OriginalUnchanged()
        {
            var original = new PointS { X = 1 };
            var copy = original;        // photocopy of the data

            copy.X = 99;

            Assert.That(original.X, Is.EqualTo(1));
            Assert.That(copy.X, Is.EqualTo(99));
        }

        [Test]
        public void CopyClass_ChangeCopy_OriginalAlsoChanges()
        {
            var original = new PointC { X = 1 };
            var copy = original;        // same object, second name

            copy.X = 99;

            Assert.That(original.X, Is.EqualTo(99));
            Assert.That(ReferenceEquals(original, copy), Is.True);
        }
    }
}
