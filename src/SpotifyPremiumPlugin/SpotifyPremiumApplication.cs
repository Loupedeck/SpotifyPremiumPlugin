// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    /// <summary>
    /// Target application - process name for Windows, bundle name for macOS
    /// </summary>
    public class SpotifyPremiumApplication : ClientApplication
    {
        public SpotifyPremiumApplication()
        {
        }

        protected override String GetProcessName() => "Spotify";

        protected override String GetBundleName() => "com.spotify.client";
    }
}