using System;
using System.Runtime.CompilerServices;

namespace Slicer.Geometry
{
    /// <summary>
    /// Line segment connecting two points on the same plane. Can also be used for vector math.
    /// </summary>
    public class Segment
    {
        public Point2D P;
        public Point2D Q;

        public Point2D Normal;

        public Segment(Point2D p, Point2D q, Point2D normal = null)
        {
            P = p;
            Q = q;
            Normal = normal;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", P, Q);
        }

        public void Swap()
        {
            var temp = P;
            P = Q;
            Q = temp;
        }

        public float GetLength()
        {
            return (float) Math.Abs(Math.Sqrt(Math.Pow(P.X - Q.X, 2) + Math.Pow(P.Y - Q.Y, 2)));
        }

        public bool IntersectsX(float x)
        {
            return (Q.X > x && P.X < x) || (P.X > x && Q.X < x);
        }

        public bool IntersectsY(float y)
        {
            if (P.Y > y && Q.Y < y) return true;
            return (Q.Y > y && P.Y < y);
        }

        public bool DoesIntersect(Segment segment)
        {
            //Vector cross product
            if ((P - segment.P) * (segment.Q - segment.P) < 0 && (Q - segment.P) * (segment.Q - segment.P) > 0)
                return (segment.Q - P) * (Q - P) < 0 && (segment.P - P) * (Q - P) > 0;
            return false;
        }

        public Point2D FindYIntersect(float x)
        {
            if (P.X == Q.X) return new Point2D(x, P.Y);
            float m = (P.Y - Q.Y) / (P.X - Q.X);
            float y = m*x - m*P.X + P.Y;
            return new Point2D(x, y);
        }
        
        public Point2D FindXIntersect(float y)
        {
            if (P.Y == Q.Y) return new Point2D(P.X, P.Y);
            float m = (P.X - Q.X) / (P.Y - Q.Y);
            float x = m*y - m*P.Y + P.X;
            return new Point2D(x, y);
        }

        public Point2D GetIntersection(Segment segment)
        {
            //Slope of each segment
            float m1 = 0.0f;
            float m2 = 0.0f;

            if (P.Y != Q.Y && P.X != Q.X)
            {
                m1 = (P.Y - Q.Y) / (P.X - Q.X);
            }
            
            if (segment.P.Y != segment.Q.Y && segment.P.X != segment.Q.X)
            {
                m2 = (segment.P.Y - segment.Q.Y) / (segment.P.X - segment.Q.X);
            }
            
            float x = (m1 * P.X - m2 * segment.P.X + segment.P.Y - P.Y) / (m1 - m2); //Solve for x 
            float y = m1 * (x - P.X) + P.Y; //Substitute for y
            
            return new Point2D(x, y);
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(Segment))
            {
                return ((Segment)obj).P.Equals(P) && ((Segment)obj).Q.Equals(Q);
            }

            return false;
        }

    }
}