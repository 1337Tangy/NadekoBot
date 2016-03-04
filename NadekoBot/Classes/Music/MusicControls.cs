﻿using Discord;
using Discord.Audio;
using NadekoBot.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace NadekoBot.Classes.Music {

    public enum MusicType {
        Radio,
        Normal,
        Local
    }

    public enum StreamState {
        Resolving,
        Queued,
        Buffering, //not using it atm
        Playing,
        Completed
    }

    public class MusicPlayer {
        public static int MaximumPlaylistSize => 50;

        private IAudioClient audioClient { get; set; }

        private readonly List<Song> playlist = new List<Song>();
        public IReadOnlyCollection<Song> Playlist => playlist;
        private readonly object playlistLock = new object();

        public Song CurrentSong { get; set; } = default(Song);
        private CancellationTokenSource SongCancelSource { get; set; }
        private CancellationToken cancelToken { get; set; }

        public bool Paused { get; set; }

        public float Volume { get; private set; }

        public Action<Song> OnCompleted = delegate { };
        public Action<Song> OnStarted = delegate { };

        public Channel PlaybackVoiceChannel { get; private set; }

        private bool Stopped { get; set; } = false;

        public MusicPlayer(Channel startingVoiceChannel, float? defaultVolume) {
            if (startingVoiceChannel == null)
                throw new ArgumentNullException(nameof(startingVoiceChannel));
            if (startingVoiceChannel.Type != ChannelType.Voice)
                throw new ArgumentException("Channel must be of type voice");
            Volume = defaultVolume ?? 1.0f;

            PlaybackVoiceChannel = startingVoiceChannel;
            SongCancelSource = new CancellationTokenSource();
            cancelToken = SongCancelSource.Token;

            Task.Run(async () => {
                while (!Stopped) {
                    try {
                        audioClient = await PlaybackVoiceChannel.JoinAudio();
                    }
                    catch {
                        await Task.Delay(1000);
                        continue;
                    }
                    CurrentSong = GetNextSong();
                    if (CurrentSong != null) {
                        try {
                            OnStarted(CurrentSong);
                            await CurrentSong.Play(audioClient, cancelToken);
                        }
                        catch (OperationCanceledException) {
                            Console.WriteLine("Song canceled");
                        }
                        catch (Exception ex) {
                            Console.WriteLine($"Exception in PlaySong: {ex}");
                        }
                        try {
                            OnCompleted(CurrentSong);
                        }
                        catch { }
                        SongCancelSource = new CancellationTokenSource();
                        cancelToken = SongCancelSource.Token;
                    }
                    await Task.Delay(1000);
                }
            });
        }

        public void Next() {
            lock (playlistLock) {
                if (!SongCancelSource.IsCancellationRequested) {
                    Paused = false;
                    SongCancelSource.Cancel();
                }
            }
        }

        public void Stop() {
            lock (playlistLock) {
                playlist.Clear();
                if (!SongCancelSource.IsCancellationRequested)
                    SongCancelSource.Cancel();
            }
        }

        public void TogglePause() => Paused = !Paused;

        public void Shuffle() {
            lock (playlistLock) {
                playlist.Shuffle();
            }
        }

        public int SetVolume(int volume) {
            if (volume < 0)
                volume = 0;
            if (volume > 150)
                volume = 150;

            Volume = volume / 100.0f;
            return volume;
        }

        private Song GetNextSong() {
            lock (playlistLock) {
                if (playlist.Count == 0)
                    return null;
                var toReturn = playlist[0];
                playlist.RemoveAt(0);
                return toReturn;
            }
        }

        public void AddSong(Song s) {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            lock (playlistLock) {
                playlist.Add(s);
            }
        }

        public void RemoveSong(Song s) {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            lock (playlistLock) {
                playlist.Remove(s);
            }
        }

        public void RemoveSongAt(int index) {
            lock (playlistLock) {
                if (index < 0 || index >= playlist.Count)
                    throw new ArgumentException("Invalid index");
                playlist.RemoveAt(index);
            }
        }

        internal Task MoveToVoiceChannel(Channel voiceChannel) {
            if (audioClient?.State != ConnectionState.Connected)
                throw new InvalidOperationException("Can't move while bot is not connected to voice channel.");
            PlaybackVoiceChannel = voiceChannel;
            return PlaybackVoiceChannel.JoinAudio();
        }

        internal void ClearQueue() {
            lock (playlistLock) {
                playlist.Clear();
            }
        }

        public void Destroy() {
            lock (playlistLock) {
                playlist.Clear();
                if (!SongCancelSource.IsCancellationRequested)
                    SongCancelSource.Cancel();
                try {
                    Stopped = true;
                    audioClient.Disconnect();
                }
                catch {}
            }
        }
    }
}
