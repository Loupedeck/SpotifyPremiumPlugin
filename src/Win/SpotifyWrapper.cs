#region Usings

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

#endregion

namespace Loupedeck.Plugins.SpotifyPremium
{
    internal sealed class SpotifyWrapper : IDisposable
    {
        private const string CLIENT_ID = "ClientId";
        private const string CLIENT_SECRET = "ClientSecret";
        private const string TCP_PORTS = "TcpPorts";

        private SpotifyPremiumPlugin Plugin { get; }

        internal SpotifyWrapper(SpotifyPremiumPlugin plugin)
        {
            Plugin = plugin;
            SpotifyConfiguration();
            CurrentDeviceId = plugin.GetCachedDeviceID();
        }

        private static Token token = new Token();
        private static AuthorizationCodeAuth auth;
        private static string spotifyTokenFilePath;

        private static Dictionary<string, string> _spotifyConfiguration;

        private List<int> tcpPorts = new List<int>();

        private SpotifyWebAPI Api { get; set; }

        private string CurrentDeviceId
        {
            get => _currentDeviceId;
            set
            {
                _currentDeviceId = value;
                Plugin.SaveDeviceToCache(value);
            }
        }

        #region Authentication & Configuration

        private bool SpotifyApiConnectionOk()
        {
            if (Api == null)
            {
                // User not logged in -> Automatically start login
                LoginToSpotify();

                // and skip action for now
                return false;
            }

            if (token != null && DateTime.Now > token.CreateDate.AddSeconds(token.ExpiresIn) && !string.IsNullOrEmpty(token.RefreshToken))
            {
                RefreshToken(token.RefreshToken);
            }

            return true;
        }

        private bool ReadConfigurationFile()
        {
            // Get Spotify App configuration from spotify-client.txt file: client id and client secret
            // Windows path: %LOCALAPPDATA%/Loupedeck/PluginData/SpotifyPremium/spotify-client.txt
            string spotifyClientConfigurationFile = Plugin.ClientConfigurationFilePath;

            if (!File.Exists(spotifyClientConfigurationFile))
            {
                // Check path
                Directory.CreateDirectory(Path.GetDirectoryName(spotifyClientConfigurationFile));

                // Create the file
                using (FileStream fs = File.Create(spotifyClientConfigurationFile))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"{CLIENT_ID}{Environment.NewLine}{CLIENT_SECRET}{Environment.NewLine}{TCP_PORTS}");

                    // Add parameter titles to file.
                    fs.Write(info, 0, info.Length);
                }

