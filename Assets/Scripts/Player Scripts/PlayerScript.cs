using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : AnimationBrain
{
    [Header("Player Movement")]
    public float playerSpeed = 1.9f;
    public float playerSprint = 3f;

    [Header("Player Health")]
    private float playerHealth = 120f;
    public float presentHealth;
    private bool isTakingDamage = false;
    private float lastHitTime = 0f;
    public float hitCooldown = 1f; // Prevent rapid hits

    [Header("Player Script Cameras")]
    public Transform playerCamera; // Main camera transform
    public Cinemachine.CinemachineVirtualCamera aimCamera; // Reference to aim camera
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    [Header("Player Animator and Gravity")]
    public CharacterController cController;
    public float gravity = -30f;
    public Animator animator;

    [Header("Player Jumping and Velocity")]
    public float turnCalTime = 0.1f;
    float turnCalVelocity;
    public float jumpRange = 1f;
    Vector3 velocity;
    public Transform surfaceCheck;
    bool onSurface;
    public float surfaceDistance = 0.4f;
    public LayerMask surfaceMask;

    [Header("Camera Shake")]
    public SwitchCamera switchCamera;
    public RifleLogic rifleLogic;

    public const int UPPERBODY = 0;
    public const int LOWERBODY = 1;

    bool isSprinting;
    private Vector3 direction;

    private void Start()
    {
        Initialize(GetComponent<Animator>().layerCount, Animations.Idle1, GetComponent<Animator>(), DefaultAnimation);
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        presentHealth = playerHealth;
    }

    private void Update()
    {
        if (isTakingDamage) return;

        onSurface = Physics.CheckSphere(surfaceCheck.position, surfaceDistance, surfaceMask);

        if (onSurface && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        cController.Move(velocity * Time.deltaTime);

        HandleMouseLook();
        PlayerMove();
        Jump();
        Sprint();

        CheckTopAnimation();
        CheckBottomAnimation();
    }

    void HandleMouseLook()
    {
        if (Input.GetMouseButton(1)) // Right-click to aim/rotate
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Rotate player based on aim camera's rotation
            if (aimCamera != null)
            {
                // Get the aim camera's forward direction (ignoring pitch)
                Vector3 cameraForward = aimCamera.transform.forward;
                cameraForward.y = 0;
                cameraForward.Normalize();

                // Set player rotation to match aim camera's horizontal rotation
                if (cameraForward != Vector3.zero)
                {
                    transform.forward = cameraForward;
                }
            }
            else
            {
                // Fallback to regular rotation if aim camera is not assigned
                transform.Rotate(Vector3.up * mouseX);
            }

            // Tilt camera vertically (clamped)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -45f, 45f);
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void PlayerMove()
    {
        float horizontal_axis = 0f;
        float vertical_axis = 0f;

        if (Input.GetKey(KeyCode.A))
            horizontal_axis = -1f;
        else if (Input.GetKey(KeyCode.D))
            horizontal_axis = 1f;

        if (Input.GetKey(KeyCode.W))
            vertical_axis = 1f;
        else if (Input.GetKey(KeyCode.S))
            vertical_axis = -1f;

        direction = new Vector3(horizontal_axis, 0f, vertical_axis).normalized;

        if (direction.magnitude >= 0.1f && !Input.GetMouseButton(1)) // Prevent auto-rotation during aiming
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalVelocity, turnCalTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            cController.Move(moveDirection.normalized * playerSpeed * Time.deltaTime);
        }
        else if (direction.magnitude >= 0.1f && Input.GetMouseButton(1)) // Move in direction player is facing (now synced with aim camera)
        {
            Vector3 moveDirection = transform.forward * direction.z + transform.right * direction.x;
            cController.Move(moveDirection.normalized * playerSpeed * Time.deltaTime);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && onSurface && !isSprinting)
        {
            velocity.y = Mathf.Sqrt(jumpRange * -2 * gravity);
        }
    }

    void Sprint()
    {
        if ((Input.GetKey(KeyCode.LeftShift) && direction.magnitude >= 0.1f) && onSurface && !Input.GetMouseButton(1))
        {
            isSprinting = true;
            gravity = -1000f;

            Vector3 moveDirection;
            if (!Input.GetMouseButton(1))
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalVelocity, turnCalTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
            else
            {
                moveDirection = transform.forward * direction.z + transform.right * direction.x;
            }

            cController.Move(moveDirection.normalized * playerSprint * Time.deltaTime);
        }
        else
        {
            isSprinting = false;
            gravity = -30f;
        }
    }

    public void PlayerHitDamage(float takeDamage)
    {
        if (isTakingDamage || Time.time < lastHitTime + hitCooldown) return;

        lastHitTime = Time.time;
        presentHealth -= takeDamage;

        // Randomly choose between two hit reactions
        Animations hitAnim = Random.Range(0, 2) == 0 ? Animations.HitReaction : Animations.HitReaction2;

        // Play hit reaction on both layers
        Play(hitAnim, UPPERBODY, true, true);
        Play(hitAnim, LOWERBODY, true, true);

        StartCoroutine(HandleHitReaction());

        if (presentHealth <= 0)
        {
            PlayerDie();
        }
    }

    private IEnumerator HandleHitReaction()
    {
        isTakingDamage = true;
        yield return new WaitForSeconds(0.5f); // Wait for hit animation to complete

        // Unlock both layers
        SetLocked(false, UPPERBODY);
        SetLocked(false, LOWERBODY);

        isTakingDamage = false;
    }

    private void PlayerDie()
    {
        // Play death animation on both layers
        Play(Animations.Death, UPPERBODY, true, true);
        Play(Animations.Death, LOWERBODY, true, true);

        Cursor.lockState = CursorLockMode.None;
        Object.Destroy(gameObject, 1.0f);
    }

    private void CheckTopAnimation()
    {
        if (isTakingDamage) return;
        if (rifleLogic != null && rifleLogic.IsReloading) return;

        CheckMovementAnimation(UPPERBODY);
    }

    private void CheckBottomAnimation()
    {
        if (isTakingDamage) return;

        CheckMovementAnimation(LOWERBODY);
    }

    private void CheckMovementAnimation(int layer)
    {
        if (!onSurface)
        {
            Play(Animations.Jump, layer, false, false);
        }
        //if (Input.GetKey(KeyCode.Space))
        //{
        //    Play(Animations.Jump, layer, false, false);
        //}
        else if (Input.GetMouseButton(1) && Input.GetMouseButton(0) && direction.magnitude > 0.1f)
        {
            Play(Animations.WalkRifleFire, layer, false, false);
        }
        else if (Input.GetMouseButton(1) && direction.magnitude > 0.1f)
        {
            Play(Animations.WalkRifleAim, layer, false, false);
        }
        else if (Input.GetMouseButton(1) && Input.GetMouseButton(0))
        {
            Play(Animations.StandRifleFire, layer, false, false);
        }
        else if (Input.GetMouseButton(1))
        {
            Play(Animations.StandRifleAim, layer, false, false);
        }
        else if (Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1) && direction.magnitude > 0.1f)
        {
            Play(Animations.Run, layer, false, false);
        }
        else if (direction.magnitude > 0.1f)
        {
            Play(Animations.Walk, layer, false, false);
        }
        else
        {
            Play(Animations.Idle1, layer, false, false);
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