using Game.Core.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Common
{
    public sealed class SeededRandomTests
    {
        [Test]
        public void Next_SameSeed_ReturnsSameSequence()
        {
            var first = new SeededRandom(42);
            var second = new SeededRandom(42);

            for (var i = 0; i < 20; i++)
                Assert.That(first.Next(100), Is.EqualTo(second.Next(100)));
        }

        [Test]
        public void Next_ManyCalls_StaysBelowMaximum()
        {
            var random = new SeededRandom(7);

            for (var i = 0; i < 500; i++)
                Assert.That(random.Next(5), Is.InRange(0, 4));
        }
    }
}
