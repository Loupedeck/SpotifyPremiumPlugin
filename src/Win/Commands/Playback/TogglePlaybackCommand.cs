// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Playback
{
    internal class TogglePlaybackCommand : SpotifyCommand
    {
        private bool isPlaying = true;

        public TogglePlaybackCommand() : base("Toggle Playback", "Toggles audio playback", "Playback") { }

        protected override void RunCommand(string actionParameter)
        {
            isPlaying = Wrapper.TogglePlayback();

            ActionImageChanged();
        }

        protected override string IconResource => isPlaying ? "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Play.png" : "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Pause.png";
    }
}