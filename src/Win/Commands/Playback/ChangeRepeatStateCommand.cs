// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    using SpotifyAPI.Web.Enums;

    internal class ChangeRepeatStateCommand : SpotifyCommand
    {
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
            this._repeatState= Wrapper.ChangeRepeatState();
            this.ActionImageChanged();
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
    }
}
