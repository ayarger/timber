using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ArborAudioManager : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    static ArborAudioManager instance;

    static List<AudioStreamPlayer> sfx_audio_players = new List<AudioStreamPlayer>();

    static AudioStreamPlayer bgm_audio_player1 = new AudioStreamPlayer();
    static AudioStreamPlayer bgm_audio_player2 = new AudioStreamPlayer();
    static bool bgm_audio_flipper = false;

    public static float master_volume = 1;
    static float min_volume = -60; // Volume goes from min_volume to 0

    static AudioStreamPlayer GetCurrentBGMPlayer()
    {
        if (!bgm_audio_flipper)
            return bgm_audio_player1;
        else
            return bgm_audio_player2;
    }
    static AudioStreamPlayer GetOtherBGMPlayer()
    {
        if (bgm_audio_flipper)
            return bgm_audio_player1;
        else
            return bgm_audio_player2;
    }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        instance = this;

        bgm_audio_player1 = new AudioStreamPlayer();
        AddChild(bgm_audio_player1);
        bgm_audio_player2 = new AudioStreamPlayer();
        AddChild(bgm_audio_player2);
    }

    static AudioStreamPlayer GetAvailableSFXAudioPlayer()
    {
        /* Find an audio player that is finished playing */
        foreach (AudioStreamPlayer audio_player in sfx_audio_players)
        {
            if (!audio_player.Playing)
                return audio_player;
        }

        /* Add a new audio player */
        AudioStreamPlayer new_audio_player = new AudioStreamPlayer();
        new_audio_player.VolumeDb = min_volume + master_volume * (-min_volume);
        instance.AddChild(new_audio_player);
        sfx_audio_players.Add(new_audio_player);
        return new_audio_player;
    }

    public static AudioStreamPlayer RequestSFX(AudioStream stream)
    {
        if (stream == null)
            return null;

        AudioStreamPlayer sfx_audio_player = GetAvailableSFXAudioPlayer();
        sfx_audio_player.Stream = stream;
        sfx_audio_player.Play();
        return sfx_audio_player;
    }

    public static AudioStreamPlayer RequestBGM(AudioStream stream, bool start_loud = false, bool loop = true)
    {
        AudioStreamPlayer current_bgm_player = GetCurrentBGMPlayer();
        AudioStreamPlayer other_bgm_player = GetOtherBGMPlayer();
        bgm_audio_flipper = !bgm_audio_flipper;

        ArborCoroutine.StartCoroutine(DoRequestBGM(current_bgm_player, other_bgm_player, stream, start_loud, loop), instance);

        return current_bgm_player;
    }

    static IEnumerator DoRequestBGM(AudioStreamPlayer current_bgm_player, AudioStreamPlayer other_bgm_player, AudioStream stream, bool start_loud, bool loop)
    {
        ArborCoroutine.StartCoroutine(FadeOut(other_bgm_player), instance);

        yield return ArborCoroutine.WaitForSeconds(0.25f);

        ArborCoroutine.StartCoroutine(FadeIn(current_bgm_player, stream, start_loud: start_loud, loop: loop), instance);

    }

    static IEnumerator FadeIn(AudioStreamPlayer player, AudioStream stream, bool start_loud = false, bool loop = true)
    {
        if (loop)
        {
            if (stream is AudioStreamOGGVorbis)
            {
                ((AudioStreamOGGVorbis)stream).Loop = true;
            }
            else if (stream is AudioStreamSample)
            {
                ((AudioStreamSample)stream).LoopMode = AudioStreamSample.LoopModeEnum.Forward;
            }
        }

        player.Stop();
        player.Stream = stream;
        if (!start_loud)
            player.VolumeDb = min_volume;
        player.Play();

        if (!start_loud)
        {
            void DoIncreaseVolume(float progress)
            {
                player.VolumeDb = (min_volume + master_volume * (-min_volume) * (progress));
            }
            yield return ArborCoroutine.DoOverTime(DoIncreaseVolume, 1.0f);
        }
    }

    static IEnumerator FadeOut(AudioStreamPlayer player)
    {
        void DoDecreaseVolume(float progress)
        {
            player.VolumeDb = min_volume + (-min_volume) * master_volume + (min_volume * master_volume) * (progress);
        }
        yield return ArborCoroutine.DoOverTime(DoDecreaseVolume, 1.0f);

        player.VolumeDb = min_volume;
        player.Stop();
    }

    public static void SetMasterVolume(float volume)
    {
        master_volume = volume / 100;

        foreach (AudioStreamPlayer audio_player in sfx_audio_players)
        {
            audio_player.VolumeDb = min_volume + master_volume * (-min_volume);
        }

        AudioStreamPlayer current_bgm_player = GetCurrentBGMPlayer();
        AudioStreamPlayer other_bgm_player = GetOtherBGMPlayer();

        current_bgm_player.VolumeDb = min_volume + master_volume * (-min_volume);
        other_bgm_player.VolumeDb = min_volume + master_volume * (-min_volume);
    }

}