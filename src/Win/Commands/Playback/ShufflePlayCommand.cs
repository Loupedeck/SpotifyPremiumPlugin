// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    internal class ShufflePlayCommand : SpotifyCommand
    {
        private Boolean _shuffleState;

        public ShufflePlayCommand()
            : base(
                  "Shuffle Play",
                  "Shuffle Play description",
                  "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this._shuffleState = Wrapper.ShufflePlay();

            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._shuffleState ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Shuffle.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.ShuffleOff.png");
        }
    }
}
