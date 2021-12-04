// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    using SpotifyAPI.Web.Models;

    internal class NextTrackCommand : SpotifyCommand
    {
        public NextTrackCommand()
            : base(
                  "Next Track",
                  "Next Track description",
                  "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.SkipPlaybackToNext);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify NextTrackCommand action obtain an error: ", e);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.NextTrack.png");
            return bitmapImage;
        }

        public ErrorResponse SkipPlaybackToNext() => this.SpotifyPremiumPlugin.Api.SkipPlaybackToNext(this.SpotifyPremiumPlugin.CurrentDeviceId);
    }
}
