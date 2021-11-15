// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.ParameterizedCommands
{
    using System;
    using System.Linq;
    using SpotifyAPI.Web.Models;

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

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.StartPlaylist, actionParameter);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify StartPlaylistCommand action obtain an error: ", e);
            }
        }

        public ErrorResponse StartPlaylist(String contextUri)
        {
            return this.SpotifyPremiumPlugin.Api.ResumePlayback(this.SpotifyPremiumPlugin.CurrentDeviceId, contextUri, null, String.Empty);
        }

        protected override PluginActionParameter[] GetParameters()
        {
            var playlists = this.SpotifyPremiumPlugin.GetAllPlaylists();
            return playlists?.Items
                        .Select(x => new PluginActionParameter(x.Uri, x.Name, String.Empty))
                        .ToArray();
        }
    }
}
