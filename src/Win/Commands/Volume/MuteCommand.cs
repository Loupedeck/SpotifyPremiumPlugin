// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Volume
{
    using System;

    internal class MuteCommand : SpotifyCommand
    {
        public MuteCommand() : base("Mute", "Mute description", "Spotify Volume") { }

        protected override void RunCommand(String actionParameter)
        {
            Wrapper.Mute();
        }

        public override string IconResource => "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png";
    }
}