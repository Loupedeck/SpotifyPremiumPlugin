
namespace Loupedeck.Plugins.SpotifyPremium.Commands
{
    internal abstract class SpotifyCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;
        protected SpotifyWrapper Wrapper => this.SpotifyPremiumPlugin.Wrapper;

        protected SpotifyCommand(string displayName, string description, string groupName)
            : base(displayName, description, groupName) { }

        protected SpotifyCommand() { }

        protected virtual string IconResource { get; } = string.Empty;

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadImage(this.IconResource);
    }
}