// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web.Enums;

    internal class ChangeRepeatStateCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public ChangeRepeatStateCommand()
            : base("Change Repeat State", "Change Repeat State description", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter) => this.SpotifyPremiumPlugin.Wrapper.ChangeRepeatState();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            String icon;
            switch (this.SpotifyPremiumPlugin.Wrapper.CachedRepeatState)
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
