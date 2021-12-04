// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Volume
{
    using System;

    internal class UnmuteCommand : SpotifyCommand
    {
        public UnmuteCommand() : base("Unmute", "Unmute description", "Spotify Volume") { }

        protected override void RunCommand(String actionParameter)
        {
            this.Wrapper.Unmute();
        }

        protected override string IconResource => "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Volume.png";
    }
}