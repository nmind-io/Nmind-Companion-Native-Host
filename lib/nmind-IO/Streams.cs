//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 
/// </summary>
namespace Nmind.IO {

    /// <summary>
    /// 
    /// </summary>
    class ByteStream : MemoryStream {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public ByteStream(byte[] bytes) : base(bytes) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long BytesToRead() {
            return Length - Position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Available() {
            return Position < Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool AvailableFrom(long position, int count) {
            return position + count < Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Available(int count) {
            return Position + count < Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public byte ReadByteAt(long position) {
            Position = position;
            return (byte)ReadByte();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadBytes(long length) {
            return ReadBytesFrom(Position, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadLastBytes(long length) {
            return ReadBytesFrom(length * -1, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadBytesFrom(long position, long length) {
            if (position < 0) {
                Position = Length - position;
            } else {
                Position = position;
            }

            List<byte> bytes = new List<byte>();

            while (Available() && bytes.Count < length) {
                bytes.Add((byte)ReadByte());
            }

            while (bytes.Count < length) {
                bytes.Add(0);
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte ReadLastByte() {
            return ReadByteAt(Length - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte ReadFirstByte() {
            return ReadByteAt(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="end"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public byte[] ReadBytesUntil(byte end, bool include) {
            return ReadBytesUntil(Position, end, include);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="end"></param>
        /// <param name="include"></param>
        /// <param name="bytes"></param>
        public void ReadBytesUntil(byte end, bool include, List<byte> bytes) {
            ReadBytesUntil(Position, end, include, bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public byte[] ReadBytesUntil(byte end) {
            return ReadBytesUntil(Position, end, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="end"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public void ReadBytesUntil(byte end, List<byte> bytes) {
            ReadBytesUntil(Position, end, false, bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="end"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public byte[] ReadBytesUntil(long position, byte end, Boolean include) {
            List<byte> bytes = new List<byte>();
            ReadBytesUntil(position, end, include, bytes);
            return bytes.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="end"></param>
        /// <param name="include"></param>
        /// <param name="bytes"></param>
        public void ReadBytesUntil(long position, byte end, Boolean include, List<byte> bytes) {
            Position = position;
            bytes.Clear();

            while (Available()) {
                byte b = (byte)ReadByte();

                if (b == end) {
                    if (include) {
                        bytes.Add(b);
                    }
                    break;
                }

                bytes.Add(b);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string ReadString(long length) {
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public short ReadShort() {
            return BitConverter.ToInt16(ReadBytes(2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ReadInt() {
            return BitConverter.ToInt32(ReadBytes(4));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long ReadLong() {
            return BitConverter.ToInt64(ReadBytes(8));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public char ReadChar() {
            return (char)ReadByte();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public short ReadStringToShort(long length) {
            return Int16.Parse(ReadString(length));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public int ReadStringToInt(long length) {
            return Int32.Parse(ReadString(length));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public long ReadStringToLong(long length) {
            return Int64.Parse(ReadString(length));
        }

    }

}
