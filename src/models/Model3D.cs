using System.Collections.Generic;
using Slicer.formats;

namespace Slicer.models {
    
    public class Model3D {
        
        private string Name { get; set; }
        private Facet[] Facets { get; set; }

        // Creates a generic Model3D
        public Model3D(string name = "Object", Facet[] facets = null) {
            Name = name;
            Facets = facets;
        }
        
        // Creates a Model3D from the specified file format. 
        public static Model3D CreateFromStl(string path, string name = "Object") {
            var stl = new StlFile(path);
            return new Model3D(name, stl.GetFacets()); 
        }

        public Facet[] getFacets() {
            return Facets;
        }

        public class Facet {
            private Vertex[] Vertices { get; }

            public Facet(Vertex[] vertices) {
                Vertices = vertices;
            }

            public Vertex[] GetVertices() {
                return Vertices;
            }

            public override string ToString() {
                var s = "Facet with ";
                for (var i = 0; i < Vertices.Length; i++) {
                    s += "V" + (i + 1) + ": " + Vertices[i] + " ";
                }

                return s;
            }
        }
        
        public class Vertex {
            private float X { get; }
            private float Y { get; }
            private float Z { get; }

            public Vertex(float x = 0, float y = 0, float z = 0) {
                X = x;
                Y = y;
                Z = z;
            }

            public float GetX() { return X; }
            public float GetY() { return Y; }
            public float GetZ() { return Z; }

            public override string ToString() {
                return "(" + X + ", " + Y + ", " + Z + ")";
            }
        }   
    }
    
}