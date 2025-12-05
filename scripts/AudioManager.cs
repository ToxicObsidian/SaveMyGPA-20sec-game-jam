using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    [ExportGroup("Bus Settings")]
    [Export]
    protected string MasterBusName = "Master";
    [Export]
    protected string SFXBusName = "SFX";
    [Export]
    protected string BGMBusName = "BGM";
    [Export(PropertyHint.Range, "1,20,")]
    protected int SFXBusCount = 8;
    [ExportGroup("Tracks")]
    [Export]
    protected Godot.Collections.Dictionary<string, AudioStream> BGMTracks = new();
    [Export]
    protected Godot.Collections.Dictionary<string, AudioStream> SFXTracks = new();

    protected AudioStreamPlayer _bgm_player;
    protected List<AudioStreamPlayer> _sfx_players = new();
    protected AudioStream _current_bgm = null;

    public override void _Ready()
    {
        if (Instance == null) Instance = this;

        _bgm_player = new AudioStreamPlayer();
        _bgm_player.Bus = BGMBusName;
        AddChild(_bgm_player);

        for (int i = 0; i < SFXBusCount; i++)
        {
            var _sfx = new AudioStreamPlayer();
            _sfx.Bus = SFXBusName;
            _sfx_players.Add(_sfx);
            AddChild(_sfx);
        }
    }


    public void PlayBGM(string bgm_name)
    {
        if (BGMTracks.ContainsKey(bgm_name))
        {
            var _cur_stream = _bgm_player.Stream;
            var _tar_stream = BGMTracks[bgm_name];
            if (_cur_stream == _tar_stream && _bgm_player.Playing) return;


            if (_tar_stream is AudioStreamOggVorbis ogg_stream) ogg_stream.Loop = true;
            else if (_tar_stream is AudioStreamWav wav_stream) wav_stream.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
            else if (_tar_stream is AudioStreamMP3 mp3_stream) mp3_stream.Loop = true;
            _bgm_player.Stop();
            _bgm_player.Stream = _tar_stream;
            _bgm_player.Play();
        }
    }

    public void PlaySFX(string sfx_name)
    {
        if (!SFXTracks.ContainsKey(sfx_name)) return;

        var _tar_track = SFXTracks[sfx_name];
        foreach (var channel in _sfx_players)
        {
            if (!channel.Playing)
            {
                channel.Stream = _tar_track;
                channel.StreamPaused = false;
                channel.Play();
                return;
            }
        }

        _sfx_players[0].Stream = _tar_track;
        _sfx_players[0].StreamPaused = false;
        _sfx_players[0].Play();
    }


    public void PauseBGM()
    {
        _bgm_player.StreamPaused = true;
    }
    public void ResumeBGM()
    {
        _bgm_player.StreamPaused = false;
    }



    public void SetMasterVolume(float volume)
    {
        _SetBusVolume(MasterBusName, volume);
    }
    public float GetMasterVolumeLinear()
    {
        return _GetBusVolumeLinear(MasterBusName);
    }

    public void SetSFXVolume(float volume)
    {
        _SetBusVolume(SFXBusName, volume);
    }
    public float GetSFXVolumeLinear()
    {
        return _GetBusVolumeLinear(SFXBusName);
    }

    public void SetBGMVolume(float volume)
    {
        _SetBusVolume(BGMBusName, volume);
    }
    public float GetBGMVolumeLinear()
    {
        return _GetBusVolumeLinear(BGMBusName);
    }

    protected void _SetBusVolume(string bus_name, float linear_volume)
    {
        linear_volume = Mathf.Clamp(linear_volume, 0.0f, 1.0f);
        AudioServer.SetBusVolumeLinear(AudioServer.GetBusIndex(bus_name), linear_volume);
    }
    protected float _GetBusVolumeLinear(string bus_name)
    {
        return AudioServer.GetBusVolumeLinear(AudioServer.GetBusIndex(bus_name));
    }
}
