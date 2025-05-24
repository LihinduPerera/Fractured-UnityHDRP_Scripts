using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    [Header("Player punch var")]
    public Camera cam;
    public float punchDamage = 10f;
    public float punchRange = 5f;

    public void Punch()
    {
        RaycastHit hitinfo;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitinfo, punchRange))
        {
            DestroyObjects destroyObjects = hitinfo.transform.GetComponent<DestroyObjects>();
            Zombie1 zombie1 = hitinfo.transform.GetComponent<Zombie1>();
            Zombie2 zombie2 = hitinfo.transform.GetComponent<Zombie2>();

            if (destroyObjects != null)
            {
                destroyObjects.DamageObject(punchRange);
            }
            else if(zombie1 != null)
            {
                zombie1.ZombieHitDamage(punchDamage);
            }
            else if(zombie2 != null)
            {
                zombie2.ZombieHitDamage(punchDamage);
            }
        }
    }
}
