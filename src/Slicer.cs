using System;
using System.Collections.Specialized;
using Slicer.models;

namespace Slicer {

    public class Slicer {
        
        public static void Main(string[] args) {
            var slicer = new Slicer();
            slicer.Run();
        }

        private void Run() {
            const string path = "Cube.stl";
            var model = Model3D.CreateFromStl(path);

            Console.WriteLine(model.getFacets().Length);
            for (var i = 0; i < model.getFacets().Length; i++) {
                Console.WriteLine(i + " : " + model.getFacets()[i]);
            }
        }

        private void SliceByLayer(Model3D model, float layerheight) {
            var height = 0.0f;
            
            //Calculate max height of model
            foreach (var facet in model.getFacets()) {
                for (var i = 1; i < 4; i++) {
                    var z = facet.GetVertices()[i].GetZ();
                    if (z > height) height = z;
                }    
            }

            var layercount = (long) (height / layerheight);
            for (var layer = 0; layer < layercount; layer++) {
                foreach (var facet in model.getFacets()) {
                    var layerZ = layerheight * layer; 
                    var ab = CheckVertices(facet.GetVertices()[1], facet.GetVertices()[2], layerZ);
                    var bc = CheckVertices(facet.GetVertices()[2], facet.GetVertices()[3], layerZ);
                    var ca = CheckVertices(facet.GetVertices()[3], facet.GetVertices()[1], layerZ);

                    var pointA = new Model3D.Vertex();
                    var pointB = new Model3D.Vertex();
                    
                }
            }
        }

        private bool CheckVertices(Model3D.Vertex a, Model3D.Vertex b, float height) {
            return a.GetZ() > height && b.GetZ() < height || a.GetZ() < height && b.GetZ() > height;
        } 
    }

}