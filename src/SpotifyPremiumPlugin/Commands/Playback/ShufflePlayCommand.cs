// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    internal class ShufflePlayCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public ShufflePlayCommand()
            : base("Shuffle Play", "Shuffle Play description", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter) => this.SpotifyPremiumPlugin.Wrapper.ShufflePlay();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this.SpotifyPremiumPlugin.Wrapper.CachedShuffleState ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Shuffle.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.ShuffleOff.png");
        }
    }
}
