// Copyright (c) Loupedeck. All rights reserved.

#region Usings

using System.Collections.Generic;
using System.Linq;

using SpotifyAPI.Web.Models;

#endregion

namespace Loupedeck.Plugins.SpotifyPremium.CommandFolders
{
    /// <summary>
    /// Dynamic folder (control center) for Spotify devices. https://developer.loupedeck.com/docs/Actions-taxonomy
    /// </summary>
    internal class DeviceSelectorCommandFolder : PluginDynamicFolder
    {
        private List<Device> _devices;

        private SpotifyPremiumPlugin SpotifyPremiumPlugin => Plugin as SpotifyPremiumPlugin;
        private SpotifyWrapper Wrapper => SpotifyPremiumPlugin.Wrapper;

        public DeviceSelectorCommandFolder()
        {
            DisplayName = "Devices";
            GroupName = "Others";
            Navigation = PluginDynamicFolderNavigation.EncoderArea;
        }

        public override BitmapImage GetButtonImage(PluginImageSize imageSize)
        {
            BitmapImage bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Devices.png");
            return bitmapImage;
        }

        public override IEnumerable<string> GetButtonPressActionNames()
        {
            _devices = Wrapper.GetDevices();

            return _devices?.Any() == true ? _devices.Select(x => CreateCommandName(x.Id)) : new List<string>();
        }

        public override string GetCommandDisplayName(string commandParameter, PluginImageSize imageSize)
        {
            string deviceDisplayName = _devices.FirstOrDefault(x => x.Id == commandParameter)?.Name;

            if (deviceDisplayName?.Contains(" ") == false && deviceDisplayName.Length > 9)
            {
                string updatedDisplayName = deviceDisplayName.Insert(9, "\n");
                return updatedDisplayName.Length > 18 ? updatedDisplayName.Insert(18, "\n") : updatedDisplayName;
            }

            return deviceDisplayName;
        }

        public override void RunCommand(string commandParameter)
        {
            Wrapper.TransferPlayback(commandParameter);
        }
    }
}