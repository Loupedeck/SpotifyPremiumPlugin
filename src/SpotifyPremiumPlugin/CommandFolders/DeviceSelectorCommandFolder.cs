// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin.CommandFolders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SpotifyAPI.Web;

    /// <summary>
    /// Dynamic folder (control center) for Spotify devices. https://developer.loupedeck.com/docs/Actions-taxonomy
    /// </summary>
    internal class DeviceSelectorCommandFolder : PluginDynamicFolder
    {
        private List<Device> _devices;

        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        public DeviceSelectorCommandFolder()
        {
            this.DisplayName = "Devices";
            this.GroupName = "Others";
            this.Navigation = PluginDynamicFolderNavigation.EncoderArea;
        }

        public override BitmapImage GetButtonImage(PluginImageSize imageSize)
        {
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width80.Devices.png");
            return bitmapImage;
        }

        public override IEnumerable<String> GetButtonPressActionNames()
        {
            this._devices = this.SpotifyPremiumPlugin.Wrapper.GetDevices();
            if (this._devices != null && this._devices.Any())
            {
                return this._devices.Select(x => this.CreateCommandName(x.Id));
            }

            return new List<String>();
        }

        public override String GetCommandDisplayName(String commandParameter, PluginImageSize imageSize)
        {
            var deviceDisplayName = this._devices.FirstOrDefault(x => x.Id == commandParameter)?.Name;
            if (deviceDisplayName != null && !deviceDisplayName.Contains(" ") && deviceDisplayName.Length > 9)
            {
                var updatedDisplayName = deviceDisplayName.Insert(9, "\n");
                return updatedDisplayName.Length > 18 ? updatedDisplayName.Insert(18, "\n") : updatedDisplayName;
            }

            return deviceDisplayName;
        }

        public override void RunCommand(String commandParameter) => this.SpotifyPremiumPlugin.Wrapper.TransferPlayback(commandParameter);
    }
}