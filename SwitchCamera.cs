using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCamera : MonoBehaviour
{
    [Header("Switch Camera")]
    public GameObject aimCam;
    public GameObject aimCanvas;
    public GameObject tpcam;
    public GameObject tpCanvas;

    [Header("Camera Animator")]
    public Animator animator;

    private void Update()
    {
        if (Input.GetMouseButton(1) && Input.GetKey(KeyCode.W))
        {
            animator.SetBool("Idle", false);
            animator.SetBool("IdleAim", true);
            animator.SetBool("RifleWalk", true);
            animator.SetBool("Walk", true);

            tpcam.SetActive(false);
            tpCanvas.SetActive(false);
            aimCam.SetActive(true);
            aimCanvas.SetActive(true);

        } else if (Input.GetMouseButton(1))
        {
            animator.SetBool("Idle", false);
            animator.SetBool("IdleAim", true);
            animator.SetBool("RifleWalk", false);
            animator.SetBool("Walk", false);

            tpcam.SetActive(false);
            tpCanvas.SetActive(false);
            aimCam.SetActive(true);
            aimCanvas.SetActive(true);

        } else
        {
            animator.SetBool("Idle", true);
            animator.SetBool("IdleAim", false);
            animator.SetBool("RifleWalk", false);

            tpcam.SetActive(true);
            tpCanvas.SetActive(true);
            aimCam.SetActive(false);
            aimCanvas.SetActive(false);
        }
    }
}
