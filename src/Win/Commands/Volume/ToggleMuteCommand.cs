// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    internal class ToggleMuteCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public ToggleMuteCommand() : base("Toggle Mute", "Toggles audio mute state", "Spotify Volume")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this.SpotifyPremiumPlugin.Wrapper.ToggleMute();
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this.SpotifyPremiumPlugin.Wrapper.CachedMuteState ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Volume.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png");
        }
    }
}
