namespace Loupedeck.SpotifyPremiumPlugin.Wrapper
{
    using System;
    using System.Collections.Generic;

    internal class WrapperConfigurationModel
    {
        public String ClientId { get; set; }

        public String SecretId { get; set; }

        public List<Int32> Ports { get; set; }
    }
}
