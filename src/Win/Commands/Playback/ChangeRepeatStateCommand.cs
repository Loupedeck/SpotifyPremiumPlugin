// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web.Enums;
    using SpotifyAPI.Web.Models;

    internal class ChangeRepeatStateCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        private RepeatState _repeatState;

        public ChangeRepeatStateCommand()
            : base(
                  "Change Repeat State",
                  "Change Repeat State description",
                  "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                var playback = this.SpotifyPremiumPlugin.Api.GetPlayback();
                switch (playback.RepeatState)
                {
                    case RepeatState.Off:
                        this._repeatState = RepeatState.Context;
                        this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.ChangeRepeatState, this._repeatState);
                        break;

                    case RepeatState.Context:
                        this._repeatState = RepeatState.Track;
                        this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.ChangeRepeatState, this._repeatState);
                        break;

                    case RepeatState.Track:
                        this._repeatState = RepeatState.Off;
                        this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.ChangeRepeatState, this._repeatState);
                        break;

                    default:
                        // Set plugin status and message
                        this.SpotifyPremiumPlugin.CheckStatusCode(System.Net.HttpStatusCode.NotFound, "Not able to change repeat state (check device etc.)");
                        break;
                }
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify ChangeRepeatStateCommand action obtain an error: ", e);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            String icon;
            switch (this._repeatState)
            {
                case RepeatState.Off:
                    icon = "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.RepeatOff.png";
                    break;

                case RepeatState.Context:
                    icon = "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.RepeatList.png";
                    break;

                case RepeatState.Track:
                    icon = "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Repeat.png";
                    break;

                default:
                    // Set plugin status and message
                    icon = "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.RepeatOff.png";
                    break;
            }

            var bitmapImage = EmbeddedResources.ReadImage(icon);
            return bitmapImage;
        }

        public ErrorResponse ChangeRepeatState(RepeatState repeatState)
        {
            var response = this.SpotifyPremiumPlugin.Api.SetRepeatMode(repeatState, this.SpotifyPremiumPlugin.CurrentDeviceId);

            this.ActionImageChanged();

            return response;
        }
    }
}
