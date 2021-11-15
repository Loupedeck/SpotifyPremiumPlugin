// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.IO;

    /// <summary>
    /// Plugin: Store Spotify devices locally
    /// </summary>
    public partial class SpotifyPremiumPlugin : Plugin
    {
        private readonly String _deviceCacheFileName = "CachedDevice.txt";

        private readonly Object _locker = new Object();

        private String GetCacheFilePath(String fileName) => Path.Combine(this.GetPluginDataDirectory(), "Cache", fileName);

        public void SaveDeviceToCache(String deviceId)
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

        public String GetCachedDeviceID()
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
    }
}
