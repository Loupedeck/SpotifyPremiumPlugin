// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Volume
{
    using System;

    internal class ToggleMuteCommand : SpotifyCommand
    {
        private Boolean currentlyMuted;

        public ToggleMuteCommand()
            : base(
                  "Toggle Mute",
                  "Toggles audio mute state",
                  "Spotify Volume")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this.currentlyMuted = Wrapper.ToggleMute();

            this.ActionImageChanged();
        }


        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this.currentlyMuted ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png") : 
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Volume.png");
        }
    }
}
