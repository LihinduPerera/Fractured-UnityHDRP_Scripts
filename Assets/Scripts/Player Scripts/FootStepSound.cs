using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepSound : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("FootSteps Sources")]
    public AudioClip[] footStepSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float footStepVolume = 1.0f; // Default: Max

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private AudioClip GetRandomFootStep()
    {
        return footStepSound[UnityEngine.Random.Range(0, footStepSound.Length)];
    }

    private void Step()
    {
        AudioClip clip = GetRandomFootStep();
        audioSource.PlayOneShot(clip, footStepVolume);
    }
}
