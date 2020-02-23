//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using Newtonsoft.Json;

/// <summary>
/// 
/// </summary>
namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    abstract public class Manifest {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("path")]
        public string ExecuteablePath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("type")]
        public string Type { get { return "stdio"; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="description"></param>
        /// <param name="executeablePath"></param>
        public Manifest(string hostname, string description, string executeablePath) {
            Name = hostname;
            Description = description;
            ExecuteablePath = executeablePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string AsJson() {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class FirefoxManifest : Manifest {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("allowed_extensions")]
        public string[] AllowedExtensions { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="description"></param>
        /// <param name="executeablePath"></param>
        /// <param name="allowedExtensions"></param>
        public FirefoxManifest(string hostname, string description, string executeablePath, string[] allowedExtensions) : base(hostname, description, executeablePath) {
            AllowedExtensions = allowedExtensions;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class ChromeManifest : Manifest {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("allowed_origins")]
        public string[] AllowedOrigins { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="description"></param>
        /// <param name="executeablePath"></param>
        /// <param name="allowedOrigins"></param>
        public ChromeManifest(string hostname, string description, string executeablePath, string[] allowedOrigins) : base(hostname, description, executeablePath) {
            AllowedOrigins = allowedOrigins;
        }
    }

}
