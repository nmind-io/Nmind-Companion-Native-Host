//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

/// <summary>
/// 
/// </summary>
namespace Nmind.NativeMessaging.Messages {

    /// <summary>
    /// 
    /// </summary>
    public class RequestBuilder {

        /// <summary>
        /// 
        /// </summary>
        private readonly Request request;

        /// <summary>
        /// 
        /// </summary>
        private readonly JObject parameters;

        /// <summary>
        /// 
        /// </summary>
        private JToken value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private RequestBuilder(string name) {
            request = new Request(name);
            parameters = new JObject();
            value = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private RequestBuilder(Request request) {
            this.request = request;

            if(request.Parameters == null) {
                parameters = new JObject();
            } else if(request.Parameters.HasValues) {
                parameters = request.Parameters as JObject;
                value = null;
            } else {
                parameters = new JObject();
                value = request.Parameters as JValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        static public RequestBuilder From(string json) {
            return new RequestBuilder(JsonConvert.DeserializeObject<Request>(json));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static public RequestBuilder Route(string name) {
            return new RequestBuilder(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RequestBuilder Name(string value) {
            request.Name = value;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RequestBuilder Async(bool value) {
            request.Async = value;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RequestBuilder Silent(bool value) {
            request.Silent = value;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RequestBuilder Id(string value) {
            request.Id = value;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RequestBuilder Delay(int value) {
            request.Delay = value;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public RequestBuilder With(string key, object value) {
            parameters.Add(key, JToken.FromObject(value));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RequestBuilder Value(object value) {
            this.value = JToken.FromObject(value);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Request Create() {

            if(parameters.Count > 0) {
                request.Parameters = parameters;
            } else if(value != null) {
                request.Parameters = value;
            } else {
                request.Parameters = JValue.CreateUndefined();
            }

            return request;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Request {

        /// <summary>
        /// 
        /// </summary>
        public const int BUFFER_SIZE_MAX = 1024 * 1024 * 2;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = "-1";

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("delay")]
        public int Delay { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("silent")]
        public bool Silent { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("async")]
        public bool Async { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("params")]
        public JToken Parameters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        public Request(string name, JToken parameters) {
            this.Name = name;
            this.Parameters = parameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public Request(string name) {
            this.Name = name;
            this.Parameters = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public Request() {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool HasParameters() {
            return Parameters != null
                && Parameters.Type != JTokenType.Undefined
                && Parameters.Type != JTokenType.Null
                && Parameters.Type != JTokenType.None
                && Parameters.Type != JTokenType.Comment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override public string ToString() {
            return String.Format(
                "{0} '{1}' ({2}) {4} Delay({5}) : {3}", 
                this.GetType().Name, 
                Name, 
                Id, 
                HasParameters() ? Parameters.ToString() : "--", 
                Silent ? "Silent" : "",
                Delay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Response Response(string code) {
            return new Response(code) {
                Refid = Id,
                Name = Name,
                Silent = Silent
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public Success Success(Object content) {
            return new Success(content) {
                Refid = Id,
                Name = Name,
                Silent = Silent
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Failure Failure(string type, string message) {
            return new Failure(type, message) {
                Refid = Id,
                Name = Name,
                Silent = Silent
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Failure Failure(string code, string type, string message) {
            return new Failure(code, type, message) {
                Refid = Id,
                Name = Name,
                Silent = Silent
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Error Error(string type, string message) {
            return new Error(type, message) {
                Refid = Id,
                Name = Name,
                Silent = Silent
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Unknown Unknown(string message) {
            return new Unknown(message) {
                Refid = Id,
                Name = Name,
                Silent = Silent
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        static public Request From(string json) {
            return JsonConvert.DeserializeObject<Request>(json);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Response {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("refid")]
        public string Refid { get; set; } = "-1";

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool Silent { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        public Response(string code) {
            this.Code = code;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override public string ToString() {
            return String.Format("{0} ({1}-{2}) : ___", this.GetType().FullName, Code, Silent);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Success : Response {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("content")]
        public Object Content { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public Success(Object content) : base("200") {
           Content = content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override public string ToString() {
            return String.Format("{0} ({1}) : {2}", this.GetType().FullName, Code, Content);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Failure : Response {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public Failure(string type, string message) : base("403") {
            Type = type;
            Message = message;
        }

        public Failure(string code, string type, string message) : base(code) {
            Type = type;
            Message = message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override public string ToString() {
            return String.Format("{0} ({1}-{2}) : {3}", this.GetType().FullName, Code, Message, Silent);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Error : Failure {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public Error(string type, string message) : base("500", type, message) {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Unknown : Failure {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public Unknown(string message) : base("404", "", message) {
        }
    }

}
