// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    internal class TogglePlaybackCommand : SpotifyCommand
    {
        private Boolean _isPlaying = true;

        public TogglePlaybackCommand()
            : base(
                  "Toggle Playback",
                  "Toggles audio playback",
                  "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this._isPlaying = Wrapper.TogglePlayback();

            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._isPlaying ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Play.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Pause.png");
        }
    }
}
