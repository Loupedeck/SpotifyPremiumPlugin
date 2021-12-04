// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Volume
{
    using System;

    internal class MuteCommand : SpotifyCommand
    {
        public MuteCommand() : base("Mute", "Mute description", "Spotify Volume") { }

        protected override void RunCommand(String actionParameter)
        {
            this.Wrapper.Mute();
        }

        protected override string IconResource => "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png";
    }
}