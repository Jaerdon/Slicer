using System;

namespace Slicer.Geometry
{
    /// <summary>
    ///     Point in 2D space (assumed to be on the XY plane). Can also be used for vector maths.
    /// </summary>
    public class Point2D
    {
        public float X;
        public float Y;

        public Point2D(float x = 0, float y = 0)
        {
            X = x;
            Y = y;
        }

        public string ToPair()
        {
            return $"{X:0.0000},{Y:0.0000}";
        }

        public override string ToString()
        {
            return $"X{X:0.0000} Y{Y:0.0000}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Point2D)) return false;
            return Math.Abs(((Point2D) obj).X - X) < 0.00001 && Math.Abs(((Point2D) obj).Y - Y) < 0.00001;
        }

        public static Point2D operator +(Point2D a, Point2D b)
        {
            return new Point2D(a.X + b.X, a.Y + b.Y);
        }

        public static Point2D operator -(Point2D a, Point2D b)
        {
            return new Point2D(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        ///     Returns a scalar value
        /// </summary>
        public static float operator *(Point2D a, Point2D b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}