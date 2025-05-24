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

    [Header("Player Script Cameras")]
    public Transform playerCamera;

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

    private string currentAnimation = "";

    private const int UPPERBODY = 0;
    private const int LOWERBODY = 1;

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
        onSurface = Physics.CheckSphere(surfaceCheck.position, surfaceDistance, surfaceMask);

        if (onSurface && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        cController.Move(velocity * Time.deltaTime);

        PlayerMove();
        Jump();
        Sprint();

        CheckTopAnimation();
        CheckBottomAnimation();
    }

    void PlayerMove()
    {
        float horizontal_axis = 0f;
        float vertical_axis = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            horizontal_axis = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontal_axis = 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            vertical_axis = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            vertical_axis = -1f;
        }

        direction = new Vector3(horizontal_axis, 0f, vertical_axis).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalVelocity, turnCalTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            cController.Move(moveDirection.normalized * playerSpeed * Time.deltaTime);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && onSurface && !isSprinting)
        {
            velocity.y = Mathf.Sqrt(jumpRange * -2 * gravity);
        }
        else
        {

        }
    }

    void Sprint()
    {
        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.D)) && onSurface)
        {
            isSprinting = true;
            gravity = -1000f;

            float horizontal_axis = 0f;
            float vertical_axis = 0f;

            if (Input.GetKey(KeyCode.A))
            {
                horizontal_axis = -1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                horizontal_axis = 1f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                vertical_axis = 1f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                vertical_axis = -1f;
            }

            direction = new Vector3(horizontal_axis, 0f, vertical_axis).normalized;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalVelocity, turnCalTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                cController.Move(moveDirection.normalized * playerSprint * Time.deltaTime);
            }
        } else
        {
            isSprinting = false;
            gravity = -30f;
        }
    }

    public void PlayerHitDamage(float takeDamage)
    {
        presentHealth -= takeDamage;

        if (presentHealth <= 0)
        {
            PlayerDie();
        }
    }

    private void PlayerDie()
    {
        Cursor.lockState = CursorLockMode.None;
        Object.Destroy(gameObject, 1.0f);
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
        if (!onSurface)
        {
            Play(Animations.Jump, layer, false, false);
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.D))
        {
            Play(Animations.Run, layer, false, false);
        }
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S))
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
