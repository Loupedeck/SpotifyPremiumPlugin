namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using Loupedeck.SpotifyPremiumPlugin.Wrapper;

    using Newtonsoft.Json;

    using SpotifyAPI.Web;
    using SpotifyAPI.Web.Auth;

    public partial class SpotifyWrapper
    {
        private String SpotifyTokenFilePath => Path.Combine(this._cacheDirectory, "spotify.dat");

        public void StartAuth()
        {
            if (!this.TryReadConfigurationFile(out var configurationModel))
            {
                return;
            }

            if (!File.Exists(this.SpotifyTokenFilePath))
            {
                this.OnWrapperStatusChanged(WrapperStatus.Error, "Please login to Spotify. Click More Details below", "loupedeck:plugin/SpotifyPremium/callback/login");
                return;
            }

            var token = this.ReadTokenFromLocalFile();
            if (token != null)
            {
                // Use the existing token
                this.InitSpotifyClient(token, configurationModel);
            }
        }

        internal void StartLogin()
        {
            if (this.TryReadConfigurationFile(out var configurationModel))
            {
                this.StartLogin(configurationModel);
            }
        }

        internal void StartLogin(WrapperConfigurationModel configurationModel)
        {
            if (!NetworkHelpers.TryGetFreeTcpPort(configurationModel.TcpPorts, out var selectedPort))
            {
                Tracer.Error("No available ports for Spotify!");
                return;
            }

            var server = new EmbedIOAuthServer(new Uri($"http://localhost:{selectedPort}/callback"), selectedPort);

            server.Start();

            server.AuthorizationCodeReceived += (sender, response) =>
            {
                AuthorizationCodeTokenResponse token =
                new OAuthClient()
                            .RequestToken(
                            new AuthorizationCodeTokenRequest(
                                configurationModel.ClientId,
                                configurationModel.ClientSecret,
                                response.Code,
                                server.BaseUri)).Result;

                this.SaveTokenToLocalFile(token);
                this.InitSpotifyClient(token, configurationModel);

                server.Stop();
                server.Dispose();
                return null;
            };

            server.ErrorReceived += (sender, error, state) =>
            {
                server.Stop();
                server.Dispose();
                return null;
            };

            var request = new LoginRequest(server.BaseUri, configurationModel.ClientId, LoginRequest.ResponseType.Code)
            {
                Scope = new[]
                {
                    Scopes.PlaylistReadPrivate,
                    Scopes.Streaming,
                    Scopes.UserReadCurrentlyPlaying,
                    Scopes.UserReadPlaybackState,
                    Scopes.UserLibraryRead,
                    Scopes.UserLibraryModify,
                    Scopes.UserReadPrivate,
                    Scopes.UserModifyPlaybackState,
                    Scopes.PlaylistReadCollaborative,
                    Scopes.PlaylistModifyPublic,
                    Scopes.PlaylistModifyPrivate,
                    Scopes.PlaylistReadPrivate,
                    Scopes.UserReadEmail,
                },
            };

            try
            {
                BrowserUtil.Open(request.ToUri());
            }
            catch (Exception)
            {
                this.OnWrapperStatusChanged(WrapperStatus.Error, "Login to Spotify failed. Click More Details below", null);

                server.Stop();
                server.Dispose();
            }
        }

        internal void InitSpotifyClient(IRefreshableToken refreshableToken, WrapperConfigurationModel configurationModel)
        {
            var localToken = refreshableToken as AuthorizationCodeTokenResponse;

            // Refreshes token automatically on demand
            var authenticator = new AuthorizationCodeAuthenticator(configurationModel.ClientId, configurationModel.ClientSecret, localToken);
            authenticator.TokenRefreshed += (sender, token) => this.SaveTokenToLocalFile(token);

            var config = SpotifyClientConfig.CreateDefault()
              .WithAuthenticator(authenticator);

            this.Client = new SpotifyClient(config);

            this.OnWrapperStatusChanged(WrapperStatus.Normal, "Connected", null);
        }

        private Boolean TryReadConfigurationFile(out WrapperConfigurationModel model)
        {
            model = null;

            // Get Spotify App configuration from spotify-client.yml file: client id and client secret
            // Windows path: %LOCALAPPDATA%/Loupedeck/PluginData/SpotifyPremium/spotify-client.yml
            var spotifyClientConfigurationFile = this.ClientConfigurationFilePath;
            if (!File.Exists(spotifyClientConfigurationFile))
            {
                this.OnWrapperStatusChanged(WrapperStatus.Error, $"Spotify configuration is missing. Click More Details below", null);
                return false;
            }

            if (!YamlHelpers.TryDeserializeFromFile<WrapperConfigurationModel>(spotifyClientConfigurationFile, out var configurationModel))
            {
                this.OnWrapperStatusChanged(WrapperStatus.Error, $"Check Spotify API app 'ClientId' / 'ClientSecret' and 'TcpPorts' in configuration file. Click More Details below", null);
                return false;
            }

            model = configurationModel;

            return true;
        }

        private IRefreshableToken ReadTokenFromLocalFile()
        {
            IRefreshableToken localToken = null;

            try
            {
                var encryptedBase64Data = File.ReadAllText(this.SpotifyTokenFilePath);
                var encryptedData = Convert.FromBase64String(encryptedBase64Data);
                var unparsedToken = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                var encoding = new UTF8Encoding();

                localToken = JsonConvert.DeserializeObject<IRefreshableToken>(encoding.GetString(unparsedToken));
            }
            catch (Exception ex)
            {
                Tracer.Warning("Unable to read cached token!", ex);
            }

            return localToken;
        }

        private void SaveTokenToLocalFile(IRefreshableToken token)
        {
            var serializedToken = JsonHelpers.SerializeObject(token);

            var encoding = new UTF8Encoding();
            var plain = encoding.GetBytes(serializedToken);
            var secret = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            var encryptedData = Convert.ToBase64String(secret);

            File.WriteAllText(this.SpotifyTokenFilePath, encryptedData);
        }
    }
}
