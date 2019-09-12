using System;
using System.IO;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;

namespace CanyonExtractor.Data
{
    class InputData
    {
        /// <summary>
        /// file browser
        /// </summary>
        /// <returns>choose file</returns>
        public string GetFilename(string fliter)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = fliter
            };
            string filename = "";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filename = ofd.FileName;
            }
            return filename;
        }
        /// <summary>
        /// folder browser
        /// </summary>
        /// <returns>choose folder</returns>
        public string GetFoldername()
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            string folderName = "";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                folderName = folderBrowser.SelectedPath;
            }
            return folderName;
        }       
        /// <summary>
        /// input shapefile
        /// </summary>
        /// <param name="featureFile">feature file</param>
        /// <returns>container</returns>
        public IFeatureClass InputShp(string featureFile)
        {
            IFeatureClass ifc = null;
            if (featureFile != "")
            {
                IWorkspaceFactory iwf = new ShapefileWorkspaceFactoryClass();
                IWorkspace iw = iwf.OpenFromFile(Path.GetDirectoryName(featureFile), 0);
                IFeatureWorkspace ifw = iw as IFeatureWorkspace;
                if (ifw != null)
                {
                    ifc = ifw.OpenFeatureClass(Path.GetFileName(featureFile));
                }
                Marshal.ReleaseComObject(iwf);
                Marshal.ReleaseComObject(iw);
                Marshal.ReleaseComObject(ifw);
            }
            return ifc;
        }
        /// <summary>
        /// delete files of a folder
        /// </summary>
        /// <param name="foldname">folder</param>
        /// <returns>IsDone?</returns>
        public bool DeleteFiles(string foldname)
        {
            DirectoryInfo dir = new DirectoryInfo(foldname);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //all files and folders
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)            
                {
                    DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                    subdir.Delete(true);          
                }
                else
                {
                    File.Delete(i.FullName);      
                }
            }
            dir.Refresh();
            return true;
        }        
        /// <summary>
        /// clear memory
        /// </summary>
        /// <param name="process"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
    }
}