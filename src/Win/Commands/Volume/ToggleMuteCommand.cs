// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Volume
{
    using System;

    internal class ToggleMuteCommand : SpotifyCommand
    {
        private Boolean currentlyMuted;

        public ToggleMuteCommand() : base("Toggle Mute", "Toggles audio mute state", "Spotify Volume") { }

        protected override void RunCommand(String actionParameter)
        {
            this.currentlyMuted = Wrapper.ToggleMute();

            this.ActionImageChanged();
        }

        public override string IconResource => this.currentlyMuted ? "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png" : "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Volume.png";

    }
}