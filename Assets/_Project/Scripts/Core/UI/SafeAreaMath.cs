using System;

namespace Game.Core.UI
{
    /// <summary>Anchor values from 0 to 1 for a rectangle inside the screen: (0,0) is bottom-left, (1,1) is top-right.</summary>
    public readonly struct AnchorRange
    {
        public float MinX { get; }
        public float MinY { get; }
        public float MaxX { get; }
        public float MaxY { get; }

        public AnchorRange(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }
    }

    /// <summary>Turns the part of the screen that is safe to draw on into UI anchors.</summary>
    public static class SafeAreaMath
    {
        /// <summary>
        /// The safe area is in pixels, as Unity's Screen.safeArea reports it (origin bottom-left): it leaves out camera
        /// cut-outs and gesture bars. Anchors are fractions of the screen, so the same UI works at any resolution.
        /// </summary>
        public static AnchorRange ToAnchors(
            float screenWidth, float screenHeight, float safeX, float safeY, float safeWidth, float safeHeight)
        {
            if (screenWidth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(screenWidth));

            if (screenHeight <= 0f)
                throw new ArgumentOutOfRangeException(nameof(screenHeight));

            return new AnchorRange(
                Clamp01(safeX / screenWidth),
                Clamp01(safeY / screenHeight),
                Clamp01((safeX + safeWidth) / screenWidth),
                Clamp01((safeY + safeHeight) / screenHeight));
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;

            return value > 1f ? 1f : value;
        }
    }
}
