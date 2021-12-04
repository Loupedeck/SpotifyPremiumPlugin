
namespace Loupedeck.Plugins.SpotifyPremium
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Timers;

    using Newtonsoft.Json;

    using SpotifyAPI.Web;
    using SpotifyAPI.Web.Auth;
    using SpotifyAPI.Web.Enums;
    using SpotifyAPI.Web.Models;

    internal sealed class SpotifyWrapper : IDisposable
    {
        private const String CLIENT_ID = "ClientId";
        private const String CLIENT_SECRET = "ClientSecret";
        private const String TCP_PORTS = "TcpPorts";

        private SpotifyPremiumPlugin Plugin { get; }

        internal SpotifyWrapper(SpotifyPremiumPlugin plugin)
        {
            this.Plugin = plugin;
            this.SpotifyConfiguration();
            this.CurrentDeviceId = plugin.GetCachedDeviceID();
        }
        
        private static Token token = new Token();
        private static AuthorizationCodeAuth auth;
        private static String spotifyTokenFilePath;

        private static Dictionary<String, String> _spotifyConfiguration;

        private List<Int32> tcpPorts = new List<Int32>();

        private SpotifyWebAPI Api { get; set; }

        private String CurrentDeviceId
        {
            get => this._currentDeviceId;
            set
            {
                this._currentDeviceId = value;
                this.Plugin.SaveDeviceToCache(value);
            }
        }
        
        #region Authentication & Configuration

        private Boolean SpotifyApiConnectionOk()
        {
            if (this.Api == null)
            {
                // User not logged in -> Automatically start login
                this.LoginToSpotify();

                // and skip action for now
                return false;
            }
            else if (token != null && DateTime.Now > token.CreateDate.AddSeconds(token.ExpiresIn) && !String.IsNullOrEmpty(token.RefreshToken))
            {
                this.RefreshToken(token.RefreshToken);
            }

            return true;
        }

        private Boolean ReadConfigurationFile()
        {
            // Get Spotify App configuration from spotify-client.txt file: client id and client secret
            // Windows path: %LOCALAPPDATA%/Loupedeck/PluginData/SpotifyPremium/spotify-client.txt
            var spotifyClientConfigurationFile = this.Plugin.ClientConfigurationFilePath;
            if (!File.Exists(spotifyClientConfigurationFile))
            {
                // Check path
                Directory.CreateDirectory(Path.GetDirectoryName(spotifyClientConfigurationFile));

                // Create the file
                using (FileStream fs = File.Create(spotifyClientConfigurationFile))
                {
                    var info = new UTF8Encoding(true).GetBytes($"{CLIENT_ID}{Environment.NewLine}{CLIENT_SECRET}{Environment.NewLine}{TCP_PORTS}");

                    // Add parameter titles to file.
                    fs.Write(info, 0, info.Length);
                }

                this.OnPluginStatusChanged(PluginStatus.Error, "Spotify configuration is missing. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            // Read configuration file, skip # comments, trim key and value
            _spotifyConfiguration = File.ReadAllLines(spotifyClientConfigurationFile)
                                                .Where(x => !x.StartsWith("#", StringComparison.InvariantCulture))
                                                .Select(x => x.Split('='))
                                                .ToDictionary(x => x[0].Trim(), x => x[1].Trim());

            if (!(_spotifyConfiguration.ContainsKey(CLIENT_ID) &&
                _spotifyConfiguration.ContainsKey(CLIENT_SECRET) &&
                _spotifyConfiguration.ContainsKey(TCP_PORTS)))
            {
                this.OnPluginStatusChanged(PluginStatus.Error, "Check Spotify API app 'ClientId' / 'ClientSecret' and 'TcpPorts' in configuration file. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            // Check TCP Ports
            this.tcpPorts = _spotifyConfiguration[TCP_PORTS]
                .Split(',')
                .Select(x => new { valid = Int32.TryParse(x.Trim(), out var val), port = val })
                .Where(x => x.valid)
                .Select(x => x.port)
                .ToList();

            if (this.tcpPorts.Count == 0)
            {
                this.OnPluginStatusChanged(PluginStatus.Error, "Check 'TcpPorts' values in configuration file. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            return true;
        }

        private void SpotifyConfiguration()
        {
            if (!this.ReadConfigurationFile())
            {
                return;
            }

            // Is there a token available
            token = null;
            spotifyTokenFilePath = Path.Combine(this.Plugin.GetPluginDataDirectory(), "spotify.json");
            if (File.Exists(spotifyTokenFilePath))
            {
                token = this.ReadTokenFromLocalFile();
            }

            // Check token and the expiration datetime
            if (token != null && DateTime.Now < token.CreateDate.AddSeconds(token.ExpiresIn))
            {
                // Use the existing token
                this.Api = new SpotifyWebAPI
                {
                    AccessToken = token.AccessToken,
                    TokenType = "Bearer",
                };
                this.OnPluginStatusChanged(PluginStatus.Normal, "Connected", null);
            }
            else if (!String.IsNullOrEmpty(token?.RefreshToken))
            {
                // Get a new access token based on the Refresh Token
                this.RefreshToken(token.RefreshToken);
            }
            else
            {
                // User has to login from Loupedeck application Plugin UI: Login - Login to Spotify. See LoginToSpotifyCommand.cs
                this.OnPluginStatusChanged(PluginStatus.Error, "Login to Spotify as Premium user", null);
            }
        }

        private Token ReadTokenFromLocalFile()
        {
            var json = File.ReadAllText(spotifyTokenFilePath);
            var localToken = JsonConvert.DeserializeObject<Token>(json);

            // Decrypt refresh token
            if (!String.IsNullOrEmpty(localToken?.RefreshToken))
            {
                var secret = Convert.FromBase64String(localToken.RefreshToken);
                var plain = ProtectedData.Unprotect(secret, null, DataProtectionScope.CurrentUser);
                var encoding = new UTF8Encoding();
                localToken.RefreshToken = encoding.GetString(plain);
            }

            return localToken;
        }

        private void SaveTokenToLocalFile(Token newToken, String refreshToken)
        {
            // Decrypt refresh token
            var encoding = new UTF8Encoding();
            var plain = encoding.GetBytes(refreshToken);
            var secret = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            newToken.RefreshToken = Convert.ToBase64String(secret);

            File.WriteAllText(spotifyTokenFilePath, JsonConvert.SerializeObject(newToken));
        }

        private void RefreshToken(String refreshToken)
        {
            auth = this.GetAuthorizationCodeAuth(out _);

            Token newToken = auth.RefreshToken(refreshToken).Result;

            if (!String.IsNullOrWhiteSpace(newToken.Error))
            {
                Tracer.Error($"Error happened during refreshing Spotify account token: {newToken.Error}: {newToken.ErrorDescription}");
                this.OnPluginStatusChanged(PluginStatus.Error, "Failed getting access to Spotify. Login as Premium user", null);
            }

            if (this.Api == null)
            {
                this.Api = new SpotifyWebAPI
                {
                    AccessToken = newToken.AccessToken,
                    TokenType = "Bearer",
                };
            }

            this.OnPluginStatusChanged(PluginStatus.Normal, "Connected", null);

            this.Api.AccessToken = newToken.AccessToken;
            this.SaveTokenToLocalFile(newToken, refreshToken);
        }

        internal void LoginToSpotify()
        {
            auth = this.GetAuthorizationCodeAuth(out _);

            auth.AuthReceived += this.Auth_AuthReceived;

            auth.Start();
            auth.OpenBrowser();
        }

        private async void Auth_AuthReceived(Object sender, AuthorizationCode payload)
        {
            try
            {
                auth.Stop();

                var previousToken = await auth.ExchangeCode(payload.Code);
                if (!String.IsNullOrWhiteSpace(previousToken.Error))
                {
                    Tracer.Error($"Error happened during adding Spotify account: {previousToken.Error}: {previousToken.ErrorDescription}");
                    return;
                }

                this.Api = new SpotifyWebAPI
                {
                    AccessToken = previousToken.AccessToken,
                    TokenType = previousToken.TokenType,
                };

                this.OnPluginStatusChanged(PluginStatus.Normal, null, null);

                this.SaveTokenToLocalFile(previousToken, previousToken.RefreshToken);
            }
            catch (Exception ex)
            {
                Tracer.Error($"Error happened during Spotify authentication: {ex.Message}");
            }
        }

        private AuthorizationCodeAuth GetAuthorizationCodeAuth(out Int32 timeout)
        {
            timeout = 240000; // ?!?

            if (!NetworkHelpers.TryGetFreeTcpPort(this.tcpPorts, out var selectedPort))
            {
                Tracer.Error("No available ports for Spotify!");
                return null;
            }

            var scopes =
                Scope.PlaylistReadPrivate |
                Scope.Streaming |
                Scope.UserReadCurrentlyPlaying |
                Scope.UserReadPlaybackState |
                Scope.UserLibraryRead |
                Scope.UserLibraryModify |
                Scope.UserReadPrivate |
                Scope.UserModifyPlaybackState |
                Scope.PlaylistReadCollaborative |
                Scope.PlaylistModifyPublic |
                Scope.PlaylistModifyPrivate |
                Scope.PlaylistReadPrivate |
                Scope.UserReadEmail;

            return !this.ReadConfigurationFile()
                ? null
                : new AuthorizationCodeAuth(
                    _spotifyConfiguration[CLIENT_ID], // Spotify API Client Id
                    _spotifyConfiguration[CLIENT_SECRET], // Spotify API Client Secret
                    $"http://localhost:{selectedPort}", // selectedPort must correspond to that on the Spotify developer's configuration!
                    $"http://localhost:{selectedPort}",
                    scopes);
        }

        #endregion Authentication & Configuration
        
        #region Error handling

        private void OnPluginStatusChanged(PluginStatus status, string message, string supportUrl)
        {
            this.Plugin?.OnPluginStatusChanged(status, message, supportUrl);
        }

        private void CheckSpotifyResponse<T>(Func<T, ErrorResponse> apiCall, T param)
        {
            if (!this.SpotifyApiConnectionOk())
            {
                return;
            }

            var response = apiCall(param);

            this.CheckStatusCode(response.StatusCode(), response.Error?.Message);
        }

        private void CheckSpotifyResponse(Func<ErrorResponse> apiCall)
        {
            if (!this.SpotifyApiConnectionOk())
            {
                return;
            }

            var response = apiCall();

            this.CheckStatusCode(response.StatusCode(), response.Error?.Message);
        }

        private void CheckStatusCode(HttpStatusCode statusCode, String spotifyApiMessage)
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

                    if (this.Plugin.PluginStatus.Status != PluginStatus.Normal)
                    {
                        this.OnPluginStatusChanged(PluginStatus.Normal, null, null);
                    }

                    break;

                case HttpStatusCode.Unauthorized:
                    // This should never happen?
                    this.OnPluginStatusChanged(PluginStatus.Error, "Login to Spotify", null);
                    break;

                case HttpStatusCode.NotFound:
                    // User doesn't have device set or some other Spotify related thing. User action needed.
                    this.OnPluginStatusChanged(PluginStatus.Warning, $"Spotify message: {spotifyApiMessage}", null);
                    break;

                default:
                    if (this.Plugin.PluginStatus.Status != PluginStatus.Error)
                    {
                        this.OnPluginStatusChanged(PluginStatus.Error, spotifyApiMessage, null);
                    }

                    break;
            }
        }

        #endregion Error Handling

        private PrivateProfile _privateProfile;

        private Paging<SimplePlaylist> GetUserPlaylists(Int32 offset = 0)
        {
            if (this.Api != null)
            {
                try
                {
                    if (this._privateProfile == null)
                    {
                        this._privateProfile = this.Api.GetPrivateProfile();
                    }

                    var profileId = this._privateProfile?.Id;
                    if (!String.IsNullOrEmpty(profileId))
                    {
                        var playlists = this.Api.GetUserPlaylists(profileId, 50, offset);
                        if (playlists?.Items?.Any() == true)
                        {
                            return playlists;
                        }
                    }
                }
                catch (Exception e)
                {
                    Tracer.Trace(e, "Error obtaining Spotify playlists");
                }
            }

            return new Paging<SimplePlaylist>
            {
                Items = new List<SimplePlaylist>(),
            };
        }

        internal List<SimplePlaylist> GetAllPlaylists()
        {
            Paging<SimplePlaylist> playlists = this.GetUserPlaylists();
            if (playlists != null)
            {
                var totalPlaylistsCount = playlists.Total;
                while (playlists.Items.Count < totalPlaylistsCount)
                {
                    playlists.Items.AddRange(this.GetUserPlaylists(playlists.Items.Count).Items);
                }

                return playlists.Items;
            }

            return null;
        }

        internal void SkipPlaybackToNext() => this.CheckSpotifyResponse(this.Api.SkipPlaybackToNext, this.CurrentDeviceId);

        internal void SkipPlaybackToPrevious() => this.CheckSpotifyResponse(this.Api.SkipPlaybackToPrevious, this.CurrentDeviceId);

        internal bool TogglePlayback()
        {
            var playback = this.Api.GetPlayback();

            if (playback.IsPlaying)
            {
                this.CheckSpotifyResponse(this.Api.PausePlayback, this.CurrentDeviceId);
            }
            else
            {
                ErrorResponse Func() => this.Api.ResumePlayback(this.CurrentDeviceId, String.Empty, null, String.Empty);
                this.CheckSpotifyResponse(Func);
            }

            return !playback.IsPlaying; // presume we switched it at this point.
        }

        #region Volume

        /// <summary>
        /// This is our most recently known Volume. Used when muting to remember the previous volume.  Used for dials when
        /// incrementing rapidly.
        /// </summary>
        private Int32 PreviousVolume { get; set; }

        internal void SetVolume(String volumeString)
        {
            if (int.TryParse(volumeString, out var volume))
            {
                this.SetVolume(volume);
            }
        }

        private void SetVolume(Int32 percents)
        {
            if (percents > 100)
            {
                percents = 100;
            }

            if (percents < 0)
            {
                percents = 0;
            }

            this.InitVolumeBlockedTimer();

            this.PreviousVolume = percents;

            ErrorResponse Func() => this.Api.SetVolume(percents, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);
        }

        private bool volumeCallsBlocked;
        private Timer volumeBlockedTimer;
        private string _currentDeviceId;

        internal void AdjustVolume(int ticks)
        {
            var modifiedVolume = 0;

            // Because this can be called in rapid succession with a dial turn, and it take Spotify a bit of time to register
            // volume changes round trip to the api, we don't want to Get the current Volume from Spotify if we've very recently set it
            // a few times.  Thus, we have a 2 second buffer after the last volume set, before we try to get the actual current volume 
            // from Spotify.
            if (this.volumeCallsBlocked)
            {
                modifiedVolume = this.PreviousVolume + ticks;
            }
            else
            {
                var playback = this.Api.GetPlayback();
                if (playback?.Device == null)
                {
                    // Set plugin status and message
                    this.CheckStatusCode(HttpStatusCode.NotFound, "Cannot adjust volume, no device");
                    return;
                }
                else
                {
                    modifiedVolume = playback.Device.VolumePercent + ticks;
                }
            }

            this.SetVolume(modifiedVolume);
        }

        private void InitVolumeBlockedTimer()
        {
            if (this.volumeBlockedTimer == null)
            {
                this.volumeBlockedTimer = new Timer(2000);
                this.volumeBlockedTimer.Elapsed += this.VolumeBlockExpired;
            }

            this.volumeCallsBlocked = true;
            if (this.volumeBlockedTimer.Enabled)
            {
                this.volumeBlockedTimer.Stop();
            }

            this.volumeBlockedTimer.Start();
        }

        private void VolumeBlockExpired(Object o, ElapsedEventArgs e) => this.volumeCallsBlocked = false;

        internal void Mute()
        {
            var playback = this.Api.GetPlayback();

            if (playback?.Device?.VolumePercent > 0)
            {
                this.PreviousVolume = playback.Device.VolumePercent;
            }

            ErrorResponse Func() => this.Api.SetVolume(0, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);
        }

        internal void Unmute()
        {
            var unmuteVolume = this.PreviousVolume != 0 ? this.PreviousVolume : 50;

            ErrorResponse Func() => this.Api.SetVolume(unmuteVolume, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);
        }

        /// <summary>
        /// Toggle current Mute setting
        /// </summary>
        /// <returns>true if muted after this call</returns>
        internal bool ToggleMute()
        {
            var playback = this.Api.GetPlayback();

            if (playback?.Device.VolumePercent != 0)
            {
                this.Mute();
                return true;
            }
            else
            {
                this.Unmute();
                return false;
            }
        }

        #endregion Volume

        internal bool ShufflePlay()
        {
            var playback = this.Api.GetPlayback();
            bool shuffleState = !playback.ShuffleState;

            ErrorResponse Func() => this.Api.SetShuffle(shuffleState, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);

            return shuffleState;
        }

        internal void StartPlaylist(String contextUri)
        {
            ErrorResponse Func() => this.Api.ResumePlayback(this.CurrentDeviceId, contextUri, null, String.Empty);
            this.CheckSpotifyResponse(Func);
        }

        internal void SaveToPlaylist(String playlistId)
        {
            playlistId = playlistId.Replace("spotify:playlist:", String.Empty);

            var playback = this.Api.GetPlayback();
            var currentTrackUri = playback.Item.Uri;

            ErrorResponse Func() => this.Api.AddPlaylistTrack(playlistId, currentTrackUri);
            this.CheckSpotifyResponse(Func);
        }

        internal bool ToggleLiked()
        {
            var playback = this.Api.GetPlayback();
            var trackId = playback.Item?.Id;
            if (String.IsNullOrEmpty(trackId))
            {
                // Set plugin status and message
                this.CheckStatusCode(HttpStatusCode.NotFound, "No track");
                return false;
            }

            var trackItemId = new List<String> { trackId };
            var tracksExist = this.Api.CheckSavedTracks(trackItemId);
            if (tracksExist.List == null || tracksExist.Error != null)
            {
                // Set plugin status and message
                this.CheckStatusCode(HttpStatusCode.NotFound, "No track list");
                return false;
            }

            if (tracksExist.List.Any() && tracksExist.List.FirstOrDefault() == false)
            {
                this.CheckSpotifyResponse(this.Api.SaveTrack, trackId);
                return true;
            }
            else
            {
                this.CheckSpotifyResponse(this.Api.RemoveSavedTracks, trackItemId);
                return false;
            }
        }

        internal List<Device> GetDevices()
        {
            var devices = this.Api?.GetDevices()?.Devices;

            if (devices?.Any() == true)
            {
                devices.Add(new Device { Id = ACTIVE_DEVICE, Name = "Active Device" });
            }

            return devices;
        }

        private const string ACTIVE_DEVICE = "activedevice";

        internal void TransferPlayback(String commandParameter)
        {
            if (commandParameter == ACTIVE_DEVICE)
            {
                commandParameter = String.Empty;
            }

            this.CurrentDeviceId = commandParameter;

            ErrorResponse Func() => this.Api.TransferPlayback(this.CurrentDeviceId, true);
            this.CheckSpotifyResponse(Func);
        }

        internal RepeatState ChangeRepeatState()
        {
            var playback = this.Api.GetPlayback();

            RepeatState newRepeatState = RepeatState.Off;
            
            switch (playback.RepeatState)
            {
                case RepeatState.Off:
                    newRepeatState = RepeatState.Context;
                    break;

                case RepeatState.Context:
                    newRepeatState = RepeatState.Track;
                    break;

                case RepeatState.Track:
                    newRepeatState = RepeatState.Off;
                    break;

                default:
                    // Set plugin status and message
                    this.CheckStatusCode(HttpStatusCode.NotFound, "Not able to change repeat state (check device etc.)");
                    break;
            }

            ErrorResponse Func() => this.Api.SetRepeatMode(newRepeatState, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);

            return newRepeatState;
        }

        public void Dispose()
        {
            // this.Plugin?.Dispose();  // we don't own this..

            this.Api?.Dispose();
        }
    }
}