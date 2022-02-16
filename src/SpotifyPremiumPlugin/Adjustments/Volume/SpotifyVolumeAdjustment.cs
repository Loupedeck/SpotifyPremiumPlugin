// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    internal class SpotifyVolumeAdjustment : PluginDynamicAdjustment
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public SpotifyVolumeAdjustment()
            : base("Spotify Volume", "Spotify Volume description", "Spotify Volume", true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks) => this.SpotifyPremiumPlugin.Wrapper.SetVolume(ticks);

        // Overwrite the RunCommand method that is called every time the user presses the encoder to which this command is assigned
        protected override void RunCommand(String actionParameter) => this.SpotifyPremiumPlugin.Wrapper.TogglePlayback();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            // Dial strip 50px
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width50.Volume.png");
            return bitmapImage;
        }
    }
}
