using System;
using Slicer.models;

namespace Slicer
{
    public class Polygon
    {
        private float area = 0.0f;
        private Point2D centroid = null;

        /// <summary>
        /// Area of a given polygon
        /// </summary>
        public float Area
        {
            get
            {
                if (area == 0.0f)
                {
                    area = CalculateArea();
                }

                return area;
            }
        }

        public Point2D Centroid
        {
            get
            {
                if (centroid == null)
                {
                    centroid = CalculateCentroid();
                }

                return centroid;
            }
        }

        public readonly Line[] Sides;

        public Polygon(Line[] sides)
        {
            Sides = sides;
        }

        private float CalculateArea()
        {
            float innerArea = 0.0f;
            for (int i = 0; i < Sides.Length - 1; i++)
            {
                innerArea += Sides[i].P.X * Sides[i].Q.Y - Sides[i].Q.X * Sides[i].P.Y;
                innerArea += Sides[i].Q.X * Sides[i + 1].P.Y - Sides[i + 1].P.X * Sides[i].Q.Y;
            }

            innerArea /= 2;

            return innerArea;
        }

        //TODO: Loop over a collection of points instead of each side
        private Point2D CalculateCentroid()
        {
            float x = 0f;
            float y = 0f;

            for (int i = 0; i < Sides.Length - 1; i++)
            {
                x += (Sides[i].P.X + Sides[i].Q.X) * (Sides[i].P.X * Sides[i].Q.Y - Sides[i].Q.X * Sides[i].P.Y);
                x += (Sides[i].Q.X + Sides[i + 1].P.X) *
                     (Sides[i].Q.X * Sides[i + 1].P.Y - Sides[i + 1].P.X * Sides[i].Q.Y);

                y += (Sides[i].P.Y + Sides[i].Q.Y) * (Sides[i].P.X * Sides[i].Q.Y - Sides[i].Q.X * Sides[i].P.Y);
                y += (Sides[i].Q.Y + Sides[i + 1].P.Y) *
                     (Sides[i].Q.X * Sides[i + 1].P.Y - Sides[i + 1].P.X * Sides[i].Q.Y);
            }

            x /= (6 * Area);
            y /= (6 * Area);

            return new Point2D(x, y);
        }
    }

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
    }

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

        public override string ToString()
        {
            return string.Format("X{0} Y{1}", X.ToString("0.0000"), Y.ToString("0.0000"));
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(Point2D))
            {
                return Math.Abs(((Point2D) obj).X - X) < 0.00001 && Math.Abs(((Point2D) obj).Y - Y) < 0.00001;
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