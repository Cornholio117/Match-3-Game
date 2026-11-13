using System;

namespace Game.Core.Board
{
    /// <summary>Screen and safe-area size in pixels. The safe area excludes notches, rounded corners and gesture bars.</summary>
    public readonly struct ScreenArea
    {
        public float Width { get; }
        public float Height { get; }
        public float SafeX { get; }
        public float SafeY { get; }
        public float SafeWidth { get; }
        public float SafeHeight { get; }

        public ScreenArea(float width, float height, float safeX, float safeY, float safeWidth, float safeHeight)
        {
            Width = width;
            Height = height;
            SafeX = safeX;
            SafeY = safeY;
            SafeWidth = safeWidth;
            SafeHeight = safeHeight;
        }
    }

    /// <summary>Where to put an orthographic camera so the whole board sits inside the safe area.</summary>
    public readonly struct CameraFitResult
    {
        public float OrthographicSize { get; }
        public float CameraX { get; }
        public float CameraY { get; }

        public CameraFitResult(float orthographicSize, float cameraX, float cameraY)
        {
            OrthographicSize = orthographicSize;
            CameraX = cameraX;
            CameraY = cameraY;
        }
    }

    public static class CameraFit
    {
        /// <summary>Fits a board (plus a margin on every side) into the safe area and centres it there.</summary>
        public static CameraFitResult Calculate(
            float boardWidth, float boardHeight, float boardCenterX, float boardCenterY, float margin, ScreenArea screen)
        {
            if (screen.SafeWidth <= 0f || screen.SafeHeight <= 0f || screen.Height <= 0f)
                throw new ArgumentException("Screen and safe area must have a positive size.", nameof(screen));

            var neededWidth = boardWidth + 2f * margin;
            var neededHeight = boardHeight + 2f * margin;

            // World units covered by one pixel; the tighter of the two limits decides, so nothing is cut off.
            var unitsPerPixel = Math.Max(neededWidth / screen.SafeWidth, neededHeight / screen.SafeHeight);

            // An orthographic camera shows 2 * size units from top to bottom of the screen.
            var size = unitsPerPixel * screen.Height / 2f;

            // How far the safe area's centre is from the screen's centre, in pixels.
            var offsetX = screen.SafeX + screen.SafeWidth / 2f - screen.Width / 2f;
            var offsetY = screen.SafeY + screen.SafeHeight / 2f - screen.Height / 2f;

            // Moving the camera the opposite way pushes the board toward the safe area's centre.
            return new CameraFitResult(size, boardCenterX - offsetX * unitsPerPixel, boardCenterY - offsetY * unitsPerPixel);
        }
    }
}
