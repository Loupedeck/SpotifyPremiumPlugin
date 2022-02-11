namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.Net;
    using System.Linq;
    using System.Timers;
    using System.Collections.Generic;

    using SpotifyAPI.Web;
    using SpotifyAPI.Web.Enums;
    using SpotifyAPI.Web.Models;

    public partial class SpotifyWrapper : IDisposable
    {
        private const String _clientId = "ClientId";
        private const String _clientSecret = "ClientSecret";
        private const String _tcpPorts = "TcpPorts";

        private PrivateProfile _privateProfile;

        //private SpotifyPremiumPlugin Plugin { get; }

        internal SpotifyWebAPI Api { get; set; }

        public RepeatState CachedRepeatState { get; private set; }

        public Boolean CachedShuffleState { get; private set; }

        public Boolean CachedPlayingState { get; private set; }

        public Boolean CachedMuteState { get; private set; }

        public Boolean CachedLikeState { get; private set; }

        internal String CurrentDeviceId
        {
            get => this._currentDeviceId;
            set
            {
                this._currentDeviceId = value;
                this.SaveDeviceToCache(value);
            }
        }

        public SpotifyWrapper(String cacheDirectory)
        {
            this._cacheDirectory = cacheDirectory;
            this.ReadSpotifyConfiguration();
            this.CurrentDeviceId = this.GetCachedDeviceID();
        }

        public void Dispose()
        {
            this.Api?.Dispose();
        }

        #region Volume

        /// <summary>
        /// This is our most recently known Volume. Used when muting to remember the previous volume.  Used for dials when
        /// incrementing rapidly.
        /// </summary>
        internal Int32 PreviousVolume { get; set; }

        public void SetVolume(String volumeString)
        {
            if (Int32.TryParse(volumeString, out var volume))
            {
                this.SetVolume(volume);
            }
        }

        public void SetVolume(Int32 percents)
        {
            if (percents > 100)
            {
                percents = 100;
            }

            if (percents < 0)
            {
                percents = 0;
            }

            this.InitVolumeBlockedTimer();

            this.PreviousVolume = percents;

            ErrorResponse Func() => this.Api.SetVolume(percents, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);
        }

        private Boolean _volumeCallsBlocked;
        private Timer _volumeBlockedTimer;
        private String _currentDeviceId;

        public void AdjustVolume(Int32 ticks)
        {
            var modifiedVolume = 0;

            // Because this can be called in rapid succession with a dial turn, and it take Spotify a bit of time to register
            // volume changes round trip to the api, we don't want to Get the current Volume from Spotify if we've very recently set it
            // a few times.  Thus, we have a 2 second buffer after the last volume set, before we try to get the actual current volume 
            // from Spotify.
            if (this._volumeCallsBlocked)
            {
                modifiedVolume = this.PreviousVolume + ticks;
            }
            else
            {
                var playback = this.Api.GetPlayback();
                if (playback?.Device == null)
                {
                    // Set plugin status and message
                    this.CheckStatusCode(HttpStatusCode.NotFound, "Cannot adjust volume, no device");
                    return;
                }
                else
                {
                    modifiedVolume = playback.Device.VolumePercent + ticks;
                }
            }

            this.SetVolume(modifiedVolume);
        }

        private void InitVolumeBlockedTimer()
        {
            if (this._volumeBlockedTimer == null)
            {
                this._volumeBlockedTimer = new Timer(2000);
                this._volumeBlockedTimer.Elapsed += this.VolumeBlockExpired;
            }

            this._volumeCallsBlocked = true;
            if (this._volumeBlockedTimer.Enabled)
            {
                this._volumeBlockedTimer.Stop();
            }

            this._volumeBlockedTimer.Start();
        }

        private void VolumeBlockExpired(Object o, ElapsedEventArgs e) => this._volumeCallsBlocked = false;

        public void Mute()
        {
            var playback = this.Api.GetPlayback();

            if (playback?.Device?.VolumePercent > 0)
            {
                this.PreviousVolume = playback.Device.VolumePercent;
            }

            ErrorResponse Func() => this.Api.SetVolume(0, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);
        }

        public void Unmute()
        {
            var unmuteVolume = this.PreviousVolume != 0 ? this.PreviousVolume : 50;

            ErrorResponse Func() => this.Api.SetVolume(unmuteVolume, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);
        }

        /// <summary>
        /// Toggle current Mute setting
        /// </summary>
        /// <returns>true if muted after this call</returns>
        public Boolean ToggleMute()
        {
            var playback = this.Api.GetPlayback();

            if (playback?.Device.VolumePercent != 0)
            {
                this.Mute();
                return this.CachedMuteState = true;
            }
            else
            {
                this.Unmute();
                return this.CachedMuteState = false;
            }
        }

        #endregion Volume

        #region Playback

        public const String _activeDevice = "activedevice";

        public void TransferPlayback(String commandParameter)
        {
            if (commandParameter == _activeDevice)
            {
                commandParameter = String.Empty;
            }

            this.CurrentDeviceId = commandParameter;

            ErrorResponse Func() => this.Api.TransferPlayback(this.CurrentDeviceId, true);
            this.CheckSpotifyResponse(Func);
        }

        public Boolean TogglePlayback()
        {
            var playback = this.Api.GetPlayback();

            if (playback.IsPlaying)
            {
                this.CheckSpotifyResponse(this.Api.PausePlayback, this.CurrentDeviceId);
            }
            else
            {
                ErrorResponse Func() => this.Api.ResumePlayback(this.CurrentDeviceId, String.Empty, null, String.Empty, 0);
                this.CheckSpotifyResponse(Func);
            }

            return this.CachedPlayingState = !playback.IsPlaying; // presume we switched it at this point.
        }

        public void SkipPlaybackToNext() => this.CheckSpotifyResponse(this.Api.SkipPlaybackToNext, this.CurrentDeviceId);

        public void SkipPlaybackToPrevious() => this.CheckSpotifyResponse(this.Api.SkipPlaybackToPrevious, this.CurrentDeviceId);
        
        public RepeatState ChangeRepeatState()
        {
            var playback = this.Api.GetPlayback();

            RepeatState newRepeatState = RepeatState.Off;

            switch (playback.RepeatState)
            {
                case RepeatState.Off:
                    newRepeatState = RepeatState.Context;
                    break;

                case RepeatState.Context:
                    newRepeatState = RepeatState.Track;
                    break;

                case RepeatState.Track:
                    newRepeatState = RepeatState.Off;
                    break;

                default:
                    // Set plugin status and message
                    this.CheckStatusCode(HttpStatusCode.NotFound, "Not able to change repeat state (check device etc.)");
                    break;
            }

            this.CachedRepeatState = newRepeatState;

            ErrorResponse Func() => this.Api.SetRepeatMode(newRepeatState, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);

            return newRepeatState;
        }

        public Boolean ShufflePlay()
        {
            var playback = this.Api.GetPlayback();
            var shuffleState = !playback.ShuffleState;

            ErrorResponse Func() => this.Api.SetShuffle(shuffleState, this.CurrentDeviceId);
            this.CheckSpotifyResponse(Func);

            this.CachedShuffleState = shuffleState;

            return shuffleState;
        }

        #endregion Playback

        #region Playlists

        public void StartPlaylist(String contextUri)
        {
            ErrorResponse Func() => this.Api.ResumePlayback(this.CurrentDeviceId, contextUri, null, String.Empty);
            this.CheckSpotifyResponse(Func);
        }

        public void SaveToPlaylist(String playlistId)
        {
            playlistId = playlistId.Replace("spotify:playlist:", String.Empty);

            var playback = this.Api.GetPlayback();
            var currentTrackUri = playback.Item.Uri;

            ErrorResponse Func() => this.Api.AddPlaylistTrack(playlistId, currentTrackUri);
            this.CheckSpotifyResponse(Func);
        }

        public List<SimplePlaylist> GetAllPlaylists()
        {
            Paging<SimplePlaylist> playlists = this.GetUserPlaylists();
            if (playlists != null)
            {
                var totalPlaylistsCount = playlists.Total;
                while (playlists.Items.Count < totalPlaylistsCount)
                {
                    playlists.Items.AddRange(this.GetUserPlaylists(playlists.Items.Count).Items);
                }

                return playlists.Items;
            }

            return null;
        }

        private Paging<SimplePlaylist> GetUserPlaylists(Int32 offset = 0)
        {
            if (this.Api != null)
            {
                try
                {
                    if (this._privateProfile == null)
                    {
                        this._privateProfile = this.Api.GetPrivateProfile();
                    }

                    var profileId = this._privateProfile?.Id;
                    if (!String.IsNullOrEmpty(profileId))
                    {
                        var playlists = this.Api.GetUserPlaylists(profileId, 50, offset);
                        if (playlists?.Items?.Any() == true)
                        {
                            return playlists;
                        }
                    }
                }
                catch (Exception e)
                {
                    Tracer.Trace(e, "Error obtaining Spotify playlists");
                }
            }

            return new Paging<SimplePlaylist>
            {
                Items = new List<SimplePlaylist>(),
            };
        }

        #endregion Playlists

        #region Devices

        public List<Device> GetDevices()
        {
            var devices = this.Api?.GetDevices()?.Devices;

            if (devices?.Any() == true)
            {
                devices.Add(new Device { Id = _activeDevice, Name = "Active Device" });
            }

            return devices;
        }

        #endregion Devices

        #region Likes

        public Boolean ToggleLike()
        {
            var playback = this.Api.GetPlayback();
            var trackId = playback.Item?.Id;
            if (String.IsNullOrEmpty(trackId))
            {
                // Set plugin status and message
                this.CheckStatusCode(HttpStatusCode.NotFound, "No track");
                return this.CachedMuteState = false;
            }

            var trackItemId = new List<String> { trackId };
            var tracksExist = this.Api.CheckSavedTracks(trackItemId);
            if (tracksExist.List == null && tracksExist.Error != null)
            {
                // Set plugin status and message
                this.CheckStatusCode(HttpStatusCode.NotFound, "No track list");
                return this.CachedMuteState = false;
            }

            if (tracksExist.List.Any() && tracksExist.List.FirstOrDefault() == false)
            {
                this.CheckSpotifyResponse(this.Api.SaveTrack, trackId);
                return this.CachedMuteState = true;
            }
            else
            {
                this.CheckSpotifyResponse(this.Api.RemoveSavedTracks, trackItemId);
                return this.CachedMuteState = false;
            }
        }

        #endregion Likes

    }
}