// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web.Models;

    internal class ToggleMuteCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        private Boolean _mute;

        public ToggleMuteCommand()
            : base(
                  "Toggle Mute",
                  "Toggles audio mute state",
                  "Spotify Volume")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.ToggleMute);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify ToggleMuteCommand action obtain an error: ", e);
            }
        }


        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._mute ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Volume.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png");
        }

        public ErrorResponse ToggleMute()
        {
            this.ActionImageChanged();

            var playback = this.SpotifyPremiumPlugin.Api.GetPlayback();
            return playback?.Device.VolumePercent != 0 ? this.Mute() : this.Unmute();
        }

        public ErrorResponse Unmute()
        {
            this._mute = false;
            var unmuteVolume = this.SpotifyPremiumPlugin.PreviousVolume != 0 ? this.SpotifyPremiumPlugin.PreviousVolume : 50;
            var result = this.SpotifyPremiumPlugin.Api.SetVolume(unmuteVolume, this.SpotifyPremiumPlugin.CurrentDeviceId);
            return result;
        }

        public ErrorResponse Mute()
        {
            this._mute = true;
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
