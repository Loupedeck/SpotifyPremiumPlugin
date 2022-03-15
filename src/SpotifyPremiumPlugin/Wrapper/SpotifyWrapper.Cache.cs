namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.IO;

    public partial class SpotifyWrapper
    {
        private readonly String _cacheDirectory;

        private readonly String _deviceCacheFileName = "CachedDevice";

        private readonly Object _locker = new Object();

        public static String GetClientConfigurationFilePath(String cacheDirectory) => Path.Combine(cacheDirectory, ClientConfigurationFileName);

        public static String ClientConfigurationFileName => "spotify-client.yml";

        public String ClientConfigurationFilePath => Path.Combine(this._cacheDirectory, ClientConfigurationFileName);

        private String GetCacheFilePath(String fileName) => Path.Combine(this._cacheDirectory, fileName);

        public void SaveDeviceToCache(String deviceId)
        {
            var cacheDirectory = this._cacheDirectory;
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
