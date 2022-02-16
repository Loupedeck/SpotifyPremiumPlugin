// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    internal class ToggleLikeCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public ToggleLikeCommand()
            : base("Toggle Like", "Toggle Like", "Others")
        {
        }

        protected override void RunCommand(String actionParameter) => this.SpotifyPremiumPlugin.Wrapper.ToggleLike();

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this.SpotifyPremiumPlugin.Wrapper.CachedLikeState ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongLike.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongDislike.png");
        }
    }
}
