namespace Loupedeck.Plugins.SpotifyPremium.Commands
{
    internal abstract class SpotifyCommand : PluginDynamicCommand
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => Plugin as SpotifyPremiumPlugin;
        protected SpotifyWrapper Wrapper => SpotifyPremiumPlugin.Wrapper;

        protected SpotifyCommand(string displayName, string description, string groupName)
            : base(displayName, description, groupName) { }

        protected SpotifyCommand() { }

        protected virtual string IconResource { get; } = string.Empty;

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            return EmbeddedResources.ReadImage(IconResource);
        }
    }
}