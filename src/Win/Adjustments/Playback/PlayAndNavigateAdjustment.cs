// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Adjustments.Playback
{
    using System;

    using Adjustments;

    internal class PlayAndNavigateAdjustment : SpotifyAdjustment
    {
        public PlayAndNavigateAdjustment()
            : base(
                "Play And Navigate Tracks(s)",
                "Play And Navigate Tracks(s) description",
                "Playback",
                true) { }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            if (ticks > 0)
            {
                this.Wrapper.SkipPlaybackToNext();
            }
            else
            {
                this.Wrapper.SkipPlaybackToPrevious();
            }
        }

        // Overwrite the RunCommand method that is called every time the user presses the encoder to which this command is assigned
        protected override void RunCommand(String actionParameter)
        {
            this.Wrapper.TogglePlayback();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            // Dial strip 50px
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width50.PlayAndNavigateTracks.png");
            return bitmapImage;
        }
    }
}