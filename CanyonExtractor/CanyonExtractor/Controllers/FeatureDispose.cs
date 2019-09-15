using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using CanyonExtractor.Data;

namespace CanyonExtractor.Controllers
{
    /// <summary>
    /// classes of disposing features
    /// </summary>
    class FeatureDispose
    {
        public static double percent = 0;
        /// <summary>
        /// do canyon extracting
        /// </summary>
        /// <param name="waterlineFile">river file</param>
        /// <param name="workFolder">work space</param>
        /// <returns>Is Done?</returns>
        public string DoIdentify(string waterlineFile, string canyonsFolder, string canyonName,
            double distance, int enclosurePara, int code1, int code2)
        {
            if (File.Exists(waterlineFile) && Directory.Exists(canyonsFolder))
            {
                InputData inputData = new InputData();
                LineAnalysis lineAnalysis = new LineAnalysis();
                DataTrans dataTrans = new DataTrans();
                BufferlineClass bufferlineClass = new BufferlineClass();
                IFeatureClass featureClass = inputData.InputShp(waterlineFile);//read the river data
                WaterInDgree waterInDgree = new WaterInDgree();
                if (waterInDgree.HasIndgree(featureClass))
                    waterInDgree.InDgreeCalculate(ref featureClass);//indegree calculating
                int maxDegree = 0;
                int id = featureClass.FindField("InDgree");
                for (int i = 0; i < featureClass.FeatureCount(null); i++)
                {
                    if (maxDegree < Convert.ToInt32(featureClass.GetFeature(i).Value[id]))
                        maxDegree = Convert.ToInt32(featureClass.GetFeature(i).Value[id]);
                }
                List<Canyon> canyons = new List<Canyon>();
                for (int i = 0; i < featureClass.FeatureCount(null); i++)//travese the river features
                {
                    double width = distance * Convert.ToDouble(featureClass.GetFeature(i).Value[id]) / Convert.ToDouble(maxDegree);
                    IPolyline waterline = dataTrans.FeaturetoPolyline(featureClass, i);//read one river feature and get its geometry data
                    IPointCollection pointCollection = waterline as IPointCollection;//transform polyline to point list       
                    BufferLines bufferLines = new BufferLines();
                    bufferLines.LeftIpc = bufferlineClass.GetLeftBufferEdgeCoords(pointCollection, width);//generate left borderline
                    IPointCollection rpointCollection = bufferlineClass.Reverse(pointCollection);//reverse the point list
                    IPointCollection rightCollection = bufferlineClass.GetLeftBufferEdgeCoords(rpointCollection, width);//generate right borderline
                    bufferLines.RightIpc = bufferlineClass.Reverse(rightCollection);//reverse the right borderline, ensure its clock wise
                    ProfileLine leftProfile = ProfileByAPI(bufferLines.LeftIpc, 30);//get topologic profile of left borderline
                    ProfileLine rightProfile = ProfileByAPI(bufferLines.RightIpc, 30);//get topologic profile of right borderline
                    //ProfileLine waterProfile = ProfileByAPI(pointCollection, 30);//get topologic profile of chanel
                    //ProfileLine leftProfileN = NewProfile(leftProfile, waterProfile);
                    //ProfileLine rightProfileN = NewProfile(rightProfile, waterProfile);
                    List<CurvePeak> leftPeaks = lineAnalysis.GetCurvePeak(leftProfile, enclosurePara, code1);//获取右侧界线波峰
                    List<CurvePeak> rightPeaks = lineAnalysis.GetCurvePeak(rightProfile, enclosurePara, code1);//获取左侧界线波峰
                    //List<CurvePeak> leftPeaks = lineAnalysis.GetCurvePeak(leftProfileN, bufferLines.LeftIpc, enclosurePara, code1);//获取右侧界线波峰
                    //List<CurvePeak> rightPeaks = lineAnalysis.GetCurvePeak(rightProfileN, bufferLines.RightIpc, enclosurePara, code1);//获取左侧界线波峰
                    List<PeakPair> pairs = new List<PeakPair>();//generate peak pairs
                    switch (code2)
                    {
                        case 0:
                            pairs = CreatePeakPairsByDistance(bufferLines, leftPeaks, rightPeaks);
                            break;//distance-prior
                        case 1:
                            pairs = CreatePeakPairsByLength(bufferLines, leftPeaks, rightPeaks);
                            break;//length-prior
                        case 2:
                            pairs = CreatePeakPairsByAmplitude(bufferLines, leftPeaks, rightPeaks);
                            break;//amplitude-prior
                        default:
                            MessageBox.Show("please choose peaks matching method！");
                            return "";
                    }
                    CreateCanyon(pairs, bufferLines, pointCollection, i, width, ref canyons);
                }
                Marshal.ReleaseComObject(featureClass);
                ShpFromPolygon(canyonsFolder, canyonName, canyons);
                InputData.ClearMemory();
                return canyonsFolder + "//" + canyonName + ".shp";
            }
            else
            {
                MessageBox.Show("please check the input data！");
                return "";
            }
        }
        /// <summary>
        /// calculate the topologic curve
        /// </summary>
        /// <param name="profile">profile</param>
        /// <param name="water">river</param>
        /// <returns></returns>
        private ProfileLine NewProfile(ProfileLine profile, ProfileLine water)
        {
            ProfileLine line = new ProfileLine();
            int pCount = profile.X.Count;
            int wCount = water.X.Count;
            //discrete
            for (int i = 0; i < pCount; i++)
            {
                int k = Convert.ToInt32((double)i * (double)wCount / (double)pCount);
                double z = profile.Z[i] - water.Z[k];
                line.X.Add(profile.X[i]);
                line.Z.Add(z);
            }
            return line;
        }
        /// <summary>
        /// calculate the distance of two points
        /// </summary>
        /// <param name="p1">point 1</param>
        /// <param name="p2">point 2</param>
        /// <returns>distance</returns>
        public double Distance(IPoint p1, IPoint p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        /// <summary>
        /// attain the topologic profile from service
        /// </summary>
        /// <param name="pointCollection">borderline</param>
        /// <param name="step">step of profile</param>
        /// <returns>topologic curve</returns>
        private ProfileLine ProfileByAPI(IPointCollection pointCollection, int step)
        {
            ProfileLine profileLine = new ProfileLine();
            profileLine.X = new List<double>();
            profileLine.Z = new List<double>();
            double distance = 0;
            for (int i = 0; i < pointCollection.PointCount; i += 100)
            {
                double d = 0;
                IPointCollection collection = new PolylineClass();
                for (int j = 0; j < 100 && i + j < pointCollection.PointCount; j++)
                {
                    collection.AddPoint(pointCollection.Point[i + j]);
                    if (j > 0)
                        d += Distance(collection.Point[j - 1], collection.Point[j]);
                }
                List<ProfileValue> profileValues = ElevationAPI.GetElevation(collection, step);
                for (int j = 0; j < profileValues.Count; j++)
                {
                    if (profileValues[j].Altitude != null || Convert.ToDouble(profileValues[j].Altitude) == -9999)
                    {
                        profileLine.Z.Add(Convert.ToDouble(profileValues[j].Altitude));
                        if (profileLine.X.Count == 0)
                            profileLine.X.Add(0);
                        else
                        {
                            if (j == 0)
                            {
                                profileLine.X.Add(profileLine.X[profileLine.X.Count - 1] + step + distance);
                            }
                            else
                            {
                                profileLine.X.Add(profileLine.X[profileLine.X.Count - 1] + step);
                            }
                        }
                    }
                    else
                        break;//out of range
                }
                distance = step - d % step;
            }
            return profileLine;
        }
        /// <summary>
        /// Distance-prior
        /// </summary>
        /// <param name="bufferLines">borderline</param>
        /// <param name="leftPeaks"></param>
        /// <param name="rightPeaks"></param>
        /// <param name="waterlineIndex">index of river segment</param>
        /// <returns></returns>
        private List<PeakPair> CreatePeakPairsByDistance(BufferLines bufferLines,
            List<CurvePeak> leftPeaks, List<CurvePeak> rightPeaks)
        {
            List<PeakPair> canyons = new List<PeakPair>();
            bool flag = false;
            if (leftPeaks.Count > rightPeaks.Count)
            {
                flag = true;
                List<CurvePeak> tpeaks = rightPeaks;
                rightPeaks = leftPeaks;
                leftPeaks = tpeaks;
            }
            for (int i = 0; i < leftPeaks.Count; i++)
            {
                int leftId = i, rightId = 0;
                double min = 10000000;
                for (int j = 0; j < rightPeaks.Count; j++)
                {
                    if (Math.Abs(rightPeaks[j].Centerx - leftPeaks[i].Centerx) < min)
                    {
                        min = Math.Abs(rightPeaks[j].Centerx - leftPeaks[i].Centerx);
                        rightId = j;
                    }
                }
                if (min < 6000 && rightPeaks.Count > 0)
                {
                    PeakPair canyon = new PeakPair();
                    if (flag)
                    {
                        canyon.LeftPeak = rightPeaks[rightId];
                        canyon.RightPeak = leftPeaks[leftId];
                    }
                    else
                    {
                        canyon.LeftPeak = leftPeaks[leftId];
                        canyon.RightPeak = rightPeaks[rightId];
                    }
                    canyons.Add(canyon);
                    rightPeaks.RemoveAt(rightId);
                }
            }
            return canyons;
        }
        /// <summary>
        /// Amplitude-prior
        /// </summary>
        /// <param name="bufferLines"></param>
        /// <param name="leftPeaks"></param>
        /// <param name="rightPeaks"></param>
        /// <param name="waterlineIndex"></param>
        /// <returns></returns>
        private List<PeakPair> CreatePeakPairsByAmplitude(BufferLines bufferLines,
            List<CurvePeak> leftPeaks, List<CurvePeak> rightPeaks)
        {
            List<PeakPair> canyons = new List<PeakPair>();
            bool flag = false;
            if (leftPeaks.Count > rightPeaks.Count)
            {
                flag = true;
                List<CurvePeak> tpeaks = rightPeaks;
                rightPeaks = leftPeaks;
                leftPeaks = tpeaks;
            }
            for (int i = 0; i < leftPeaks.Count; i++)
            {
                int leftId = i, rightId = 0;
                double min = 10000000;
                for (int j = 0; j < rightPeaks.Count; j++)
                {
                    if (Math.Abs(rightPeaks[j].Length - leftPeaks[i].Length) < min)
                    {
                        min = Math.Abs(rightPeaks[j].Length - leftPeaks[i].Length);
                        rightId = j;
                    }
                }
                if (rightPeaks.Count > 0)
                {
                    PeakPair canyon = new PeakPair();
                    if (flag)
                    {
                        canyon.LeftPeak = rightPeaks[rightId];
                        canyon.RightPeak = leftPeaks[leftId];
                    }
                    else
                    {
                        canyon.LeftPeak = leftPeaks[leftId];
                        canyon.RightPeak = rightPeaks[rightId];
                    }
                    canyons.Add(canyon);
                    rightPeaks.RemoveAt(rightId);
                }
            }
            return canyons;
        }
        /// <summary>
        /// Length-prior
        /// </summary>
        /// <param name="bufferLines"></param>
        /// <param name="leftPeaks"></param>
        /// <param name="rightPeaks"></param>
        /// <param name="waterlineIndex"></param>
        /// <param name="gorgesFolder"></param>
        /// <returns></returns>
        private List<PeakPair> CreatePeakPairsByLength(BufferLines bufferLines,
            List<CurvePeak> leftPeaks, List<CurvePeak> rightPeaks)
        {
            List<PeakPair> canyons = new List<PeakPair>();
            bool flag = false;
            if (leftPeaks.Count > rightPeaks.Count)
            {
                flag = true;
                List<CurvePeak> tpeaks = rightPeaks;
                rightPeaks = leftPeaks;
                leftPeaks = tpeaks;
            }
            for (int i = 0; i < leftPeaks.Count; i++)
            {
                int leftId = i, rightId = 0;
                double min = 10000000;
                for (int j = 0; j < rightPeaks.Count; j++)
                {
                    if (Math.Abs(rightPeaks[j].Amplitude - leftPeaks[i].Amplitude) < min)
                    {
                        min = Math.Abs(rightPeaks[j].Amplitude - leftPeaks[i].Amplitude);
                        rightId = j;
                    }
                }
                if (rightPeaks.Count > 0)
                {
                    PeakPair canyon = new PeakPair();
                    if (flag)
                    {
                        canyon.LeftPeak = rightPeaks[rightId];
                        canyon.RightPeak = leftPeaks[leftId];
                    }
                    else
                    {
                        canyon.LeftPeak = leftPeaks[leftId];
                        canyon.RightPeak = rightPeaks[rightId];
                    }
                    canyons.Add(canyon);
                    rightPeaks.RemoveAt(rightId);
                }
            }
            return canyons;
        }
        /// <summary>
        /// generate canyons
        /// </summary>
        /// <param name="gorgesFolder"></param>
        /// <param name="pairs"></param>
        /// <param name="waterlineIndex"></param>
        /// <param name="bufferLines"></param>
        private void CreateCanyon(List<PeakPair> pairs, BufferLines bufferLines,
            IPointCollection waterCollection, int waterid, double r, ref List<Canyon> canyons)
        {
            LineAnalysis lineAnalysis = new LineAnalysis();
            for (int i = 0; i < pairs.Count; i++)
            {
                IPointCollection lpointCollection = lineAnalysis.GetRange(bufferLines.LeftIpc, pairs[i].LeftPeak.Startx, pairs[i].LeftPeak.Endx);
                IPointCollection rpointCollection = lineAnalysis.GetRange(bufferLines.RightIpc, pairs[i].RightPeak.Startx, pairs[i].RightPeak.Endx);
                IPointCollection canyonRange = AlterRange(lpointCollection, rpointCollection); 
                Canyon canyon = new Canyon();
                canyon.CanyonCollection = new PolygonClass();
                canyon.Waterid = waterid;
                canyon.Depth = Math.Max(pairs[i].LeftPeak.Amplitude, pairs[i].RightPeak.Amplitude);
                canyon.CanyonCollection = canyonRange;
                PropertyCal(canyonRange, waterCollection, r, ref canyon);
                canyon.Area = canyon.Length * canyon.Width;
                canyons.Add(canyon);
            }
        }
        /// <summary>
        /// adjust canyon ranges
        /// </summary>
        /// <param name="lline">left borderline</param>
        /// <param name="rline">right borderline</param>
        /// <returns></returns>
        private IPointCollection AlterRange(IPointCollection lline, IPointCollection rline)
        {
            IPointCollection range = new PolygonClass();
            BufferlineClass bufferlineClass = new BufferlineClass();
            int lsid = 0, leid = lline.PointCount - 1, rsid = 0, reid = rline.PointCount - 1;
            IPoint lsp = new PointClass(); IPoint lep = new PointClass();
            IPoint rsp = new PointClass(); IPoint rep = new PointClass();
            for (int i = 1; i < lline.PointCount - 1; i++)
            {
                if (bufferlineClass.InsectionJudge(lline.Point[0], rline.Point[0], lline.Point[i], lline.Point[i + 1]))
                {
                    lsp = bufferlineClass.Inter(lline.Point[0], rline.Point[0], lline.Point[i], lline.Point[i + 1]);
                    lsid = i + 1;
                    break;
                }
            }
            for (int i = lline.PointCount - 2; i > 0; i--)
            {
                if (bufferlineClass.InsectionJudge(lline.Point[lline.PointCount - 1], rline.Point[rline.PointCount - 1], lline.Point[i], lline.Point[i - 1]))
                {
                    lep = bufferlineClass.Inter(lline.Point[lline.PointCount - 1], rline.Point[rline.PointCount - 1], lline.Point[i], lline.Point[i - 1]);
                    leid = i - 1;
                    break;
                }
            }
            for (int i = 1; i < rline.PointCount - 1; i++)
            {
                if (bufferlineClass.InsectionJudge(lline.Point[0], rline.Point[0], rline.Point[i], rline.Point[i + 1]))
                {
                    rsp = bufferlineClass.Inter(lline.Point[0], rline.Point[0], rline.Point[i], rline.Point[i + 1]);
                    rsid = i + 1;
                    break;
                }
            }
            for (int i = rline.PointCount - 2; i > 0; i--)
            {
                if (bufferlineClass.InsectionJudge(lline.Point[lline.PointCount - 1], rline.Point[rline.PointCount - 1], rline.Point[i], rline.Point[i - 1]))
                {
                    rep = bufferlineClass.Inter(lline.Point[lline.PointCount - 1], rline.Point[rline.PointCount - 1], rline.Point[i], rline.Point[i - 1]);
                    reid = i - 1;
                    break;
                }
            }
            if (lsid != 0)
                range.AddPoint(lsp);
            for (int i = lsid; i <= leid; i++)
                range.AddPoint(lline.Point[i]);
            if (leid != lline.PointCount - 1)
                range.AddPoint(lep);
            if (reid != rline.PointCount - 1)
                range.AddPoint(rep);
            for (int i = reid; i >= rsid; i--)
                range.AddPoint(rline.Point[i]);
            if (rsid != 0)
                range.AddPoint(rsp);
            return range;
        }
        /// <summary>
        /// calculate canyon attributes
        /// </summary>
        /// <param name="pointCollection">range polygon</param>
        /// <param name="waterCollection">river polyline</param>
        /// <param name="canyon"></param>
        public void PropertyCal(IPointCollection pointCollection, IPointCollection waterCollection, double r, ref Canyon canyon)
        {
            IPointCollection points = new PolylineClass();
            BufferlineClass bufferline = new BufferlineClass();
            for (int i = 0; i < waterCollection.PointCount - 1; i++)
            {
                for (int j = 0; j < pointCollection.PointCount - 1; j++)
                {
                    if (bufferline.InsectionJudge(waterCollection.Point[i], waterCollection.Point[i + 1],
                        pointCollection.Point[j], pointCollection.Point[j + 1]))
                    {
                        IPoint point = bufferline.Inter(waterCollection.Point[i], waterCollection.Point[i + 1],
                            pointCollection.Point[j], pointCollection.Point[j + 1]);
                        points.AddPoint(point);
                    }
                }
            }
            double max = 0;
            IPoint point1 = new PointClass();
            IPoint point2 = new PointClass();
            if (points.PointCount == 1)
            {
                points.AddPoint(waterCollection.Point[waterCollection.PointCount - 1]);
            }
            for (int i = 0; i < points.PointCount; i++)
            {
                for (int j = 0; j < points.PointCount; j++)
                {
                    double d = Distance(points.Point[i], points.Point[j]);
                    if (d > max)
                    {
                        max = d;
                        point1 = points.Point[i];
                        point2 = points.Point[j];
                    }
                }
            }
            canyon.Length = max;
            double x0 = 0, y0 = 0;
            for (int i = 0; i < canyon.CanyonCollection.PointCount; i++)
            {
                x0 = (x0 * Convert.ToDouble(i) + canyon.CanyonCollection.Point[i].X) / (Convert.ToDouble(i) + 1);
                y0 = (y0 * Convert.ToDouble(i) + canyon.CanyonCollection.Point[i].Y) / (Convert.ToDouble(i) + 1);
            }
            canyon.X0 = x0;
            canyon.Y0 = y0;
            WidthCal(point1, point2, r, ref canyon);
        }
        /// <summary>
        /// calculate width
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="r"></param>
        /// <param name="gorge"></param>
        private void WidthCal(IPoint point1, IPoint point2, double r, ref Canyon gorge)
        {
            IPoint point3 = new PointClass();
            IPoint point4 = new PointClass();
            if (point1.X == point2.X)
            {
                point3.PutCoords(point1.X - r, point1.Y);
                point4.PutCoords(point1.X + r, point1.Y);
            }
            else if (point1.Y == point2.Y)
            {
                point3.PutCoords(point1.X, point1.Y - r);
                point4.PutCoords(point1.X, point1.Y + r);
            }
            else
            {
                double k = -(point1.X - point2.X) / (point1.Y - point2.Y);
                double dx = 1 / Math.Sqrt(k * k + 1) * r;
                double dy = -k * dx;
                point3.PutCoords(point1.X + dx, point1.Y + dy);
                point4.PutCoords(point1.X - dx, point1.Y - dy);
            }
            IPointCollection pointCollection = new PolylineClass();
            pointCollection.AddPoint(point3);
            pointCollection.AddPoint(point4);
            ProfileLine profileLine = ProfileByAPI(pointCollection, 30);
            LineAnalysis lineAnalysis = new LineAnalysis();
            lineAnalysis.ThroughAnalysis(profileLine, ref gorge);
        }  
        /// <summary>
        /// generate polygon and save as shapefile
        /// </summary>
        /// <param name="shpFolder">folder</param>
        /// <param name="shpName">name</param>
        /// <param name="ipc">point list</param>
        /// <returns>Over?</returns>
        public bool ShpFromPolygon(string shpFolder, string shpName,
            List<Canyon> canyons)
        {
            if (File.Exists(shpFolder + shpName + ".shp"))
            {
                File.Delete(shpFolder + shpName + ".shp");
                File.Delete(shpFolder + shpName + ".sbn");
                File.Delete(shpFolder + shpName + ".sbx");
                File.Delete(shpFolder + shpName + ".dbf");
                File.Delete(shpFolder + shpName + ".shx");
                File.Delete(shpFolder + shpName + ".shp.xml");
            }
            IWorkspaceFactory pWF = new ShapefileWorkspaceFactoryClass();
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = fcDescription as IObjectClassDescription;
            IFields pFields = new Fields();
            IGeometryDef pGeometryDef = new GeometryDefClass();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new Field();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = "SHAPE";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFieldEdit.GeometryDef_2 = pGeometryDef;
            pFieldEdit.IsNullable_2 = true;
            pFieldsEdit.AddField(pField);
            AddField(ref pFields, "X", esriFieldType.esriFieldTypeDouble);
            AddField(ref pFields, "Y", esriFieldType.esriFieldTypeDouble);
            AddField(ref pFields, "WaterId", esriFieldType.esriFieldTypeSmallInteger);
            AddField(ref pFields, "Depth", esriFieldType.esriFieldTypeDouble);
            AddField(ref pFields, "Width", esriFieldType.esriFieldTypeDouble);
            AddField(ref pFields, "Ratio", esriFieldType.esriFieldTypeDouble);
            AddField(ref pFields, "Length", esriFieldType.esriFieldTypeDouble);
            AddField(ref pFields, "Area", esriFieldType.esriFieldTypeDouble);
            IFeatureWorkspace pFWs = pWF.OpenFromFile(shpFolder, 0) as IFeatureWorkspace;
            IFeatureClass pFC = pFWs.CreateFeatureClass(shpName, pFields, null,
                null, esriFeatureType.esriFTSimple, "SHAPE", "");
            IFeature pFeature = null;
            for (int i = 0; i < canyons.Count; i++)
            {
                pFeature = pFC.CreateFeature();
                pFeature.Shape = canyons[i].CanyonCollection as IPolygon;
                pFeature.Value[2] = canyons[i].X0;
                pFeature.Value[3] = canyons[i].Y0;
                pFeature.Value[4] = canyons[i].Waterid;
                pFeature.Value[5] = canyons[i].Depth;
                pFeature.Value[6] = canyons[i].Width;
                pFeature.Value[7] = canyons[i].DwRatio;
                pFeature.Value[8] = canyons[i].Length;
                pFeature.Value[9] = canyons[i].Area;
                pFeature.Store();
            }
            if (pFeature != null)
                Marshal.ReleaseComObject(pFeature);
            Marshal.ReleaseComObject(pWF);
            Marshal.ReleaseComObject(pFC);
            Marshal.ReleaseComObject(pFWs);
            Marshal.ReleaseComObject(pFields);
            Marshal.ReleaseComObject(pGeometryDef);
            Marshal.ReleaseComObject(pGeometryDefEdit);
            return true;
        }
        /// <summary>
        /// add attribute field
        /// </summary>
        /// <param name="pFields"></param>
        /// <param name="FieldName"></param>
        /// <param name="esriFieldType"></param>
        public void AddField(ref IFields pFields, string FieldName, esriFieldType esriFieldType)
        {
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new Field();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = FieldName;
            pFieldEdit.Type_2 = esriFieldType;
            pFieldEdit.IsNullable_2 = true;
            pFieldsEdit.AddField(pField);
        }
        /// <summary>
        /// save polyline as shapefile
        /// </summary>
        /// <param name="shpFolder"></param>
        /// <param name="shpName"></param>
        /// <param name="ipc"></param>
        /// <returns></returns>
        public bool ShpFromPolyline(string shpFolder, string shpName, IPointCollection ipc)
        {
            IWorkspaceFactory pWF = new ShapefileWorkspaceFactoryClass();
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = fcDescription as IObjectClassDescription;
            IFields pFields = new Fields();
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new Field();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            IGeometryDef pGeometryDef = new GeometryDefClass();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFieldEdit.GeometryDef_2 = pGeometryDef;
            pFieldEdit.Name_2 = "SHAPE";
            pFieldsEdit.AddField(pField);
            IFeatureWorkspace pFWs = pWF.OpenFromFile(shpFolder, 0) as IFeatureWorkspace;
            IFeatureClass pFC = pFWs.CreateFeatureClass(shpName, pFields, null,
                null, esriFeatureType.esriFTSimple, "Shape", "");
            IFeature pFeature = pFC.CreateFeature();
            pFeature.Shape = ipc as IPolyline;
            pFeature.Store();
            Marshal.ReleaseComObject(pWF);
            Marshal.ReleaseComObject(pFieldsEdit);
            Marshal.ReleaseComObject(pFeature);
            Marshal.ReleaseComObject(pFieldEdit);
            Marshal.ReleaseComObject(pFC);
            Marshal.ReleaseComObject(pFWs);
            Marshal.ReleaseComObject(pFields);
            Marshal.ReleaseComObject(pGeometryDef);
            Marshal.ReleaseComObject(pGeometryDefEdit);
            return true;
        }      
    }
}
