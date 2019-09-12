using System;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using CanyonExtractor.Data;

namespace CanyonExtractor.Controllers
{
    /// <summary>
    /// generate river buffer
    /// </summary>
    class BufferlineClass
    {
        FeatureDispose fd = new FeatureDispose();
        InputData id = new InputData();
        DataTrans dt = new DataTrans();
        /// <summary>
        /// generate borderlines of buffer
        /// </summary>
        /// <param name="waterlineFile">river file</param>
        /// <param name="outfolder">ouput folder</param>
        /// <param name="radius">radius of buffer</param>
        public void BufferlineAnalyse(string waterlineFile,string outfolder, double radius)
        {
            id.DeleteFiles(outfolder);
            IFeatureClass featureClass = id.InputShp(waterlineFile);//read river file
            for (int i = 0; i < featureClass.FeatureCount(null); i++)//traverse the river features in file 
            {
                IPolyline waterline = dt.FeaturetoPolyline(featureClass, i);//attain a river feature and get its geometry information
                IPointCollection pointCollection = waterline as IPointCollection; 
                GetBufferEdgeCoords(outfolder, pointCollection, radius, i);
            }
        }
        /// <summary>
        /// generate the coordinate list of the river buffer in counterclockwise direction
        /// </summary>
        /// <param name="strPolyLineCoords">point list of river feature</param>
        /// <param name="radius">buffer radius</param>
        /// <returns>coordinate list</returns>
        public void GetBufferEdgeCoords(string outfolder, IPointCollection polyline, double radius, int i)
        {
            if (polyline.PointCount > 0)
            {
                //generate the coordinate lists of buffer borderline
                IPointCollection lpc = GetLeftBufferEdgeCoords(polyline, radius);
                IPointCollection polyliner = Reverse(polyline);
                IPointCollection rpc = GetLeftBufferEdgeCoords(polyliner, radius);
                IPointCollection rpcr = Reverse(rpc);
                fd.ShpFromPolyline(outfolder, "left" + i.ToString(), lpc);
                fd.ShpFromPolyline(outfolder, "right" + i.ToString(), rpcr);
            }
        }
        /// <summary>
        /// reverse a point list
        /// </summary>
        /// <param name="ipc"></param>
        /// <returns></returns>
        public IPointCollection Reverse(IPointCollection ipc)
        {
            IPointCollection ipc0 = new Polyline();
            for (int i = ipc.PointCount - 1; i >= 0; i--)
            {
                ipc0.AddPoint(ipc.Point[i]);
            }
            return ipc0;
        }
        /// <summary>
        /// calculate Azimuth
        /// </summary>
        /// <param name="preCoord">vector start point</param>
        /// <param name="nextCoord">end point</param>
        /// <returns>返回弧度角</returns>
        public double GetQuadrantAngle(IPoint preCoord, IPoint nextCoord)
        {
            return GetQuadrantAngle(nextCoord.X - preCoord.X, nextCoord.Y - preCoord.Y);
        }
        /// <summary>
        /// calculate the angle of new vector and original vector 
        /// </summary>
        /// <param name="x">change of X (new vector and original vector)</param>
        /// <param name="y">change of Y (new vector and original vector)</param>
        /// <returns>angle</returns>
        public double GetQuadrantAngle(double x, double y)
        {
            double theta = Math.Atan(y / x);
            if (x > 0 && y < 0) return Math.PI * 2 + theta;
            if (x < 0) return theta + Math.PI;
            return theta;
        }
        /// <summary>
        /// calculate the angle of two vectors, which is formed by three adjcent coordinate
        /// </summary>
        /// <param name="preCoord">first coordinate</param>
        /// <param name="midCoord">middle coordinate</param>
        /// <param name="nextCoord">last coordinate</param>
        /// <returns></returns>
        public double GetIncludedAngle(IPoint preCoord, IPoint midCoord, IPoint nextCoord)
        {
            double innerProduct = (midCoord.X - preCoord.X) * (nextCoord.X - midCoord.X) + (midCoord.Y - preCoord.Y) * (nextCoord.Y - midCoord.Y);
            double mode1 = Math.Sqrt(Math.Pow((midCoord.X - preCoord.X), 2.0) + Math.Pow((midCoord.Y - preCoord.Y), 2.0));
            double mode2 = Math.Sqrt(Math.Pow((nextCoord.X - midCoord.X), 2.0) + Math.Pow((nextCoord.Y - midCoord.Y), 2.0));
            double acos = innerProduct / (mode1 * mode2);
            if (acos > 1) acos = 1;
            if (acos < -1) acos = -1;
            return Math.Acos(acos);
        }

