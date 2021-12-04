#region Header

// Copyright © Anker Technology, BV 2021

#endregion

namespace Loupedeck.SpotifyPremiumPlugin.Adjustments
{
    internal abstract class SpotifyAdjustment : PluginDynamicAdjustment
    {
        protected SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        protected SpotifyWrapper Wrapper => SpotifyPremiumPlugin.Wrapper;

        protected SpotifyAdjustment(string displayName, string description, string groupName, bool hasReset, DeviceType supportedDevices = DeviceType.All) : base(displayName, description, groupName, hasReset, supportedDevices) { }
    }
}