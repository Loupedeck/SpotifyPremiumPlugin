﻿// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.ParameterizedCommands
{
    using System;
    using System.Linq;

    using Commands;

    internal class SaveToPlaylistCommand : SpotifyCommand
    {
        public SaveToPlaylistCommand()
        {
            // Profile actions do not belong to a group in the current UI, they are on the top level
            this.DisplayName = "Save To Playlist"; // so this will be shown as "group name" for parameterized commands
            this.GroupName = "Not used";

            this.MakeProfileAction("list;Select playlist to save:");
        }

        protected override void RunCommand(String actionParameter)
        {
            this.Wrapper.SaveToPlaylist(actionParameter);
        }

        protected override PluginActionParameter[] GetParameters()
        {
            var playlists = this.Wrapper.GetAllPlaylists();
            return playlists?.Select(x => new PluginActionParameter(x.Uri, x.Name, String.Empty))
                        .ToArray();
        }
    }
}
