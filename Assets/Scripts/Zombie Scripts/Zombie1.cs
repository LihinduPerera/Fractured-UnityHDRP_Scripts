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
    public float zombieRunSpeed;
    public float rotationSpeed = 10f; // Increased rotation speed
    float walkingPointRadius = 2;
    private float idleAnimationTimer = 0f;
    private float idleAnimationInterval = 5f;

    [Header("Zombie Attacking Var")]
    public float timeBtwAttack = 1.5f;
    public float attackAnimationLength = 1f;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private float pathUpdateDelay = 0.25f; // Added path update delay
    private float lastPathUpdateTime = 0f;

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
        zombieAgent.angularSpeed = 360f; // Increased angular speed
        zombieAgent.acceleration = 20f; // Increased acceleration
    }

    private void Update()
    {
        if (isDead) return;

        playerInVisionRadius = Physics.CheckSphere(transform.position, visionRadius, playerLayer);
        playerInAttackingRadius = Physics.CheckSphere(transform.position, attackingRadius, playerLayer);

        if (!playerInVisionRadius && !playerInAttackingRadius)
        {
            Guard();
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

        zombieAgent.SetDestination(walkPoints[currentZombiePosition].transform.position);
        Play(ZombieAnimations.Walk, UPPERBODY, false, false);
        Play(ZombieAnimations.Walk, LOWERBODY, false, false);
    }

    private void PursutePlayer()
    {
        if (Time.time > lastPathUpdateTime + pathUpdateDelay)
        {
            lastPathUpdateTime = Time.time;
            if (zombieAgent.isOnNavMesh)
            {
                zombieAgent.SetDestination(playerBody.position);
            }
        }

        // Smooth rotation towards player
        Vector3 direction = (playerBody.position - transform.position).normalized;
        direction.y = 0; // Keep the rotation only on Y axis
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        Play(ZombieAnimations.Run, UPPERBODY, false, false);
        Play(ZombieAnimations.Run, LOWERBODY, false, false);
    }

    private void AttackPlayer()
    {
        zombieAgent.SetDestination(transform.position);

        // Immediate and smooth rotation towards player
        Vector3 direction = (playerBody.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 2 * Time.deltaTime);
        }

        if (!isAttacking && Time.time > lastAttackTime + timeBtwAttack)
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            Play(ZombieAnimations.Attack, UPPERBODY, true, false);
            Play(ZombieAnimations.Attack, LOWERBODY, true, false);

            Invoke(nameof(DealDamage), attackAnimationLength * 0.5f);
            Invoke(nameof(ResetAttack), attackAnimationLength);
        }
        else if (!isAttacking)
        {
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
            //StartCoroutine(InterruptMovementBriefly());
            StartCoroutine(HitReactRoutine(hitReaction));
        }
    }

    private IEnumerator HitReactRoutine(ZombieAnimations hitReaction)
    {
        // Stop movement
        zombieAgent.isStopped = true;

        // Lock animation layers
        SetLocked(true, UPPERBODY);
        SetLocked(true, LOWERBODY);

        // Play hit reaction
        Play(hitReaction, UPPERBODY, true, true);
        Play(hitReaction, LOWERBODY, true, true);

        // Wait for the animation to play
        yield return new WaitForSeconds(0.5f); // You can tweak this depending on animation length

        // Unlock animation layers
        SetLocked(false, UPPERBODY);
        SetLocked(false, LOWERBODY);

        // Resume movement if still alive
        if (!isDead)
        {
            zombieAgent.isStopped = false;
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