// Copyright(c) Loupedeck.All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.Timers;
    using SpotifyAPI.Web.Models;

    internal class SpotifyVolumeAdjustment : PluginDynamicAdjustment
    {
        private SpotifyPremiumPlugin SpotifyPremiumPlugin => this.Plugin as SpotifyPremiumPlugin;

        private Boolean _volumeBlocked;

        private Timer _volumeBlockedTimer;

        public SpotifyVolumeAdjustment()
            : base(
                "Spotify Volume",
                "Spotify Volume description",
                "Spotify Volume",
                true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            try
            {
                var modifiedVolume = 0;
                if (this._volumeBlocked)
                {
                    modifiedVolume = this.SpotifyPremiumPlugin.PreviousVolume + ticks;
                }
                else
                {
                    var playback = this.SpotifyPremiumPlugin.Api.GetPlayback();
                    if (playback?.Device == null)
                    {
                        // Set plugin status and message
                        this.SpotifyPremiumPlugin.CheckStatusCode(System.Net.HttpStatusCode.NotFound, "Cannot adjust volume, no device");
                        return;
                    }
                    else
                    {
                        this.InitVolumeBlockedTimer();
                        modifiedVolume = playback.Device.VolumePercent + ticks;
                    }
                }

                this.SpotifyPremiumPlugin.PreviousVolume = modifiedVolume;
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.SetVolume, modifiedVolume);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify SpotifyVolumeAdjustment action obtain an error: ", e);
            }
        }

        // Overwrite the RunCommand method that is called every time the user presses the encoder to which this command is assigned
        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.SpotifyPremiumPlugin.CheckSpotifyResponse(this.TogglePlayback);
            }
            catch (Exception e)
            {
                Tracer.Trace($"Spotify SpotifyVolumeAdjustment action obtain an error: ", e);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            // Dial strip 50px
            var bitmapImage = EmbeddedResources.ReadImage("Loupedeck.SpotifyPremiumPlugin.Icons.Width50.Volume.png");
            return bitmapImage;
        }

        public ErrorResponse TogglePlayback()
        {
            var playback = this.SpotifyPremiumPlugin.Api.GetPlayback();
            return playback.IsPlaying
                ? this.SpotifyPremiumPlugin.Api.PausePlayback(this.SpotifyPremiumPlugin.CurrentDeviceId)
                : this.SpotifyPremiumPlugin.Api.ResumePlayback(this.SpotifyPremiumPlugin.CurrentDeviceId, String.Empty, null, String.Empty, 0);
        }

        private void InitVolumeBlockedTimer()
        {
            if (this._volumeBlockedTimer == null)
            {
                this._volumeBlockedTimer = new Timer(2000);
                this._volumeBlockedTimer.Elapsed += this.VolumeBlockExpired;
            }

            this._volumeBlocked = true;
            if (this._volumeBlockedTimer.Enabled)
            {
                this._volumeBlockedTimer.Stop();
            }

            this._volumeBlockedTimer.Start();
        }

        public ErrorResponse SetVolume(Int32 percents)
        {
            if (percents > 100)
            {
                percents = 100;
            }

            if (percents < 0)
            {
                percents = 0;
            }

            var response = this.SpotifyPremiumPlugin.Api.SetVolume(percents, this.SpotifyPremiumPlugin.CurrentDeviceId);
            return response;
        }

        private void VolumeBlockExpired(Object o, ElapsedEventArgs e) => this._volumeBlocked = false;
    }
}
