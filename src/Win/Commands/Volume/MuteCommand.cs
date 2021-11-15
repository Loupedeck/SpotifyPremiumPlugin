// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web.Models;

    internal class MuteCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public MuteCommand()
            : base(
                  "Mute",
                  "Mute description",
                  "Spotify Volume")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.Mute);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify MuteCommand action obtain an error: ", e);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png");
            return bitmapImage;
        }

        public ErrorResponse Mute()
        {
            var playback = this.SpotifyPremiumPlugin.Api.GetPlayback();
            if (playback?.Device != null)
            {
                this.SpotifyPremiumPlugin.PreviousVolume = playback.Device.VolumePercent;
            }

            var result = this.SpotifyPremiumPlugin.Api.SetVolume(0, this.SpotifyPremiumPlugin.CurrentDeviceId);

            return result;
        }
    }
}
