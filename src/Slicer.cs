using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Slicer.models;

namespace Slicer
{
    public class Slicer
    {
        public static void Main(string[] args)
        {
            var slicer = new Slicer();
            Console.WriteLine("/  ___| (_)\n" +
                              "\\ `--.| |_  ___ ___ _ __ \n" +
                              "`--. \\ | |/ __/ _ \\ '__|\n" +
                              "/\\__/ / | | (_|  __/ |\n" +
                              "\\____/|_|_|\\___\\___|_|   ");
            if (args.Length > 0)
            {
                slicer.Slice(args[0]);
            }
        }

        private void Slice(string file)
        {
            Model3D model = Model3D.CreateFromStl(file);
            SliceByLayer(model, 1.5f, 10);
            Console.WriteLine("Done slicing " + Path.GetFileName(file));
        }

        /// <summary>
        /// Slices model
        /// </summary>
        /// <param name="model">Model to be sliced.</param>
        /// <param name="layerHeight">Height of each layer in mm.</param>
        /// <param name="infillPercent">Decimal percent of space between infill lines.</param>
        private static void SliceByLayer(Model3D model, float layerHeight, int infillPercent)
        {
            List<string> toFile = new List<string>();
            float height = 0.0f;

            float minX = 0.0f;
            float maxX = 0.0f;
            float minY = 0.0f;
            float maxY = 0.0f;

            //Calculate max height of model
            foreach (Model3D.Facet facet in model.getFacets())
            {
                for (int i = 1; i < 4; i++)
                {
                    float x = facet.GetVertices()[i].X;
                    float y = facet.GetVertices()[i].Y;
                    float z = facet.GetVertices()[i].Z;
                    
                    if (x > maxX) maxX = x;
                    if (x < minX) minX = x;
                    if (y > maxY) maxY = y;
                    if (y < minY) minY = y;
                    
                    if (z > height) height = z;
                }
            }
            
            float xInfill = (maxX - minX) / infillPercent;
            float yInfill = 0.2f;

            //Initialize GCode
            toFile.Add("T0"); //Tool 0
            toFile.Add("M104 S200"); //Set target temperature in Celsius
            toFile.Add("M109 S200"); //Set target temperature and wait.
            toFile.Add("M82"); //Absolute extrusion mode.
            toFile.Add("G28"); //Home all axes

            //Slice individual layers
            long layerCount = (long) (height / layerHeight);
            for (int layer = 0; layer < layerCount; layer++)
            {
                float layerZ = layerHeight * layer;
                List<Line> lines = new List<Line>();

                toFile.Add(string.Format("G0 Z{0}", layerZ));

                foreach (Model3D.Facet facet in model.getFacets())
                {
                    List<Point2D> points = new List<Point2D>();

                    //Check to see which pairs of vertices intersect the given Z-plane
                    for (int a = 1; a < 4; a++)
                    {
                        int b = a == 3 ? 1 : a + 1;
                        if (CheckVertices(facet.GetVertices()[a], facet.GetVertices()[b], layerZ))
                        {
                            points.Add(GetIntersect(facet.GetVertices()[a], facet.GetVertices()[b], layerZ));
                        }
                        else if (CheckVertices(facet.GetVertices()[b], facet.GetVertices()[a], layerZ))
                        {
                            points.Add(GetIntersect(facet.GetVertices()[b], facet.GetVertices()[a], layerZ));
                        }
                    }

                    if (points.Count > 1)
                    {
                        lines.Add(new Line(points[0], points[1], facet.GetVertices()[0].To2DPoint()));
                        /*toFile.Add(string.Format("G0 {0}", points[0]));
                        for (int i = 1; i < points.Count; i++)
                        {
                            toFile.Add(string.Format("G1 {0}", points[i]));
                        }*/
                    }
                }

                //Reorganize lines so that ones with connecting points connect
                List<Polygon> layerShapes = new List<Polygon>();
                
                int count = lines.Count;
                if (lines.Count > 0)
                {
                    List<Line> shape = new List<Line>();
                    Line currentLine = lines[0];
                    int linesUsed = 1;
                    shape.Add(currentLine);
                    lines.Remove(lines[0]);
                    while (linesUsed < count)
                    {
                        //Attempt to create one long continuous shape
                        for (int i = 1; i < lines.Count; i++)
                        {
                            if (lines[i].P.Equals(currentLine.Q))
                            {
                                currentLine = lines[i];
                                shape.Add(currentLine);
                                ++linesUsed;
                                lines.Remove(lines[i]);
                            }
                            else if (lines[i].Q.Equals(currentLine.Q))
                            {
                                lines[i].Swap();
                                currentLine = lines[i];
                                shape.Add(currentLine);
                                ++linesUsed;
                                lines.Remove(lines[i]);
                            }
                        }

                        //Add shape to list
                        shape.Add(shape[0]);
                        layerShapes.Add(new Polygon(shape.ToArray()));
                        shape.Clear();

                        //Could not connect with any other lines, create a new shape.
                        currentLine = lines[0];
                        shape.Add(currentLine);
                        lines.Remove(lines[0]);
                        ++linesUsed;
                    }

                    //Add final shape to list
                    shape.Add(shape[0]);
                    layerShapes.Add(new Polygon(shape.ToArray()));
                }

                //Enumerate over each polygon in the layer
                foreach (Polygon polygon in layerShapes)
                {
                    //Outer Walls
                    //toFile.Add(string.Format("G0 {0} Z{1}", polygon.Centroid, layerZ));
                    foreach (Line line in polygon.Sides)
                    {
                        toFile.Add(string.Format("G0 {0}", line.P));
                        toFile.Add(string.Format("G1 {0} E{1}", line.Q, line.GetLength()));
                    }
                    
                    //Infill
                    Console.WriteLine(polygon + "ooof" + layerZ);
                    //Y-Axis infill
                    for (float y = minY; y < maxY; y += yInfill)
                    {
                        foreach (Line line in polygon.Sides)
                        {
                            if (line.IntersectsY(y))
                            {
                                //Console.WriteLine(y);
                                foreach (Line line2 in polygon.Sides)
                                {
                                    if (line2 != line && line2.IntersectsY(y))
                                    {
                                        toFile.Add(string.Format("G0 {0}", line.GetIntersection(
                                            new Line(
                                                new Point2D(minX, y), 
                                                new Point2D(maxX, y)
                                            ))));
                                        
                                        toFile.Add(string.Format("G1 {0}; {1} {2} {3}", line2.GetIntersection(
                                            new Line(
                                                new Point2D(minX, y), 
                                                new Point2D(maxX, y)
                                            )),
                                            polygon.Sides.Length,
                                            line,
                                            line2
                                        ));        
                                    }
                                }
                            }
                        }
                    
                        //Y-Axis infill
                        float yVal = minY;
                    }
                }
            }

            File.WriteAllLines(model.GetName() + ".gcode", toFile);
        }

        private static Point2D GetIntersect(Point3D a, Point3D b, float z)
        {
            float t = (z - b.Z) / (a.Z - b.Z); //Scalar variable 't'

            float x = b.X + t * (a.X - b.X); //Point of X intersection
            float y = b.Y + t * (a.Y - b.Y); //Point of Y intersection

            return new Point2D(x, y);
        }

        private static bool CheckVertices(Point3D a, Point3D b, float height)
        {
            return a.Z > height && b.Z < height;
        }
    }
}