// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.Commands
{
    using System;

    internal class LoginToSpotifyCommand : SpotifyCommand
    {
        public LoginToSpotifyCommand() : base("Login to Spotify", "Premium user login to Spotify API", "Login") { }

        protected override void RunCommand(String actionParameter) => this.Wrapper.LoginToSpotify();
    }
}
