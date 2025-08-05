using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleLogic : MonoBehaviour
{
    [Header("Rifle Settings")]
    public Camera cam;
    public float gunDamage = 10f;
    public float shootRange = 100f;
    [Tooltip("Rounds per second")]
    public float fireRate = 15f; // Changed from fireCharge to fireRate for clarity
    private float nextTimeToShoot = 0f;
    public PlayerScript player;
    public Transform hand;

    [Header("Rifle Ammunition")]
    public int magazineSize = 32; // Renamed from maximumAmunition for clarity
    public int totalMagazines = 10; // Renamed from mag for clarity
    private int currentAmmo;
    private bool setReloading = false;

    [Header("Rifle Effects")]
    public ParticleSystem muzzleSpark;
    public GameObject woodEffect;
    public GameObject goreEffect;

    public bool IsReloading => setReloading;
    public int CurrentAmmo => currentAmmo;
    public int TotalMagazines => totalMagazines;

    private void Awake()
    {
        transform.SetParent(hand);
        currentAmmo = magazineSize;
    }

    private void Update()
    {
        if (setReloading)
            return;

        // Manual reload with R key if not full and not already reloading
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        // Auto reload when ammo runs out
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButton(1) && Input.GetMouseButton(0) && Time.time >= nextTimeToShoot)
        {
            nextTimeToShoot = Time.time + 1f / fireRate; // Calculate delay based on fireRate
            Shoot();
        }
    }

    private void Shoot()
    {
        if (totalMagazines <= 0 && currentAmmo <= 0)
        {
            // Optionally play empty click sound here
            return;
        }

        currentAmmo--;

        if (currentAmmo <= 0)
        {
            totalMagazines--;
        }

        muzzleSpark.Play();
        RaycastHit hitinfo;

        // Get the appropriate forward direction based on aiming state
        Vector3 shootDirection;
        if (player.aimCamera != null && player.aimCamera.gameObject.activeInHierarchy)
        {
            // Use aim camera's forward when aiming
            shootDirection = player.aimCamera.transform.forward;
        }
        else
        {
            // Use main camera's forward when not aiming
            shootDirection = cam.transform.forward;
        }

        if (Physics.Raycast(cam.transform.position, shootDirection, out hitinfo, shootRange))
        {
            Debug.Log("Shot object: " + hitinfo.transform.name);

            // Try to get any zombie component first
            MonoBehaviour zombie = hitinfo.transform.GetComponent<MonoBehaviour>();

            if (zombie is Zombie1 zombie1)
            {
                zombie1.ZombieHitDamage(gunDamage);
                GameObject goreEffectGo = Instantiate(goreEffect, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
                Destroy(goreEffectGo, 1f);
            }
            else if (zombie is Zombie2 zombie2)
            {
                zombie2.ZombieHitDamage(gunDamage);
                GameObject goreEffectGo = Instantiate(goreEffect, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
                Destroy(goreEffectGo, 1f);
            }

            DestroyObjects destroyObjects = hitinfo.transform.GetComponent<DestroyObjects>();
            if (destroyObjects != null)
            {
                destroyObjects.DamageObject(gunDamage);
                GameObject woodGo = Instantiate(woodEffect, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
                Destroy(woodGo, 3f);
            }
        }
    }

    IEnumerator Reload()
    {
        //player.playerSpeed = 0f;
        //player.playerSprint = 0f;
        setReloading = true;

        // Get the length of the reload animation
        float reloadTime = player.GetAnimationLength(Animations.Reload);

        // Trigger reload animation on upper body only
        player.Play(Animations.Reload, PlayerScript.UPPERBODY, true, true);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        player.playerSpeed = 1.9f;
        player.playerSprint = 3f;
        setReloading = false;

        // Unlock the upper body layer after reload is complete
        
        player.SetLocked(false, PlayerScript.UPPERBODY);

        player.Play(Animations.Walk, PlayerScript.UPPERBODY, false, false); //To reset the animation controller to default in Reload state
        player.Play(Animations.Walk, PlayerScript.LOWERBODY, false, false); //To reset the animation controller to default in Reload state

    }

    // Method to add ammo (for pickups)
    public void AddAmmo(int amount)
    {
        totalMagazines += amount;
    }
}