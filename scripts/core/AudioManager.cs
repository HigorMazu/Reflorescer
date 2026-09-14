using Godot;
using System;
using System.Collections.Generic;

namespace Joguim.Core
{
    public partial class AudioManager : Node
    {
        public static AudioManager Instance { get; private set; }

        private AudioStreamPlayer _musicPlayer;
        private List<AudioStreamPlayer> _sfxPlayers = new();
        private int _maxSfxPlayers = 8;

        private const string MusicPath = "res://assets/audio/music/";
        private const string SfxPath = "res://assets/audio/sfx/";

        public override void _Ready()
        {
            Instance = this;

            _musicPlayer = new AudioStreamPlayer();
            _musicPlayer.Bus = "Music";
            AddChild(_musicPlayer);

            for (int i = 0; i < _maxSfxPlayers; i++)
            {
                var sfxPlayer = new AudioStreamPlayer();
                sfxPlayer.Bus = "SFX";
                AddChild(sfxPlayer);
                _sfxPlayers.Add(sfxPlayer);
            }
        }

        public void PlayMusic(string fileName, float fadeDuration = 1.0f)
        {
            string path = MusicPath + fileName;
            if (!ResourceLoader.Exists(path))
            {
                GD.PrintErr($"AudioManager: Music not found at {path}");
                return;
            }

            var stream = GD.Load<AudioStream>(path);
            if (stream == null) return;

            if (_musicPlayer.Playing)
            {
                var tween = CreateTween();
                tween.TweenProperty(_musicPlayer, "volume_db", -40.0f, fadeDuration / 2f);
                tween.TweenCallback(Callable.From(() =>
                {
                    _musicPlayer.Stream = stream;
                    _musicPlayer.Play();
                    var fadeIn = CreateTween();
                    fadeIn.TweenProperty(_musicPlayer, "volume_db", 0.0f, fadeDuration / 2f);
                }));
            }
            else
            {
                _musicPlayer.Stream = stream;
                _musicPlayer.VolumeDb = 0f;
                _musicPlayer.Play();
            }
        }

        public void StopMusic(float fadeDuration = 1.0f)
        {
            if (!_musicPlayer.Playing) return;

            var tween = CreateTween();
            tween.TweenProperty(_musicPlayer, "volume_db", -40.0f, fadeDuration);
            tween.TweenCallback(Callable.From(() => _musicPlayer.Stop()));
        }

        public void PlaySfx(string fileName, float volumeDb = 0f, float pitchScale = 1.0f)
        {
            string path = SfxPath + fileName;
            if (!ResourceLoader.Exists(path))
            {
                GD.PrintErr($"AudioManager: SFX not found at {path}");
                return;
            }

            var stream = GD.Load<AudioStream>(path);
            if (stream == null) return;

            var player = GetAvailableSfxPlayer();
            if (player == null) return;

            player.Stream = stream;
            player.VolumeDb = volumeDb;
            player.PitchScale = pitchScale;
            player.Play();
        }

        public void PlaySfxAtPosition(string fileName, Vector2 position, float volumeDb = 0f)
        {
            string path = SfxPath + fileName;
            if (!ResourceLoader.Exists(path)) return;

            var stream = GD.Load<AudioStream>(path);
            if (stream == null) return;

            var player = GetAvailableSfxPlayer();
            if (player == null) return;

            player.Stream = stream;
            player.VolumeDb = volumeDb;
            player.Bus = "SFX";
            player.Play();
        }

        private AudioStreamPlayer GetAvailableSfxPlayer()
        {
            foreach (var player in _sfxPlayers)
            {
                if (!player.Playing) return player;
            }
            return _sfxPlayers[0];
        }
    }
}
