// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    internal class TogglePlaybackCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public TogglePlaybackCommand()
            : base("Toggle Playback", "Toggles audio playback", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter) => this.SpotifyPremiumPlugin.Wrapper.TogglePlayback();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this.SpotifyPremiumPlugin.Wrapper.CachedPlayingState ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Play.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Pause.png");
        }
    }
}
