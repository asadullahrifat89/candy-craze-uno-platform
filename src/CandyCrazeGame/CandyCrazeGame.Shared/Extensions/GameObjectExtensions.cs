﻿using Windows.Foundation;

namespace CandyCrazeGame
{
    public static class GameObjectExtensions
    {
        #region Methods      

        /// <summary>
        /// Checks if a two rects intersect.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IntersectsWith(this Rect source, Rect target)
        {
            var targetX = target.X;
            var targetY = target.Y;
            var sourceX = source.X;
            var sourceY = source.Y;

            var sourceWidth = source.Width;
            var sourceHeight = source.Height;

            var targetWidth = target.Width;
            var targetHeight = target.Height;

            if (source.Width >= 0.0 && target.Width >= 0.0
                && targetX <= sourceX + sourceWidth && targetX + targetWidth >= sourceX
                && targetY <= sourceY + sourceHeight)
            {
                return targetY + targetHeight >= sourceY;
            }

            return false;
        }

        public static void PlaceRelativeToCloud(this GameObject gameObject, Cloud landedCloud)
        {
            gameObject.SetPosition(
                 left: (landedCloud.GetLeft() + landedCloud.Width / 2) - gameObject.Width / 2,
                 top: (landedCloud.GetTop() + landedCloud.Height / 2) - gameObject.Height);
        }

        public static Rect GetHitBox(this GameObject gameObject)
        {
            var rect = new Rect(
              x: gameObject.GetLeft(),
              y: gameObject.GetTop(),
              width: gameObject.Width,
              height: gameObject.Height);

            gameObject.SetHitBoxBorder(rect);
            return rect;
        }

        public static Rect GetPlatformHitBox(this GameObject gameObject, double scale)
        {
            var rect = new Rect(
              x: gameObject.GetLeft() + gameObject.Width / 4,
              y: gameObject.GetTop() + gameObject.Height - 10 * scale,
              width: gameObject.Width - gameObject.Width / 4,
              height: 10 * scale);

            gameObject.SetHitBoxBorder(rect);
            return rect;
        }

        public static Rect GetStandingHitBox(this GameObject gameObject, double scale)
        {
            var rect = new Rect(
              x: gameObject.GetLeft() + gameObject.Width / 4,
              y: gameObject.GetTop() + gameObject.Height - 18 * scale,
              width: gameObject.Width - gameObject.Width / 4,
              height: gameObject.Height - 18 * scale);

            gameObject.SetHitBoxBorder(rect);
            return rect;
        }

        public static Rect GetDistantHitBox(this GameObject gameObject)
        {
            return new Rect(
                x: gameObject.GetLeft() - (gameObject.Width * 3),
                y: gameObject.GetTop() - (gameObject.Height * 3),
                width: gameObject.Width + (gameObject.Width * 3),
                height: gameObject.Height + (gameObject.Height * 3));
        }

        #endregion
    }
}
