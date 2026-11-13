using Game.Core.Board;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class CameraFitTests
    {
        private const float Tolerance = 0.001f;

        private static ScreenArea FullScreen(float width, float height)
        {
            return new ScreenArea(width, height, 0f, 0f, width, height);
        }

        [Test]
        public void Calculate_SquareScreen_FitsBoardExactly()
        {
            var result = CameraFit.Calculate(8f, 8f, 3.5f, 3.5f, 0f, FullScreen(1000f, 1000f));

            Assert.That(result.OrthographicSize, Is.EqualTo(4f).Within(Tolerance));
            Assert.That(result.CameraX, Is.EqualTo(3.5f).Within(Tolerance));
            Assert.That(result.CameraY, Is.EqualTo(3.5f).Within(Tolerance));
        }

        [Test]
        public void Calculate_TallPhone_WidthIsTheLimit()
        {
            var result = CameraFit.Calculate(8f, 8f, 3.5f, 3.5f, 0f, FullScreen(1080f, 1920f));

            // The camera must show 8 units across: visible width = 2 * size * aspect.
            var visibleWidth = 2f * result.OrthographicSize * (1080f / 1920f);
            Assert.That(visibleWidth, Is.EqualTo(8f).Within(Tolerance));
        }

        [Test]
        public void Calculate_WideScreen_HeightIsTheLimit()
        {
            var result = CameraFit.Calculate(8f, 8f, 3.5f, 3.5f, 0f, FullScreen(1920f, 1080f));

            Assert.That(result.OrthographicSize, Is.EqualTo(4f).Within(Tolerance));
        }

        [Test]
        public void Calculate_WithMargin_ShowsMoreThanTheBoard()
        {
            var noMargin = CameraFit.Calculate(8f, 8f, 3.5f, 3.5f, 0f, FullScreen(1080f, 1920f));
            var withMargin = CameraFit.Calculate(8f, 8f, 3.5f, 3.5f, 1f, FullScreen(1080f, 1920f));

            // 8 + 2 * 1 = 10 units across instead of 8.
            Assert.That(withMargin.OrthographicSize / noMargin.OrthographicSize, Is.EqualTo(10f / 8f).Within(Tolerance));
        }

        [Test]
        public void Calculate_NotchAtTop_ShiftsCameraUp()
        {
            // 100 px at the top is not safe, so the safe area's centre is 50 px below the screen's centre.
            var screen = new ScreenArea(1080f, 1920f, 0f, 0f, 1080f, 1820f);

            var result = CameraFit.Calculate(8f, 8f, 3.5f, 3.5f, 0f, screen);

            // One pixel covers 8 / 1080 units, so 50 px is about 0.37 units; the camera moves up to push the board down.
            Assert.That(result.CameraY, Is.EqualTo(3.5f + 50f * 8f / 1080f).Within(Tolerance));
            Assert.That(result.CameraX, Is.EqualTo(3.5f).Within(Tolerance));
        }

        [Test]
        public void Calculate_ZeroSizedSafeArea_Throws()
        {
            var screen = new ScreenArea(1080f, 1920f, 0f, 0f, 0f, 0f);

            Assert.Throws<System.ArgumentException>(() => CameraFit.Calculate(8f, 8f, 3.5f, 3.5f, 0f, screen));
        }
    }
}
