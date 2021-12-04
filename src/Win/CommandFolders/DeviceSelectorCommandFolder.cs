// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.Plugins.SpotifyPremium.CommandFolders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SpotifyAPI.Web.Models;

    /// <summary>
    /// Dynamic folder (control center) for Spotify devices. https://developer.loupedeck.com/docs/Actions-taxonomy
    /// </summary>
    internal class DeviceSelectorCommandFolder : PluginDynamicFolder
    {
        private List<Device> _devices;

        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;
        private SpotifyWrapper Wrapper => this.SpotifyPremiumPlugin.Wrapper;

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
            this._devices = this.Wrapper.GetDevices();

            return this._devices?.Any() == true ? this._devices.Select(x => this.CreateCommandName(x.Id)) : new List<String>();
        }

        public override String GetCommandDisplayName(String commandParameter, PluginImageSize imageSize)
        {
            var deviceDisplayName = this._devices.FirstOrDefault(x => x.Id == commandParameter)?.Name;
            if (deviceDisplayName?.Contains(" ") == false && deviceDisplayName.Length > 9)
            {
                var updatedDisplayName = deviceDisplayName.Insert(9, "\n");
                return updatedDisplayName.Length > 18 ? updatedDisplayName.Insert(18, "\n") : updatedDisplayName;
            }

            return deviceDisplayName;
        }

        public override void RunCommand(String commandParameter)
        {
            this.Wrapper.TransferPlayback(commandParameter);
        }
    }
}