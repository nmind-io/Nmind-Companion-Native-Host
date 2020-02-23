//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using System.IO;
using Microsoft.Win32;
using Serilog;

namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    public sealed class RegistryService {

        public HostProperties Properties { get; }

        /// <summary>
        /// 
        /// </summary>
        private const string RegKeyChromeLocation = "SOFTWARE\\Google\\Chrome\\NativeMessagingHosts\\";

        /// <summary>
        /// 
        /// </summary>
        private const string RegKeyFirefoxLocation = "SOFTWARE\\Mozilla\\NativeMessagingHosts\\";

        /// <summary>
        /// Creates the Host Object
        /// </summary>
        public RegistryService(HostProperties properties) {
            Properties = properties;
        }

        #region Manifest

        /// <summary>
        /// Generates the manifest & saves it to the correct location.
        /// </summary>
        public void GenerateChromeManifest() {
            Log.Debug("Generating Chrome Manifest");
            File.WriteAllText(Properties.ChromeManifestPath, CreateChromeManifest().AsJson());
            Log.Debug("Manifest Generated");
        }

        /// <summary>
        /// Generates the manifest & saves it to the correct location.
        /// </summary>
        public void GenerateFirefoxManifest() {
            Log.Debug("Generating Firefox Manifest");
            File.WriteAllText(Properties.FirefoxManifestPath, CreateFirefoxManifest().AsJson());
            Log.Debug("Manifest Generated");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Manifest CreateFirefoxManifest() {
            return new FirefoxManifest(Properties.Hostname, Properties.Description, Utils.AssemblyExecuteablePath(), Properties.AllowedExtensions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Manifest CreateChromeManifest() {
            return new ChromeManifest(Properties.Hostname, Properties.Description, Utils.AssemblyExecuteablePath(), Properties.AllowedOrigins);
        }

        #endregion

        #region Registration with Chrome and Firefox

        /// <summary>
        /// Register the application to open with Chrome.
        /// </summary>
        public void RegisterWithChrome() {
            GenerateChromeManifest();
            Register(RegKeyChromeLocation + Properties.Hostname, Properties.ChromeManifestPath);
        }

        /// <summary>
        /// Register the application to open with Firefox.
        /// </summary>
        public void RegisterWithFirefox() {
            GenerateFirefoxManifest();
            Register(RegKeyFirefoxLocation + Properties.Hostname, Properties.FirefoxManifestPath);
        }

        /// <summary>
        /// Register the application
        /// </summary>
        private void Register(string regHostnameKeyLocation, string manifestPath) {

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(regHostnameKeyLocation, true);

            if (regKey == null) {
                regKey = Registry.CurrentUser.CreateSubKey(regHostnameKeyLocation);
            }

            regKey.SetValue("", manifestPath, RegistryValueKind.String);
            regKey.Close();

            Log.Debug("Registered:" + Properties.Hostname);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsRegisteredWithChrome() {
            return IsRegistered(RegKeyChromeLocation + Properties.Hostname, Properties.ChromeManifestPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsRegisteredWithFirefox() {
            return IsRegistered(RegKeyFirefoxLocation + Properties.Hostname, Properties.FirefoxManifestPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regHostnameKeyLocation"></param>
        /// <returns></returns>
        private bool IsRegistered(string regHostnameKeyLocation, string manifestPath) {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(regHostnameKeyLocation, true);

            if (regKey != null && regKey.GetValue("").ToString() == manifestPath) {
                return true;
            } else {
                return false;
            }

        }

        /// <summary>
        /// Unregister the application to open with Chrome.
        /// </summary>
        public void UnregisterWithChrome() {
            Unregister(RegKeyChromeLocation + Properties.Hostname);
        }

        /// <summary>
        /// Unregister the application to open with Chrome.
        /// </summary>
        public void UnregisterWithFirefox() {
            Unregister(RegKeyFirefoxLocation + Properties.Hostname);
        }

        /// <summary>
        /// Unregister the application
        /// </summary>
        private void Unregister(string regHostnameKeyLocation) {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(regHostnameKeyLocation, true);
            if (regKey != null) {
                regKey.DeleteSubKey("", true);
                regKey.Close();

                Log.Debug("Unregistered:" + Properties.Hostname);
            }
        }

        #endregion

    }

}
