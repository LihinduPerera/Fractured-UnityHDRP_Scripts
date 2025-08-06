using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSounds : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Zombie Sounds")]
    public AudioClip idleSound;
    public AudioClip screamSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float idleSoundVolume = 1.0f;
    [Range(0f, 1f)] public float screamSoundVolume = 1.0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Idle()
    {
        audioSource.PlayOneShot(idleSound, idleSoundVolume);
    }

    public void Scream()
    {
        audioSource.PlayOneShot(screamSound, screamSoundVolume);
    }
}
