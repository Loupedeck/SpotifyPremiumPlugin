﻿// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.ParameterizedCommands
{
    using System;
    using System.Linq;

    internal class StartPlaylistCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public StartPlaylistCommand()
            : base()
        {
            // Profile actions do not belong to a group in the current UI, they are on the top level
            this.DisplayName = "Start Playlist"; // so this will be shown as "group name" for parameterized commands
            this.GroupName = "Not used";

            this.MakeProfileAction("list;Select playlist to play:");
        }

        protected override void RunCommand(String actionParameter) => this.SpotifyPremiumPlugin.Wrapper.StartPlaylist(actionParameter);

        protected override PluginActionParameter[] GetParameters()
        {
            var playlists = this.SpotifyPremiumPlugin.Wrapper.GetAllPlaylists();
            return playlists?
                        .Select(x => new PluginActionParameter(x.Uri, x.Name, String.Empty))
                        .ToArray();
        }
    }
}