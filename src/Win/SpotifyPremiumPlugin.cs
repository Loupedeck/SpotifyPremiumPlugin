// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.IO;

    using Loupedeck;

    /// <summary>
    /// Plugin main class - Loupedeck device commands and adjustment
    /// </summary>
    internal class SpotifyPremiumPlugin : Plugin
    {
        // This plugin has Spotify API -only actions.
        public override Boolean UsesApplicationApiOnly => true;

        // This plugin does not require an application (i.e. Spotify application installed on pc).
        public override Boolean HasNoApplication => true;

        private SpotifyWrapper wrapper;

        internal SpotifyWrapper Wrapper => this.wrapper ?? (this.wrapper = new SpotifyWrapper(this));

        public override void Load()
        {
            this.LoadPluginIcons();
        }

        public override void Unload() { }

        public override void RunCommand(String commandName, String parameter) { }

        public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff) { }

        private void LoadPluginIcons()
        {
            // Icons for Loupedeck application UI
            this.Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon16x16.png");
            this.Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon32x32.png");
            this.Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon48x48.png");
            this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon256x256.png");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.wrapper?.Dispose();
                this.wrapper = null;
            }

            base.Dispose(disposing);
        }

        #region Installer

        internal String ClientConfigurationFilePath => Path.Combine(this.GetPluginDataDirectory(), "spotify-client.txt");

        public override Boolean Install()
        {
            // Here we ensure the plugin data directory is there.
            // See Storing-Plugin-Data
            var pluginDataDirectory = this.GetPluginDataDirectory();

            if (!IoHelpers.EnsureDirectoryExists(pluginDataDirectory))
            {
                Tracer.Error("Plugin data is not created. Cannot continue installation");
                return false;
            }

            // Now we put a template configuration file from resources
            var filePath = Path.Combine(pluginDataDirectory, this.ClientConfigurationFilePath);

            using (new StreamWriter(filePath))
            {
                // Write data
                this.Assembly.ExtractFile("spotify-client-template.txt", this.ClientConfigurationFilePath);
            }

            return true;
        }

        #endregion Installer

        #region DevicesSelector

        private readonly String _deviceCacheFileName = "CachedDevice.txt";

        private readonly Object _locker = new Object();

        private String GetCacheFilePath(String fileName) => Path.Combine(this.GetPluginDataDirectory(), "Cache", fileName);

        internal void SaveDeviceToCache(String deviceId)
        {
            var cacheDirectory = Path.Combine(this.GetPluginDataDirectory(), "Cache");

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

            var cacheFilePath = this.GetCacheFilePath(this._deviceCacheFileName);

            lock (this._locker)
            {
                File.WriteAllText(cacheFilePath, deviceId);
            }
        }

        internal String GetCachedDeviceID()
        {
            try
            {
                var cacheFilePath = this.GetCacheFilePath(this._deviceCacheFileName);
                var cachedActions = File.ReadAllText(cacheFilePath);

                if (!String.IsNullOrEmpty(cachedActions))
                {
                    return cachedActions;
                }
            }
            catch (Exception ex)
            {
                Tracer.Warning(ex, "Spotify: error during reading cached deviceID");
            }

            return String.Empty;
        }

        #endregion DevicesSelector
    }
}
