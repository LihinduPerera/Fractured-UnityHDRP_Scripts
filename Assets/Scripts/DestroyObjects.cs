using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjects : MonoBehaviour
{
    public float objHealth = 30f;
    
    public void DamageObject(float amount)
    {
        objHealth -= amount;
        if(objHealth <= 0)
        {
            Destroy();
        }
    }

    void Destroy()
    {
        Destroy(gameObject);
    }
}
