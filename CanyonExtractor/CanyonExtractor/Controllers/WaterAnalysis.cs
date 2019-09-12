using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Collections.Generic;

namespace CanyonExtractor.Controllers
{
    class WaterAnalysis
    {
        /// <summary>
        /// analysis the river feature
        /// </summary>
        /// <param name="featureClass">river features</param>
        /// <returns></returns>
        public bool DoAnalysis(IFeatureClass featureClass)
        {
            for (int i = 0; i < featureClass.FeatureCount(null); i++)//ergodic the river features
            {
                IFeature feature = featureClass.GetFeature(i);
                IPolyline waterline = (IPolyline)feature.Shape;//read a river feature, and get it geometry information
                IPointCollection pointCollection = waterline as IPointCollection;//tanform polyline to point list        
                List<double> K = new List<double>();
                List<int> ID = new List<int>();
                K.Add(0);ID.Add(0);
                for (int j = 0; j < pointCollection.PointCount - 1; j++)
                {
                    K.Add(SlopeCal(pointCollection.Point[j], pointCollection.Point[j + 1]));
                    ID.Add(j + 1);
                }
            }
            return true;
        }
        /// <summary>
        /// calculate slope
        /// </summary>
        /// <param name="p1">point 1</param>
        /// <param name="p2">point 2</param>
        /// <returns></returns>
        private double SlopeCal(IPoint p1, IPoint p2)
        {
            if (p1.X == p2.X)
                return double.MaxValue;
            else
                return (p1.Y - p2.Y) / (p1.X - p2.X);
        }
    }
}
