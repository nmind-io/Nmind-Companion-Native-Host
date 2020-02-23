//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using CommandLine;
using CommandLine.Text;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    abstract class NativeHost {

        /// <summary>
        /// 
        /// </summary>
        static protected Endpoint endpoint;

        /// <summary>
        /// 
        /// </summary>
        public class Options {

            /// <summary>
            /// 
            /// </summary>
            [CommandLine.Option("register", Required = false, Default = "", HelpText = "Register the target browser", SetName = "browserAction")]
            public string RegisterBrowser { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [Option("unregister", Required = false, Default = "", HelpText = "Unregister the target browser", SetName = "browserAction")]
            public string UnregisterBrowser { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [Option("manifest", Required = false, Default = false, HelpText = "show the manifest", SetName = "browserAction")]
            public bool ShowManifest { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [Usage(ApplicationAlias = "Companion")]
            public static IEnumerable<Example> Examples {
                get {
                    yield return new Example("Register companion as Firefox native messaging", new Options { RegisterBrowser = "firefox" });
                    yield return new Example("Unregister companion as Firefox native messaging", new Options { UnregisterBrowser = "chrome" });
                    yield return new Example("Register companion as Firefox and Chrome native messaging", new Options { RegisterBrowser = "all" });
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private static ParserResult<Options> parserResult;

        /// <summary>
        /// 
        /// </summary>
        private static RegistryService regService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consoleMode"></param>
        public static void EnableLog(Boolean consoleMode) {

            string template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Properties} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

            if (consoleMode) {

                Log.Logger = new LoggerConfiguration()
                    //.Enrich.WithThreadId()
                    .Enrich.WithThreadName()
                    .MinimumLevel.Debug()
                    .WriteTo.Console(
                        standardErrorFromLevel: Serilog.Events.LogEventLevel.Debug,
                        outputTemplate: template
                    )
                    .WriteTo.File(
                        endpoint.LocationLog,
                        outputTemplate: template,
                        rollingInterval: RollingInterval.Infinite,
                        fileSizeLimitBytes: 1024 * 1024 * 5, // Mb
                        retainedFileCountLimit: 5,
                        flushToDiskInterval: TimeSpan.FromMilliseconds(500),
                        buffered : false
                    )
                    .CreateLogger();

            } else {

                Log.Logger = new LoggerConfiguration()
                    //.Enrich.WithThreadId()
                    .Enrich.WithThreadName()
                    .MinimumLevel.Information()
                    .WriteTo.File(
                        endpoint.LocationLog,
                        outputTemplate: template,
                        rollingInterval: RollingInterval.Infinite,
                        fileSizeLimitBytes: 1024 * 1024 * 5 // Mb
                    )
                    .CreateLogger();

            }


        }

        #region Signal handler

        /// <summary>
        /// Declare the SetConsoleCtrlHandler function as external and receiving a delegate.
        /// </summary>
        /// <param name="Handler"></param>
        /// <param name="Add"></param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        /// <summary>
        /// A delegate type to be used as the handler routine for SetConsoleCtrlHandler
        /// </summary>
        /// <param name="CtrlType"></param>
        /// <returns></returns>
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        /// <summary>
        /// An enumerated type for the control messages sent to the handler routine
        /// </summary>
        public enum CtrlTypes {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        /// <summary>
        /// Beware ! That method will not be called from the main thread.
        /// Use StopService to signal in the main thread that an exit is requested.
        /// </summary>
        /// <param name="ctrlType"></param>
        /// <returns></returns>
        private static bool ConsoleCtrlHandler(CtrlTypes ctrlType) {

            switch (ctrlType) {
                case CtrlTypes.CTRL_C_EVENT:
                    StopService("CTRL+C received !");
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    StopService("CTRL+BREAK received !");
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    StopService("Host being closed !");
                    break;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    StopService("User is logging off !");
                    break;
            }

            return true;

        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static protected void Run(string[] args) {

            Thread.CurrentThread.Name = "NativeHost::Main";

            try {

                SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlHandler), true);
                regService = new RegistryService(endpoint.Properties);

                BrowserType caller = endpoint.CheckBrowserCall(args);

                if(caller == BrowserType.Unknown) {
                    Log.Debug("Native host started with args {0} ", args);

                    Parser parser = new CommandLine.Parser(with => with.HelpWriter = null);
                    parserResult = parser.ParseArguments<Options>(args);
                    parser.Dispose();

                    parserResult
                        .WithParsed(options => RunAndReturnExitCode(options))
                        .WithNotParsed(errors => DisplayHelp(errors));

                } else {
                    Log.Debug("Native host called by {1} with args {0} ", args, caller);
                    DoHostListen(caller);
                }


            } catch (Exception e) {
                Log.Fatal(e.Message);
            }

            Log.CloseAndFlush();
            Log.Debug("Native host will now exit");

        }

        /// <summary>
        /// In any case, signals in main thread that an exit is requested
        /// </summary>
        static public void StopService(string reason) {
            Log.Debug("Request StopService");
            endpoint.Stop(reason);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errors"></param>
        static protected void DisplayHelp(IEnumerable<Error> errors) {
            Console.WriteLine(CreateHelpText(errors));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        static protected void Usage(string message) {
            Console.WriteLine(message);
            Console.WriteLine();
            Console.WriteLine(CreateHelpText(null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errs"></param>
        /// <returns></returns>
        static protected HelpText CreateHelpText(IEnumerable<Error> errors) {
            return HelpText.AutoBuild(parserResult, h => {
                h.AdditionalNewLineAfterOption = false;
                return HelpText.DefaultParsingErrorsHandler(parserResult, h);
            }, e => e); ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        static protected int RunAndReturnExitCode(Options options) {

            try {

                if (options.ShowManifest) {
                    DoShowManifest();
                    return 0;

                } else if (options.RegisterBrowser.Length > 0) {
                    DoRegisterBrowser(options.RegisterBrowser);
                    return 0;

                } else if (options.UnregisterBrowser.Length > 0) {
                    DoUnregisterBrowser(options.UnregisterBrowser);
                    return 0;

                } else {
                    Usage("No, no, no !! You can't do it that way !");
                    return 0;
                }

            } catch (Exception e) {
                Usage(e.GetType().Name + " : " + e.Message);
                return 1;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caller"></param>
        static protected void DoHostListen(BrowserType caller) {

            if (!regService.IsRegisteredWithChrome() && !regService.IsRegisteredWithFirefox()) {
                throw new NotRegisteredException(String.Format("'{0}' is not registered with Chrome or Firefox", endpoint.Properties.Hostname)); ;
            }

            // Blocked here until Endpoint::stopSignal
            endpoint.Listen(caller);
              
        }

        /// <summary>
        /// 
        /// </summary>
        static protected void DoShowManifest() {
            Console.WriteLine(regService.CreateChromeManifest().AsJson());
            Console.WriteLine(regService.CreateFirefoxManifest().AsJson());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="browser"></param>
        static protected void DoRegisterBrowser(string browser) {

            switch (browser) {
                case "firefox":
                    regService.RegisterWithFirefox();
                    break;

                case "chrome":
                    regService.RegisterWithChrome();
                    break;

                case "all":
                    regService.RegisterWithChrome();
                    regService.RegisterWithFirefox();
                    break;

                default:
                    throw new NotRecognizedParameter(String.Format("Unknown '{0}' as target for register", browser));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="browser"></param>
        static protected void DoUnregisterBrowser(string browser) {

            switch (browser) {
                case "firefox":
                    regService.UnregisterWithFirefox();
                    break;

                case "chrome":
                    regService.UnregisterWithChrome();
                    break;

                case "all":
                    regService.UnregisterWithFirefox();
                    regService.UnregisterWithChrome();
                    break;

                default:
                    throw new NotRecognizedParameter(String.Format("Unknown '{0}' as target for unregister", browser));
            }

        }

    }
}
