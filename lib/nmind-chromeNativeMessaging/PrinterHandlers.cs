//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using System.Drawing.Printing;

/// <summary>
/// 
/// </summary>
namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    public class PrinterHandlers {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.printers.list")]
        public static Messages.Response PrintersList(Messages.Request request) {
            return request.Success(PrinterSettings.InstalledPrinters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.printers.test")]
        public static Messages.Response PrintersTest(Messages.Request request) {
            string printerName = request.Parameters.Value<string>("printerName");
            Utils.ShellPrintTestDocument(printerName);
            return request.Success(printerName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// TODO Supprimer les anciens fichiers télécharger à imprimer
        [RequestMapping("companion.document.print")]
        public static Messages.Response DocumentPrint(Messages.Request request) {
            string printerName = request.Parameters.Value<string>("printerName");
            string path = request.Parameters.Value<string>("path");
            Utils.ShellPrintDocument(printerName, path);
            return request.Success(path);
        }

    }
}
