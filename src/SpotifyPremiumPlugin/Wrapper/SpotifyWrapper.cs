namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;

    using SpotifyAPI.Web;

    public partial class SpotifyWrapper
    {
        public SpotifyClient Client { get; private set; }

        public PlayerSetRepeatRequest.State CachedRepeatState { get; private set; }

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

        public SpotifyWrapper(String cacheDirectory) => this._cacheDirectory = cacheDirectory;

        public void Init()
        {
            this.StartAuth();
            this.CurrentDeviceId = this.GetCachedDeviceID();
        }

        /// <summary>
        /// Gets or sets Volume. Used when muting to remember the previous volume.  Used for dials when
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

            var request = new PlayerVolumeRequest(percents) { DeviceId = this._currentDeviceId };
            this.CheckSpotifyResponse(this.Client.Player.SetVolume, request);
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
                var playback = this.GetCurrentPlayback();
                if (playback?.Device == null)
                {
                    // Set plugin status and message
                    this.OnWrapperStatusChanged(WrapperStatus.Warning, "Cannot adjust volume, no device", null);
                    return;
                }
                else
                {
                    modifiedVolume = (Int32)playback.Device.VolumePercent + ticks;
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
            var playback = this.GetCurrentPlayback();
            var volumePercent = playback?.Device?.VolumePercent;
            if (volumePercent > 0)
            {
                this.PreviousVolume = (Int32)volumePercent;
            }

            this.SetVolume(0);
        }

        public void Unmute()
        {
            var unmuteVolume = this.PreviousVolume != 0 ? this.PreviousVolume : 50;

            this.SetVolume(unmuteVolume);
        }

        /// <summary>
        /// Toggle current Mute setting
        /// </summary>
        /// <returns>true if muted after this call</returns>
        public Boolean ToggleMute()
        {
            var playback = this.GetCurrentPlayback();

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

        public const String _activeDevice = "activedevice";

        public void TransferPlayback(String commandParameter)
        {
            if (commandParameter == _activeDevice)
            {
                commandParameter = null;
            }

            this.CurrentDeviceId = commandParameter;

            this.CheckSpotifyResponse(this.Client.Player.TransferPlayback, new PlayerTransferPlaybackRequest(new[] { this.CurrentDeviceId }));
        }

        public void TogglePlayback()
        {
            var playback = this.Client.Player.GetCurrentPlayback().Result;
            if (playback.IsPlaying)
            {
                this.Client.Player.PausePlayback(new PlayerPausePlaybackRequest() { DeviceId = this.CurrentDeviceId });
            }
            else
            {
                this.Client.Player.ResumePlayback(new PlayerResumePlaybackRequest() { DeviceId = this.CurrentDeviceId });
            }

            this.CachedPlayingState = !playback.IsPlaying; // presume we switched it at this point.
        }

        public void SkipPlaybackToNext() => this.CheckSpotifyResponse(this.Client.Player.SkipNext);

        public void SkipPlaybackToPrevious() => this.CheckSpotifyResponse(this.Client.Player.SkipPrevious);

        public PlayerSetRepeatRequest.State ChangeRepeatState()
        {
            var playback = this.GetCurrentPlayback();

            var newRepeatState = PlayerSetRepeatRequest.State.Off;

            switch (playback.RepeatState.ToLower())
            {
                case "off":
                    newRepeatState = PlayerSetRepeatRequest.State.Context;
                    break;

                case "context":
                    newRepeatState = PlayerSetRepeatRequest.State.Track;
                    break;

                case "track":
                    newRepeatState = PlayerSetRepeatRequest.State.Off;
                    break;

                default:
                    this.OnWrapperStatusChanged(WrapperStatus.Warning, "Not able to change repeat state (check device etc.)", null);
                    break;
            }

            this.CachedRepeatState = newRepeatState;

            this.CheckSpotifyResponse(this.Client.Player.SetRepeat, new PlayerSetRepeatRequest(newRepeatState) { DeviceId = this.CurrentDeviceId });

            return newRepeatState;
        }

        public Boolean ShufflePlay()
        {
            var playback = this.GetCurrentPlayback();
            var shuffleState = !playback.ShuffleState;

            this.CheckSpotifyResponse(this.Client.Player.SetShuffle, new PlayerShuffleRequest(shuffleState) { DeviceId = this.CurrentDeviceId });

            this.CachedShuffleState = shuffleState;

            return shuffleState;
        }

        public void StartPlaylist(String contextUri) =>
            this.CheckSpotifyResponse(this.Client.Player.ResumePlayback, new PlayerResumePlaybackRequest() { ContextUri = contextUri, DeviceId = this.CurrentDeviceId });

        public void SaveToPlaylist(String playlistId)
        {
            playlistId = playlistId.Replace("spotify:playlist:", String.Empty);

            var playbackResponse = this.GetCurrentPlayback();
            if (playbackResponse.Item is FullTrack track)
            {
                SnapshotResponse GetPlayistAddResponse() => this.Client.Playlists.AddItems(playlistId, new PlaylistAddItemsRequest(new[] { track.Uri })).Result;
                this.CheckSpotifyResponse(GetPlayistAddResponse);
            }
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
            var userProfile = this.CheckSpotifyResponse(this.Client.UserProfile.Current).Result;
            Paging<SimplePlaylist> GetUsers() => this.Client.Playlists.GetUsers(userProfile.Id, new PlaylistGetUsersRequest() { Limit = 50, Offset = offset }).Result;
            return this.CheckSpotifyResponse(GetUsers);
        }

        public List<Device> GetDevices()
        {
            var devicesResponse = this.CheckSpotifyResponse(this.Client.Player.GetAvailableDevices).Result;
            var devices = devicesResponse.Devices;

            if (devices?.Any() == true)
            {
                devices.Add(new Device { Id = _activeDevice, Name = "Active Device" });
            }

            return devices;
        }

        public void ToggleLike()
        {
            var currentlyPlaying = this.GetCurrentPlayback();
            switch (currentlyPlaying.Item)
            {
                case FullTrack track:
                    var trackId = new[] { track.Id };
                    var trackLikedResponse = this.CheckSpotifyResponse(this.Client.Library.CheckTracks, new LibraryCheckTracksRequest(trackId));
                    var trackLiked = trackLikedResponse.Result.FirstOrDefault();
                    if (trackLiked)
                    {
                        this.CheckSpotifyResponse(this.Client.Library.RemoveTracks, new LibraryRemoveTracksRequest(trackId));
                    }
                    else
                    {
                        this.CheckSpotifyResponse(this.Client.Library.SaveTracks, new LibrarySaveTracksRequest(trackId));
                    }
                    break;

                case FullEpisode episode:
                    var episodeId = new[] { episode.Id };
                    var episodeLikedResponse = this.CheckSpotifyResponse(this.Client.Library.CheckEpisodes, new LibraryCheckEpisodesRequest(episodeId));
                    var episodeLiked = episodeLikedResponse.Result.FirstOrDefault();
                    if (episodeLiked)
                    {
                        this.CheckSpotifyResponse(this.Client.Library.RemoveEpisodes, new LibraryRemoveEpisodesRequest(episodeId));
                    }
                    else
                    {
                        this.CheckSpotifyResponse(this.Client.Library.SaveEpisodes, new LibrarySaveEpisodesRequest(episodeId));
                    }
                    break;
            }
        }

        public CurrentlyPlayingContext GetCurrentPlayback() =>
            this.CheckSpotifyResponse(this.Client.Player.GetCurrentPlayback).Result;
    }
}