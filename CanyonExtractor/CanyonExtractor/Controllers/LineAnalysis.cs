using System.Collections.Generic;
using System;
using ESRI.ArcGIS.Geometry;
using CanyonExtractor.Data;

namespace CanyonExtractor.Controllers
{
    class LineAnalysis
    {
        /// <summary>
        /// Cross profile analysis
        /// </summary>
        /// <param name="profileLine"></param>
        /// <param name="canyon"></param>
        public void ThroughAnalysis(ProfileLine profileLine, ref Canyon canyon)
        {
            double min = 10000;
            int minx = 0;
            for (int i = 0; i < profileLine.X.Count; i++)
            {
                if (min > profileLine.Z[i])
                {
                    min = profileLine.Z[i];
                    minx = i;
                }
            }
            double mins = 10000;
            int minsx1 = 0;
            int minsx2 = 0;
            ProfileLine sp = SecondDerive(profileLine);
            for (int i = minx - 1; i >= 0; i--)
            {
                if (sp.Z[i] < mins)
                {
                    mins = sp.Z[i];
                    minsx1 = i;
                }
            }
            mins = 10000;
            for (int i = minx + 1; i < sp.X.Count; i++)
            {
                if (sp.Z[i] < mins)
                {
                    mins = sp.Z[i];
                    minsx2 = i;
                }
            }
            canyon.Width = Math.Abs(sp.X[minsx1] - sp.X[minsx2]);
            canyon.DwRatio = canyon.Depth / canyon.Width;
        }
        /// <summary>
        /// extract peaks
        /// </summary>
        /// <param name="workFolder"></param>
        /// <param name="dbfName"></param>
        /// <param name="pointCollection"></param>
        /// <param name="enclosurePara"></param>
        /// <returns></returns>
        public List<CurvePeak> GetCurvePeak(ProfileLine pline,int enclosurePara, int code)
        {
            List<CurvePeak> peaks = new List<CurvePeak>();
            ProfileLine epline = Envelope(pline, enclosurePara);
            switch (code)
            {
                case 0://envelop and accumulate mean curve
                    ProfileLine apline = AccAverge(epline);
                    ProfileLine dpline = Differ(epline, apline);
                    peaks = FindPeak(dpline, epline, epline, 30);
                    break;
                case 1://Secondary difference
                    ProfileLine spline = SecondDerive(epline);
                    peaks = FindPeakByS(spline, epline, 30);
                    break;
                case 2:
                    ProfileLine mpline = Morphologize(epline, 5, 100);
                    ProfileLine smpline = SecondDerive(mpline);
                    peaks = FindPeakByS(smpline, mpline, 30);
                    break;
                default: break;
            }
            return peaks;
        }
        /// <summary>
        /// min elevation of topologic profile
        /// </summary>
        /// <returns></returns>
        private double Min(ProfileLine pline)
        {
            double min = 9999;
            foreach (double z in pline.Z)
            {
                if (min > z)
                    min = z;
            }
            return min;
        }
        /// <summary>
        /// max elevation of topologic profile
        /// </summary>
        /// <returns></returns>
        private double Max(ProfileLine pline)
        {
            double max = 0;
            foreach (double z in pline.Z)
            {
                if (max < z)
                    max = z;
            }
            return max;
        }
        /// <summary>
        /// point list of peak
        /// </summary>
        /// <param name="line"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IPointCollection GetRange(IPointCollection line, double start, double end)
        {
            FeatureDispose featureDispose = new FeatureDispose();
            IPointCollection points = new PolylineClass();
            if (line.PointCount > 0)
            {
                IPoint SPoint = new PointClass();
                IPoint EPoint = new PointClass();
                double sum = 0;
                double sd = 0, ed = 0, d = 0;
                int i = 0, j = 0;
                while (sum < start && i < line.PointCount - 1)
                {
                    d = featureDispose.Distance(line.Point[i], line.Point[i + 1]);
                    sum += d;
                    i++;
                }
                sd = sum - start;
                j = i;
                while (sum < end && j < line.PointCount - 1)
                {
                    d = featureDispose.Distance(line.Point[j], line.Point[j + 1]);
                    sum += d;
                    j++;
                }
                ed = sum - end;
                double sx = 0, sy = 0, ex = 0, ey = 0;
                double d1 = featureDispose.Distance(line.Point[i], line.Point[i + 1]);
                sx = (line.Point[i].X - line.Point[i + 1].X) * sd / d1 + line.Point[i + 1].X;
                sy = (line.Point[i].Y - line.Point[i + 1].Y) * sd / d1 + line.Point[i + 1].Y;
                if (j <= line.PointCount - 2)
                {
                    double d2 = featureDispose.Distance(line.Point[j], line.Point[j + 1]);
                    ex = (line.Point[j].X - line.Point[j + 1].X) * ed / d2 + line.Point[j + 1].X;
                    ey = (line.Point[j].Y - line.Point[j + 1].Y) * ed / d2 + line.Point[j + 1].Y;
                }
                else
                {
                    ex = line.Point[j].X;
                    ey = line.Point[j].Y;
                }
                SPoint.PutCoords(sx, sy);
                EPoint.PutCoords(ex, ey);
                points.AddPoint(SPoint);
                for (int k = i + 1; k < j; k++)
                {
                    points.AddPoint(line.Point[k]);
                }
                points.AddPoint(EPoint);
            }
            return points;
        }
        /// <summary>
        /// coordinate of peak
        /// </summary>
        /// <param name="pointCollection">point list of borderline</param>
        /// <param name="peaks">peak list</param>
        /// <returns>coordinate</returns>
        public IPoint GetPeaksXY(IPointCollection pointCollection, double distance)
        {
            IPoint point = new PointClass();
            if (pointCollection.PointCount > 0)
            {
                double sum = 0;
                double d = 0;
                int j = 0;
                while (sum < distance && j < pointCollection.PointCount - 1)
                {
                    d = Math.Sqrt(Math.Pow(pointCollection.Point[j].X - pointCollection.Point[j + 1].X, 2)
                         + Math.Pow(pointCollection.Point[j].Y - pointCollection.Point[j + 1].Y, 2));
                    sum += d;
                    j++;
                }
                sum -= d;
                double x = 0, y = 0;
                if (d == 0)
                {
                    x = pointCollection.Point[0].X;
                    y = pointCollection.Point[0].Y;
                }
                else
                {
                    if (j < pointCollection.PointCount - 1)
                    {
                        x = (pointCollection.Point[j + 1].X - pointCollection.Point[j].X) * (distance - sum) / d
                            + pointCollection.Point[j].X;
                        y = (pointCollection.Point[j + 1].Y - pointCollection.Point[j].Y) * (distance - sum) / d
                            + pointCollection.Point[j].Y;
                    }
                    else
                    {
                        x = pointCollection.Point[pointCollection.PointCount - 1].X;
                        y = pointCollection.Point[pointCollection.PointCount - 1].Y;
                    }
                }
                point.PutCoords(x, y);
            }
            return point;
        }
        /// <summary>
        /// extract envelope curve
        /// </summary>
        /// <param name="pl">topologic curve</param>
        /// <param name="d">threshold</param>
        /// <returns></returns>
        public ProfileLine Envelope(ProfileLine pl, int d)
        {
            ProfileLine epl = new ProfileLine();
            epl.X = new List<double>();
            epl.Z = new List<double>();
            if (pl.X.Count > 0)
            {
                for (int i = d / 2; i < pl.X.Count - d / 2; i += d)
                {
                    double max = 0;
                    for (int j = i - d / 2; j < i + d / 2; j++)
                    {
                        if (pl.Z[j] > max)
                        {
                            max = pl.Z[j];
                        }
                    }
                    epl.Z.Add(max);
                    epl.X.Add(pl.X[i]);
                }
            }
            return epl;
        }
        /// <summary>
        /// First Derivative
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public ProfileLine FirstDerive(ProfileLine pl)
        {
            ProfileLine fpl = new ProfileLine();
            fpl.X = new List<double>();
            fpl.Z = new List<double>();
            if (pl.X.Count > 0)
            {
                for (int i = 1; i < pl.X.Count; i++)
                {
                    fpl.Z.Add(pl.Z[i] - pl.Z[i - 1]);
                    fpl.X.Add(pl.X[i]);
                }
                fpl.Z.Add(0);
                fpl.X.Add(pl.X[pl.X.Count - 1]);
            }
            return fpl;
        }
        /// <summary>
        /// Second Derivative
        /// </summary>
        /// <param name="pl">original curve</param>
        /// <returns></returns>
        public ProfileLine SecondDerive(ProfileLine pl)
        {
            ProfileLine spl = new ProfileLine();
            spl.X = new List<double>();
            spl.Z = new List<double>();
            if (pl.X.Count > 0)
            {
                List<double> fdy = new List<double>();
                fdy.Add(0);
                for (int i = 1; i < pl.X.Count; i++)
                {
                    fdy.Add(pl.Z[i] - pl.Z[i - 1]);
                }
                for (int i = 0; i < pl.X.Count - 1; i++)
                {
                    spl.Z.Add((fdy[i + 1] - fdy[i]) / (pl.X[1] * pl.X[1]));
                    spl.X.Add(pl.X[i]);
                }
                spl.Z.Add(0);
                spl.X.Add(pl.X[pl.X.Count - 1]);
            }
            return spl;
        }
        /// <summary>
        /// extract accumulate curve
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <returns></returns>
        public ProfileLine AccAverge(ProfileLine profileLine)
        {
            ProfileLine ap = new ProfileLine();
            ap.X = new List<double>();
            ap.Z = new List<double>();
            double ave = 0;
            for (int i = 0; i < profileLine.X.Count; i++)
            {
                double pave = (ave * i + profileLine.Z[i]) / (i + 1);
                ap.X.Add(profileLine.X[i]);
                ap.Z.Add(pave);
                ave = pave;
            }
            return ap;
        }
        /// <summary>
        /// move mean curve
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public ProfileLine MoveAverage(ProfileLine profileLine, int d)
        {
            ProfileLine mp = new ProfileLine();
            mp.X = new List<double>();
            mp.Z = new List<double>();
            if (profileLine.X.Count > 0)
            {
                for (int i = 0; i < profileLine.X.Count - d; i++)
                {
                    double sum = 0;
                    for (int j = i; j < i + d; j++)
                    {
                        sum += profileLine.Z[j];
                    }
                    mp.Z.Add(sum / d);
                    mp.X.Add(profileLine.X[i + d / 2]);
                }
            }
            return mp;
        }
        /// <summary>
        /// differ mean curve and envelope curve
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="ey"></param>
        /// <param name="mx"></param>
        /// <param name="my"></param>
        /// <returns></returns>
        public ProfileLine Differ(ProfileLine ep, ProfileLine mp)
        {
            ProfileLine dp = new ProfileLine();
            dp.X = new List<double>();
            dp.Z = new List<double>();
            if (ep.X.Count > 0 && mp.X.Count > 0)
            {
                int i = 0;
                while (ep.X[i] < mp.X[0])
                {
                    i++;
                    dp.Z.Add(0);
                    dp.X.Add(ep.X[i]);
                }
                for (; i < mp.X.Count; i++)
                {
                    if (ep.Z[i] >= mp.Z[i])
                        dp.Z.Add(ep.Z[i]);
                    else
                        dp.Z.Add(0);
                    dp.X.Add(mp.X[i]);
                }
            }
            return dp;
        }
        /// <summary>
        /// search peak
        /// </summary>
        /// <param name="spl">First Derivative curve</param>
        /// <param name="pl">envelop curve</param>
        /// <param name="amplitude"</param>
        /// <returns>peak list</returns>
        public List<CurvePeak> FindPeakByS(ProfileLine spl, ProfileLine pl, double amplitude)
        {
            List<CurvePeak> peaklist = new List<CurvePeak>();
            for (int i = 1; i < spl.X.Count; i++)
            {
                if (spl.Z[i] < 0 && spl.Z[i - 1] > 0)//in range
                {
                    int j = 0, k = 0;
                    CurvePeak peak = new CurvePeak();
                    peak.Peakx = spl.X[i];//index of peak
                    for (j = i - 1; j >= 0; j--)//index of start
                    {
                        if (spl.Z[j + 1] < 0 && spl.Z[j] > 0)
                        {
                            peak.Startx = spl.X[j];//index of start
                            break;
                        }
                    }
                    if (j < 0) j++;
                    for (k = i + 1; k < spl.X.Count; k++)//index of end 
                    {
                        if (spl.Z[k - 1] < 0 && spl.Z[k] > 0)
                        {
                            peak.Endx = spl.X[k - 1];//index of end
                            break;
                        }
                    }
                    if (k == spl.X.Count) k--;
                    //if no end point has been detected, use the end point of the profile as end of the peak
                    if (peak.Endx == 0)
                    {
                        peak.Endx = spl.X[spl.X.Count - 1];
                    }
                    //
                    peak.Amplitude = Math.Max(pl.Z[i] - pl.Z[j], pl.Z[i] - pl.Z[k]);
                    if (peak.Amplitude >= amplitude)
                    {
                        peak.Centerx = (peak.Startx + peak.Endx) / 2;
                        peak.Length = peak.Endx - peak.Startx;
                        peaklist.Add(peak);//add the peak to list
                    }
                }
            }
            return peaklist;
        }
        /// <summary>
        /// search peak
        /// </summary>
        /// <param name="spl"></param>
        /// <param name="pline"></param>
        /// <param name="amplitude"></param>
        /// <returns></returns>
        public List<CurvePeak> FindPeak(ProfileLine dpline, ProfileLine pline,
            ProfileLine epline, double amplitude)
        {
            List<CurvePeak> peaklist = new List<CurvePeak>();
            int i = 0;
            while (i < dpline.X.Count)
            {
                while (i < dpline.X.Count && dpline.Z[i] == 0)
                    i++;
                if (i < dpline.X.Count)
                {
                    int j = i, k = i;
                    CurvePeak peak = new CurvePeak();
                    peak.Startx = dpline.X[j];
                    double max = pline.Z[i];//peak
                    while (i < dpline.X.Count && dpline.Z[i] > 0)
                    {
                        if (pline.Z[i] > max)
                        {
                            max = pline.Z[i];
                            k = i;
                        }
                        i++;
                    }
                    if (i < dpline.X.Count)//in range
                    {
                        peak.Peakx = dpline.X[k];
                        peak.Endx = dpline.X[i];
                        peak.Amplitude = Math.Max(epline.Z[k] - epline.Z[j], dpline.Z[k] - epline.Z[i]);
                        if (peak.Amplitude >= amplitude)
                        {
                            peak.Centerx = (peak.Startx + peak.Endx) / 2;
                            peak.Length = peak.Endx - peak.Startx;
                            peaklist.Add(peak);
                        }
                    }
                }
            }
            return peaklist;
        }
        /// <summary>
        /// Expansion operation
        /// </summary>
        /// <param name="pline">curve</param>
        /// <param name="m">Structural element length</param>
        /// <param name="value">Structural element value</param>
        /// <returns></returns>
        public ProfileLine Swell(ProfileLine pline, int m, double value)
        {
            ProfileLine sp = new ProfileLine();
            sp.X = new List<double>();
            sp.Z = new List<double>();
            int n = pline.X.Count;
            for (int i = 0; i < n - m; i++)
            {
                double max = pline.Z[i] + value;
                for (int j = i + 1; j < i + m; j++)
                {
                    if (max < pline.Z[j] + value)
                        max = pline.Z[j] + value;
                }
                sp.X.Add(pline.X[i]);
                sp.Z.Add(max);
            }
            return sp;
        }
        /// <summary>
        /// Corrosion operation
        /// </summary>
        /// <param name="pline">curve</param>
        /// <param name="m">Structural element length</param>
        /// <param name="value">Structural element value</param>
        /// <returns></returns>
        public ProfileLine Corrosion(ProfileLine pline, int m, double value)
        {
            ProfileLine cp = new ProfileLine();
            cp.X = new List<double>();
            cp.Z = new List<double>();
            int n = pline.X.Count;
            for (int i = 0; i < n - m; i++)
            {
                double min = pline.Z[i] - value;
                for (int j = i + 1; j < i + m; j++)
                {
                    if (min > pline.Z[j] - value)
                        min = pline.Z[j] - value;
                }
                cp.X.Add(pline.X[i]);
                cp.Z.Add(min);
            }
            return cp;
        }
        /// <summary>
        /// open operation
        /// </summary>
        /// <param name="pline">curve</param>
        /// <param name="m">Structural element length</param>
        /// <param name="value">Structural element value</param>
        /// <returns></returns>
        public ProfileLine Open(ProfileLine pline, int m, double value)
        {
            ProfileLine op = Corrosion(pline, m, value);
            op = Swell(op, m, value);
            return op;
        }
        /// <summary>
        /// closed operation
        /// </summary>
        /// <param name="pline">curve</param>
        /// <param name="m">Structural element length</param>
        /// <param name="value">Structural element value</param>
        /// <returns></returns>
        public ProfileLine Close(ProfileLine pline, int m, double value)
        {
            ProfileLine cp = Corrosion(pline, m, value);
            cp = Swell(cp, m, value);
            return cp;
        }
        /// <summary>
        /// Morphological method
        /// </summary>
        /// <param name="pline">curve</param>
        /// <param name="m">Structural element length</param>
        /// <param name="value">Structural element value</param>
        /// <returns></returns>
        public ProfileLine Morphologize(ProfileLine pline, int m, double value)
        {
            //open first and close next
            ProfileLine pline1 = Open(pline, m, value);
            pline1 = Close(pline1, m, value);
            //close first and open first
            ProfileLine pline2 = Close(pline, m, value);
            pline2 = Open(pline1, m, value);
            //combine
            ProfileLine pline3 = new ProfileLine();
            pline3.X = new List<double>();
            pline3.Z = new List<double>();
            for (int i = 0; i < pline1.X.Count; i++)
            {
                pline3.X.Add(pline1.X[i]);
                pline3.Z.Add((pline1.Z[i] + pline1.Z[i]) / 2);
            }
            return pline3;
        }
    }
}