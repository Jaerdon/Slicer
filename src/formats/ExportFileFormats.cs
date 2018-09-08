using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Slicer.Formats
{
    public class SvgFile
    {
        private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
        private const string SvgHeader = "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\">";
        private const string SvgFooter = "</svg>";
        private const string LineHeader = "<polyline points=\"";
        private const string LineFooter = "\" stroke=\"red\" stroke-width=\"1\" fill=\"none\"/>";

        private List<Polygon> polylines;

        public SvgFile(List<Polygon> polylines)
        {
            this.polylines = polylines;
        }

        public void WriteToFile(string filePath)
        {
            List<string> lines = new List<string>();
            lines.Add(XmlHeader);
            lines.Add(SvgHeader);
            foreach (Polygon polyline in polylines)
            {
                string lineString = LineHeader;
                foreach (var line in polyline.Sides)
                {
                    lineString += line.P.ToPair();
                }
                lineString += polyline.Sides[polyline.Sides.Length - 1].P.ToPair(); 
                lines.Add(lineString + LineFooter);
            }
            lines.Add(SvgFooter);

            File.WriteAllLines(filePath, lines);
        }
    }

    public class GcodeFile
    {
        
    }
}