using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class Sound
{
    public string name;

    [Range(0,1)]
    public float volume;

    [Range(0.1f,3)]
    public float pitch;

    public bool isLoop;

    public AudioClip clip;
    public AudioMixerGroup mixer;
}
