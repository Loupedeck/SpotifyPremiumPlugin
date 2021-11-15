// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using Loupedeck;

    /// <summary>
    /// Plugin main class - Loupedeck device commands and adjustment
    /// </summary>
    public partial class SpotifyPremiumPlugin : Plugin
    {
        // This plugin has Spotify API -only actions.
        public override Boolean UsesApplicationApiOnly => true;

        // This plugin does not require an application (i.e. Spotify application installed on pc).
        public override Boolean HasNoApplication => true;

        public override void Load()
        {
            this.LoadPluginIcons();

            // Set everything ready and connect to Spotify API
            this.SpotifyConfiguration();

            // Get current (active) device id from internal cache
            this.CurrentDeviceId = this.GetCachedDeviceID();
        }

        public override void Unload()
        {
        }

        public override void RunCommand(String commandName, String parameter)
        {
        }

        public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff)
        {
        }

        private void LoadPluginIcons()
        {
            // Icons for Loupedeck application UI
            this.Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon16x16.png");
            this.Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon32x32.png");
            this.Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon48x48.png");
            this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.PluginIcon256x256.png");
        }
    }
}
