// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    internal class NextTrackCommand : SpotifyCommand
    {
        public NextTrackCommand()
            : base(
                  "Next Track",
                  "Next Track description",
                  "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            Wrapper.SkipPlaybackToNext();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.NextTrack.png");
            return bitmapImage;
        }
    }
}
