//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    public enum BrowserType { 
        Unknown, 
        Firefox, 
        Chrome
    };

    /// <summary>
    /// 
    /// </summary>
    public enum EndpointState {
        Idle,
        Setup,
        Listening,
        ExitPending,
        Teardown
    };

    /// <summary>
    /// 
    /// </summary>
    public sealed class Endpoint {

        /// <summary>
        /// 
        /// </summary>
        public string LocationLog { get; }

        /// <summary>
        /// 
        /// </summary>
        public HostProperties Properties { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public delegate Messages.Response Handler(Messages.Request request);

        /// <summary>
        /// 
        /// </summary>
        public bool TestMode { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>();

        /// <summary>
        /// 
        /// </summary>
        private readonly ConcurrentQueue<Messages.Request> requestQueue = new ConcurrentQueue<Messages.Request>();

        /// <summary>
        /// 
        /// </summary>
        private readonly ConcurrentQueue<Messages.Response> responseQueue = new ConcurrentQueue<Messages.Response>();

        /// <summary>
        /// 
        /// </summary>
        private BrowserType browserType;

        /// <summary>
        /// 
        /// </summary>
        private ManualResetEvent stopSignal;

        /// <summary>
        /// 
        /// </summary>
        private readonly Mutex writeMessageMutex = new Mutex();

        /// <summary>
        /// For a synchrnous request, prevents asynchronous response to be written
        /// </summary>
        private readonly Mutex processRequestMutex = new Mutex();

        /// <summary>
        /// 
        /// </summary>
        private EndpointState state = EndpointState.Idle;

        /// <summary>
        /// 
        /// </summary>
        private Thread mainWorker;

        /// <summary>
        /// 
        /// </summary>
        private Thread requestQueueWorker;

        /// <summary>
        /// 
        /// </summary>
        private Thread responseQueueWorker;

        /// <summary>
        /// Creates the Host Object
        /// </summary>
        public Endpoint(HostProperties properties) {
            Properties = properties;
            LocationLog = Utils.CombineExePath(Properties.Hostname + ".log");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public BrowserType CheckBrowserCall(string[] args) {

            // Firefox
            // __Two__ arguments are passed to the native app when it starts:
            //      - the complete path to the app manifest
            //      - (new in Firefox 55) the ID(as given in the browser_specific_settings manifest.json key) of the add - on that started it.
            // Chrome
            // On Linux and Mac, 
            //      Chrome passes one argument, the origin of the extension that started it in the form: 
            //      chrome - extension://[extensionID]. This enables the app to identify the extension.
            // On Windows, 
            //      Chrome passes __two__ arguments: the first is the origin of the extension, 
            //      and the second is a handle to the Chrome native window that started the app.

            // So we are on window :) and we don't want browser below FF50
            if (args.Length != 2) {
                return BrowserType.Unknown;
            }

            // Is it Firefox ?
            if (Properties.HasExtension(args[1])) {
                return BrowserType.Firefox;
            }

            // Is it Chrome ?
            if (Properties.HasOrigin(args[0])) {
                return BrowserType.Chrome;
            }

            // Nope !
            return BrowserType.Unknown;
        }

        /// <summary>
        /// Lod all compatibles handler defined in holder. To be compatible, an handler 
        /// must be :
        ///     - annotated with [RequestMapping("name")], 
        ///     - public and static
        ///     - compatble withe the delegate Messages.Response Handler(JToken message)
        /// </summary>
        /// <param name="holder"></param>
        public void LoadHandlers(Type holder) {

            MethodInfo[] methodInfos = holder.GetMethods(BindingFlags.Public | BindingFlags.Static);

            var it = methodInfos.GetEnumerator();
            while (it.MoveNext()) {
                MethodInfo method = (MethodInfo)it.Current;

                if(Delegate.CreateDelegate(typeof(Handler), method, false) != null) {

                    if(method.GetCustomAttribute<RequestMapping>() == null) {
                        Log.Debug("Ignore unmapped handler {0} :: {1} ", holder.Name, method.Name);
                        continue;
                    }

                    string route = method.GetCustomAttribute<RequestMapping>().Route.Trim();

                    if(route.Length == 0) {
                        Log.Debug("Ignore handler(Wrong route) {0} :: {1} ", holder.Name, method.Name);
                        continue;
                    }

                    Log.Debug("Find new handler in {0} :: {1} mapped to {2}", holder.Name, method.Name, route);
                    Handlers.Add(route, (Handler)method.CreateDelegate(typeof(Handler)));

                } else {
                    Log.Debug("Ignore handler(incompatible) {0} :: {1} ", holder.Name, method.Name);
                }
                    

            }

        }

        /// <summary>
        /// Sets up everything and starts listenning for request
        /// </summary>
        /// <param name="type"></param>
        public void Listen(BrowserType type) {
            Setup(type);

            Log.Debug("Endpoint enter listenning mode for {0} ...", browserType);
            state = EndpointState.Listening;

            requestQueueWorker.Start();
            responseQueueWorker.Start();
            mainWorker.Start();

            // All workers started, waiting here for stopSignal
            stopSignal.WaitOne();

            Log.Debug("Endpoint exit listenning mode ...");
            state = EndpointState.ExitPending;

            Teardown();
            state = EndpointState.Idle;

        }

        /// <summary>
        /// In any case, signals in main thread that an exit is requested
        /// Enters in ExitPending state and signals to the main worker
        /// that an exit is required
        /// </summary>
        public void Stop(string reason) {
            Log.Debug("Endpoint will exist because '{0}'", reason);

            if (state == EndpointState.Listening) {
                state = EndpointState.ExitPending;
                stopSignal.Set();
            } else if(state == EndpointState.Idle) {
                Environment.Exit(0);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        public void Test(Messages.Request request) {

            state = EndpointState.Listening;

            // All exceptions until a valid request was read
            // causes a message to be sent 
            try {
                HandleRequest(request);
            } catch (Exception e) {
                SendException(e);
            }

            state = EndpointState.Idle;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        private void Setup(BrowserType type) {

            if (state != EndpointState.Idle) {
                throw new EndpointException("Call Endpoint::Setup from an incompatible state : " + state.ToString());
            }

            Log.Debug("Endpoint Setting up ...");
            state = EndpointState.Setup;

            browserType = type;

            stopSignal = new ManualResetEvent(false);
            stopSignal.Reset();

            requestQueueWorker = new Thread(RequestQueueJob) {
                IsBackground = false,
                Name = "Endpoint::requestQueueWorker"
            };

            responseQueueWorker = new Thread(ResponseQueueJob) {
                IsBackground = false,
                Name = "Endpoint::responseQueueWorker"
            };

            mainWorker = new Thread(MainJob) {
                IsBackground = false,
                Name = "Endpoint::mainWorker"
            };

        }

        /// <summary>
        /// 
        /// </summary>
        private void Teardown() {

            if (state != EndpointState.ExitPending) {
                throw new EndpointException("Call Endpoint::Teardown from an incompatible state : " + state.ToString());
            }

            if(state == EndpointState.Teardown) {
                return;
            }

            Log.Debug("Endpoint Tearing down ...");
            state = EndpointState.Teardown;

            stopSignal.Close();
            stopSignal = null;

            try {

                if (mainWorker.IsAlive) {
                    mainWorker.Abort();
                }

                if (requestQueueWorker.IsAlive) {
                    requestQueueWorker.Abort();
                }

                if (responseQueueWorker.IsAlive) {
                    responseQueueWorker.Abort();
                }

            } finally {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Wait for input on stdin and try to parse the request. 
        /// Firefox and Chrome use the same protocol
        /// @see Firefox https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/Native_messaging#Exchanging_messages
        /// @see Chrome https://developer.chrome.com/apps/nativeMessaging#native-messaging-host-protocol
        /// </summary>
        /// <returns>The request parsed</returns>
        private Messages.Request WaitForMessage(){
            Log.Debug("Waiting for message ...");
            using var stdin = Console.OpenStandardInput();

            // read message length, thread is blocked until data
            var lengthBytes = new byte[4];
            stdin.Read(lengthBytes, 0, 4);

            // State has changed ?
            if(state == EndpointState.ExitPending) {
                Log.Debug("D'oh! State has changed !");
                return null;
            }

            int bufferSize = BitConverter.ToInt32(lengthBytes, 0);

            // Prevent exception when reading wrong length input
            if(bufferSize >= Messages.Request.BUFFER_SIZE_MAX) {
                throw new BadNativeMessagingProtocol( String.Format(
                    "D'oh! Message with declared length of {0} bytes exceed BUFFER_SIZE_MAX {1} bytes", 
                    bufferSize, 
                    Messages.Request.BUFFER_SIZE_MAX
                ));
            } else if (bufferSize <= 0) {

                // !NOT SURE! When SIGTERM was sent, bufferSize has a 0-length
                if (browserType == BrowserType.Chrome) {
                    Stop("Chrome did it !");
                    return null;

                } else if (browserType == BrowserType.Firefox) {
                    Stop("Firefox did it !");
                    return null;

                } else {
                    Stop("Message with declared length of 0 byte ! SIGTERM ?");
                    return null;
                }
            }

            // ... then read the message
            char[] buffer = new char[bufferSize];
            int read = 0;

            using (var reader = new StreamReader(stdin)) {
                while (reader.Peek() >= 0) {
                    read += reader.Read(buffer, 0, buffer.Length);
                }
            }

            if(read != bufferSize) {
                throw new BadNativeMessagingProtocol(String.Format(
                    "D'oh! Invalid length of request ! Expected {0} bytes, read {1} bytes",
                    bufferSize,
                    read
                ));
            }

            // ... and return request
            return ParseRequest(new string(buffer));
        }

        /// <summary>
        /// Tries to convert a json string to a Messages.Request object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private Messages.Request ParseRequest(string json) {
            Messages.Request request;

            try {
                request = JsonConvert.DeserializeObject<Messages.Request>(json);
            } catch (Exception e) {
                throw new BadNativeMessagingProtocol(String.Format("D'oh! Invalid request format {0} : {1}", e.Message, json));
            }

            return request;
        }

        /// <summary>
        /// Handles a request. if that request is async, enqueues in the request queue for later treatment
        /// otherwise process the request immediatly
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private void HandleRequest(Messages.Request request) {

            if (request == null) {
                Log.Debug("No route : Empty Request body");
                throw new BadNativeMessagingProtocol("D'oh! Invalid request format : Empty request body");
            }

            if (request.Async) {
                Log.Debug("Async-Request received: {0}", request.ToString());
                requestQueue.Enqueue(request);
            } else {
                Log.Debug("Sync-Request received: {0}", request.ToString());

                processRequestMutex.WaitOne();
                Messages.Response response = ProcessRequest(request);
                SendMessage(response);
                processRequestMutex.ReleaseMutex();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void MainJob() {

            // All exceptions until a valid request was read
            // causes a message to be sent 
            while (true) {

                try {

                    Messages.Request request = WaitForMessage();

                    // State has changed ?
                    if (state != EndpointState.Listening) {
                        break; ;
                    }

                    HandleRequest(request);

                } catch (Exception e) {
                    SendException(e);
                    continue;
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RequestQueueJob() {
            while (true) {
                if (!requestQueue.IsEmpty && requestQueue.TryDequeue(out var request)) {
                    Task.Factory.StartNew(ActionForRequest, request);
                }

                if (requestQueue.IsEmpty) {
                    Thread.Sleep(10);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_request"></param>
        private void ActionForRequest(object _request) {
            Messages.Request request = _request as Messages.Request;

            if(request.Delay > 0) {
                Thread.Sleep(request.Delay);
            }

            Messages.Response response = ProcessRequest(request);
            responseQueue.Enqueue(response);
        }

        /// <summary>
        /// 
        /// </summary>
        private void ResponseQueueJob() {

            while (true) {

                if (!responseQueue.IsEmpty) {

                    // Waiting here if a synchronous request exists
                    processRequestMutex.WaitOne();

                    while (!responseQueue.IsEmpty) {

                        if (responseQueue.TryDequeue(out var response)) {
                            SendMessage(response);
                        }

                    }

                    processRequestMutex.ReleaseMutex();
                }

                Thread.Sleep(10);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Messages.Response ProcessRequest(Messages.Request request) {

            Messages.Response response = null;
            if (Handlers.ContainsKey(request.Name)) {
                Log.Debug("Route request to {0} ", request.Name);

                try {
                    Handlers.TryGetValue(request.Name, out Handler handler);
                    response = handler.Invoke(request);

                } catch (Exception e) {
                    response = request.Failure(e.GetType().Name, e.Message);
                }

            } else {
                Log.Debug("No handler defined for {0}", request.Name);
                response = request.Unknown(String.Format("Unknown request {0}", request.Name));
            }

            return response;

        }

        /// <summary>
        /// Convenient method to send a response with an exception
        /// </summary>
        /// <param name="e"></param>
        private void SendException(Exception e) {
            Log.Debug(e.GetType().Name + " : " + e.Message);
            Messages.Response response = new Messages.Failure(e.GetType().Name, e.Message);
            SendMessage(response);
        }

        /// <summary>
        /// Sends a message to the browser, note that the message might not be able to reach  if the stdIn / stdOut 
        /// aren't properly configured (i.e. Process needs to be started)
        /// </summary>
        /// <param name="response"></param>
        private void SendMessage(Messages.Response response) {

            if (state == EndpointState.Listening) {

                writeMessageMutex.WaitOne();

                try {

                    if (TestMode) {
                        WriteDebugMessage(response);
                    } else {
                        WriteMessage(response);
                    }

                } catch(Exception e) {
                    Log.Information("{0} during SendMessage : {1}", e.GetType(), e.Message);
                } finally {
                    writeMessageMutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Write response to stdout using native message protocol, message lentgh first int32 and then data
        /// Firefox and Chrome use the same protocol
        /// @see Firefox https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/Native_messaging#Exchanging_messages
        /// @see Chrome https://developer.chrome.com/apps/nativeMessaging#native-messaging-host-protocol
        /// </summary>
        /// <param name="response"></param>
        private void WriteMessage(Messages.Response response) {
            string data = JsonConvert.SerializeObject(response, Formatting.None);
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);

            Log.Debug("Sending Message: {0}", data);

            using var stdout = Console.OpenStandardOutput();
            stdout.WriteByte((byte)((bytes.Length >> 0) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 8) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 16) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 24) & 0xFF));
            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
        }

        /// <summary>
        /// Write response on stdout by serializing response using json format.
        /// Used to debugging purpose.
        /// </summary>
        /// <param name="data"></param>
        private void WriteDebugMessage(Messages.Response response) {
            Log.Debug("Sending debug message: {0}", response.ToString());
            using var stdout = Console.OpenStandardOutput();
            stdout.Write(System.Text.Encoding.UTF8.GetBytes(response.ToString()));
        }

    }

}
