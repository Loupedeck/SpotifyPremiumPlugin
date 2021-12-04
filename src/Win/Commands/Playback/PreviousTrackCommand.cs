// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    internal class PreviousTrackCommand : SpotifyCommand
    {
        public PreviousTrackCommand()
            : base(
                  "Previous Track",
                  "Previous Track description",
                  "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            Wrapper.SkipPlaybackToPrevious();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.PreviousTrack.png");
            return bitmapImage;
        }
   }
}