                OnPluginStatusChanged(PluginStatus.Error, "Spotify configuration is missing. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
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
                OnPluginStatusChanged(PluginStatus.Error, "Check Spotify API app 'ClientId' / 'ClientSecret' and 'TcpPorts' in configuration file. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            // Check TCP Ports
            tcpPorts = _spotifyConfiguration[TCP_PORTS]
                .Split(',')
                .Select(x => new { valid = int.TryParse(x.Trim(), out int val), port = val })
                .Where(x => x.valid)
                .Select(x => x.port)
                .ToList();

            if (tcpPorts.Count == 0)
            {
                OnPluginStatusChanged(PluginStatus.Error, "Check 'TcpPorts' values in configuration file. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            return true;
        }

        private void SpotifyConfiguration()
        {
            if (!ReadConfigurationFile())
            {
                return;
            }

            // Is there a token available
            token = null;
            spotifyTokenFilePath = Path.Combine(Plugin.GetPluginDataDirectory(), "spotify.json");

            if (File.Exists(spotifyTokenFilePath))
            {
                token = ReadTokenFromLocalFile();
            }

            // Check token and the expiration datetime
            if (token != null && DateTime.Now < token.CreateDate.AddSeconds(token.ExpiresIn))
            {
                // Use the existing token
                Api = new SpotifyWebAPI
                {
                    AccessToken = token.AccessToken,
                    TokenType = "Bearer"
                };

                OnPluginStatusChanged(PluginStatus.Normal, "Connected", null);
            }
            else if (!string.IsNullOrEmpty(token?.RefreshToken))
            {
                // Get a new access token based on the Refresh Token
                RefreshToken(token.RefreshToken);
            }
            else
            {
                // User has to login from Loupedeck application Plugin UI: Login - Login to Spotify. See LoginToSpotifyCommand.cs
                OnPluginStatusChanged(PluginStatus.Error, "Login to Spotify as Premium user", null);
            }
        }

        private Token ReadTokenFromLocalFile()
        {
            string json = File.ReadAllText(spotifyTokenFilePath);
            var localToken = JsonConvert.DeserializeObject<Token>(json);

            // Decrypt refresh token
            if (!string.IsNullOrEmpty(localToken?.RefreshToken))
            {
                byte[] secret = Convert.FromBase64String(localToken.RefreshToken);
                byte[] plain = ProtectedData.Unprotect(secret, null, DataProtectionScope.CurrentUser);
                var encoding = new UTF8Encoding();
                localToken.RefreshToken = encoding.GetString(plain);
            }

            return localToken;
        }

        private void SaveTokenToLocalFile(Token newToken, string refreshToken)
        {
            // Decrypt refresh token
            var encoding = new UTF8Encoding();
            byte[] plain = encoding.GetBytes(refreshToken);
            byte[] secret = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            newToken.RefreshToken = Convert.ToBase64String(secret);

            File.WriteAllText(spotifyTokenFilePath, JsonConvert.SerializeObject(newToken));
        }

        private void RefreshToken(string refreshToken)
        {
            auth = GetAuthorizationCodeAuth(out _);

            Token newToken = auth.RefreshToken(refreshToken).Result;

            if (!string.IsNullOrWhiteSpace(newToken.Error))
            {
                Tracer.Error($"Error happened during refreshing Spotify account token: {newToken.Error}: {newToken.ErrorDescription}");
                OnPluginStatusChanged(PluginStatus.Error, "Failed getting access to Spotify. Login as Premium user", null);
            }

            if (Api == null)
            {
                Api = new SpotifyWebAPI
                {
                    AccessToken = newToken.AccessToken,
                    TokenType = "Bearer"
                };
            }

            OnPluginStatusChanged(PluginStatus.Normal, "Connected", null);

            Api.AccessToken = newToken.AccessToken;
            SaveTokenToLocalFile(newToken, refreshToken);
        }

        internal void LoginToSpotify()
        {
            auth = GetAuthorizationCodeAuth(out _);

            auth.AuthReceived += Auth_AuthReceived;

            auth.Start();
            auth.OpenBrowser();
        }

        private async void Auth_AuthReceived(object sender, AuthorizationCode payload)
        {
            try
            {
                auth.Stop();

                Token previousToken = await auth.ExchangeCode(payload.Code);

                if (!string.IsNullOrWhiteSpace(previousToken.Error))
                {
                    Tracer.Error($"Error happened during adding Spotify account: {previousToken.Error}: {previousToken.ErrorDescription}");
                    return;
                }

                Api = new SpotifyWebAPI
                {
                    AccessToken = previousToken.AccessToken,
                    TokenType = previousToken.TokenType
                };

                OnPluginStatusChanged(PluginStatus.Normal, null, null);

                SaveTokenToLocalFile(previousToken, previousToken.RefreshToken);
            }
            catch (Exception ex)
            {
                Tracer.Error($"Error happened during Spotify authentication: {ex.Message}");
            }
        }

        private AuthorizationCodeAuth GetAuthorizationCodeAuth(out int timeout)
        {
            timeout = 240000; // ?!?

            if (!NetworkHelpers.TryGetFreeTcpPort(tcpPorts, out int selectedPort))
            {
                Tracer.Error("No available ports for Spotify!");
                return null;
            }

            Scope scopes =
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

            return !ReadConfigurationFile()
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
            Plugin?.OnPluginStatusChanged(status, message, supportUrl);
        }

        private void CheckSpotifyResponse<T>(Func<T, ErrorResponse> apiCall, T param)
        {
            if (!SpotifyApiConnectionOk())
            {
                return;
            }

            ErrorResponse response = apiCall(param);

            CheckStatusCode(response.StatusCode(), response.Error?.Message);
        }

        private void CheckSpotifyResponse(Func<ErrorResponse> apiCall)
        {
            if (!SpotifyApiConnectionOk())
            {
                return;
            }

            ErrorResponse response = apiCall();

            CheckStatusCode(response.StatusCode(), response.Error?.Message);
        }

        private void CheckStatusCode(HttpStatusCode statusCode, string spotifyApiMessage)
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

                    if (Plugin.PluginStatus.Status != PluginStatus.Normal)
                    {
                        OnPluginStatusChanged(PluginStatus.Normal, null, null);
                    }

                    break;

                case HttpStatusCode.Unauthorized:
                    // This should never happen?
                    OnPluginStatusChanged(PluginStatus.Error, "Login to Spotify", null);
                    break;

                case HttpStatusCode.NotFound:
                    // User doesn't have device set or some other Spotify related thing. User action needed.
                    OnPluginStatusChanged(PluginStatus.Warning, $"Spotify message: {spotifyApiMessage}", null);
                    break;

                default:
                    if (Plugin.PluginStatus.Status != PluginStatus.Error)
                    {
                        OnPluginStatusChanged(PluginStatus.Error, spotifyApiMessage, null);
                    }

                    break;
            }
        }

        #endregion Error Handling

        private PrivateProfile _privateProfile;

        private Paging<SimplePlaylist> GetUserPlaylists(int offset = 0)
        {
            if (Api != null)
            {
                try
                {
                    if (_privateProfile == null)
                    {
                        _privateProfile = Api.GetPrivateProfile();
                    }

                    string profileId = _privateProfile?.Id;

                    if (!string.IsNullOrEmpty(profileId))
                    {
                        Paging<SimplePlaylist> playlists = Api.GetUserPlaylists(profileId, 50, offset);

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
                Items = new List<SimplePlaylist>()
            };
        }

        internal List<SimplePlaylist> GetAllPlaylists()
        {
            Paging<SimplePlaylist> playlists = GetUserPlaylists();

            if (playlists != null)
            {
                int totalPlaylistsCount = playlists.Total;

                while (playlists.Items.Count < totalPlaylistsCount)
                {
                    playlists.Items.AddRange(GetUserPlaylists(playlists.Items.Count).Items);
                }

                return playlists.Items;
            }

            return null;
        }

        internal void SkipPlaybackToNext()
        {
            CheckSpotifyResponse(Api.SkipPlaybackToNext, CurrentDeviceId);
        }

        internal void SkipPlaybackToPrevious()
        {
            CheckSpotifyResponse(Api.SkipPlaybackToPrevious, CurrentDeviceId);
        }

        internal bool TogglePlayback()
        {
            PlaybackContext playback = Api.GetPlayback();

            if (playback.IsPlaying)
            {
                CheckSpotifyResponse(Api.PausePlayback, CurrentDeviceId);
            }
            else
            {
                ErrorResponse Func()
                {
                    return Api.ResumePlayback(CurrentDeviceId, string.Empty, null, string.Empty);
                }

                CheckSpotifyResponse(Func);
            }

            return !playback.IsPlaying; // presume we switched it at this point.
        }

        #region Volume

        /// <summary>
        /// This is our most recently known Volume. Used when muting to remember the previous volume.  Used for dials when
        /// incrementing rapidly.
        /// </summary>
        private int PreviousVolume { get; set; }

        internal void SetVolume(string volumeString)
        {
            if (int.TryParse(volumeString, out int volume))
            {
                SetVolume(volume);
            }
        }

        private void SetVolume(int percents)
        {
            if (percents > 100)
            {
                percents = 100;
            }

            if (percents < 0)
            {
                percents = 0;
            }

            InitVolumeBlockedTimer();

            PreviousVolume = percents;

            ErrorResponse Func()
            {
                return Api.SetVolume(percents, CurrentDeviceId);
            }

            CheckSpotifyResponse(Func);
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
            if (volumeCallsBlocked)
            {
                modifiedVolume = PreviousVolume + ticks;
            }
            else
            {
                PlaybackContext playback = Api.GetPlayback();

                if (playback?.Device == null)
                {
                    // Set plugin status and message
                    CheckStatusCode(HttpStatusCode.NotFound, "Cannot adjust volume, no device");
                    return;
                }

                modifiedVolume = playback.Device.VolumePercent + ticks;
            }

            SetVolume(modifiedVolume);
        }

        private void InitVolumeBlockedTimer()
        {
            if (volumeBlockedTimer == null)
            {
                volumeBlockedTimer = new Timer(2000);
                volumeBlockedTimer.Elapsed += VolumeBlockExpired;
            }

            volumeCallsBlocked = true;

            if (volumeBlockedTimer.Enabled)
            {
                volumeBlockedTimer.Stop();
            }

            volumeBlockedTimer.Start();
        }

        private void VolumeBlockExpired(object o, ElapsedEventArgs e)
        {
            volumeCallsBlocked = false;
        }

        internal void Mute()
        {
            PlaybackContext playback = Api.GetPlayback();

            if (playback?.Device?.VolumePercent > 0)
            {
                PreviousVolume = playback.Device.VolumePercent;
            }

            ErrorResponse Func()
            {
                return Api.SetVolume(0, CurrentDeviceId);
            }

            CheckSpotifyResponse(Func);
        }

        internal void Unmute()
        {
            int unmuteVolume = PreviousVolume != 0 ? PreviousVolume : 50;

            ErrorResponse Func()
            {
                return Api.SetVolume(unmuteVolume, CurrentDeviceId);
            }

            CheckSpotifyResponse(Func);
        }

        /// <summary>
        /// Toggle current Mute setting
        /// </summary>
        /// <returns>true if muted after this call</returns>
        internal bool ToggleMute()
        {
            PlaybackContext playback = Api.GetPlayback();

            if (playback?.Device.VolumePercent != 0)
            {
                Mute();
                return true;
            }

            Unmute();
            return false;
        }

        #endregion Volume

        internal bool ShufflePlay()
        {
            PlaybackContext playback = Api.GetPlayback();
            bool shuffleState = !playback.ShuffleState;

            ErrorResponse Func()
            {
                return Api.SetShuffle(shuffleState, CurrentDeviceId);
            }

            CheckSpotifyResponse(Func);

            return shuffleState;
        }

        internal void StartPlaylist(string contextUri)
        {
            ErrorResponse Func()
            {
                return Api.ResumePlayback(CurrentDeviceId, contextUri, null, string.Empty);
            }

            CheckSpotifyResponse(Func);
        }

        internal void SaveToPlaylist(string playlistId)
        {
            playlistId = playlistId.Replace("spotify:playlist:", string.Empty);

            PlaybackContext playback = Api.GetPlayback();
            string currentTrackUri = playback.Item.Uri;

            ErrorResponse Func()
            {
                return Api.AddPlaylistTrack(playlistId, currentTrackUri);
            }

            CheckSpotifyResponse(Func);
        }

        internal bool ToggleLiked()
        {
            PlaybackContext playback = Api.GetPlayback();
            string trackId = playback.Item?.Id;

            if (string.IsNullOrEmpty(trackId))
            {
                // Set plugin status and message
                CheckStatusCode(HttpStatusCode.NotFound, "No track");
                return false;
            }

            var trackItemId = new List<string> { trackId };
            ListResponse<bool> tracksExist = Api.CheckSavedTracks(trackItemId);

            if (tracksExist.List == null || tracksExist.Error != null)
            {
                // Set plugin status and message
                CheckStatusCode(HttpStatusCode.NotFound, "No track list");
                return false;
            }

            if (tracksExist.List.Any() && tracksExist.List.FirstOrDefault() == false)
            {
                CheckSpotifyResponse(Api.SaveTrack, trackId);
                return true;
            }

            CheckSpotifyResponse(Api.RemoveSavedTracks, trackItemId);
            return false;
        }

        internal List<Device> GetDevices()
        {
            List<Device> devices = Api?.GetDevices()?.Devices;

            if (devices?.Any() == true)
            {
                devices.Add(new Device { Id = ACTIVE_DEVICE, Name = "Active Device" });
            }

            return devices;
        }

        private const string ACTIVE_DEVICE = "activedevice";

        internal void TransferPlayback(string commandParameter)
        {
            if (commandParameter == ACTIVE_DEVICE)
            {
                commandParameter = string.Empty;
            }

            CurrentDeviceId = commandParameter;

            ErrorResponse Func()
            {
                return Api.TransferPlayback(CurrentDeviceId, true);
            }

            CheckSpotifyResponse(Func);
        }

        internal RepeatState ChangeRepeatState()
        {
            PlaybackContext playback = Api.GetPlayback();

            var newRepeatState = RepeatState.Off;

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
                    CheckStatusCode(HttpStatusCode.NotFound, "Not able to change repeat state (check device etc.)");
                    break;
            }

            ErrorResponse Func()
            {
                return Api.SetRepeatMode(newRepeatState, CurrentDeviceId);
            }

            CheckSpotifyResponse(Func);

            return newRepeatState;
        }

        public void Dispose()
        {
            // this.Plugin?.Dispose();  // we don't own this..

            Api?.Dispose();
        }
    }
}