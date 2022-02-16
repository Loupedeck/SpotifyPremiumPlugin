// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.IO;

    public partial class SpotifyPremiumPlugin : Plugin
    {
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
            var filePath = SpotifyWrapper.GetClientConfigurationFilePath(pluginDataDirectory);
            using (var streamWriter = new StreamWriter(filePath))
            {
                // Write data
                this.Assembly.ExtractFile("spotify-client-template.txt", filePath);
            }

            return true;
        }
    }
}