        /// <summary>
        /// generate the left borderline based on the right borderline
        /// </summary>
        /// <param name="coords">coordinate list of right borderline</param>
        /// <param name="radius">radius</param>
        /// <returns>coordinate list of left borderline</returns>
        public IPointCollection GetLeftBufferEdgeCoords(IPointCollection coords, double radius)
        {
            IPointCollection polyline = new PolylineClass();
            IPoint point = new PointClass();
            //main variables
            double alpha = 0.0;//angle based on horizontal axis
            double delta = 0.0;//The angle between the vectors formed by the front and back line segments
            double l = 0.0;//Cross product of vectors formed by front and back line segments
            //Auxiliary variables
            double startRadian = 0.0;
            double endRadian = 0.0;
            double beta = 0.0;
            double x = 0.0, y = 0.0;
            //middle coordinates
            for (int i = 1; i < coords.PointCount - 1; i++)
            {
                alpha = GetQuadrantAngle(coords.Point[i], coords.Point[i + 1]);
                delta = GetIncludedAngle(coords.Point[i - 1], coords.Point[i], coords.Point[i + 1]);
                l = GetVectorProduct(coords.Point[i - 1], coords.Point[i], coords.Point[i + 1]);
                if (l > 0)
                {
                    startRadian = alpha + (3 * Math.PI) / 2 - delta;
                    endRadian = alpha + (3 * Math.PI) / 2;
                    IPointCollection ipc1 = GetBufferCoordsByRadian(coords.Point[i], startRadian, endRadian, radius);
                    for (int j = 0; j < ipc1.PointCount; j++)
                    {
                        if (ipc1.Point[j].X > 0)
                        { }
                        Alter(ref polyline, ipc1.Point[j]);
                        polyline.AddPoint(ipc1.Point[j]);
                    }
                }
                else if (l < 0)
                {
                    beta = alpha - (Math.PI - delta) / 2;
                    x = Math.Round(coords.Point[i].X + radius * Math.Cos(beta),2);
                    y = Math.Round(coords.Point[i].Y + radius * Math.Sin(beta),2);
                    IPoint ipoint = new PointClass();
                    ipoint.PutCoords(x, y);
                    Alter(ref polyline, ipoint);
                    polyline.AddPoint(ipoint);
                }
            }
            return polyline;
        }
        /// <summary>
        /// Determine whether the newly added point will be the boundary generated and intersected
        /// </summary>
        /// <param name="ipc"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Alter(ref IPointCollection ipc, IPoint point)
        {
            int count = ipc.PointCount;
            if (count >= 3)
            {
                bool flag = false;
                int index = count - 1;
                for (int i = index - 1; i >= 1; i--)
                {

                    if (InsectionJudge(ipc.Point[index], point, ipc.Point[i], ipc.Point[i - 1]))
                    {
                        flag = true;
                        index = i;
                        break;
                    }
                }
                if (flag)
                {
                    IPoint insectp = Inter(ipc.Point[count - 1], point, ipc.Point[index], ipc.Point[index - 1]);
                    ipc.RemovePoints(index, count - index);
                    ipc.AddPoint(insectp);
                }
            }
            return true;
        }

