using System.Collections.Generic;
using System.IO;
using Slicer.Formats;
using Slicer.Geometry;

namespace Slicer.models
{
    public class Model3D
    {
        private readonly Facet[] Facets;
        private readonly string Name;

        /// <summary>
        ///     Creates a generic Model3D with given information.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="facets"></param>
        public Model3D(string name = "Object", Facet[] facets = null)
        {
            Name = name;
            Facets = facets;
        }

        /// <summary>
        ///     Creates a Model3D from the specified file format.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Model3D CreateFromStl(string path, string name = null)
        {
            StlFile stl = new StlFile(path);
            if (name == null) name = Path.GetFileNameWithoutExtension(path);
            return new Model3D(name, stl.GetFacets());
        }

        /// <summary>
        ///     Returns the facets of a 3D model as an array.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Facet> getFacets()
        {
            return Facets;
        }
        

        public string GetName()
        {
            return Name;
        }

        public void Rotate(float x, float y, float z)
        {
            if (x == 0.0f && y == 0.0f && z == 0.0f) return;
        }

        public void Translate(float x, float y, float z)
        {
            if (x == 0.0f && y == 0.0f && z == 0.0f) return;
            foreach (Facet facet in Facets)
            {
                foreach (Point3D vertex in facet.GetVertices())
                {
                    vertex.X += x;
                    vertex.Y += y;
                    vertex.Z += z;
                }
            }
        }

        /// <summary>
        ///     Description of a surface on a Model3D. This is normally a collection of three vertices which make a triangle in 3D
        ///     space.
        /// </summary>
        public class Facet
        {
            public Facet(Point3D[] vertices)
            {
                Vertices = vertices;
            }

            private Point3D[] Vertices { get; }

            public Point3D[] GetVertices()
            {
                return Vertices;
            }

            public override string ToString()
            {
                string s = "Facet with ";
                for (int i = 0; i < Vertices.Length; i++) s += $"V{i + 1}: {Vertices[i]} ";
                return s;
            }
        }
    }

    /// <summary>
    ///     Point in 3D space.
    /// </summary>
    public class Point3D
    {
        public float X;
        public float Y;
        public float Z;

        /// <summary>
        ///     Returns a 3D point from given X, Y, and Z values.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Point3D(float x = 0, float y = 0, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public Point2D To2DPoint()
        {
            return new Point2D(X, Y);
        }
    }
}