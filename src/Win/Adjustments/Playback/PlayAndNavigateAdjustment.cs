// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web.Models;

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
                    this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.SkipPlaybackToNext);
                }
                else
                {
                    this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.SkipPlaybackToPrevious);
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
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.TogglePlayback);
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

        public ErrorResponse SkipPlaybackToNext() => this.SpotifyPremiumPlugin.Api.SkipPlaybackToNext(this.SpotifyPremiumPlugin.CurrentDeviceId);

        public ErrorResponse SkipPlaybackToPrevious() => this.SpotifyPremiumPlugin.Api.SkipPlaybackToPrevious(this.SpotifyPremiumPlugin.CurrentDeviceId);

        public ErrorResponse TogglePlayback()
        {
            var playback = this.SpotifyPremiumPlugin.Api.GetPlayback();
            return playback.IsPlaying
                ? this.SpotifyPremiumPlugin.Api.PausePlayback(this.SpotifyPremiumPlugin.CurrentDeviceId)
                : this.SpotifyPremiumPlugin.Api.ResumePlayback(this.SpotifyPremiumPlugin.CurrentDeviceId, String.Empty, null, String.Empty, 0);
        }
    }
}
