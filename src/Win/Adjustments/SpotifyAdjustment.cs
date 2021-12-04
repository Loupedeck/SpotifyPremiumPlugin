namespace Loupedeck.SpotifyPremiumPlugin.Adjustments
{
    internal abstract class SpotifyAdjustment : PluginDynamicAdjustment
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        protected SpotifyWrapper Wrapper => this.SpotifyPremiumPlugin.Wrapper;

        protected SpotifyAdjustment(string displayName, string description, string groupName, bool hasReset, DeviceType supportedDevices = DeviceType.All) : base(displayName, description, groupName, hasReset, supportedDevices) { }
    }
}