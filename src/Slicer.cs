using Slicer.models;

namespace Slicer {

    internal class Slicer {
        
        public static void Main(string[] args) {
            var slicer = new Slicer();
            slicer.Run();
        }

        private void Run() {
            const string path = "Wheel v4.stl";
            var model = Model3D.CreateFromStl(path);
        }
        
    }

}