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
    
        // Adds a single vertex to the STL object
        public void addVertex(double x = 0, double y = 0, double z = 0) {
            
        }

        public struct Facet {
            private Vertex[] Vertices { get; }

            public Facet(Vertex[] vertices) {
                Vertices = vertices;
            }
        }
        
        public class Vertex {
            private double X { get; }
            private double Y { get; }
            private double Z { get; }

            public Vertex(double x = 0, double y = 0, double z = 0) {
                X = x;
                Y = y;
                Z = z;
            }
        }   
    }
    
}