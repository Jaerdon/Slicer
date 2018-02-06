using Slicer.formats;

namespace Slicer {

    internal class Slicer {
        public static void Main(string[] args) {
            const string path = "Wheel v4.stl";
            var stl = new StlFile(path);
        }
    }

}