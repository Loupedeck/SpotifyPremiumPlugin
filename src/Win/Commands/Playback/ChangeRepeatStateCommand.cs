// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Playback
{
    using System;

    using SpotifyAPI.Web.Enums;

    internal class ChangeRepeatStateCommand : SpotifyCommand
    {
        private RepeatState State { get; set; }

        public ChangeRepeatStateCommand()
            : base("Change Repeat State", "Change Repeat State description", "Playback")
        {
        }

        protected override string IconResource
        {
            get
            {
                switch (this.State)
                {
                    case RepeatState.Off:
                        return "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.RepeatOff.png";

                    case RepeatState.Context:
                        return "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.RepeatList.png";

                    case RepeatState.Track:
                        return "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Repeat.png";

                    default:
                        // Set plugin status and message
                        return "Loupedeck.SpotifyPremiumPlugin.Icons.Width80.RepeatOff.png";
                }
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            this.State = this.Wrapper.ChangeRepeatState();
            this.ActionImageChanged();
        }
    }
}
