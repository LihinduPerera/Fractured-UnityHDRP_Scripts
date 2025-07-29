using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class Zombie1 : ZombieAnimationBrain
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

    [Header("Zombie Guarding var")]
    public GameObject[] walkPoints;
    int currentZombiePosition = 0;
    public float zombieSpeed;
    float walkingPointRadius = 2;

    [Header("Zombie Attacking Var")]
    public float timeBtwAttack;
    bool previouslyAttack;
    bool isAttacking;

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
    }

    private void Update()
    {
        if (isDead) return;

        playerInVisionRadius = Physics.CheckSphere(transform.position, visionRadius, playerLayer);
        playerInAttackingRadius = Physics.CheckSphere(transform.position, attackingRadius, playerLayer);

        if (!playerInVisionRadius && !playerInAttackingRadius) Guard();
        if (playerInVisionRadius && !playerInAttackingRadius) PursutePlayer();
        if (playerInVisionRadius && playerInAttackingRadius) AttackPlayer();

        // Update animations based on state
        CheckBottomAnimation();
        CheckTopAnimation();
    }

    private void Guard()
    {
        if (Vector3.Distance(walkPoints[currentZombiePosition].transform.position, transform.position) < walkingPointRadius)
        {
            currentZombiePosition = Random.Range(0, walkPoints.Length);
            if (currentZombiePosition >= walkPoints.Length)
            {
                currentZombiePosition = 0;
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, walkPoints[currentZombiePosition].transform.position, Time.deltaTime * zombieSpeed);
        transform.LookAt(walkPoints[currentZombiePosition].transform.position);
    }

    private void PursutePlayer()
    {
        if (zombieAgent.SetDestination(playerBody.position))
        {
            // Running animation handled in CheckMovementAnimation
        }
    }

    private void AttackPlayer()
    {
        zombieAgent.SetDestination(transform.position);
        transform.LookAt(LookPoint);

        if (!previouslyAttack && !isAttacking)
        {
            isAttacking = true;
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        // Play attack animation
        previouslyAttack = true;

        // Raycast to check if player is in front
        RaycastHit hitInfo;
        if (Physics.Raycast(AttackingRaycastArea.transform.position, AttackingRaycastArea.transform.forward, out hitInfo, attackingRadius))
        {
            PlayerScript playerBody = hitInfo.transform.GetComponent<PlayerScript>();
            if (playerBody != null)
            {
                playerBody.PlayerHitDamage(giveDamage);
            }
        }

        // Wait for attack cooldown
        yield return new WaitForSeconds(timeBtwAttack);

        previouslyAttack = false;
        isAttacking = false;
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
        if (isDead)
        {
            // Don't change animations if dead
            return;
        }

        if (presentHealth <= 0)
        {
            // Randomly choose between two death animations
            ZombieAnimations deathAnim = Random.Range(0, 2) == 0 ? ZombieAnimations.Dead1 : ZombieAnimations.Dead2;
            Play(deathAnim, layer, true, true);
        }
        else if (isAttacking)
        {
            Play(ZombieAnimations.Attack, layer, true, false);
        }
        else if (playerInAttackingRadius)
        {
            // Idle stance when in attacking radius but not currently attacking
            Play(ZombieAnimations.Idle, layer, false, false);
        }
        else if (playerInVisionRadius)
        {
            Play(ZombieAnimations.Run, layer, false, false);
        }
        else
        {
            Play(ZombieAnimations.Walk, layer, false, false);
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

    public void ZombieHitDamage(float takeDamage)
    {
        if (isDead) return;

        presentHealth -= takeDamage;

        if (presentHealth <= 0)
        {
            isDead = true;
            ZombieDie();
        }
        else
        {
            // Play hit reaction
            Play(ZombieAnimations.HitReact, UPPERBODY, false, true, 0.1f);
        }
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

        // Disable collider to prevent further hits
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Notify server about the kill (only once)
        StartCoroutine(ReportKillToServer("Zombie1"));

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
}