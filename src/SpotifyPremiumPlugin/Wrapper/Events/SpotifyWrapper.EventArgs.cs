namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;

    public class WrapperChangedEventArgs : EventArgs
    {
        public WrapperStatus WrapperStatus { get; set; }

        public String Message { get; set; }

        public String SupportUrl { get; set; }

        public WrapperChangedEventArgs(WrapperStatus wrapperStatus, String message, String supportUrl)
        {
            this.WrapperStatus = wrapperStatus;
            this.Message = message;
            this.SupportUrl = supportUrl;
        }
    }

    public enum WrapperStatus
    {
        Unknown = 0,
        Normal = 1,
        Warning = 2,
        Error = 3,
    }
}
