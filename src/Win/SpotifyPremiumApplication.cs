// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium
{
    /// <summary>
    /// Target application - process name for Windows, bundle name for macOS
    /// </summary>
    public class SpotifyPremiumApplication : ClientApplication
    {
        protected override string GetProcessName()
        {
            return "Spotify";
        }

        protected override string GetBundleName()
        {
            return "com.spotify.client";
        }
    }
}