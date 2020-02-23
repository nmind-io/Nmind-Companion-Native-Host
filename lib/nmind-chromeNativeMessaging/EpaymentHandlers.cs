//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using Nmind.Protocols.Concert;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    public class EpaymentHandlers {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.epayment.supportedDevices")]
        public static Messages.Response EpaymentSupportedTerminals(Messages.Request request) {
            Dictionary<string, string> map = new Dictionary<string, string>();
            map.Add("Ingenico-ICT220", "Ingenico ICT 220");
            map.Add("Ingenico-ICT250", "Ingenico ICT 250");
            return request.Success(map);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.epayment.supportedProtocols")]
        public static Messages.Response EpaymentSupportedProtocols(Messages.Request request) {
            return request.Success(ConcertProtocol.SupportedProtocols());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.epayment.process")]
        public static Messages.Response EpaymentProcess(Messages.Request request) {
            float amount = request.Parameters.Value<float>("amount");
            string device = request.Parameters.Value<string>("device");
            string protocol = request.Parameters.Value<string>("protocol");
            string port = request.Parameters.Value<string>("port");
            string data = request.Parameters.Value<string>("data");
            int pos = request.Parameters.Value<int>("pos");

            PaymentRequest paymentRequest = new PaymentRequest(amount);

            ConcertProtocol concertProtocol = ConcertProtocol.find(protocol);
            concertProtocol.Terminal = new SerialTerminal(port);

            Issue issue = concertProtocol.SendPaymentRequest(paymentRequest);

            if(issue.Type == Issue.SUCCESS) {
                return request.Success(issue.Response);
            } else {
                return request.Failure("", issue.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.epayment.ping")]
        public static Messages.Response EpaymentPing(Messages.Request request) {
            string device = request.Parameters.Value<string>("device");
            string port = request.Parameters.Value<string>("port");
            string protocol = request.Parameters.Value<string>("protocol");

            ConcertProtocol concertProtocol = ConcertProtocol.find(protocol);
            concertProtocol.Terminal = new SerialTerminal(port);
            concertProtocol.SendTest();

            return request.Success(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [RequestMapping("companion.epayment.test")]
        public static Messages.Response EpaymentTest(Messages.Request request) {

            Concert2Protocol protocol = new Concert2Protocol(new SerialTerminal(""));

            string payload = request.Parameters.Value<string>("payload");
            payload += ConcertProtocol.FromCharCode(ConcertProtocol.ETX).ToString();

            string message = ConcertProtocol.FromCharCode(ConcertProtocol.STX);
            message += payload;
            message += ConcertProtocol.FromCharCode(ConcertProtocol.LRC(payload));

            protocol.PrepareResponse(ConcertProtocol.ToBytes(message));

            return request.Success("OK");
        }

    }
}
