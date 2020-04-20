using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOnClick : MonoBehaviour
{
    public AudioSource Source;
    public AudioClip Clip;
    public AudioClip Clip2;

    public void PlayClick(float volume = 0.25f) {
        Source.volume = volume;
        Source.PlayOneShot(Clip);
    }

    public void PlayClick2(float volume = 0.25f)
    {
        Source.volume = volume;
        Source.PlayOneShot(Clip2);
    }
}
