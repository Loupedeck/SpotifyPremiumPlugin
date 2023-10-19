// Copyright(c) Loupedeck.All rights reserved.

#region Usings

using System.Collections.Generic;
using System.Linq;

using Loupedeck.Plugins.SpotifyPremium.Commands;

using SpotifyAPI.Web.Models;

#endregion

namespace Loupedeck.Plugins.SpotifyPremium.ParameterizedCommands
{
    internal class SaveToPlaylistCommand : SpotifyCommand
    {
        public SaveToPlaylistCommand()
        {
            // Profile actions do not belong to a group in the current UI, they are on the top level
            DisplayName = "Save To Playlist"; // so this will be shown as "group name" for parameterized commands
            GroupName = "Not used";

            MakeProfileAction("list;Select playlist to save:");
        }

        protected override void RunCommand(string actionParameter)
        {
            Wrapper.SaveToPlaylist(actionParameter);
        }

        protected override PluginActionParameter[] GetParameters()
        {
            List<SimplePlaylist> playlists = Wrapper.GetAllPlaylists();

            return playlists?.Select(x => new PluginActionParameter(x.Uri, x.Name, string.Empty))
                .ToArray();
        }
    }
}