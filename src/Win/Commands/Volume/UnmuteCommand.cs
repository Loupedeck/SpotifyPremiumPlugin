// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Volume
{
    using System;

    internal class UnmuteCommand : SpotifyCommand
    {
        public UnmuteCommand()
            : base(
                  "Unmute",
                  "Unmute description",
                  "Spotify Volume")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            Wrapper.Unmute();
        }


        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Volume.png");
            return bitmapImage;
        }
    }
}
