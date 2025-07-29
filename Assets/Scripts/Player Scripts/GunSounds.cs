using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Gun Sound Sources")]
    public AudioClip[] fireSounds;
    public AudioClip[] reloadSounds;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private AudioClip GetRandomFireSound()
    {
        return fireSounds[UnityEngine.Random.Range(0, fireSounds.Length)];
    }

    private void Fire()
    {
        AudioClip clip = GetRandomFireSound();
        audioSource.PlayOneShot(clip);
    }
}
