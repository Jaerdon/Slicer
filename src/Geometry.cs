using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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
                innerArea += Sides[i].A.X * Sides[i].B.Y - Sides[i].B.X * Sides[i].A.Y;
                innerArea += Sides[i].B.X * Sides[i + 1].A.Y - Sides[i + 1].A.X * Sides[i].B.Y;
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
                x += (Sides[i].A.X + Sides[i].B.X) * (Sides[i].A.X * Sides[i].B.Y - Sides[i].B.X * Sides[i].A.Y);
                x += (Sides[i].B.X + Sides[i + 1].A.X) *
                     (Sides[i].B.X * Sides[i + 1].A.Y - Sides[i + 1].A.X * Sides[i].B.Y);

                y += (Sides[i].A.Y + Sides[i].B.Y) * (Sides[i].A.X * Sides[i].B.Y - Sides[i].B.X * Sides[i].A.Y);
                y += (Sides[i].B.Y + Sides[i + 1].A.Y) *
                     (Sides[i].B.X * Sides[i + 1].A.Y - Sides[i + 1].A.X * Sides[i].B.Y);
            }

            x /= (6 * Area);
            y /= (6 * Area);

            return new Point2D(x, y);
        }
    }

    /// <summary>
    /// Line segment connecting two points on the same plane.
    /// </summary>
    public class Line
    {
        public Point2D A;
        public Point2D B;

        public Line(Point2D a, Point2D b)
        {
            A = a;
            B = b;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", A, B);
        }

        public void Swap()
        {
            var temp = A;
            A = B;
            B = temp;
        }

        public float GetLength()
        {
            return (float) Math.Abs(Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2)));
        }

        public bool Intersects(Line line)
        {
            //Vector cross product


            return false;
        }
    }

    /// <summary>
    /// Point in 2D space on a given or assumed plane.
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
    }
}