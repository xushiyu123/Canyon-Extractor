using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace CanyonExtractor
{
    /// <summary>
    /// transform data form
    /// </summary>
    class DataTrans
    {
        /// <summary>
        /// transform feature to polyline
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <param name="index">index of feature</param>
        /// <returns></returns>
        public IPolyline FeaturetoPolyline(IFeatureClass pFeatureClass,int index)
        {
            IFeature feature = pFeatureClass.GetFeature(index);
            IPolyline polyline = (IPolyline)feature.Shape;
            return polyline;
        }
        /// <summary>
        /// transform feature to layer
        /// </summary>
        /// <param name="ifc"></param>
        /// <returns></returns>
        public ILayer FeaturetoLayer(IFeatureClass ifc)
        {
            IFeatureLayer ifLayer = new FeatureLayerClass();
            if (ifc != null)
            {
                ifLayer.FeatureClass = ifc;
                ifLayer.Name = ifc.AliasName;
                ILayer iLayer = ifLayer as ILayer;
            }
            return ifLayer;
        }      
    }
}
