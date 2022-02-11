// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    internal class NextTrackCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public NextTrackCommand() : base("Next Track", "Next Track description", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this.SpotifyPremiumPlugin.Wrapper.SkipPlaybackToNext();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.NextTrack.png");
            return bitmapImage;
        }
    }
}
