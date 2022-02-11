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
        private static Token token = new Token();
        private static AuthorizationCodeAuth auth;
        private static String spotifyTokenFilePath;

        private static Dictionary<String, String> _spotifyConfiguration;

        private List<Int32> tcpPorts = new List<Int32>();

        public Boolean SpotifyApiConnected()
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
            var spotifyClientConfigurationFile = this.ClientConfigurationFilePath;
            if (!File.Exists(spotifyClientConfigurationFile))
            {
                // Check path
                Directory.CreateDirectory(Path.GetDirectoryName(spotifyClientConfigurationFile));

                // Create the file
                using (FileStream fileStream = File.Create(spotifyClientConfigurationFile))
                {
                    var info = new UTF8Encoding(true).GetBytes($"{_clientId}{Environment.NewLine}{_clientSecret}{Environment.NewLine}{_tcpPorts}");

                    // Add parameter titles to file.
                    fileStream.Write(info, 0, info.Length);
                }

                this.OnWrapperStatusChanged(WrapperStatus.Error, $"Spotify configuration is missing. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            // Read configuration file, skip # comments, trim key and value
            _spotifyConfiguration = File.ReadAllLines(spotifyClientConfigurationFile)
                                                .Where(x => !x.StartsWith("#"))
                                                .Select(x => x.Split('='))
                                                .ToDictionary(x => x[0].Trim(), x => x[1].Trim());

            if (!(_spotifyConfiguration.ContainsKey(_clientId) &&
                _spotifyConfiguration.ContainsKey(_clientSecret) &&
                _spotifyConfiguration.ContainsKey(_tcpPorts)))
            {
                this.OnWrapperStatusChanged(WrapperStatus.Error, $"Check Spotify API app 'ClientId' / 'ClientSecret' and 'TcpPorts' in configuration file. Click More Details below", $"file:/{spotifyClientConfigurationFile}");
                return false;
            }

            // Check TCP Ports
            this.tcpPorts = _spotifyConfiguration[_tcpPorts]
                .Split(',')
                .Select(x => new { valid = Int32.TryParse(x.Trim(), out var val), port = val })
                .Where(x => x.valid)
                .Select(x => x.port)
                .ToList();

            if (this.tcpPorts.Count == 0)
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
            token = null;
            spotifyTokenFilePath = Path.Combine(this._cacheDirectory, "spotify.json");
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
                this.OnWrapperStatusChanged(WrapperStatus.Normal, "Connected", null);
            }
            else if (!String.IsNullOrEmpty(token?.RefreshToken))
            {
                // Get a new access token based on the Refresh Token
                this.RefreshToken(token.RefreshToken);
            }
            else
            {
                // User has to login from Loupedeck application Plugin UI: Login - Login to Spotify. See LoginToSpotifyCommand.cs
                this.OnWrapperStatusChanged(WrapperStatus.Error, "Login to Spotify as Premium user", null);
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

        public void RefreshToken(String refreshToken)
        {
            auth = this.GetAuthorizationCodeAuth(out var timeout);

            Token newToken = auth.RefreshToken(refreshToken).Result;

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
            auth = this.GetAuthorizationCodeAuth(out var timeout);

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
                    _spotifyConfiguration[_clientId],     // Spotify API Client Id
                    _spotifyConfiguration[_clientSecret], // Spotify API Client Secret
                    $"http://localhost:{selectedPort}",   // selectedPort must correspond to that on the Spotify developers's configuration!
                    $"http://localhost:{selectedPort}",
                    scopes);
        }
    }
}
