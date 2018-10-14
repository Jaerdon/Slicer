using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Slicer.Formats;
using Slicer.Geometry;
using Slicer.models;

namespace Slicer
{
    /// <summary>
    ///     Slicer
    /// </summary>
    public class Slicer
    {
        private const float FilamentArea = 0.765625f * (float) Math.PI;
        private const float FilamentDensity = 1.25f;
        
        /// <summary>
        ///     Slices model
        /// </summary>
        /// <param name="model">Model to be sliced.</param>
        /// <param name="layerHeight">Height of each layer in mm.</param>
        /// <param name="infillPercent">Decimal percent of space between infill lines.</param>
        /// <param name="format">File format for the export</param>
        /// <param name="hotEndTemp">Temperature of the hot-end</param>
        /// <param name="bedTemp">Temperature of the bed (if any)</param>
        public void SliceByLayer(Model3D model, float layerHeight, float infillPercent, ExportFormat format, float hotEndTemp, float bedTemp)
        {
            List<string> toFile = new List<string>();
            float height = 0.0f;
            float floor = 0.0f;

            float minX = 0.0f;
            float maxX = 0.0f;
            float minY = 0.0f;
            float maxY = 0.0f;

            //Calculate max height of model
            foreach (Model3D.Facet facet in model.getFacets())
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
                    if (z < floor) floor = z;
                }

            float infillDiff = 10 * infillPercent;

            long layerCount = (long) (height / layerHeight);

            float filamentUsed = 0.0f;

            List<List<Polyline>> layers = new List<List<Polyline>>();

            //Slice individual layers
            for (int layer = 0; layer < layerCount; layer++)
            {
                float layerZ = layerHeight * layer + floor;
                List<Segment> lines = new List<Segment>();

                toFile.Add($"G0 Z{layerZ}; Layer {layer}");

                foreach (Model3D.Facet facet in model.getFacets())
                {
                    List<Point2D> points = new List<Point2D>();

                    //Check to see which pairs of vertices intersect the given Z-plane
                    for (int a = 1; a < 4; a++)
                    {
                        int b = a == 3 ? 1 : a + 1;
                        if (CheckVertices(facet.GetVertices()[a], facet.GetVertices()[b], layerZ))
                            points.Add(GetIntersect(facet.GetVertices()[a], facet.GetVertices()[b], layerZ));
                        else if (CheckVertices(facet.GetVertices()[b], facet.GetVertices()[a], layerZ))
                            points.Add(GetIntersect(facet.GetVertices()[b], facet.GetVertices()[a], layerZ));
                    }

                    if (points.Count > 1)
                        lines.Add(new Segment(points[0], points[1], facet.GetVertices()[0].To2DPoint()));
                }

                //Polyline Creation v2
                List<Polyline> layerPolylines = new List<Polyline>();
                List<Segment> currentPolyline = new List<Segment>();
                if (lines.Count > 0)
                {
                    Segment currentSegment = lines[0];
                    lines.Remove(currentSegment);
                    currentPolyline.Add(currentSegment);

                    while (lines.Count > 0)
                        foreach (Segment segment in lines)
                        {
                            if (segment.P.Equals(currentSegment.Q))
                            {
                                lines.Remove(segment);
                                currentPolyline.Add(segment);
                                currentSegment = segment;
                                break;
                            }

                            if (segment.Q.Equals(currentSegment.Q))
                            {
                                lines.Remove(segment);
                                segment.Swap();
                                currentPolyline.Add(segment);
                                currentSegment = segment;
                                break;
                            }

                            if (!segment.Equals(lines.Last()) || currentPolyline.Count <= 0) continue;
                            foreach (Segment line in lines)
                            {
                                if (line.Q.Equals(currentPolyline[0].P) || line.P.Equals(currentPolyline[0].P))
                                {
                                    currentSegment = currentPolyline[0];
                                    currentPolyline.Reverse();
                                    foreach (Segment seg in currentPolyline) seg.Swap();

                                    break;
                                }

                                if (!line.Equals(lines.Last())) continue;
                                if (!currentPolyline[0].Equals(currentPolyline.Last()))
                                    currentPolyline.Add(new Segment(currentPolyline.Last().Q, currentPolyline[0].P));

                                layerPolylines.Add(new Polyline(currentPolyline.ToArray()));
                                currentPolyline.Clear();
                                currentSegment = lines[0];
                                lines.Remove(currentSegment);
                                currentPolyline.Add(currentSegment);
                                break;
                            }

                            break;
                        }

                    if (!currentPolyline[0].Equals(currentPolyline.Last()))
                        currentPolyline.Add(new Segment(currentPolyline.Last().Q, currentPolyline[0].P));

                    layerPolylines.Add(new Polyline(currentPolyline.ToArray()));
                }
                layers.Add(layerPolylines);
            }
            
