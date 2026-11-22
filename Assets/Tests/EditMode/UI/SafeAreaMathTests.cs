using System;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class SafeAreaMathTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ToAnchors_SafeAreaIsTheWholeScreen_ReturnsZeroToOne()
        {
            var anchors = SafeAreaMath.ToAnchors(1080f, 2340f, 0f, 0f, 1080f, 2340f);

            Assert.That(anchors.MinX, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(anchors.MinY, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(anchors.MaxX, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(anchors.MaxY, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void ToAnchors_NotchAtTheTop_LowersTheTopAnchor()
        {
            // A camera cut-out takes the top 140 of 2340 pixels, so the safe area is 2200 pixels tall from the bottom.
            var anchors = SafeAreaMath.ToAnchors(1080f, 2340f, 0f, 0f, 1080f, 2200f);

            Assert.That(anchors.MinY, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(anchors.MaxY, Is.EqualTo(2200f / 2340f).Within(Tolerance));
        }

        [Test]
        public void ToAnchors_GestureBarAtTheBottom_RaisesTheBottomAnchor()
        {
            // A gesture bar takes the bottom 60 pixels: the safe area starts at y = 60 and reaches the top.
            var anchors = SafeAreaMath.ToAnchors(1080f, 2340f, 0f, 60f, 1080f, 2280f);

            Assert.That(anchors.MinY, Is.EqualTo(60f / 2340f).Within(Tolerance));
            Assert.That(anchors.MaxY, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void ToAnchors_CutOutOnTheSide_NarrowsTheHorizontalAnchors()
        {
            var anchors = SafeAreaMath.ToAnchors(2400f, 1080f, 100f, 0f, 2200f, 1080f);

            Assert.That(anchors.MinX, Is.EqualTo(100f / 2400f).Within(Tolerance));
            Assert.That(anchors.MaxX, Is.EqualTo(2300f / 2400f).Within(Tolerance));
        }

        [Test]
        public void ToAnchors_SafeAreaLargerThanTheScreen_IsClampedToZeroAndOne()
        {
            var anchors = SafeAreaMath.ToAnchors(1000f, 2000f, -10f, -20f, 1100f, 2100f);

            Assert.That(anchors.MinX, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(anchors.MinY, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(anchors.MaxX, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(anchors.MaxY, Is.EqualTo(1f).Within(Tolerance));
        }

        [TestCase(0f, 2340f)]
        [TestCase(1080f, 0f)]
        [TestCase(-1f, 100f)]
        public void ToAnchors_ScreenWithNoSize_Throws(float width, float height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SafeAreaMath.ToAnchors(width, height, 0f, 0f, 10f, 10f));
        }
    }
}
