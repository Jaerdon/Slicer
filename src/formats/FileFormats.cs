using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using Slicer.models;

namespace Slicer.formats
{
    public class StlFile
    {
        private Model3D.Facet[] Facets { get; set; }

        private const int HeaderLen = 80;
        private const int UnitSize = 4;

        private const string ModelPrefix = "solid ";
        private const string ModelSuffix = "endsolid";

        private const string FacetPrefix = "facet normal";

        private const string LoopPrefix = "outer";
        private const string LoopSuffix = "endloop";

        /// <summary>
        /// Reads an .stl file from a given file path. Accepts both binary and ASCII .stl files.
        /// </summary>
        /// <param name="path"></param>
        public StlFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            //Console.WriteLine(Encoding.ASCII.GetString(bytes).Substring(0, 80));

            var isAscii = Encoding.ASCII.GetString(bytes).Substring(0, HeaderLen).StartsWith(ModelPrefix);

            var facets = new List<Model3D.Facet>();
            if (isAscii)
            {
                //ASCII STL Reading
                var ascii = File.ReadAllLines(path);
                string name = ascii[0].Substring(6);
                for (int i = 1; i < ascii.Length; i++)
                {
                    string line = ascii[i].Trim();
                    if (line.StartsWith(ModelSuffix)) { break; } //End of solid

                    if (line.StartsWith(FacetPrefix))
                    {
                        Point3D[] vertices = {};
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
                                var vertex = ascii[i].Trim().Substring(6).Split(' ');
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
                var triangles = BitConverter.ToInt32(bytes, HeaderLen); //First UINT32 is number of triangles
                var cur = HeaderLen + UnitSize;
                for (var i = 0; i < triangles; i++)
                {
                    //Facet Normal
                    var normal = new Point3D(
                        BitConverter.ToSingle(bytes, cur),
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize));
                    //Vertex 1
                    var v1 = new Point3D(
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize));
                    //Vertex 2
                    var v2 = new Point3D(
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize));
                    //Vertex 3
                    var v3 = new Point3D(
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize),
                        BitConverter.ToSingle(bytes, cur += UnitSize));

                    cur += UnitSize + UnitSize / 2; //This is the ending bytes from the starting location plus the 2 Attribute bytes used for color (a single UINT16)
                    //While we could be using the color attribute bytes, we're not because it's not necessary.

                    facets.Add(new Model3D.Facet(new[] {normal, v1, v2, v3}));
                }
            }

            Facets = facets.ToArray();
        }

        /// <summary>
        /// Returns all facets for a 3D model as an array.
        /// </summary>
        /// <returns></returns>
        public Model3D.Facet[] GetFacets()
        {
            return Facets;
        }
    }
}