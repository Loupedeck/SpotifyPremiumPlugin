// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Adjustments.Volume
{
    using System;

    using Adjustments;

    internal class SpotifyVolumeAdjustment : SpotifyAdjustment
    {
        public SpotifyVolumeAdjustment()
            : base(
                "Spotify Volume",
                "Spotify Volume description",
                "Spotify Volume",
                true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            this.Wrapper.AdjustVolume(ticks);
        }

        // Overwrite the RunCommand method that is called every time the user presses the encoder to which this command is assigned
        protected override void RunCommand(String actionParameter)
        {
            this.Wrapper.TogglePlayback();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            // Dial strip 50px
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width50.Volume.png");
            return bitmapImage;
        }
    }
}
