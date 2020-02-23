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
    public sealed class HostProperties {

        /// <summary>
        /// 
        /// </summary>gjhgjhgjjhkaa
        public string Hostname { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 
        /// </summary>
        public string[] AllowedOrigins { get; }

        /// <summary>
        /// 
        /// </summary>
        public string[] AllowedExtensions { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ChromeManifestPath { get; }

        /// <summary>
        /// 
        /// </summary>
        public string FirefoxManifestPath { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public HostProperties(string hostname, string description, string[] origins, string[] extensions) {

            for (int i = 0; i < origins.Length; i++) {
                origins.SetValue(String.Format("chrome-extension://{0}/", origins[i]), i);
            }

            Hostname = hostname;
            Description = description;
            AllowedExtensions = extensions;
            AllowedOrigins = origins;

            ChromeManifestPath = Utils.CombineExePath(Hostname + "-chrome-manifest.json");
            FirefoxManifestPath = Utils.CombineExePath(Hostname + "-firefox-manifest.json");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public bool HasOrigin(string origin) {

            foreach(string allowed in AllowedOrigins) {
                if(String.Compare(allowed, origin, true) == 0) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public bool HasExtension(string extension) {

            foreach (string allowed in AllowedExtensions) {
                if (String.Compare(allowed, extension, true) == 0) {
                    return true;
                }
            }

            return false;
        }
    }

}
