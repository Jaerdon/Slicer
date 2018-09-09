using System;

namespace Slicer.Geometry
{
    /// <summary>
    /// Line segment connecting two points on the same plane. Can also be used for vector math.
    /// </summary>
    public class Line
    {
        public Point2D P;
        public Point2D Q;

        public Point2D Normal;

        public Line(Point2D p, Point2D q, Point2D normal = null)
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

        public bool DoesIntersect(Line line)
        {
            //Vector cross product
            if ((P - line.P) * (line.Q - line.P) < 0 && (Q - line.P) * (line.Q - line.P) > 0)
                return (line.Q - P) * (Q - P) < 0 && (line.P - P) * (Q - P) > 0;
            return false;
        }

        public Point2D GetIntersection(Line line)
        {
            //Slope of each line
            float m1 = 0.0f;
            float m2 = 0.0f;

            if (P.Y != Q.Y && P.X != Q.X)
            {
                m1 = (P.Y - Q.Y) / (P.X - Q.X);
            }
            
            if (line.P.Y != line.Q.Y && line.P.X != line.Q.X)
            {
                m2 = (line.P.Y - line.Q.Y) / (line.P.X - line.Q.X);
            }
            
            float x = (m1 * P.X - m2 * line.P.X + line.P.Y - P.Y) / (m1 - m2); //Solve for x 
            float y = m1 * (x - P.X) + P.Y; //Substitute for y
            
            return new Point2D(x, y);
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(Line))
            {
                return ((Line)obj).P.Equals(P) && ((Line)obj).Q.Equals(Q);
            }

            return false;
        }

    }
}