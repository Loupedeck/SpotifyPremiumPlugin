// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Volume
{
    using System;

    internal class MuteCommand : SpotifyCommand
    {
        public MuteCommand()
            : base(
                  "Mute",
                  "Mute description",
                  "Spotify Volume")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            Wrapper.Mute();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.MuteVolume.png");
            return bitmapImage;
        }
    }
}
