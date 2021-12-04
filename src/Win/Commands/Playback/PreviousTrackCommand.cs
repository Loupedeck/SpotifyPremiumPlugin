// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    using SpotifyAPI.Web.Models;

    internal class PreviousTrackCommand : SpotifyCommand
    {
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
