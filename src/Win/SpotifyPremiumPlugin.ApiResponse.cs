// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.Net;
    using Loupedeck;
    using SpotifyAPI.Web.Models;

    /// <summary>
    /// Plugin: Check Spotify API responses
    /// </summary>
    public partial class SpotifyPremiumPlugin : Plugin
    {
        public void CheckSpotifyResponse<T>(Func<T, ErrorResponse> apiCall, T param)
        {
            if (!this.SpotifyApiConnectionOk())
            {
                return;
            }

            var response = apiCall(param);

            this.CheckStatusCode(response.StatusCode(), response.Error?.Message);
        }

        public void CheckSpotifyResponse(Func<ErrorResponse> apiCall)
        {
            if (!this.SpotifyApiConnectionOk())
            {
                return;
            }

            var response = apiCall();

            this.CheckStatusCode(response.StatusCode(), response.Error?.Message);
        }

        internal void CheckStatusCode(HttpStatusCode statusCode, String spotifyApiMessage)
        {
            switch (statusCode)
            {
                case HttpStatusCode.Continue:
                case HttpStatusCode.SwitchingProtocols:
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                case HttpStatusCode.NonAuthoritativeInformation:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.ResetContent:
                case HttpStatusCode.PartialContent:

                    if (this.PluginStatus.Status != Loupedeck.PluginStatus.Normal)
                    {
                        this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, null, null);
                    }

                    break;

                case HttpStatusCode.Unauthorized:
                    // This should never happen?
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Login to Spotify", null);
                    break;

                case HttpStatusCode.NotFound:
                    // User doesn't have device set or some other Spotify related thing. User action needed.
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Warning, $"Spotify message: {spotifyApiMessage}", null);
                    break;

                default:
                    if (this.PluginStatus.Status != Loupedeck.PluginStatus.Error)
                    {
                        this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, spotifyApiMessage, null);
                    }

                    break;
            }
        }
    }
}
