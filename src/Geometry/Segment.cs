using System;

namespace Slicer.Geometry
{
    /// <summary>
    ///     Line segment connecting two points on the same plane. Can also be used for vector math.
    /// </summary>
    public class Segment
    {
        public Point2D Normal;
        public Point2D P;
        public Point2D Q;

        public Segment(Point2D p, Point2D q, Point2D normal = null)
        {
            P = p;
            Q = q;
            Normal = normal;
        }

        public override string ToString()
        {
            return $"[{P}, {Q}]";
        }

        public void Swap()
        {
            Point2D temp = P;
            P = Q;
            Q = temp;
        }

        public float GetLength()
        {
            return (float) Math.Abs(Math.Sqrt(Math.Pow(P.X - Q.X, 2) + Math.Pow(P.Y - Q.Y, 2)));
        }

        public bool IntersectsX(float x)
        {
            return Q.X > x && P.X < x || P.X > x && Q.X < x;
        }

        public bool IntersectsY(float y)
        {
            if (P.Y > y && Q.Y < y) return true;
            return Q.Y > y && P.Y < y;
        }

        public Point2D FindYIntersect(float x)
        {
            if (P.X == Q.X) return new Point2D(x, P.Y);
            float m = (P.Y - Q.Y) / (P.X - Q.X);
            float y = m * x - m * P.X + P.Y;
            return new Point2D(x, y);
        }

        public Point2D FindXIntersect(float y)
        {
            if (P.Y == Q.Y) return new Point2D(P.X, P.Y);
            float m = (P.X - Q.X) / (P.Y - Q.Y);
            float x = m * y - m * P.Y + P.X;
            return new Point2D(x, y);
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(Segment))
                return ((Segment) obj).P.Equals(P) && ((Segment) obj).Q.Equals(Q);

            return false;
        }
    }
}