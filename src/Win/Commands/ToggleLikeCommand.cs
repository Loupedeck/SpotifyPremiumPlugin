// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ToggleLikeCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        private Boolean _isLiked = true;

        public ToggleLikeCommand()
            : base(
                  "Toggle Like",
                  "Toggle Like",
                  "Others")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                var playback = this.SpotifyPremiumPlugin.Api.GetPlayback();
                var trackId = playback.Item?.Id;
                if (String.IsNullOrEmpty(trackId))
                {
                    // Set plugin status and message
                    this.SpotifyPremiumPlugin.CheckStatusCode(System.Net.HttpStatusCode.NotFound, "No track");
                    return;
                }

                var trackItemId = new List<String> { trackId };
                var tracksExist = this.SpotifyPremiumPlugin.Api.CheckSavedTracks(trackItemId);
                if (tracksExist.List == null && tracksExist.Error != null)
                {
                    // Set plugin status and message
                    this.SpotifyPremiumPlugin.CheckStatusCode(System.Net.HttpStatusCode.NotFound, "No track list");
                    return;
                }

                if (tracksExist.List.Any() && tracksExist.List.FirstOrDefault() == false)
                {
                    this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.SpotifyPremiumPlugin.Api.SaveTrack, trackId);
                    this._isLiked = true;
                    this.ActionImageChanged();
                }
                else
                {
                    this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.SpotifyPremiumPlugin.Api.RemoveSavedTracks, trackItemId);
                    this._isLiked = false;

                    this.ActionImageChanged();
                }
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify Toggle Like action obtain an error: ", e);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return this._isLiked ?
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongLike.png") :
                EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.SongDislike.png");
        }
    }
}