            Console.WriteLine($"Done slicing {model.GetName()}");
            
            //Export to file
            switch (format)
            {
                case ExportFormat.GCode:
                {
                    toFile.Add(GcodeFile.AddHeader(0, hotEndTemp, bedTemp));
                    foreach (List<Polyline> layer in layers)
                    {
                        //Enumerate over each polyline in the layer
                        foreach (Polyline polyline in layer)
                        {
                            toFile.Add("; Polyline");
                            //Outer Walls
                            toFile.Add("; Walls");
                            toFile.Add($"G0 {polyline.Sides[0].P}");
                            foreach (Segment line in polyline.Sides)
                            {
                                toFile.Add($"G1 {line.Q} E{line.GetLength()}");
                                filamentUsed += line.GetLength();
                            }

                            //Infill
                            //X-Axis Infill (Technically infill lines are Y-Axis)
                            toFile.Add("; Infill Across Y");
                            for (float x = minX; x < maxX; x += infillDiff)
                                foreach (Segment segA in polyline.Sides)
                                {
                                    if (!segA.IntersectsX(x)) continue;
                                    foreach (Segment segB in polyline.Sides)
                                    {
                                        if (segB.Equals(segA) || !segB.IntersectsX(x)) continue;
                                        Point2D ptA = segA.FindYIntersect(x);
                                        Point2D ptB = segB.FindYIntersect(x);
                                        Segment line = new Segment(ptA, ptB);
                                        toFile.Add($"G0 {ptA}");
                                        toFile.Add($"G1 {ptB} E{line.GetLength()}");
                                        filamentUsed += line.GetLength();
                                    }
                                }

                            //Y-Axis Infill (Technically infill lines are X-Axis)
                            toFile.Add("; Infill Across X");
                            for (float y = minY; y < maxY; y += infillDiff)
                                foreach (Segment segA in polyline.Sides)
                                {
                                    if (!segA.IntersectsY(y)) continue;
                                    foreach (Segment segB in polyline.Sides)
                                    {
                                        if (segB.Equals(segA) || !segB.IntersectsY(y)) continue;
                                        Point2D ptA = segA.FindXIntersect(y);
                                        Point2D ptB = segB.FindXIntersect(y);
                                        Segment line = new Segment(ptA, ptB);
                                        toFile.Add($"G0 {ptA}");
                                        toFile.Add($"G1 {ptB} E{line.GetLength()}");
                                        filamentUsed += line.GetLength();
                                    }
                                }
                        }
                    }
                    File.WriteAllLines($"{model.GetName()}.gcode", toFile);
                    Console.WriteLine($"Exported to {model.GetName()}.gcode");
                    Console.WriteLine("Model Info:");
                    filamentUsed /= 1000;
                    Console.WriteLine($"  Filament length: {filamentUsed} meters");
                    Console.WriteLine($"  Filament weight: {filamentUsed * FilamentArea * FilamentDensity} grams");
                    break;
                }
                case ExportFormat.SVG:
                {
                    if (!Directory.Exists(model.GetName())) Directory.CreateDirectory(model.GetName());
                    string modifiers = $" viewBox=\"{minX} {minY} {maxX - minX} {maxY - minY}\"";
                    for (int i = 0; i < layers.Count; i++)
                    {
                        List<Polyline> layer = layers[i];
                        SvgFile layerFile = new SvgFile(layer);
                        layerFile.WriteToFile(Path.Combine(model.GetName(), model.GetName() + i + ".svg"), modifiers);
                    }
                    Console.WriteLine($"Exported to /{model.GetName()}/");

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(null, format, null);
            }
        }

        private static Point2D GetIntersect(Point3D pointA, Point3D pointB, float z)
        {
            float t = (z - pointB.Z) / (pointA.Z - pointB.Z); //Scalar variable 't'

            float x = pointB.X + t * (pointA.X - pointB.X); //Point of X intersection
            float y = pointB.Y + t * (pointA.Y - pointB.Y); //Point of Y intersection

            return new Point2D(x, y);
        }

        private static bool CheckVertices(Point3D a, Point3D b, float height)
        {
            return a.Z > height && b.Z < height;
        }
    }
}