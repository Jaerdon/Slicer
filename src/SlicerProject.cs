using System;
using System.IO;
using Slicer.Formats;
using Slicer.models;

namespace Slicer
{
    public class SlicerProject
    {
        public static void Main(string[] args)
        {
            Slicer slicer = new Slicer();
            Console.WriteLine("/  ___| (_)\n" +
                              "\\ `--.| |_  ___ ___ _ __ \n" +
                              " `--. \\ | |/ __/ _ \\ '__|\n" +
                              "/\\__/ / | | (_|  __/ |\n" +
                              "\\____/|_|_|\\___\\___|_|   ");
            if (args.Length > 0)
            {
                string file = args[0];
                Model3D model = Model3D.CreateFromStl(file);
                slicer.SliceByLayer(model, 0.2f, 0.15f, ExportFormat.GCode);
                Console.WriteLine("Done slicing " + Path.GetFileName(file));
            }
        }
    }
}