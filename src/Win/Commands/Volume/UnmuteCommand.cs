// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web.Models;

    internal class UnmuteCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public UnmuteCommand()
            : base(
                  "Unmute",
                  "Unmute description",
                  "Spotify Volume")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.Unmute);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify UnmuteCommand action obtain an error: ", e);
            }
        }


        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Volume.png");
            return bitmapImage;
        }

        public ErrorResponse Unmute()
        {
            var unmuteVolume = this.SpotifyPremiumPlugin.PreviousVolume != 0 ? this.SpotifyPremiumPlugin.PreviousVolume : 50;
            var result = this.SpotifyPremiumPlugin.Api.SetVolume(unmuteVolume, this.SpotifyPremiumPlugin.CurrentDeviceId);
            return result;
        }
    }
}
