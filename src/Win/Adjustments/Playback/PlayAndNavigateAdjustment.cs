// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Adjustments.Playback
{
    internal class PlayAndNavigateAdjustment : SpotifyAdjustment
    {
        public PlayAndNavigateAdjustment()
            : base(
                "Play And Navigate Tracks(s)",
                "Play And Navigate Tracks(s) description",
                "Playback",
                true) { }

        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
            if (ticks > 0)
            {
                Wrapper.SkipPlaybackToNext();
            }
            else
            {
                Wrapper.SkipPlaybackToPrevious();
            }
        }

        // Overwrite the RunCommand method that is called every time the user presses the encoder to which this command is assigned
        protected override void RunCommand(string actionParameter)
        {
            Wrapper.TogglePlayback();
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            // Dial strip 50px
            BitmapImage bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width50.PlayAndNavigateTracks.png");
            return bitmapImage;
        }
    }
}