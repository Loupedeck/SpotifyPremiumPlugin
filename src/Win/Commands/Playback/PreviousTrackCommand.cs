// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    internal class PreviousTrackCommand : SpotifyCommand
    {
        public PreviousTrackCommand()
            : base("Previous Track", "Previous Track description", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this.Wrapper.SkipPlaybackToPrevious();
        }

        protected override string IconResource => "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.PreviousTrack.png";
   }
}
