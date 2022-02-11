// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    
    internal class PlayAndNavigateAdjustment : PluginDynamicAdjustment
    {
        public PlayAndNavigateAdjustment()
            : base(
                  "Play And Navigate Tracks(s)",
                  "Play And Navigate Tracks(s) description",
                  "Playback",
                  true)
        {
        }

        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            try
            {
                if (ticks > 0)
                {
                    this.SpotifyPremiumPlugin.Wrapper.SkipPlaybackToNext();
                }
                else
                {
                    this.SpotifyPremiumPlugin.Wrapper.SkipPlaybackToPrevious();
                }
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify ApplyAdjustment action obtain an error: ", e);
            }
        }

        // Overwrite the RunCommand method that is called every time the user presses the encoder to which this command is assigned
        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.Wrapper.TogglePlayback();
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify ApplyAdjustment action obtain an error: ", e);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            // Dial strip 50px
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width50.PlayAndNavigateTracks.png");
            return bitmapImage;
        }
    }
}
