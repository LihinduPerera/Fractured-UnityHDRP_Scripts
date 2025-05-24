using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleLogic : MonoBehaviour
{
    [Header("Rifle Settings")]
    public Camera cam;
    public float gunDamage = 10f;
    public float shootRange = 100f;
    public float fireCharge = 15f;
    private float nextTimeToShoot = 0f;
    public Animator animator;
    public PlayerScript player;
    public Transform hand;

    [Header("Rifle Amunition and shooting")]
    private int maximumAmunition = 32;
    public int mag = 10;
    private int presentAmunition;
    public float reloadingTime = 1.3f;
    private bool setReloading = false;

    [Header("Rifle Effects")]
    public ParticleSystem muzzleSpark;
    public GameObject woodEffect;
    public GameObject goreEffect;

    private void Awake()
    {
        transform.SetParent(hand);
        presentAmunition = maximumAmunition;
    }

    private void Update()
    {
        if (setReloading)
            return;

        if(presentAmunition <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if(Input.GetMouseButton(0) && Time.time >= nextTimeToShoot)
        {
            animator.SetBool("Fire", true);
            animator.SetBool("Idle", false);
            nextTimeToShoot = Time.time + 1f/fireCharge; 
            Shoot();

        } else if(Input.GetMouseButton(0) && Input.GetKey(KeyCode.W)) {

            animator.SetBool("Idle",false);
            animator.SetBool("FireWalk", true);
        } else if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            animator.SetBool("Idle", false);
            animator.SetBool("IdelAim", true);
            animator.SetBool("FireWalk", true);
            animator.SetBool("Walk", true);
            animator.SetBool("Reloading", false);

        } else
        {
            animator.SetBool("Fire",false );
            animator.SetBool("Idle", true);
            animator.SetBool("FireWalk", false);
        }
    }

    private void Shoot()
    {
        if(mag == 0)
        {
            //ammo out text
            return;
        }

        presentAmunition--;

        if(presentAmunition == 0)
        {
            mag--;
        }

        muzzleSpark.Play();
        RaycastHit hitinfo;

       if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitinfo, shootRange))
        {
            DestroyObjects destroyObjects = hitinfo.transform.GetComponent<DestroyObjects>();
            Zombie1 zombie1 = hitinfo.transform.GetComponent<Zombie1>();
            Zombie2 zombie2 = hitinfo.transform.GetComponent<Zombie2>();

            if(destroyObjects != null)
            {
                destroyObjects.DamageObject(gunDamage);
                GameObject woodGo = Instantiate(woodEffect, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
                Destroy(woodGo, 3f);

            } 
            else if (zombie1 != null)
            {
                zombie1.ZombieHitDamage(gunDamage);
                GameObject goreEffectGo = Instantiate(goreEffect, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
                Destroy(goreEffectGo, 1f);
            }
            else if (zombie2 != null)
            {
                zombie2.ZombieHitDamage(gunDamage);
                GameObject goreEffectGo = Instantiate(goreEffect, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
                Destroy(goreEffectGo, 1f);
            }
        }
    }

    IEnumerator Reload()
    {
        player.playerSpeed = 0f;
        player.playerSprint = 0f;
        setReloading = true;
        animator.SetBool("Reloading", true);

        yield return new WaitForSeconds(reloadingTime);
        animator.SetBool("Reloading", false);

        presentAmunition = maximumAmunition;
        player.playerSpeed = 1.9f;
        player.playerSprint = 3f;
        setReloading = false;
    }
}
