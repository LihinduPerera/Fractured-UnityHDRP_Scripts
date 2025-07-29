using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class Zombie2 : ZombieAnimationBrain
{
    [Header("Zombie Health and Damage")]
    private float zombieHealth = 100f;
    private float presentHealth;
    public float giveDamage = 5f;
    private bool isDead = false;

    [Header("Zombie Things")]
    public NavMeshAgent zombieAgent;
    public Transform LookPoint;
    public Camera AttackingRaycastArea;
    public Transform playerBody;
    public LayerMask playerLayer;

    [Header("Zombie Standing var")]
    public float zombieSpeed;
    public float zombieRunSpeed;
    private float idleAnimationTimer = 0f;
    private float idleAnimationInterval = 5f;

    [Header("Zombie Attacking Var")]
    public float timeBtwAttack = 1.5f;
    public float attackAnimationLength = 1f;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    [Header("Zombie Animations")]
    public Animator animator;

    [Header("Zombie moods/states")]
    public float visionRadius;
    public float attackingRadius;
    public bool playerInVisionRadius;
    public bool playerInAttackingRadius;

    private const int UPPERBODY = 0;
    private const int LOWERBODY = 1;

    private void Awake()
    {
        Initialize(GetComponent<Animator>().layerCount, ZombieAnimations.Idle, GetComponent<Animator>(), DefaultAnimation);
        animator = GetComponent<Animator>();
        zombieAgent = GetComponent<NavMeshAgent>();
        presentHealth = zombieHealth;
        zombieAgent.speed = zombieSpeed;
    }

    private void Update()
    {
        if (isDead) return;

        playerInVisionRadius = Physics.CheckSphere(transform.position, visionRadius, playerLayer);
        playerInAttackingRadius = Physics.CheckSphere(transform.position, attackingRadius, playerLayer);

        if (!playerInVisionRadius && !playerInAttackingRadius)
        {
            Idle();
            zombieAgent.speed = zombieSpeed;
        }
        if (playerInVisionRadius && !playerInAttackingRadius)
        {
            PursutePlayer();
            zombieAgent.speed = zombieRunSpeed;
        }
        if (playerInVisionRadius && playerInAttackingRadius)
        {
            AttackPlayer();
        }

        if (!playerInVisionRadius && !playerInAttackingRadius)
        {
            idleAnimationTimer += Time.deltaTime;
            if (idleAnimationTimer >= idleAnimationInterval)
            {
                idleAnimationTimer = 0f;
                if (Random.value > 0.5f)
                {
                    Play(ZombieAnimations.Idle, UPPERBODY, false, false);
                    Play(ZombieAnimations.Idle, LOWERBODY, false, false);
                }
                else
                {
                    Play(ZombieAnimations.Agonizing, UPPERBODY, false, false);
                    Play(ZombieAnimations.Agonizing, LOWERBODY, false, false);
                }
            }
        }
        else
        {
            idleAnimationTimer = 0f;
        }
    }

    private void Idle()
    {
        zombieAgent.SetDestination(transform.position);
    }

    private void PursutePlayer()
    {
        if (zombieAgent.SetDestination(playerBody.position))
        {
            Play(ZombieAnimations.Run, UPPERBODY, false, false);
            Play(ZombieAnimations.Run, LOWERBODY, false, false);
        }
    }

    private void AttackPlayer()
    {
        zombieAgent.SetDestination(transform.position);
        transform.LookAt(LookPoint);

        if (!isAttacking && Time.time > lastAttackTime + timeBtwAttack)
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            // Play attack animation
            Play(ZombieAnimations.Attack, UPPERBODY, true, false);
            Play(ZombieAnimations.Attack, LOWERBODY, true, false);

            // Deal damage at the peak of the attack animation
            Invoke(nameof(DealDamage), attackAnimationLength * 0.5f);

            // Reset attacking state after animation completes
            Invoke(nameof(ResetAttack), attackAnimationLength);
        }
        else if (!isAttacking)
        {
            // Play idle animation while waiting to attack again
            Play(ZombieAnimations.Idle, UPPERBODY, false, false);
            Play(ZombieAnimations.Idle, LOWERBODY, false, false);
        }
    }

    private void DealDamage()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(AttackingRaycastArea.transform.position, AttackingRaycastArea.transform.forward, out hitInfo, attackingRadius))
        {
            PlayerScript playerBody = hitInfo.transform.GetComponent<PlayerScript>();
            if (playerBody != null)
            {
                playerBody.PlayerHitDamage(giveDamage);
            }
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
        SetLocked(false, UPPERBODY);
        SetLocked(false, LOWERBODY);
    }

    public void ZombieHitDamage(float takeDamage)
    {
        if (isDead) return;

        presentHealth -= takeDamage;

        if (presentHealth <= 0)
        {
            isDead = true;
            if (Random.value > 0.5f)
            {
                Play(ZombieAnimations.Dead1, UPPERBODY, true, true);
                Play(ZombieAnimations.Dead1, LOWERBODY, true, true);
            }
            else
            {
                Play(ZombieAnimations.Dead2, UPPERBODY, true, true);
                Play(ZombieAnimations.Dead2, LOWERBODY, true, true);
            }

            ZombieDie();
        }
        else
        {
            ZombieAnimations hitReaction = Random.value > 0.5f ? ZombieAnimations.HitReact : ZombieAnimations.HitReact2;
            Play(hitReaction, UPPERBODY, false, false);
            Play(hitReaction, LOWERBODY, false, false);

            StopAllCoroutines();
            StartCoroutine(InterruptMovementBriefly());
        }
    }

    private IEnumerator InterruptMovementBriefly()
    {
        float originalSpeed = zombieAgent.speed;
        zombieAgent.speed = 0;
        zombieAgent.SetDestination(transform.position);
        yield return new WaitForSeconds(0.5f);
        zombieAgent.speed = originalSpeed;
    }

    private void ZombieDie()
    {
        zombieAgent.SetDestination(transform.position);
        zombieAgent.isStopped = true;
        zombieSpeed = 0f;
        attackingRadius = 0f;
        visionRadius = 0f;
        playerInAttackingRadius = false;
        playerInVisionRadius = false;

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        StartCoroutine(ReportKillToServer("Zombie2"));
        Object.Destroy(gameObject, 5.0f);
    }

    private IEnumerator ReportKillToServer(string zombieType)
    {
        string url = "http://localhost:8080/api/zombies/kill?zombieType=" + zombieType + "&weaponUsed=rifle";

        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, ""))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error reporting kill: " + webRequest.error);
            }
            else
            {
                Debug.Log("Kill reported successfully");
            }
        }
    }

    private void CheckTopAnimation()
    {
        CheckMovementAnimation(UPPERBODY);
    }

    private void CheckBottomAnimation()
    {
        CheckMovementAnimation(LOWERBODY);
    }

    private void CheckMovementAnimation(int layer)
    {
        if (isDead) return;

        if (!playerInVisionRadius && !playerInAttackingRadius)
        {
            // Idle animations handled by timer
        }
        else if (playerInVisionRadius && !playerInAttackingRadius)
        {
            Play(ZombieAnimations.Run, layer, false, false);
        }
        else if (playerInVisionRadius && playerInAttackingRadius && !isAttacking)
        {
            Play(ZombieAnimations.Idle, layer, false, false);
        }
    }

    void DefaultAnimation(int layer)
    {
        if (layer == UPPERBODY)
        {
            CheckTopAnimation();
        }
        else
        {
            CheckBottomAnimation();
        }
    }
}