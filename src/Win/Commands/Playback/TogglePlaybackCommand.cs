// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    internal class TogglePlaybackCommand : SpotifyCommand
    {
        private Boolean _isPlaying = true;

        public TogglePlaybackCommand() : base("Toggle Playback", "Toggles audio playback", "Playback") { }

        protected override void RunCommand(String actionParameter)
        {
            this._isPlaying = Wrapper.TogglePlayback();

            this.ActionImageChanged();
        }

        public override string IconResource => this._isPlaying ? "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Play.png" : "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Pause.png";
    }
}
