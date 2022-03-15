namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    public partial class SpotifyWrapper
    {
        public event EventHandler<WrapperChangedEventArgs> WrapperStatusChanged;

        public void OnWrapperStatusChanged(WrapperStatus wrapperStatus, String message, String supportUrl)
        {
            this.Status = wrapperStatus;

            var status = new WrapperChangedEventArgs(wrapperStatus, message, supportUrl);
            this.WrapperStatusChanged?.Invoke(this, status);
        }
    }
}
