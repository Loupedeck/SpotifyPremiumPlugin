// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web.Models;

    internal class ShufflePlayCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        private Boolean _shuffleState;

        public ShufflePlayCommand()
            : base(
                  "Shuffle Play",
                  "Shuffle Play description",
                  "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.ShufflePlay);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify ShufflePlayCommand action obtain an error: ", e);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._shuffleState ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Shuffle.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.ShuffleOff.png");
        }

        public ErrorResponse ShufflePlay()
        {
            var playback = this.SpotifyPremiumPlugin.Api.GetPlayback();
            this._shuffleState = !playback.ShuffleState;
            var response = this.SpotifyPremiumPlugin.Api.SetShuffle(this._shuffleState, this.SpotifyPremiumPlugin.CurrentDeviceId);

            this.ActionImageChanged();

            return response;
        }
    }
}
