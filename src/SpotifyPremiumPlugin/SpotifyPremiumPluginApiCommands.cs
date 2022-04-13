// Copyright (c) Loupedeck. All rights reserved.

namespace Loupedeck.SpotifyPremiumPlugin
{
    using System;
    using SpotifyAPI.Web;
    using Loupedeck;
    using SpotifyAPI.Web.Auth;
    using SpotifyAPI.Web.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using System.Net;
    using System.Threading.Tasks;
    using System.Timers;
    using SpotifyAPI.Web.Enums;

    /// <summary>
    /// Plugin Spotify API configuration and authorization
    /// </summary>
    public partial class SpotifyPremiumPlugin : Plugin
    {
    //    internal SpotifyWebAPI Api { get; set; }

    //    private async Task<PlaybackContext> CurrentPlayback() => await this.Api.GetPlaybackAsync();

    //    private Int32 _localVolume;

    //    private Timer _volumeBlockedTimer;

    //    private Boolean _volumeBlocked;

    //    public String CurrentDeviceId = String.Empty;

    //    public String CmdTogglePlayback = "Toggle Playback";

    //    private void InitVolumeBlockedTimer()
    //    {
    //        if (this._volumeBlockedTimer == null)
    //        {
    //            this._volumeBlockedTimer = new Timer(2000);
    //            this._volumeBlockedTimer.Elapsed += this.VolumeBlockExpired;
    //        }
    //        this._volumeBlocked = true;
    //        if (this._volumeBlockedTimer.Enabled)
    //        {
    //            this._volumeBlockedTimer.Stop();
    //        }
    //        this._volumeBlockedTimer.Start();
    //    }

    //    private void VolumeBlockExpired(Object o, ElapsedEventArgs e)
    //    {
    //        this._volumeBlocked = false;
    //    }

    //    private void DisposeVolumeBlockedTimer()
    //    {
    //        if (this._volumeBlockedTimer != null)
    //        {
    //            this._volumeBlockedTimer.Elapsed -= this.VolumeBlockExpired;
    //            this._volumeBlockedTimer.Dispose();
    //        }
    //    }

    //    public async Task<ErrorResponse> ExecuteAction(String actionName, String parameter, Int32 diff = 0)
    //    {
    //        if (this.Api == null)
    //        {
    //            this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Login to Spotify", null);
    //            return null;
    //        }

    //        switch (actionName)
    //        {
    //            #region Playback

    //            case "TogglePlayback":
    //                return await this.TogglePlaybackAsync();

    //            case "NextTrack":
    //                return await this.NextTrackAsync();

    //            case "PreviousTrack":
    //                return await this.PreviousTrackAsync();

    //            case "ShufflePlay":
    //                return await this.ShufflePlayAsync();

    //            case "ChangeRepeatState":
    //                return await this.ChangeRepeatStateAsync();

    //            #endregion

    //            #region Volume

    //            case "Mute":
    //                return await this.MuteAsync();

    //            case "Unmute":
    //                return await this.UnmuteAsync();

    //            case "ToggleMute":
    //                return await this.ToggleMuteAsync();

    //            case "SpotifyVolume":
    //                // Todo: Updating local model overwrites latest volume changed value and we obtaining jumping of volume level
    //                return await this.AdjustVolumeAsync(diff);

    //            case "DirectVolume":
    //                return await this.SetVolumeAsync(parameter);

    //            #endregion

    //            #region Playlists

    //            case "StartPlaylist":
    //                return await this.StartPlaylistAsync(parameter);

    //            case "SaveToPlaylist":
    //                return await this.SaveToPlaylistAsync(parameter);

    //            #endregion

    //            #region Other

    //            case "ToggleLike":
    //                return await this.ToggleLikeAsync();

    //            case "SelectDevice":
    //                if (parameter == "activedevice")
    //                {
    //                    parameter = String.Empty;
    //                }
    //                this.CurrentDeviceId = parameter;
    //                this.SaveDeviceToCache(parameter);
    //                return await this.Api.TransferPlaybackAsync(this.CurrentDeviceId, true);

    //            #endregion

    //            default:
    //                Tracer.Trace($"Unknown action '{actionName}'");
    //                break;
    //        }

    //        return null;
    //    }

    //    #region Playback

    //    public async Task<ErrorResponse> TogglePlaybackAsync()
    //    {
    //        var playback = await this.CurrentPlayback();
    //        return playback.IsPlaying
    //            ? await this.Api.PausePlaybackAsync(this.CurrentDeviceId)
    //            : await this.Api.ResumePlaybackAsync(this.CurrentDeviceId, "", null, "", 0);
    //    }

    //    public async Task<ErrorResponse> NextTrackAsync()
    //    {
    //        return await this.Api.SkipPlaybackToNextAsync(this.CurrentDeviceId);
    //    }

    //    public async Task<ErrorResponse> PreviousTrackAsync()
    //    {
    //        return await this.Api.SkipPlaybackToPreviousAsync(this.CurrentDeviceId);
    //    }

    //    public async Task<ErrorResponse> ShufflePlayAsync()
    //    {
    //        var playback = await this.CurrentPlayback();
    //        var response = await this.Api.SetShuffleAsync(!playback.ShuffleState, this.CurrentDeviceId);
    //        this.OnActionImageChanged("ShufflePlay", null);
    //        return response;
    //    }

