// Copyright(c) Loupedeck.All rights reserved.

#region Usings

using System.Collections.Generic;
using System.Linq;

using Loupedeck.Plugins.SpotifyPremium.Commands;

using SpotifyAPI.Web.Models;

#endregion

namespace Loupedeck.Plugins.SpotifyPremium.ParameterizedCommands
{
    internal class StartPlaylistCommand : SpotifyCommand
    {
        public StartPlaylistCommand()
        {
            // Profile actions do not belong to a group in the current UI, they are on the top level
            DisplayName = "Start Playlist"; // so this will be shown as "group name" for parameterized commands
            GroupName = "Not used";

            MakeProfileAction("list;Select playlist to play:");
        }

        protected override void RunCommand(string actionParameter)
        {
            Wrapper.StartPlaylist(actionParameter);
        }

        protected override PluginActionParameter[] GetParameters()
        {
            List<SimplePlaylist> playlists = Wrapper.GetAllPlaylists();

            return playlists?.Select(x => new PluginActionParameter(x.Uri, x.Name, string.Empty))
                .ToArray();
        }
    }
}