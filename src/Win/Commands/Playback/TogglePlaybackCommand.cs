// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web.Models;

    internal class TogglePlaybackCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        private Boolean _isPlaying = true;

        public TogglePlaybackCommand()
            : base(
                  "Toggle Playback",
                  "Toggles audio playback",
                  "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.TogglePlayback);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify TogglePlayback action obtain an error: ", e);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._isPlaying ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Play.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Pause.png");
        }

        public ErrorResponse TogglePlayback()
        {
            var playback = this.SpotifyPremiumPlugin.Api.GetPlayback();
            this._isPlaying = playback.IsPlaying;

            this.ActionImageChanged();

            return playback.IsPlaying
                ? this.SpotifyPremiumPlugin.Api.PausePlayback(this.SpotifyPremiumPlugin.CurrentDeviceId)
                : this.SpotifyPremiumPlugin.Api.ResumePlayback(this.SpotifyPremiumPlugin.CurrentDeviceId, String.Empty, null, String.Empty, 0);
        }
    }
}
