// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands
{
    using System;
    using System.Collections.Generic;

    internal class ToggleLikeCommand : SpotifyCommand
    {
        private Boolean _isLiked = true;

        public ToggleLikeCommand()
            : base(
                  "Toggle Like",
                  "Toggle Like",
                  "Others")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this._isLiked = Wrapper.ToggleLiked();
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._isLiked ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongLike.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongDislike.png");
        }
    }
}
