namespace Loupedeck.Plugins.SpotifyPremium.Adjustments
{
    internal abstract class SpotifyAdjustment : PluginDynamicAdjustment
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => Plugin as SpotifyPremiumPlugin;

        protected SpotifyWrapper Wrapper => SpotifyPremiumPlugin.Wrapper;

        protected SpotifyAdjustment(string displayName, string description, string groupName, bool hasReset, DeviceType supportedDevices = DeviceType.All) : base(displayName, description, groupName, hasReset, supportedDevices) { }
    }
}