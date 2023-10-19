// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    internal class MuteCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public MuteCommand()
            : base("Mute", "Mute description", "Spotify Volume")
        {
        }

        protected override void RunCommand(String actionParameter) => this.SpotifyPremiumPlugin.Wrapper.Mute();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png");
            return bitmapImage;
        }
    }
}
