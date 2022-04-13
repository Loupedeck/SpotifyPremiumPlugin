namespace Loupedeck.SpotifyPremiumPlugin.Wrapper
{
    using System;
    using System.Collections.Generic;

    internal class WrapperConfigurationModel
    {
        public String ClientId { get; set; }

        public String ClientSecret { get; set; }

        public List<Int32> TcpPorts { get; set; }
    }
}
