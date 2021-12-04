// Copyright (c) Loupedeck. All rights reserved.

#region Usings

using System;
using System.IO;

#endregion

namespace Loupedeck.Plugins.SpotifyPremium
{
    /// <summary>
    /// Plugin main class - Loupedeck device commands and adjustment
    /// </summary>
    internal class SpotifyPremiumPlugin : Plugin
    {
        // This plugin has Spotify API -only actions.
        public override bool UsesApplicationApiOnly => true;

        // This plugin does not require an application (i.e. Spotify application installed on pc).
        public override bool HasNoApplication => true;

        private SpotifyWrapper wrapper;

        internal SpotifyWrapper Wrapper => wrapper ?? (wrapper = new SpotifyWrapper(this));

        public override void Load()
        {
            LoadPluginIcons();
        }

        public override void Unload() { }

        public override void RunCommand(string commandName, string parameter) { }

        public override void ApplyAdjustment(string adjustmentName, string parameter, int diff) { }

        private void LoadPluginIcons()
        {
            // Icons for Loupedeck application UI
            Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon16x16.png");
            Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon32x32.png");
            Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon48x48.png");
            Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon256x256.png");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                wrapper?.Dispose();
                wrapper = null;
            }

            base.Dispose(disposing);
        }

        #region Installer

        internal string ClientConfigurationFilePath => Path.Combine(GetPluginDataDirectory(), "spotify-client.txt");

        public override bool Install()
        {
            // Here we ensure the plugin data directory is there.
            // See Storing-Plugin-Data
            string pluginDataDirectory = GetPluginDataDirectory();

            if (!IoHelpers.EnsureDirectoryExists(pluginDataDirectory))
            {
                Tracer.Error("Plugin data is not created. Cannot continue installation");
                return false;
            }

            // Now we put a template configuration file from resources
            string filePath = Path.Combine(pluginDataDirectory, ClientConfigurationFilePath);

            using (new StreamWriter(filePath))
            {
                // Write data
                Assembly.ExtractFile("spotify-client-template.txt", ClientConfigurationFilePath);
            }

            return true;
        }

        #endregion Installer

        #region DevicesSelector

        private readonly string _deviceCacheFileName = "CachedDevice.txt";

        private readonly object _locker = new object();

        private string GetCacheFilePath(string fileName)
        {
            return Path.Combine(GetPluginDataDirectory(), "Cache", fileName);
        }

        internal void SaveDeviceToCache(string deviceId)
        {
            string cacheDirectory = Path.Combine(GetPluginDataDirectory(), "Cache");

            if (!Directory.Exists(cacheDirectory))
            {
                try
                {
                    Directory.CreateDirectory(cacheDirectory);
                }
                catch (Exception ex)
                {
                    Tracer.Warning("Cannot create cache directory", ex);
                }
            }

            string cacheFilePath = GetCacheFilePath(_deviceCacheFileName);

            lock (_locker)
            {
                File.WriteAllText(cacheFilePath, deviceId);
            }
        }

        internal string GetCachedDeviceID()
        {
            try
            {
                string cacheFilePath = GetCacheFilePath(_deviceCacheFileName);
                string cachedActions = File.ReadAllText(cacheFilePath);

                if (!string.IsNullOrEmpty(cachedActions))
                {
                    return cachedActions;
                }
            }
            catch (Exception ex)
            {
                Tracer.Warning(ex, "Spotify: error during reading cached deviceID");
            }

            return string.Empty;
        }

        #endregion DevicesSelector
    }
}