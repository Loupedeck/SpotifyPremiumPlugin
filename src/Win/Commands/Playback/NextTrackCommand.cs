// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Playback
{
    using System;

    internal class NextTrackCommand : SpotifyCommand
    {
        public NextTrackCommand()
            : base("Next Track", "Next Track description", "Playback")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this.Wrapper.SkipPlaybackToNext();
        }

        protected override string IconResource => "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.NextTrack.png";
    }
}
