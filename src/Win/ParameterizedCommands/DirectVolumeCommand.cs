// Copyright(c) Loupedeck.All rights reserved.

#region Usings

using Loupedeck.Plugins.SpotifyPremium.Commands;

#endregion

namespace Loupedeck.Plugins.SpotifyPremium.ParameterizedCommands
{
    internal class DirectVolumeCommand : SpotifyCommand
    {
        public DirectVolumeCommand()
        {
            // Profile actions do not belong to a group in the current UI, they are on the top level
            DisplayName = "Direct Volume"; // so this will be shown as "group name" for parameterized commands
            GroupName = "Not used";

            MakeProfileAction("text;Enter volume level to set 0-100:");
        }

        protected override void RunCommand(string actionParameter)
        {
            Wrapper.SetVolume(actionParameter);
        }
    }
}