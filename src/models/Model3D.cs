using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using Slicer.formats;

namespace Slicer.models {
    
    public class Model3D {
        
        private string Name { get; set; }
        private Facet[] Facets { get; set; }

        // Creates a Model3D. There is no reason this should be used as of yet.
        public Model3D(string name = "Object", List<Facet> facets = null) {
            Name = name;
            Facets = facets.ToArray();
        }
        
        // Creates a Model3D from the specified file format. 
        public Model3D createFromStl(string path, string name = "Object") {
            var stl = new StlFile(path);
            Name = name;
            Facets = stl.GetFacets();
            return this;
        }

        public struct Facet {
            private Vertex[] Vertices { get; }

            public Facet(Vertex[] vertices) {
                Vertices = vertices;
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
            
            public override string ToString() {
                return "(" + X + ", " + Y + ", " + Z + ")";
            }
        }   
    }
    
}