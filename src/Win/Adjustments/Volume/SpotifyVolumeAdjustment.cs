// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Adjustments.Volume
{
    internal class SpotifyVolumeAdjustment : SpotifyAdjustment
    {
        public SpotifyVolumeAdjustment()
            : base(
                "Spotify Volume",
                "Spotify Volume description",
                "Spotify Volume",
                true) { }

        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
            Wrapper.AdjustVolume(ticks);
        }

        // Overwrite the RunCommand method that is called every time the user presses the encoder to which this command is assigned
        protected override void RunCommand(string actionParameter)
        {
            Wrapper.TogglePlayback();
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            // Dial strip 50px
            BitmapImage bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width50.Volume.png");
            return bitmapImage;
        }
    }
}