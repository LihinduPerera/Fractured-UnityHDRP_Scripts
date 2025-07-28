using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class timelinetest : MonoBehaviour
{
    public PlayableDirector timelineObject;
    public GameObject cinemachineVCam; // Reference to your Cinemachine virtual camera

    void Start()
    {
        if (timelineObject != null)
        {
            timelineObject.stopped += OnTimelineStopped;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (timelineObject != null)
            {
                if (cinemachineVCam != null)
                {
                    cinemachineVCam.SetActive(false); // Disable Cinemachine control
                }

                timelineObject.gameObject.SetActive(true);
                timelineObject.Play(); // Play Timeline
            }
        }
    }

    void OnTimelineStopped(PlayableDirector director)
    {
        if (cinemachineVCam != null)
        {
            cinemachineVCam.SetActive(true); // Re-enable Cinemachine after timeline
        }
    }
}
