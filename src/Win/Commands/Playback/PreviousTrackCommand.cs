// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web.Models;

    internal class PreviousTrackCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public PreviousTrackCommand()
            : base(
                  "Previous Track",
                  "Previous Track description",
                  "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.SkipPlaybackToPrevious);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify PreviousTrackCommand action obtain an error: ", e);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.PreviousTrack.png");
            return bitmapImage;
        }

        public ErrorResponse SkipPlaybackToPrevious() => this.SpotifyPremiumPlugin.Api.SkipPlaybackToPrevious(this.SpotifyPremiumPlugin.CurrentDeviceId);
    }
}
