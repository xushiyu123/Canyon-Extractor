using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;

namespace CanyonExtractor.Data
{
    /// <summary>
    /// point
    /// </summary>
    public class Point
    {
        double x;
        double y;
        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
    }
    /// <summary>
    /// 河段
    /// </summary>
    public class Water
    {
        Point InVertex;
        Point OutVertex;
        int indgree;
        public Point InVertex1 { get => InVertex; set => InVertex = value; }
        public Point OutVertex1 { get => OutVertex; set => OutVertex = value; }
        public int Indgree { get => indgree; set => indgree = value; }
    }
    /// <summary>
    /// couple of borderlines
    /// </summary>
    public struct BufferLines
    {
        private IPointCollection rightIpc;//right borderline
        private IPointCollection leftIpc;//left borderline
        public IPointCollection RightIpc { get => rightIpc; set => rightIpc = value; }
        public IPointCollection LeftIpc { get => leftIpc; set => leftIpc = value; }
    }
    /// <summary>
    /// topologic curve
    /// </summary>
    public class ProfileLine
    {
        private List<double> x;//list of x
        private List<double> z;//list of elevations

        public ProfileLine()
        {
            x = new List<double>();
            z = new List<double>();
        }

        public List<double> X { get => x; set => x = value; }
        public List<double> Z { get => z; set => z = value; }
    }
    /// <summary>
    /// peak of topologic curve
    /// </summary>
    public class CurvePeak
    {
        private double amplitude;//
        private double peakx;//index of peak
        private double endx;//index of end
        private double startx;//index of start
        private double centerx;//index of center
        private double length;//length of peak
        private double x;//x coordinate of peak
        private double y;//y coordinate of peak
        public double Amplitude { get => amplitude; set => amplitude = value; }
        public double Peakx { get => peakx; set => peakx = value; }
        public double Startx { get => startx; set => startx = value; }
        public double Endx { get => endx; set => endx = value; }
        public double Centerx { get => centerx; set => centerx = value; }
        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
        public double Length { get => length; set => length = value; }
    }
    /// <summary>
    /// peak pair
    /// </summary>
    public class PeakPair
    {
        private CurvePeak leftPeak;
        private CurvePeak rightPeak;
        public CurvePeak LeftPeak { get => leftPeak; set => leftPeak = value; }
        public CurvePeak RightPeak { get => rightPeak; set => rightPeak = value; }
    }
    /// <summary>
    /// canyon
    /// </summary>
    public class Canyon
    {
        private double x0;
        private double y0;
        private int waterid;
        private double width;
        private double depth;
        private double length;
        private double dwRatio;
        private double area;
        private IPointCollection canyonCollection;
        public double Width { get => width; set => width = value; }
        public double Depth { get => depth; set => depth = value; }
        public double Length { get => length; set => length = value; }
        public double DwRatio { get => dwRatio; set => dwRatio = value; }
        public IPointCollection CanyonCollection { get => canyonCollection; set => canyonCollection = value; }
        public int Waterid { get => waterid; set => waterid = value; }
        public double Area { get => area; set => area = value; }
        public double X0 { get => x0; set => x0 = value; }
        public double Y0 { get => y0; set => y0 = value; }
    }
}