    //    public async Task<ErrorResponse> ChangeRepeatStateAsync()
    //    {
    //        var playback = await this.CurrentPlayback();
    //        var repeatState = playback.RepeatState;
    //        switch (repeatState)
    //        {
    //            case RepeatState.Off:
    //                repeatState = RepeatState.Context;
    //                break;

    //            case RepeatState.Context:
    //                repeatState = RepeatState.Track;
    //                break;

    //            case RepeatState.Track:
    //                repeatState = RepeatState.Off;
    //                break;
    //        }
    //        var response = await this.Api.SetRepeatModeAsync(repeatState, this.CurrentDeviceId);
    //        this.OnActionImageChanged("ChangeRepeatState", null);

    //        return response;
    //    }

    //    #endregion

    //    #region Volume

    //    public async Task<ErrorResponse> AdjustVolumeAsync(Int32 percents)
    //    {
    //        var modifiedVolume = 0;
    //        if (this._volumeBlocked)
    //        {
    //            modifiedVolume = this._localVolume + percents;
    //        }
    //        else
    //        {
    //            var playback = await this.CurrentPlayback();
    //            if (playback?.Device == null)
    //            {
    //                Tracer.Trace("Spotify Api cannot adjust volume, this.CurrentPlayback.Device is null!");
    //                return null;
    //            }
    //            this.InitVolumeBlockedTimer();
    //            modifiedVolume = playback.Device.VolumePercent + percents;
    //        }

    //        this._localVolume = modifiedVolume;
    //        return await this.SetVolumeAsync(modifiedVolume.ToString());
    //    }

    //    public async Task<ErrorResponse> SetVolumeAsync(String percents)
    //    {
    //        var isConverted = Int32.TryParse(percents, out var volume);
    //        return isConverted ? await this.SetVolumeAsync(volume) : null;
    //    }

    //    public async Task<ErrorResponse> SetVolumeAsync(Int32 percents)
    //    {
    //        if (percents > 100)
    //        {
    //            percents = 100;
    //        }
    //        if (percents < 0)
    //        {
    //            percents = 0;
    //        }

    //        var response = await this.Api.SetVolumeAsync(percents, this.CurrentDeviceId);
    //        return response;
    //    }

    //    public async Task<ErrorResponse> MuteAsync()
    //    {
    //        var playback = await this.Api.GetPlaybackAsync();
    //        if (playback?.Device != null)
    //        {
    //            this.PreviousVolume = playback.Device.VolumePercent;
    //        }
    //        var result = await this.Api.SetVolumeAsync(0, this.CurrentDeviceId);

    //        return result;
    //    }

    //    internal Int32 PreviousVolume { get; set; }

    //    public async Task<ErrorResponse> UnmuteAsync()
    //    {
    //        var unmuteVolume = this.PreviousVolume != 0 ? this.PreviousVolume : 50;
    //        var result = await this.Api.SetVolumeAsync(unmuteVolume, this.CurrentDeviceId);
    //        return result;
    //    }

    //    public async Task<ErrorResponse> ToggleMuteAsync()
    //    {
    //        var playback = await this.Api.GetPlaybackAsync();
    //        if (playback?.Device.VolumePercent != 0)
    //        {
    //            return await this.MuteAsync();
    //        }
    //        else
    //        {
    //            return await this.UnmuteAsync();
    //        }
    //    }

    //    #endregion

    //    #region Playlists

    //    public async Task<ErrorResponse> StartPlaylistAsync(String contextUri)
    //    {
    //        return await this.Api.ResumePlaybackAsync(this.CurrentDeviceId, contextUri, null, String.Empty);
    //    }

    //    public async Task<ErrorResponse> SaveToPlaylistAsync(String playlistId, Boolean idWithUri = true)
    //    {
    //        if (idWithUri)
    //        {
    //            playlistId = playlistId.Replace("spotify:playlist:", String.Empty);
    //        }

    //        var playback = await this.CurrentPlayback();
    //        var currentTrackUri = playback.Item.Uri;
    //        return await this.Api.AddPlaylistTrackAsync(playlistId, currentTrackUri);
    //    }

    //    #endregion

    //    #region Other

    //    public async Task<ErrorResponse> ToggleLikeAsync()
    //    {
    //        var playback = await this.CurrentPlayback();
    //        var trackId = playback.Item?.Id;
    //        if (String.IsNullOrEmpty(trackId))
    //        {
    //            return null;
    //        }
    //        var trackItemId = new List<String> { trackId };
    //        var tracksExist = await this.Api.CheckSavedTracksAsync(trackItemId);
    //        if (tracksExist.List == null && tracksExist.Error != null)
    //        {
    //            return null;
    //        }
    //        var result = tracksExist.List.Any() && tracksExist.List.FirstOrDefault() == false
    //            ? await this.Api.SaveTrackAsync(trackId)
    //            : await this.Api.RemoveSavedTracksAsync(trackItemId);
    //        this.OnActionImageChanged("ToggleLike", null);
    //        return result;
    //    }

    //    #endregion
    }
}
