// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands
{
    internal class ToggleLikeCommand : SpotifyCommand
    {
        private bool _isLiked = true;

        public ToggleLikeCommand() : base("Toggle Like", "Toggle Like", "Others") { }

        protected override void RunCommand(string actionParameter)
        {
            _isLiked = Wrapper.ToggleLiked();
            ActionImageChanged();
        }

        protected override string IconResource => _isLiked ? "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongLike.png" : "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongDislike.png";
    }
}