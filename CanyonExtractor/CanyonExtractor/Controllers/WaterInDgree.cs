using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using CanyonExtractor.Data;

namespace CanyonExtractor.Controllers
{
    class WaterInDgree
    {
        /// <summary>
        /// calculate the indegree of river feature
        /// </summary>
        /// <param name="featureClass">river feature</param>
        /// <returns></returns>
        public bool InDgreeCalculate(ref IFeatureClass featureClass)
        {
            if (!HasIndgree(featureClass))
            {
                int Count = featureClass.FeatureCount(null);
                List<Water> waters = new List<Water>();
                for (int i = 0; i < Count; i++)
                {
                    IFeature feature = featureClass.GetFeature(i);
                    IPolyline waterline = (IPolyline)feature.Shape;
                    IPointCollection pc = waterline as IPointCollection;
                    Water water = new Water();
                    water.InVertex1 = new Data.Point();
                    water.InVertex1.X = pc.Point[0].X;
                    water.InVertex1.Y = pc.Point[0].Y;
                    water.OutVertex1 = new Data.Point();
                    water.OutVertex1.X = pc.Point[pc.PointCount - 1].X;
                    water.OutVertex1.Y = pc.Point[pc.PointCount - 1].Y;
                    water.Indgree = 1;
                    waters.Add(water);
                }
                int[,] matrix = AdjMatrix(waters);
                InCount(matrix, ref waters);
                //add a field to the attribute table (indegree)
                IField pField = new Field();
                IFieldEdit pFieldEdit = pField as IFieldEdit;
                pFieldEdit = pField as IFieldEdit;
                pFieldEdit.Name_2 = "InDgree";
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
                pFieldEdit.IsNullable_2 = true;
                featureClass.AddField(pField);               
                IFeatureCursor featureCursor = featureClass.Search(null, false);
                IWorkspace workspace = ((IDataset)featureClass).Workspace;
                IWorkspaceEdit workspaceEdit = workspace as IWorkspaceEdit;
                bool startEdit = workspaceEdit.IsBeingEdited();
                if (!startEdit)
                {
                    workspaceEdit.StartEditing(false);
                }
                //start editing
                workspaceEdit.StartEditOperation();
                for (int i = 0; i < Count; i++)
                {                  
                    IFeature feature = featureCursor.NextFeature();
                    feature.Value[3] = waters[i].Indgree;
                    feature.Store();
                }
                //update feature
                featureCursor.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
                workspaceEdit.StopEditing(true);        //stop editing        
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// have got an attribute of indegree in river feature?
        /// </summary>
        /// <param name="featureClass">river feature</param>
        /// <returns></returns>
        public bool HasIndgree(IFeatureClass featureClass)
        {
            IFields fields = featureClass.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                if (fields.Field[i].Name == "InDgree")
                    return true;
            }
            return false;
        }
        /// <summary>
        /// calculate the Adjacent matrix
        /// </summary>
        /// <param name="waters">river segments</param>
        /// <returns></returns>
        private int[,] AdjMatrix(List<Water> waters)
        {
            int Count = waters.Count;
            int[,] matrix = new int[Count, Count];
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < Count; j++)
                {
                    matrix[i, j] = 0;
                }
            }
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < Count; j++)
                {
                    if (waters[i].OutVertex1.X == waters[j].InVertex1.X &&
                        waters[i].OutVertex1.Y == waters[j].InVertex1.Y)
                    {
                        matrix[i, j] = 1;
                    }
                }
            }
            return matrix;
        }
        /// <summary>
        /// count the in vertexes
        /// </summary>
        /// <param name="matrix">adjcent matrix</param>
        /// <param name="waters">river segments</param>
        private void InCount(int[,] matrix, ref List<Water> waters)
        {
            int count = matrix.GetLength(1);
            for (int i = 0; i < count; i++)
            {
                int sum = 0;
                for (int j = 0; j < count; j++)
                {
                    sum += matrix[j, i];
                }
                waters[i].Indgree = sum + 1;
            }
        }
    }
}
