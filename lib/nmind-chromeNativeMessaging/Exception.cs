//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using System;

/// <summary>
/// 
/// </summary>
namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class NotRegisteredException : Exception {

        public NotRegisteredException() {
        }

        public NotRegisteredException(string message) : base(message) {
        }

        public NotRegisteredException(string message, Exception inner) : base(message, inner) {
        }

        protected NotRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class NotRecognizedParameter : Exception {

        public NotRecognizedParameter() {
        }

        public NotRecognizedParameter(string message) : base(message) {
        }

        public NotRecognizedParameter(string message, Exception inner) : base(message, inner) {
        }

        protected NotRecognizedParameter(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class BadNativeMessagingProtocol : Exception {

        public BadNativeMessagingProtocol() {
        }

        public BadNativeMessagingProtocol(string message) : base(message) {
        }

        public BadNativeMessagingProtocol(string message, Exception inner) : base(message, inner) {
        }

        protected BadNativeMessagingProtocol(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class EndpointException : Exception {

        public EndpointException() {
        }

        public EndpointException(string message) : base(message) {
        }

        public EndpointException(string message, Exception inner) : base(message, inner) {
        }

        protected EndpointException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
    }

}
