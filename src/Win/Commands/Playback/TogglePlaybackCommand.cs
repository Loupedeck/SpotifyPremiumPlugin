// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Playback
{
    using System;

    internal class TogglePlaybackCommand : SpotifyCommand
    {
        private Boolean _isPlaying = true;

        public TogglePlaybackCommand() : base("Toggle Playback", "Toggles audio playback", "Playback") { }

        protected override void RunCommand(String actionParameter)
        {
            this._isPlaying = this.Wrapper.TogglePlayback();

            this.ActionImageChanged();
        }

        protected override string IconResource => this._isPlaying ? "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Play.png" : "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Pause.png";
    }
}
