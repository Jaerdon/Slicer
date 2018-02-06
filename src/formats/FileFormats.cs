using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Slicer.models;

namespace Slicer.formats {

    public class StlFile {
        
        private Model3D.Facet[] Facets { get; }

        public StlFile(string path) {
            var bytes = File.ReadAllBytes(path);
            Console.WriteLine(System.Text.Encoding.ASCII.GetString(bytes).Substring(0, 80));
            
            var isAscii = System.Text.Encoding.ASCII.GetString(bytes).Substring(0, 80).StartsWith("solid");
            
            var facets = new List<Model3D.Facet>();
            if (isAscii) {
                //ASCII STL Decoding
            } else {
                //Binary STL Decoding
                var triangles = BitConverter.ToInt32(bytes, 80); //First UINT32 is nuber of triangles
                var cur = 84;
                for (var i = 0; i < triangles; i++) { //TODO: Make floats for Vertices (not Facet Normal) unsigned
                    
                    //Facet Normal
                    var normal = new Model3D.Vertex(BitConverter.ToSingle(bytes, cur), 
                                                    BitConverter.ToSingle(bytes, cur + 4), 
                                                    BitConverter.ToSingle(bytes, cur + 8)); 
                    //Vertex 1
                    var v1 = new Model3D.Vertex(BitConverter.ToSingle(bytes, cur + 12), 
                                                BitConverter.ToSingle(bytes, cur + 16), 
                                                BitConverter.ToSingle(bytes, cur + 20));
                    //Vertex 2
                    var v2 = new Model3D.Vertex(BitConverter.ToSingle(bytes, cur + 24), 
                                                BitConverter.ToSingle(bytes, cur + 28), 
                                                BitConverter.ToSingle(bytes, cur + 32));
                    //Vertex 3
                    var v3 = new Model3D.Vertex(BitConverter.ToSingle(bytes, cur + 36), 
                                                BitConverter.ToSingle(bytes, cur + 40), 
                                                BitConverter.ToSingle(bytes, cur + 44));
                    
                    cur += 50; //This is the additional bytes from the starting location plus the 2 Attribute bytes used for color (a single UINT16)
                    
                    facets.Add(new Model3D.Facet(new []{normal, v1, v2, v3}));
                }
            }
            Facets = facets.ToArray();
            for (var i = 0; i < Facets.Length; i++) Console.WriteLine(i + " - " + Facets[i]);
        }
        
        public Model3D.Facet[] GetFacets() {
            return Facets;
        }
        
    }
    
}