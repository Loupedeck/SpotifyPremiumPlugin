// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands
{
    using System;

    internal class ToggleLikeCommand : SpotifyCommand
    {
        private Boolean _isLiked = true;

        public ToggleLikeCommand() : base("Toggle Like", "Toggle Like", "Others") { }

        protected override void RunCommand(String actionParameter)
        {
            this._isLiked = this.Wrapper.ToggleLiked();
            this.ActionImageChanged();
        }

        protected override string IconResource => this._isLiked ? 
            "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongLike.png" : 
            "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongDislike.png";
    }
}