        /// <summary>
        /// Determine if the two lines intersect
        /// </summary>
        /// <param name="a">start point of line 1</param>
        /// <param name="b"></param>
        /// <param name="c">start point of line 2</param>
        /// <param name="d"></param>
        /// <returns>true or false</returns>
        public bool InsectionJudge(IPoint a, IPoint b, IPoint c, IPoint d)
        {
            /*
           Quick rejection:
             The two line segments are rectangles consisting of diagonal lines. 
             If the two rectangles have no overlapping parts, then the two line segments are unlikely to overlap.
            */
            if (!(Math.Min(a.X, b.X) <= Math.Max(c.X, d.X) &&
                Math.Min(c.Y, d.Y) <= Math.Max(a.Y, b.Y) &&
                Math.Min(c.X, d.X) <= Math.Max(a.X, b.X) &&
                Math.Min(a.Y, b.Y) <= Math.Max(c.Y, d.Y)))//This step is to determine if the two rectangles intersect.
            {
                return false;
            }
            /*
            Straight experiment:
             If the two line segments intersect, they must be straddling, that is, one line segment is used as the standard, 
                and the other end points of the other line segment must be in the two segments of the line segment.
             That is to say, a b two points on both ends of the line segment cd, c d two points on both ends of the line segment ab
            */
            double u, v, w, z;//Record two vectors separately
            u = (c.X - a.X) * (b.Y - a.Y) - (b.X - a.X) * (c.Y - a.Y);
            v = (d.X - a.X) * (b.Y - a.Y) - (b.X - a.X) * (d.Y - a.Y);
            w = (a.X - c.X) * (d.Y - c.Y) - (d.X - c.X) * (a.Y - c.Y);
            z = (b.X - c.X) * (d.Y - c.Y) - (d.X - c.X) * (b.Y - c.Y);
            return (u * v <= 0.00000001 && w * z <= 0.00000001);
        }

        /// <summary>
        /// Calculating intersection
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        public IPoint Inter(IPoint p1, IPoint p2, IPoint p3, IPoint p4)
        {
            IPoint point = new PointClass();
            double s1 = fArea(p1, p2, p3), s2 = fArea(p1, p2, p4);
            point.PutCoords(Math.Round((p4.X * s1 + p3.X * s2) / (s1 + s2),2), 
                Math.Round((p4.Y * s1 + p3.Y * s2) / (s1 + s2),2));
            return point;
        }

        public double Cross(IPoint p1, IPoint p2, IPoint p3, IPoint p4)
        {
            return (p2.X - p1.X) * (p4.Y - p3.Y) - (p2.Y - p1.Y) * (p4.X - p3.X);
        }

        public double Area(IPoint p1, IPoint p2, IPoint p3)
        {
            return Cross(p1, p2, p1, p3);
        }

        public double fArea(IPoint p1, IPoint p2, IPoint p3)
        {
            return Math.Abs(Area(p1, p2, p3));
        }
        /// <summary>
        /// Gets the buffer arc which fits the boundary point between the specified radians
        /// </summary>
        /// <param name="center">Specify the origin of the fitted arc</param>
        /// <param name="startRadian">Starting radians</param>
        /// <param name="endRadian">End radian</param>
        /// <param name="radius">Buffer radius</param>
        /// <returns>Boundary coordinates of the buffer</returns>
        private IPointCollection GetBufferCoordsByRadian(IPoint center, double startRadian, double endRadian, double radius)
        {
            IPointCollection points = new PolylineClass();
            double gamma = Math.PI / 100;
            double x = 0.0, y = 0.0;
            for (double phi = startRadian; phi <= endRadian + 0.000000000000001; phi += gamma)
            {
                IPoint point = new PointClass();
                x = Math.Round(center.X + radius * Math.Cos(phi), 2);
                y = Math.Round(center.Y + radius * Math.Sin(phi), 2);
                point.PutCoords(x, y);
                points.AddPoint(point);
            }
            return points;
        }
        /// <summary>
        /// Get the cross product of two vectors formed by three adjacent points
        /// </summary>
        /// <param name="preCoord">First node coordinate</param>
        /// <param name="midCoord">Second node coordinate</param>
        /// <param name="nextCoord">Third node coordinate</param>
        /// <returns>Cross product of two vectors formed by three adjacent points</returns>
        private double GetVectorProduct(IPoint preCoord, IPoint midCoord, IPoint nextCoord)
        {
            return (midCoord.X - preCoord.X) * (nextCoord.Y - midCoord.Y) - (nextCoord.X - midCoord.X) * (midCoord.Y - preCoord.Y);
        }
    }
}
