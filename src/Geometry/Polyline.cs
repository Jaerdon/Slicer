using System.Collections.Generic;
using System.Linq;

namespace Slicer.Geometry
{
    public class Polyline
    {
        public readonly Segment[] Sides;
        private float area;
        private Point2D centroid;

        public Polyline(Segment[] sides)
        {
            Sides = sides;
        }

        /// <summary>
        ///     Area of a given polygon
        /// </summary>
        private float Area
        {
            get
            {
                if (area == 0.0f) area = CalculateArea();

                return area;
            }
        }

        public Point2D Centroid => centroid ?? (centroid = CalculateCentroid());

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
            float x = 0.0f;
            float y = 0.0f;

            for (int i = 0; i < Sides.Length - 1; i++)
            {
                x += (Sides[i].P.X + Sides[i].Q.X) * (Sides[i].P.X * Sides[i].Q.Y - Sides[i].Q.X * Sides[i].P.Y);
                x += (Sides[i].Q.X + Sides[i + 1].P.X) *
                     (Sides[i].Q.X * Sides[i + 1].P.Y - Sides[i + 1].P.X * Sides[i].Q.Y);

                y += (Sides[i].P.Y + Sides[i].Q.Y) * (Sides[i].P.X * Sides[i].Q.Y - Sides[i].Q.X * Sides[i].P.Y);
                y += (Sides[i].Q.Y + Sides[i + 1].P.Y) *
                     (Sides[i].Q.X * Sides[i + 1].P.Y - Sides[i + 1].P.X * Sides[i].Q.Y);
            }

            x /= 6 * Area;
            y /= 6 * Area;

            return new Point2D(x, y);
        }

        public static Polyline operator +(Polyline a, Polyline b)
        {
            return new Polyline(new List<Segment>(a.Sides).Concat(b.Sides).ToArray());
        }
    }
}