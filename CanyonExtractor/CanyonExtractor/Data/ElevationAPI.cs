using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using System.Diagnostics;

namespace CanyonExtractor.Data
{
    class ElevationAPI
    {
        public static Stopwatch stopwatch = new Stopwatch();//time recorder
        public static string url = "http://127.0.0.1:8080/Spring/api/elevation/linestring";//URL of web service
        /// <summary>
        /// test the connection of web service
        /// </summary>
        /// <returns>IsSuccess</returns>
        public static bool TestApi()
        {
            bool success = false;
            StringBuilder stringBuilder = new StringBuilder(url + "?path=0 0,100 100&step=10");
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(stringBuilder.ToString());
            webRequest.Method = "get";
            webRequest.ContentType = "application/json;charset=utf-8";
            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    success = true;
                }
            }
            catch (WebException e)
            {  }
            webRequest.Abort();
            return success;
        }
        /// <summary>
        /// attain elevations from web service
        /// </summary>
        /// <param name="lon1"></param>
        /// <param name="lat1"></param>
        /// <param name="lon2"></param>
        /// <param name="lat2"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static List<ProfileValue> GetElevation(IPointCollection points, int step)
        {
            stopwatch.Start();
            List<ProfileValue> profiles = new List<ProfileValue>();
            StringBuilder stringBuilder = new StringBuilder( url + "?path=");
            for (int i = 0; i < points.PointCount; i++)
            {
                stringBuilder.Append(points.Point[i].X.ToString() + " " + points.Point[i].Y.ToString() + ",");
            }
            stringBuilder.Remove(stringBuilder.Length - 1,1);//delete the last ","
            stringBuilder.Append("&step=" + step);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(stringBuilder.ToString());
            webRequest.Method = "get";
            webRequest.ContentType = "application/json;charset=utf-8";
            string json = "";
            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        using (StreamReader sread = new StreamReader(responseStream))
                        {
                            json = sread.ReadToEnd();
                            profiles = JsonConvert.DeserializeObject<List<ProfileValue>>(json);
                        }
                    }  
                }
                else
                {
                    MessageBox.Show("connection error！");
                }
            }
            catch (WebException e)
            {
                MessageBox.Show(e.ToString());
            }
            webRequest.Abort();
            stopwatch.Stop();
            return profiles;
        }
    }
}
