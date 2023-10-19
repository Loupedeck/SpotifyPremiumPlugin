// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Playback
{
    internal class ShufflePlayCommand : SpotifyCommand
    {
        private bool ShuffleState { get; set; }

        public ShufflePlayCommand() : base("Shuffle Play", "Shuffle Play description", "Playback") { }

        protected override void RunCommand(string actionParameter)
        {
            ShuffleState = Wrapper.ShufflePlay();
            ActionImageChanged();
        }

        protected override string IconResource => ShuffleState ? "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Shuffle.png" : "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.ShuffleOff.png";
    }
}