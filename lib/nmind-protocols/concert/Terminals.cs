//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

/// <summary>
/// 
/// </summary>
namespace Nmind.Protocols.Concert {

    /// <summary>
    /// 
    /// </summary>
    public abstract class Terminal {

        /// <summary>
        /// 
        /// </summary>
        abstract public void Connect();

        /// <summary>
        /// 
        /// </summary>
        abstract public void Disconnect();

        /// <summary>
        /// 
        /// </summary>
        abstract public bool IsConnected();

        /// <summary>
        /// 
        /// </summary>
        public void CleanBuffer() {
            ReadAll();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract bool WaitData();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public abstract bool WaitData(long timeout);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        abstract public byte ReadOne();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        abstract public byte[] ReadAll();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        public void Write(byte b) {
            Write(new byte[1] { b });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        abstract public void Write(byte[] buffer);

    }

    /// <summary>
    /// 
    /// </summary>
    public class SerialTerminal : Terminal {

        /// <summary>
        /// 
        /// </summary>
        protected SerialPort port;

        /// <summary>
        /// 
        /// </summary>
        protected string portName = "";

        /// <summary>
        /// 
        /// </summary>
        protected int baudRate = 9600;

        /// <summary>
        /// 
        /// </summary>
        protected Parity parity = Parity.Even;

        /// <summary>
        /// 
        /// </summary>
        protected int databits = 7;

        /// <summary>
        /// 
        /// </summary>
        protected StopBits stopbits = StopBits.One;

        /// <summary>
        /// 
        /// </summary>
        protected Handshake handshake = Handshake.None;

        /// <summary>
        /// 
        /// </summary>
        protected int timeout = 5000;

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; } = "default";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portName"></param>
        public SerialTerminal(string portName) {
            this.portName = portName;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsConnected() {
            return port != null && !port.IsOpen;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Connect() {
            Disconnect();

            port = new SerialPort(portName, baudRate, parity, databits, stopbits) {
                ReadTimeout = timeout,
                WriteTimeout = timeout
            };

            port.Open();
            CleanBuffer();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Disconnect() {
            if (IsConnected()) {
                port.Close();
                port.Dispose();
                port = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool WaitData() {
            return WaitData(timeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public override bool WaitData(long timeout) {
            long count = 0;
            int tick = 10;

            while(count < timeout) {
                count += tick;
                Thread.Sleep(tick);

                if(port.BytesToRead > 0) {
                    break;
                }
            }
        
            return port.BytesToRead > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override byte ReadOne() {
            return (byte)port.ReadByte();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override byte[] ReadAll() {
            List<byte> buffer = new List<byte>();

            while (port.BytesToRead > 0) {
                buffer.Add((byte)port.ReadByte());
            }

            return buffer.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        public override void Write(byte[] buffer) {
            port.Write(buffer, 0, buffer.Length);
        }

    }

}
