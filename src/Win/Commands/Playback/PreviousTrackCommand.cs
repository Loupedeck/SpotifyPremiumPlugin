// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Playback
{
    internal class PreviousTrackCommand : SpotifyCommand
    {
        public PreviousTrackCommand()
            : base("Previous Track", "Previous Track description", "Playback") { }

        protected override void RunCommand(string actionParameter)
        {
            Wrapper.SkipPlaybackToPrevious();
        }

        protected override string IconResource => "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.PreviousTrack.png";
    }
}