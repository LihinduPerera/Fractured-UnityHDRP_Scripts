using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchCamera : MonoBehaviour
{
    [Header("Switch Camera")]
    public GameObject aimCam;
    public GameObject aimCanvas;
    public GameObject tpcam;
    public GameObject tpCanvas;

    [Header("Camera Shake Settings")]
    public float shakeAmplitude = 1.2f; // How much the camera shakes
    public float shakeFrequency = 2.0f; // How quickly the camera shakes
    public float shakeDuration = 0.15f; // How long the shake lasts

    private CinemachineVirtualCamera currentVCam;
    private CinemachineBasicMultiChannelPerlin currentNoise;
    private float shakeElapsedTime = 0f;

    private void Start()
    {
        // Initialize with TP camera as default
        currentVCam = tpcam.GetComponent<CinemachineVirtualCamera>();
        currentNoise = currentVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            SwitchToAimCamera();
        }
        else
        {
            SwitchToTPCamera();
        }

        // Handle camera shake
        if (shakeElapsedTime > 0)
        {
            shakeElapsedTime -= Time.deltaTime;

            if (shakeElapsedTime <= 0f)
            {
                // Reset to default values when shake is complete
                if (currentNoise != null)
                {
                    currentNoise.m_AmplitudeGain = 0f;
                    currentNoise.m_FrequencyGain = 0f;
                }
            }
        }
    }

    private void SwitchToAimCamera()
    {
        tpcam.SetActive(false);
        tpCanvas.SetActive(false);
        aimCam.SetActive(true);
        aimCanvas.SetActive(true);

        // Update current camera references
        currentVCam = aimCam.GetComponent<CinemachineVirtualCamera>();
        currentNoise = currentVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void SwitchToTPCamera()
    {
        tpcam.SetActive(true);
        tpCanvas.SetActive(true);
        aimCam.SetActive(false);
        aimCanvas.SetActive(false);

        // Update current camera references
        currentVCam = tpcam.GetComponent<CinemachineVirtualCamera>();
        currentNoise = currentVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void TriggerShake()
    {
        if (currentNoise != null)
        {
            shakeElapsedTime = shakeDuration;
            currentNoise.m_AmplitudeGain = shakeAmplitude;
            currentNoise.m_FrequencyGain = shakeFrequency;
        }
    }
}