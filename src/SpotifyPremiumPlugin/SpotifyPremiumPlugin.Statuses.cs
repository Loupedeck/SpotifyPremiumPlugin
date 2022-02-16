// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using Loupedeck;

    /// <summary>
    /// Plugin: handle wrapper status and switch plugin status
    /// </summary>
    public partial class SpotifyPremiumPlugin : Plugin
    {
        internal void WrapperStatusParser(Object o, WrapperChangedEventArgs e) =>
            this.OnPluginStatusChanged(this.GetPluginStatus(e.WrapperStatus), e.Message, e.SupportUrl);

        private PluginStatus GetPluginStatus(WrapperStatus wrapperStatus)
        {
            switch (wrapperStatus)
            {
                case WrapperStatus.Unknown:
                    return Loupedeck.PluginStatus.Unknown;

                case WrapperStatus.Normal:
                    return Loupedeck.PluginStatus.Normal;

                case WrapperStatus.Warning:
                    return Loupedeck.PluginStatus.Warning;

                case WrapperStatus.Error:
                    return Loupedeck.PluginStatus.Error;

                default:
                    return Loupedeck.PluginStatus.Unknown;
            }
        }
    }
}
