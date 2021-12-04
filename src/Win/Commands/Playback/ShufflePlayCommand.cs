// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

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
