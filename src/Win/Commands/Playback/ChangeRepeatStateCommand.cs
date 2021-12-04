// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands.Playback
{
    using System;

    using Commands;

    using SpotifyAPI.Web.Enums;

    internal class ChangeRepeatStateCommand : SpotifyCommand
    {
        private RepeatState _state;

        public RepeatState State
        {
            get => this._state;
            private set
            {
                this._state = value;
                this.ActionImageChanged();
            }
        }

        public ChangeRepeatStateCommand()
            : base("Change Repeat State", "Change Repeat State description", "Playback")
        {
        }

        public override string IconResource
        {
            get
            {
                switch (State)
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
            this.State = Wrapper.ChangeRepeatState();
        }
    }
}
