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
            if (args.Length > 0)
            {
                if (args[0].Equals("-h") || args[0].Equals("--help")) { DisplayHelpMessage(); return; }

                bool swap = false;
                
                string file = args[0];

                float translateX = 0.0f;
                float translateY = 0.0f;
                float translateZ = 0.0f;

                float rotateX = 0.0f;
                float rotateY = 0.0f;
                float rotateZ = 0.0f;

                float layerHeight = 0.2f;
                float infill = 0.15f;

                ExportFormat format = ExportFormat.GCode;

                //Parse Arguments
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].Equals("-s") || args[i].Equals("--swap")) 
                    { 
                        swap = true;
                        continue;
                    }
                    if (args.Length > i + 1)
                    {
                        if (args[i].Equals("-tX") || args[i].Equals("--translateX"))
                            translateX = float.Parse(args[i + 1]);
                        else if (args[i].Equals("-tY") || args[i].Equals("--translateY"))
                            translateY = float.Parse(args[i + 1]);
                        else if (args[i].Equals("-tZ") || args[i].Equals("--translateZ"))
                            translateZ = float.Parse(args[i + 1]);
                        else if (args[i].Equals("-rX") || args[i].Equals("--rotateX"))
                            rotateX = float.Parse(args[i + 1]);
                        else if (args[i].Equals("-rY") || args[i].Equals("--rotateY"))
                            rotateY = float.Parse(args[i + 1]);
                        else if (args[i].Equals("-rZ") || args[i].Equals("--rotateZ"))
                            rotateZ = float.Parse(args[i + 1]);
                        else if (args[i].Equals("-l") || args[i].Equals("--layerHeight"))
                            layerHeight = float.Parse(args[i + 1]);
                        else if (args[i].Equals("-i") || args[i].Equals("--infill"))
                            infill = float.Parse(args[i + 1]);
                        else if (args[i].Equals("-e") || args[i].Equals("--export"))
                        {
                            if (args[i + 1].ToLower().Equals("gcode")) 
                                format = ExportFormat.GCode;
                            else if (args[i + 1].ToLower().Equals("svg") || args[i + 1].ToLower().Equals("vector"))
                                format = ExportFormat.SVG;
                            else
                                DisplayErrorMessage();
                        }
                    }
                }
                
                Console.WriteLine("/  ___| (_)\n" +
                                  "\\ `--.| |_  ___ ___ _ __ \n" +
                                  " `--. \\ | |/ __/ _ \\ '__|\n" +
                                  "/\\__/ / | | (_|  __/ |\n" +
                                  "\\____/|_|_|\\___\\___|_|   ");

                Model3D model = Model3D.CreateFromStl(file);
                if (swap)
                {
                    model.Rotate(rotateX, rotateY, rotateZ);
                    model.Translate(translateX, translateY, translateZ);
                }
                else
                {
                    model.Translate(translateX, translateY, translateZ);
                    model.Rotate(rotateX, rotateY, rotateZ);
                }
                slicer.SliceByLayer(model, layerHeight, infill, format);
                Console.WriteLine("Done slicing " + Path.GetFileName(file));
            }

            else DisplayErrorMessage();
        }

        private static void DisplayHelpMessage()
        {
            Console.WriteLine("Usage: slice [OPTION]... [FILE]...\n" +
                              "Slices CAD FILE into GCODE FILE.\n\n" +
                              "-tX [n], --translateX [n]      translate x component by some amount n\n" +
                              "-tY [n], --translateY [n]      translate y component by some amount n\n" +
                              "-tZ [n], --translateZ [n]      translate z component by some amount n\n" +
                              "-rX [n], --rotateX [n]         rotate model around x axis by some amount n\n" +
                              "-rY [n], --rotateY [n]         rotate model around y axis by some amount n\n" +
                              "-rZ [n], --rotateZ [n]         rotate model around z axis by some amount n\n" +
                            "\n-s, --swap                       swap rotation and transformation order, default is transform first\n" +
                              "-l [n], --layerHeight [n]        set layerheight to n, default is 0.2mm\n" +
                              "-i [n], --infill [n]             set infill percentage to n, default is 15% (0.15)\n" +
                              "-f [format], --format [format]   change export format type (ex. GCode, SVG)\n" +
                              "\n-h, --help display this help\n");
        }

        private static void DisplayErrorMessage()
        {
            Console.WriteLine("Improper arguments.\nTry 'slice --help' for more information.");
        }
    }
}