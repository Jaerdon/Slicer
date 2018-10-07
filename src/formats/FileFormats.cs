using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Slicer.models;

namespace Slicer.Formats
{
    /// <summary>
    ///     STL File Format
    /// </summary>
    public class StlFile
    {
        private const int HeaderLen = 80;
        private const int UnitSize = 4;

        private const string ModelPrefix = "solid ";
        private const string ModelSuffix = "endsolid";

        private const string FacetPrefix = "facet normal";

        private const string LoopPrefix = "outer";
        private const string LoopSuffix = "endloop";

        /// <summary>
        ///     Reads an .stl file from a given file path. Accepts both binary and ASCII .stl files.
        /// </summary>
        /// <param name="path"></param>
        public StlFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            //Console.WriteLine(Encoding.ASCII.GetString(bytes).Substring(0, 80));

            bool isAscii = Encoding.ASCII.GetString(bytes).Substring(0, HeaderLen).StartsWith(ModelPrefix);

            List<Model3D.Facet> facets = new List<Model3D.Facet>();
            if (isAscii)
            {
                //ASCII STL Reading
                string[] ascii = File.ReadAllLines(path);
                string name = ascii[0].Substring(6);
                for (int i = 1; i < ascii.Length; i++)
                {
                    string line = ascii[i].Trim();
                    if (line.StartsWith(ModelSuffix)) break;

                    if (line.StartsWith(FacetPrefix))
                    {
                        Point3D[] vertices = { };
                        string[] normal = line.Substring(13).Split(' ');
                        vertices[0] = new Point3D( //Facet normal
                            float.Parse(normal[0]),
                            float.Parse(normal[1]),
                            float.Parse(normal[2]));

                        if (ascii[i + 1].Trim().StartsWith(LoopPrefix))
                        {
                            ++i;
                            for (int j = 1; !ascii[i].Trim().StartsWith(LoopSuffix); j++)
                            {
                                ++i;
                                string[] vertex = ascii[i].Trim().Substring(6).Split(' ');
                                vertices[j] = new Point3D(
                                    float.Parse(vertex[0]),
                                    float.Parse(vertex[1]),
                                    float.Parse(vertex[2]));
                            }
                        }

                        i += 2;
                        facets.Add(new Model3D.Facet(vertices));
                    }
                }

                Facets = facets.ToArray();
            }
            else
            {
                //Binary STL Reading
                int triangles = BitConverter.ToInt32(bytes, HeaderLen); //First UINT32 is number of triangles
                int cur = HeaderLen + UnitSize;
                for (int i = 0; i < triangles; i++)
                {
                    //Facet Normal
                    Point3D normal = new Point3D(
                        BitConverter.ToSingle(bytes, cur),
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize));
                    //Vertex 1
                    Point3D v1 = new Point3D(
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize));
                    //Vertex 2
                    Point3D v2 = new Point3D(
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize));
                    //Vertex 3
                    Point3D v3 = new Point3D(
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize));

                    cur += UnitSize + UnitSize /
                           2; //This is the ending bytes from the starting location plus the 2 Attribute bytes used for color (a single UINT16)
                    //While we could be using the color attribute bytes, we're not because it's not necessary.

                    facets.Add(new Model3D.Facet(new[] {normal, v1, v2, v3}));
                }
            }

            Facets = facets.ToArray();
        }

        private Model3D.Facet[] Facets { get; }

        /// <summary>
        ///     Returns all facets for a 3D model as an array.
        /// </summary>
        /// <returns></returns>
        public Model3D.Facet[] GetFacets()
        {
            return Facets;
        }
    }
}