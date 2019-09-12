using System;
using System.Windows.Forms;
using CanyonExtractor.Forms;

namespace CanyonExtractor
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主线程。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ESRI.ArcGIS.RuntimeManager.BindLicense(ESRI.ArcGIS.ProductCode.EngineOrDesktop, ESRI.ArcGIS.LicenseLevel.Standard);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ExtractorDemo());
        }
    }
}
