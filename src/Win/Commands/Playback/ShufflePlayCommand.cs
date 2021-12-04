// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Playback
{
    using System;

    internal class ShufflePlayCommand : SpotifyCommand
    {
        private bool ShuffleState { get; set; }

        public ShufflePlayCommand() : base("Shuffle Play", "Shuffle Play description", "Playback") { }

        protected override void RunCommand(String actionParameter)
        {
            this.ShuffleState = this.Wrapper.ShufflePlay();
            this.ActionImageChanged();
        }

        protected override string IconResource => this.ShuffleState ? "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Shuffle.png" : "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.ShuffleOff.png";
    }
}
