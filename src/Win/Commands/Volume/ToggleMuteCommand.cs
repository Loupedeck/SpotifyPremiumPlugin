// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Volume
{
    internal class ToggleMuteCommand : SpotifyCommand
    {
        private bool currentlyMuted;

        public ToggleMuteCommand() : base("Toggle Mute", "Toggles audio mute state", "Spotify Volume") { }

        protected override void RunCommand(string actionParameter)
        {
            currentlyMuted = Wrapper.ToggleMute();

            ActionImageChanged();
        }

        protected override string IconResource => currentlyMuted ? "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png" : "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Volume.png";
    }
}