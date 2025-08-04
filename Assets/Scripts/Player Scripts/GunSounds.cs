using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Gun Sound Sources")]
    public AudioClip[] fireSounds;
    public AudioClip reloadMagRemoveSound;
    public AudioClip reloadMagInsertSound;
    public AudioClip reloadChargeSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float gunFireVolume = 1.0f; // Default: Max
    [Range(0f, 1f)] public float gunReloadMagSound = 1.0f;
    [Range(0f, 1f)] public float gunReloadChargeSound = 1.0f;

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
        audioSource.PlayOneShot(clip, gunFireVolume);
    }

    private void Reload_MagRemove()
    {
        audioSource.PlayOneShot(reloadMagRemoveSound, gunReloadMagSound);
    }

    private void Reload_MagInsert()
    {
        audioSource.PlayOneShot(reloadMagInsertSound, gunReloadMagSound);
    }

    private void Reload_Charge()
    {
        audioSource.PlayOneShot(reloadChargeSound, gunReloadChargeSound);
    }
}
