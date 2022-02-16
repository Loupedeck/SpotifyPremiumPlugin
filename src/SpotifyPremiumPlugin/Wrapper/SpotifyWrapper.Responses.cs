namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.Net;

    using SpotifyAPI.Web.Models;

    public partial class SpotifyWrapper
    {
        public WrapperStatus Status { get; set; }

        public void CheckSpotifyResponse<T>(Func<T, ErrorResponse> apiCall, T param)
        {
            if (!this.SpotifyApiConnected())
            {
                return;
            }

            var response = apiCall(param);

            this.CheckStatusCode(response.StatusCode(), response.Error?.Message);
        }

        public void CheckSpotifyResponse(Func<ErrorResponse> apiCall)
        {
            if (!this.SpotifyApiConnected())
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

                    if (this.Status != WrapperStatus.Normal)
                    {
                        this.OnWrapperStatusChanged(WrapperStatus.Normal, null, null);
                    }

                    break;

                case HttpStatusCode.Unauthorized:
                    // This should never happen?
                    this.OnWrapperStatusChanged(WrapperStatus.Error, "Login to Spotify", null);
                    break;

                case HttpStatusCode.NotFound:
                    // User doesn't have device set or some other Spotify related thing. User action needed.
                    this.OnWrapperStatusChanged(WrapperStatus.Warning, $"Spotify message: {spotifyApiMessage}", null);
                    break;

                default:
                    if (this.Status != WrapperStatus.Error)
                    {
                        this.OnWrapperStatusChanged(WrapperStatus.Error, spotifyApiMessage, null);
                    }

                    break;
            }
        }
    }
}
