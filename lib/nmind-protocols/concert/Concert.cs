//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using Newtonsoft.Json;
using System;

/// <summary>
/// 
/// </summary>
namespace Nmind.Protocols.Concert {

    /// <summary>
    /// 
    /// </summary>
    public class Issue {

        /// <summary>
        /// 
        /// </summary>
        public const string SUCCESS = "SUCCESS";

        /// <summary>
        /// 
        /// </summary>
        public const string FAILURE = "FAILURE";

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; } = SUCCESS;

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public PaymentResponse Response { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        protected Issue(string type, string message, PaymentResponse response) {
            Type = type;
            Message = message;
            Response = response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Issue Success(string message) {
            return new Issue(SUCCESS, message, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Issue Success(string message, PaymentResponse response) {
            return new Issue(SUCCESS, message, response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Issue Failure(string message) {
            return new Issue(FAILURE, message, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Issue Failure(string message, PaymentResponse response) {
            return new Issue(FAILURE, message, response);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Message {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("pos")]
        public int Pos { get; set; } = 88;

        /// <summary>
        /// 
        /// </summary>
        /// 
        [JsonProperty("amount")]
        public float Amount { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// 
        [JsonIgnore]
        public char Mode { get; set; } = ConcertProtocol.MODE_CARD;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("currency")]
        public int Currency { get; set; } = ConcertProtocol.CURRENCY_EURO;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("data")]
        public String Data { get; set; } = "";

    }

    /// <summary>
    /// 
    /// </summary>
    public class PaymentRequest : Message {

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public char Ind { get; set; } = ConcertProtocol.IND_RESPONSE;

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public byte Type { get; set; } = ConcertProtocol.TRANSACTION_TYPE_PURCHASE;

        /// <summary>
        /// 
        /// </summary>
        public PaymentRequest(float amount) {
            Amount = amount;
        }

        /// <summary>
        /// 
        /// </summary>
        public PaymentRequest(float amount, int pos, string data) {
            Amount = amount;
            Data = data;
            Pos = pos;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class PaymentResponse : Message {

        /// <summary>
        /// 
        /// </summary>
        /// 
        [JsonProperty("status")]
        public char Status { get; set; } = ConcertProtocol.STATUS_FAILED;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public PaymentResponse() {
  
        }

    }

}
