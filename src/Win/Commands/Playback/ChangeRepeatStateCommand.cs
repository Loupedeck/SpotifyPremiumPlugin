// Copyright(c) Loupedeck.All rights reserved.

#region Usings

using SpotifyAPI.Web.Enums;

#endregion

namespace Loupedeck.Plugins.SpotifyPremium.Commands.Playback
{
    internal class ChangeRepeatStateCommand : SpotifyCommand
    {
        private RepeatState State { get; set; }

        public ChangeRepeatStateCommand()
            : base("Change Repeat State", "Change Repeat State description", "Playback") { }

        protected override string IconResource
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

        protected override void RunCommand(string actionParameter)
        {
            State = Wrapper.ChangeRepeatState();
            ActionImageChanged();
        }
    }
}