namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.Net;

    using SpotifyAPI.Web;

    public partial class SpotifyWrapper
    {
        public WrapperStatus Status { get; private set; }

        public TResult CheckSpotifyResponse<T, TResult>(Func<T, TResult> apiMethod, T parameter)
        {
            try
            {
                return apiMethod(parameter);
            }
            catch (APIUnauthorizedException)
            {
                this.StartLogin();
            }
            catch (APITooManyRequestsException)
            {
                this.OnWrapperStatusChanged(WrapperStatus.Error, "Too many requests!", null);
            }
            catch (APIException apiException)
            {
                this.CheckStatusCode(apiException.Response.StatusCode, apiException.Message);
            }

            return default;
        }

        public TResult CheckSpotifyResponse<TResult>(Func<TResult> apiMethod)
        {
            try
            {
                return apiMethod();
            }
            catch (APIUnauthorizedException)
            {
                this.StartLogin();
            }
            catch (APITooManyRequestsException)
            {
                this.OnWrapperStatusChanged(WrapperStatus.Error, "Too many requests!", null);
            }
            catch (APIException apiException)
            {
                this.CheckStatusCode(apiException.Response.StatusCode, apiException.Message);
            }

            return default;
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
