//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    internal static class Utils {

        [DllImport("printui.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern void PrintUIEntryW(IntPtr hwnd, IntPtr hinst, string lpszCmdLine, int nCmdShow);

        /// <summary>
        /// Print a Windows test page.
        /// </summary>
        /// <param name="printerName">
        /// Format: \\Server\printer name, for example:
        /// \\myserver\sap3
        /// </param>
        static public void Print(string printerName) {
            var printParams = string.Format(@"/k /n{0}", printerName);
            PrintUIEntryW(IntPtr.Zero, IntPtr.Zero, printParams, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string CombineExePath(string path) {
            return Path.Combine(Utils.AssemblyExecuteableDirectory(), path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string AssemblyExecuteableDirectory() {
            string codeBase = Assembly.GetEntryAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string AssemblyExecuteablePath() {
            string codeBase = Assembly.GetEntryAssembly().CodeBase;
            codeBase = codeBase.Replace("dll", "exe");
            UriBuilder uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printerName"></param>
        /// <param name="filepath"></param>
        static public void ShellPrintDocument(string printerName, string filepath) {

            ProcessStartInfo info = new ProcessStartInfo();

            filepath = filepath.Replace('/', '\\');

            if (filepath.EndsWith(".pdf")) {

                info.FileName = Path.Combine(AssemblyExecuteableDirectory(), "PDFtoPrinter.exe");
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.Arguments = String.Format("\"{0}\" \"{1}\"", filepath, printerName);

            } else {

                info.FileName = filepath;
                info.CreateNoWindow = true;
                info.UseShellExecute = true;
                info.Verb = "PrintTo";
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.Arguments = String.Format("\"{0}\"", printerName);

            }

            Process process = new Process();
            process.StartInfo = info;

            process.Start();

            process.WaitForExit();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printerName"></param>
        static public void ShellPrintTestDocument(string printerName) {
            PrintUIEntryW(IntPtr.Zero, IntPtr.Zero, string.Format(@"/k /n ""{0}""", printerName), 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsMainThread() {

            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA 
                && !Thread.CurrentThread.IsBackground 
                && !Thread.CurrentThread.IsThreadPoolThread 
                && Thread.CurrentThread.IsAlive) {

                MethodInfo EntryMethod = Assembly.GetEntryAssembly().EntryPoint;
                StackTrace trace = new StackTrace();
                StackFrame[] frames = trace.GetFrames();
                for (int i = frames.Length - 1; i >= 0; i--) {
                    MethodBase method = frames[i].GetMethod();
                    if (EntryMethod == method) {
                        return true;
                    }
                }
            }

            return false;
        }

    }

}
