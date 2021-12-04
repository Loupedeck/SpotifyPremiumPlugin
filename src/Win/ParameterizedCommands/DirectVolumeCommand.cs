// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.ParameterizedCommands
{
    using System;

    using Commands;

    internal class DirectVolumeCommand : SpotifyCommand
    {
        public DirectVolumeCommand()
            : base()
        {
            // Profile actions do not belong to a group in the current UI, they are on the top level
            this.DisplayName = "Direct Volume"; // so this will be shown as "group name" for parameterized commands
            this.GroupName = "Not used";

            this.MakeProfileAction("text;Enter volume level to set 0-100:");
        }

        protected override void RunCommand(String actionParameter)
        {
            Wrapper.SetVolume(actionParameter);
        }
    }
}
