// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.Commands
{
    internal class LoginToSpotifyCommand : SpotifyCommand
    {
        public LoginToSpotifyCommand() : base("Login to Spotify", "Premium user login to Spotify API", "Login") { }

        protected override void RunCommand(string actionParameter)
        {
            Wrapper.LoginToSpotify();
        }
    }
}