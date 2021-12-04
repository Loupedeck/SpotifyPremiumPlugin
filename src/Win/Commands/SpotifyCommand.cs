#region Header

// Copyright © Anker Technology, BV 2021

#endregion
namespace Loupedeck.SpotifyPremiumPlugin.Commands
{
    internal abstract class SpotifyCommand : PluginDynamicCommand
    {
        protected SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;
        protected SpotifyWrapper Wrapper => SpotifyPremiumPlugin.Wrapper;

        protected SpotifyCommand(string displayName, string description, string groupName, DeviceType supportedDevices = DeviceType.All)
            : base(displayName, description, groupName) { }

        protected SpotifyCommand() { }

    }
}