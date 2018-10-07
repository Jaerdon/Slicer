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
        /// <summary>
        ///     Slices model
        /// </summary>
        /// <param name="model">Model to be sliced.</param>
        /// <param name="layerHeight">Height of each layer in mm.</param>
        /// <param name="infillPercent">Decimal percent of space between infill lines.</param>
        /// <param name="format">File format for the export</param>
        public void SliceByLayer(Model3D model, float layerHeight, float infillPercent, ExportFormat format)
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

                //Polygon Creation v2
                List<Polygon> layerPolygons = new List<Polygon>();
                List<Segment> currentPolygon = new List<Segment>();
                if (lines.Count > 0)
                {
                    Segment currentSegment = lines[0];
                    lines.Remove(currentSegment);
                    currentPolygon.Add(currentSegment);

                    while (lines.Count > 0)
                        foreach (Segment segment in lines)
                        {
                            if (segment.P.Equals(currentSegment.Q))
                            {
                                lines.Remove(segment);
                                currentPolygon.Add(segment);
                                currentSegment = segment;
                                break;
                            }

                            if (segment.Q.Equals(currentSegment.Q))
                            {
                                lines.Remove(segment);
                                segment.Swap();
                                currentPolygon.Add(segment);
                                currentSegment = segment;
                                break;
                            }

                            if (!segment.Equals(lines.Last()) || currentPolygon.Count <= 0) continue;
                            foreach (Segment line in lines)
                            {
                                if (line.Q.Equals(currentPolygon[0].P) || line.P.Equals(currentPolygon[0].P))
                                {
                                    currentSegment = currentPolygon[0];
                                    currentPolygon.Reverse();
                                    foreach (Segment seg in currentPolygon) seg.Swap();

                                    break;
                                }

                                if (!line.Equals(lines.Last())) continue;
                                if (!currentPolygon[0].Equals(currentPolygon.Last()))
                                    currentPolygon.Add(new Segment(currentPolygon.Last().Q, currentPolygon[0].P));

                                layerPolygons.Add(new Polygon(currentPolygon.ToArray()));
                                currentPolygon.Clear();
                                currentSegment = lines[0];
                                lines.Remove(currentSegment);
                                currentPolygon.Add(currentSegment);
                                break;
                            }

                            break;
                        }

                    if (!currentPolygon[0].Equals(currentPolygon.Last()))
                        currentPolygon.Add(new Segment(currentPolygon.Last().Q, currentPolygon[0].P));

                    layerPolygons.Add(new Polygon(currentPolygon.ToArray()));
                }

                switch (format)
                {
                    case ExportFormat.GCode:
                    {
                        //Enumerate over each polygon in the layer
                        foreach (Polygon polygon in layerPolygons)
                        {
                            toFile.Add("; Polygon");
                            //Outer Walls
                            toFile.Add("; Walls");
                            //toFile.Add(string.Format("G0 {0} Z{1}", polygon.Centroid, layerZ));
                            toFile.Add($"G0 {polygon.Sides[0].P}");
                            foreach (Segment line in polygon.Sides)
                                toFile.Add($"G1 {line.Q} E{line.GetLength()}");

                            //Infill
                            //X-Axis Infill (Technically infill lines are Y-Axis)
                            toFile.Add("; Infill Across Y");
                            for (float x = minX; x < maxX; x += infillDiff)
                                foreach (Segment segA in polygon.Sides)
                                {
                                    if (!segA.IntersectsX(x)) continue;
                                    foreach (Segment segB in polygon.Sides)
                                    {
                                        if (segB.Equals(segA) || !segB.IntersectsX(x)) continue;
                                        Point2D ptA = segA.FindYIntersect(x);
                                        Point2D ptB = segB.FindYIntersect(x);
                                        Segment line = new Segment(ptA, ptB);
                                        toFile.Add($"G0 {ptA}");
                                        toFile.Add($"G1 {ptB} E{line.GetLength()}");
                                    }
                                }

                            //Y-Axis Infill (Technically infill lines are X-Axis)
                            toFile.Add("; Infill Across X");
                            for (float y = minY; y < maxY; y += infillDiff)
                                foreach (Segment segA in polygon.Sides)
                                {
                                    if (!segA.IntersectsY(y)) continue;
                                    foreach (Segment segB in polygon.Sides)
                                    {
                                        if (segB.Equals(segA) || !segB.IntersectsY(y)) continue;
                                        Point2D ptA = segA.FindXIntersect(y);
                                        Point2D ptB = segB.FindXIntersect(y);
                                        Segment line = new Segment(ptA, ptB);
                                        toFile.Add($"G0 {ptA}");
                                        toFile.Add($"G1 {ptB} E{line.GetLength()}");
                                    }
                                }
                        }

                        break;
                    }
                    case ExportFormat.SVG:
                    {
                        if (!Directory.Exists(model.GetName())) Directory.CreateDirectory(model.GetName());
                        SvgFile layerFile = new SvgFile(layerPolygons);
                        layerFile.WriteToFile(Path.Combine(model.GetName(), model.GetName() + layer));
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(null, format, null);
                }
            }

            if (format == ExportFormat.GCode) File.WriteAllLines(model.GetName() + ".gcode", toFile);
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