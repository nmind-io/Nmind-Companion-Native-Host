//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.IO.Ports;

/// <summary>
/// 
/// </summary>
namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    public class DefaultHandlers {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.ping")]
        public static Messages.Response Ping(Messages.Request request) {
            return request.Success("companion-pong");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.echo")]
        public static Messages.Response Echo(Messages.Request request) {
            return request.Success("Companion-echo : " + request.Parameters.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.version")]
        public static Messages.Response CompanionVersion(Messages.Request request) {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            return request.Success(version.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.pause")]
        public static Messages.Response CompanionPause(Messages.Request request) {
            Thread.Sleep(5000);
            return request.Success("Had a nice pause !");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.exit")]
        public static Messages.Response CompanionExit(Messages.Request request) {
            NativeHost.StopService("Receveid request companion.exit");
            return request.Success(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.serialPorts.list")]
        public static Messages.Response SerialPortsList(Messages.Request request) {
            return request.Success(SerialPort.GetPortNames());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.location.open")]
        public static Messages.Response LocationOpen(Messages.Request request) {
            string filepath = request.Parameters.First.ToObject<string>();

            ProcessStartInfo info = new ProcessStartInfo {
                Verb = "open",
                FileName = filepath,
                CreateNoWindow = false,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };

            using Process process = new Process() {
                StartInfo = info
            };
            process.Start();

            return request.Success(filepath);
        }

    }
}
