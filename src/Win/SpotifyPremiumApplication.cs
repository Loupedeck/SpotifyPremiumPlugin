// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium
{
    using System;

    /// <summary>
    /// Target application - process name for Windows, bundle name for macOS
    /// </summary>
    public class SpotifyPremiumApplication : ClientApplication
    {
        protected override String GetProcessName() => "Spotify";

        protected override String GetBundleName() => "com.spotify.client";
    }
}