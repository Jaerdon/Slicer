using System.Collections.Generic;
using System.IO;
using Slicer.Geometry;

namespace Slicer.Formats
{
    /// <summary>
    ///     File Format Type
    /// </summary>
    public enum ExportFormat
    {
        GCode = 0,
        SVG = 1
    }

    /// <summary>
    ///     GCode File Format
    /// </summary>
    public class GcodeFile
    {
        private const char ToolPrefix = 'T';
        private const string SetTempCode = "M104 S";
        private const string SetTempWaitCode = "M109 S";
        private const string AbsoluteCode = "M82";
        private const string HomeAxesCode = "G28";
    }

    /// <summary>
    ///     SVG File Format
    /// </summary>
    public class SvgFile
    {
        private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
        private const string SvgHeader = "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\">";
        private const string SvgFooter = "</svg>";
        private const string LineHeader = "<polyline points=\"";
        private const string LineFooter = "\" stroke=\"red\" stroke-width=\"1\" fill=\"none\"/>";

        private readonly List<Polyline> _polylines;

        /// <summary>
        ///     SVG File Format
        /// </summary>
        /// <param name="polylines">Polyline List</param>
        public SvgFile(List<Polyline> polylines)
        {
            _polylines = polylines;
        }

        /// <summary>
        ///     Write slices to a .svg file
        /// </summary>
        /// <param name="filePath"></param>
        public void WriteToFile(string filePath)
        {
            List<string> lines = new List<string>();
            lines.Add(XmlHeader);
            lines.Add(SvgHeader);
            foreach (Polyline polyline in _polylines)
            {
                string lineString = LineHeader;
                foreach (Segment line in polyline.Sides) lineString += line.P.ToPair() + " ";
                lineString += polyline.Sides[polyline.Sides.Length - 1].P.ToPair();
                lines.Add(lineString + LineFooter);
            }

            lines.Add(SvgFooter);

            File.WriteAllLines(filePath, lines);
        }
    }
}