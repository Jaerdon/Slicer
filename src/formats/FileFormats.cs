using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Slicer.models;

namespace Slicer.formats {

    public class StlFile {
        
        private Model3D.Facet[] Facets { get; }

        public StlFile(string path) {
            var bytes = File.ReadAllBytes(path);
            var isAscii = BitConverter.ToString(bytes).Replace('-', '').StartsWith("solid");
            if (isAscii) {
                //ASCII STL Decoding
            } else {
                //Binary STL Decoding
                for (var i = 0; i < bytes.Length; i++) {
                    Console.WriteLine(i + " : " + bytes[i]);
                }
            }
        }
        
        public Model3D.Facet[] GetFacets() {
            return Facets;
        }
        
    }
    
}