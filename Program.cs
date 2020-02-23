//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using Nmind.NativeMessaging;

/// <summary>
/// 
/// </summary>
namespace HostCompanion {

    /// <summary>
    /// 
    /// </summary>
    class Program : NativeHost {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args) {

            HostProperties properties = new HostProperties(
               "nmindcompanionhost",
               "Nmind Companion Host",
               new string[] { "phehpbpffhpnbhgmlmipddaoplhcakcj" },
               new string[] { "nmind-companion@nmind.io" }
            );

            endpoint = new Endpoint(properties);
            endpoint.LoadHandlers(typeof(DefaultHandlers));
            endpoint.LoadHandlers(typeof(PrinterHandlers));
            endpoint.LoadHandlers(typeof(EpaymentHandlers));
            endpoint.LoadHandlers(typeof(CompanionHandlers));

#if DEBUG
            EnableLog(true);
#else
            EnableLog(false);
#endif

            //endpoint.TestMode = true;
            //endpoint.Test(RequestBuilder.From("{'name':'companion.pause', 'id':'123456789', 'async':true}").Silent(true).Create());
            //endpoint.Test(RequestBuilder.From("{'name':'companion.echo','params':'foo'}").Async(true).Create());

            //endpoint.Test(RequestBuilder.From("{'name':'companion.printers.list','params': null}").Async(true).Create());
            //endpoint.Test(RequestBuilder.From("{'name':'companion.serialPorts.list','params': null}").Async(true).Create());

            //endpoint.Test(RequestBuilder.From("{'name':'companion.epayment.ping','params':{'port' : 'COM4', 'device' : 'ict250', 'protocol' : 'Concert-V2'}}").Async(false).Create());
            //endpoint.Test(RequestBuilder.From("{'name':'companion.epayment.process','params':{'amount' : 50.22, 'port' : 'COM4', 'device' : 'ict250', 'protocol' : 'Concert-V2', 'pos' : 88, 'data' : 'AQUAO'}}").Async(true).Create());
            //endpoint.Test(RequestBuilder.From("{'name':'companion.epayment.test','params':{'payload' : '0100000561519782001001001'}}").Async(false).Create());
            //endpoint.Test(RequestBuilder.From("{'name':'companion.epayment.test','params':{'payload' : '010000056001_5017670200028629   0000000000000000000000000000000000_9782001001001'}}").Async(false).Create());

            //endpoint.Test(RequestBuilder.Route("companion.version").Create());
            //endpoint.Test(RequestBuilder.Route("companion.ping").Create());
            //endpoint.Test(RequestBuilder.Route("companion.location.open").With("filepath", "C:/Users/numer/Downloads/").Delay(1000 *10).Async(true).Silent(true).Create());
            //endpoint.Test(RequestBuilder.Route("companion.exit").Async(true).Create());

            //endpoint.Test(RequestBuilder.From(@"{'name':'companion.printers.test','params':{'printerName':'Microsoft Print to PDF'}}").Create());

            //endpoint.Test(RequestBuilder.From("{'name':'companion.document.print','params':{'printerName':'Brother MFC-7460DN Printer','path':'C:/Users/numer/Downloads/34020_29300.pdf'}}").Create());
            //endpoint.Test(RequestBuilder.From("{'name':'companion.document.print','params':{'printerName':'Brother MFC-7460DN Printer','path':'C:/Users/numer/Downloads/ttt.docx'}}").Create());

            Run(args);
        }

    }
}
