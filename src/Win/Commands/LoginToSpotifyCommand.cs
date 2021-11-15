// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using Loupedeck;

    internal class LoginToSpotifyCommand : PluginDynamicCommand
    {
        public LoginToSpotifyCommand()
            : base(
                  "Login to Spotify",
                  "Premium user login to Spotify API",
                  "Login")
        {
        }

        protected override void RunCommand(String actionParameter) => (this.Plugin as SpotifyPremiumPlugin).LoginToSpotify();
    }
}
