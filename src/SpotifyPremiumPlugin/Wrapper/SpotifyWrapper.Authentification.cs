namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using Newtonsoft.Json;

    using SpotifyAPI.Web;
    using SpotifyAPI.Web.Auth;
    using SpotifyAPI.Web.Enums;
    using SpotifyAPI.Web.Models;

    public partial class SpotifyWrapper
    {
        private Token _token = new Token();
        private AuthorizationCodeAuth _auth;
        private String _spotifyTokenFilePath;

        private Dictionary<String, String> _spotifyConfiguration;

        private List<Int32> _tcpPorts = new List<Int32>();

        public Boolean SpotifyApiConnected()
        {
            if (this.Api == null)
            {
                // User not logged in -> Automatically start login
                this.LoginToSpotify();

                // and skip action for now
                return false;
            }
            else if (this._token != null && DateTime.Now > this._token.CreateDate.AddSeconds(this._token.ExpiresIn) && !String.IsNullOrEmpty(this._token.RefreshToken))
            {
                this.RefreshToken(this._token.RefreshToken);
            }

            return true;
        }

        private Boolean ReadConfigurationFile()
        {
            // Get Spotify App configuration from spotify-client.txt file: client id and client secret
            // Windows path: %LOCALAPPDATA%/Loupedeck/PluginData/SpotifyPremium/spotify-client.txt
            var spotifyClientConfigurationFile = this.ClientConfigurationFilePath;
            if (!File.Exists(spotifyClientConfigurationFile))
            {
                // Check path
                Directory.CreateDirectory(Path.GetDirectoryName(spotifyClientConfigurationFile));

                // Create the file
                using (FileStream fileStream = File.Create(spotifyClientConfigurationFile))
                {
                    var info = new UTF8Encoding(true).GetBytes($"{_clientIdName}{Environment.NewLine}{_clientSecretName}{Environment.NewLine}{_tcpPortsName}");

                    // Add parameter titles to file.
                    fileStream.Write(info, 0, info.Length);
                }

                this.OnWrapperStatusChanged(WrapperStatus.Error, $"Spotify configuration is missing. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            // Read configuration file, skip # comments, trim key and value
            this._spotifyConfiguration = File.ReadAllLines(spotifyClientConfigurationFile)
                                                .Where(x => !x.StartsWith("#"))
                                                .Select(x => x.Split('='))
                                                .ToDictionary(x => x[0].Trim(), x => x[1].Trim());

            if (!(this._spotifyConfiguration.ContainsKey(_clientIdName) &&
                this._spotifyConfiguration.ContainsKey(_clientSecretName) &&
                this._spotifyConfiguration.ContainsKey(_tcpPortsName)))
            {
                this.OnWrapperStatusChanged(WrapperStatus.Error, $"Check Spotify API app 'ClientId' / 'ClientSecret' and 'TcpPorts' in configuration file. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            // Check TCP Ports
            this._tcpPorts = this._spotifyConfiguration[_tcpPortsName]
                .Split(',')
                .Select(x => new { valid = Int32.TryParse(x.Trim(), out var val), port = val })
                .Where(x => x.valid)
                .Select(x => x.port)
                .ToList();

            if (this._tcpPorts.Count == 0)
            {
                this.OnWrapperStatusChanged(WrapperStatus.Error, $"Check 'TcpPorts' values in configuration file. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            return true;
        }

        public void ReadSpotifyConfiguration()
        {
            if (!this.ReadConfigurationFile())
            {
                return;
            }

            // Is there a token available
            this._token = null;
            this._spotifyTokenFilePath = Path.Combine(this._cacheDirectory, "spotify.json");
            if (File.Exists(this._spotifyTokenFilePath))
            {
                this._token = this.ReadTokenFromLocalFile();
            }

            // Check token and the expiration datetime
            if (this._token != null && DateTime.Now < this._token.CreateDate.AddSeconds(this._token.ExpiresIn))
            {
                // Use the existing token
                this.Api = new SpotifyWebAPI
                {
                    AccessToken = this._token.AccessToken,
                    TokenType = "Bearer",
                };
                this.OnWrapperStatusChanged(WrapperStatus.Normal, "Connected", null);
            }
            else if (!String.IsNullOrEmpty(this._token?.RefreshToken))
            {
                // Get a new access token based on the Refresh Token
                this.RefreshToken(this._token.RefreshToken);
            }
            else
            {
                // User has to login from Loupedeck application Plugin UI: Login - Login to Spotify. See LoginToSpotifyCommand.cs
                this.OnWrapperStatusChanged(WrapperStatus.Error, "Login to Spotify as Premium user", null);
            }
        }

        private Token ReadTokenFromLocalFile()
        {
            var json = File.ReadAllText(this._spotifyTokenFilePath);
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

            File.WriteAllText(this._spotifyTokenFilePath, JsonConvert.SerializeObject(newToken));
        }

        public void RefreshToken(String refreshToken)
        {
            this._auth = this.GetAuthorizationCodeAuth(out var timeout);

            Token newToken = this._auth.RefreshToken(refreshToken).Result;

            if (!String.IsNullOrWhiteSpace(newToken.Error))
            {
                Tracer.Error($"Error happened during refreshing Spotify account token: {newToken.Error}: {newToken.ErrorDescription}");
                this.OnWrapperStatusChanged(WrapperStatus.Error, "Failed getting access to Spotify. Login as Premium user", null);
            }

            if (this.Api == null)
            {
                this.Api = new SpotifyWebAPI
                {
                    AccessToken = newToken.AccessToken,
                    TokenType = "Bearer",
                };
            }

            this.OnWrapperStatusChanged(WrapperStatus.Normal, "Connected", null);

            this.Api.AccessToken = newToken.AccessToken;
            this.SaveTokenToLocalFile(newToken, refreshToken);
        }

        public void LoginToSpotify()
        {
            this._auth = this.GetAuthorizationCodeAuth(out var timeout);

            this._auth.AuthReceived += this.Auth_AuthReceived;

            this._auth.Start();
            this._auth.OpenBrowser();
        }

        private async void Auth_AuthReceived(Object sender, AuthorizationCode payload)
        {
            try
            {
                this._auth.Stop();

                var previousToken = await this._auth.ExchangeCode(payload.Code);
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

                this.OnWrapperStatusChanged(WrapperStatus.Normal, null, null);

                this.SaveTokenToLocalFile(previousToken, previousToken.RefreshToken);
            }
            catch (Exception ex)
            {
                Tracer.Error($"Error happened during Spotify authentication: {ex.Message}");
            }
        }

        public AuthorizationCodeAuth GetAuthorizationCodeAuth(out Int32 timeout)
        {
            timeout = 240000; // ?!?

            if (!NetworkHelpers.TryGetFreeTcpPort(this._tcpPorts, out var selectedPort))
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
                    this._spotifyConfiguration[_clientIdName],     // Spotify API Client Id
                    this._spotifyConfiguration[_clientSecretName], // Spotify API Client Secret
                    $"http://localhost:{selectedPort}",   // selectedPort must correspond to that on the Spotify developers's configuration!
                    $"http://localhost:{selectedPort}",
                    scopes);
        }
    }
}
