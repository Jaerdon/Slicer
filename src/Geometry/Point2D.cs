using System;

namespace Slicer.Geometry
{
    /// <summary>
    /// Point in 2D space on a plane. Can also be used for vector math.
    /// </summary>
    public class Point2D
    {
        public readonly float X;
        public readonly float Y;

        public Point2D(float x = 0, float y = 0)
        {
            X = x;
            Y = y;
        }

        public string ToPair()
        {
            return string.Format("{0},{1}", X.ToString("0.0000"), Y.ToString("0.0000"));
        }
        
        public override string ToString()
        {
            return string.Format("X{0} Y{1}", X.ToString("0.0000"), Y.ToString("0.0000"));
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(Point2D))
            {
                var foo = Math.Abs(((Point2D) obj).X - X) < 0.00001 && Math.Abs(((Point2D) obj).Y - Y) < 0.00001;
                //if (foo) Console.WriteLine(this.ToPair() + " compares to " + obj.ToString() + ": " + foo);
                return foo;
            }

            return false;
        }

        public static Point2D operator +(Point2D a, Point2D b )
        {
            return new Point2D(a.X + b.X, a.Y + b.Y);
        }
        
        public static Point2D operator -(Point2D a, Point2D b)
        {
            return new Point2D(a.X - b.X, a.Y - b.Y);
        }
        
        /// <summary>
        /// Returns a scalar value
        /// </summary>
        public static float operator *(Point2D a, Point2D b)
        {
            return (a.X * b.Y) - (a.Y * b.X);
        }
    }
}