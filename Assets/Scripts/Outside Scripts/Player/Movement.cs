
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Rigidbody rb;
    public Transform head;
    public float speed = 5f;
    public float runSpeed = 9f;
    public float maxForce = 10f;
    public float jumpForce = 3f;
    public float airControl = 1f;

    [Header("Crouch Collider")]
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private float crouchHeight = 1f;

    private float standingHeight;
    private Vector3 standingCenter;

    [Header("Footstep Audio")]
    [SerializeField] private float walkFootstepInterval = 0.5f;
    [SerializeField] private float runFootstepInterval = 0.32f;

    private float footstepTimer;

    [Header("Look Settings")]
    public float sensitivity = 100f;
    public Vector2 move, look;
    private float lookRotation;
    public static Movement Instance;

    [Header("Grounded")]
    public bool grounded;
    private bool wasGrounded;
    public PlayerAnimation playerAnimation;

    [Header("Attack")]
    private bool attackEnabled = true;

    [Header("Particles")]
    public ParticleSystem walkrunParticles;
    public ParticleSystem JumpParticles;

    [Header("Stamina Settings")]
    public StaminaBar staminaBar;
    public float maxStamina = 100f;
    public float staminaDrainRate = 10f;
    public float JumpDrain = 7f;
    public float staminaRegenRate = 15f;
    private float currentStamina;

    private bool isRunning;
    private bool isCrouching;
    private bool crouchEnabled = true;
    private bool jumpEnabled = true;

    public bool IsRunning
    {
        get
        {
            return isRunning &&
                   !isCrouching &&
                   grounded &&
                   currentStamina > 0f &&
                   move.y > 0f;
        }
    }

    public bool IsCrouching
    {
        get { return isCrouching; }
    }

    [Header("Pause")]
    public bool isPaused;
    public bool lookPaused;
    public bool updatingRotation = true;

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isPaused)
            return;

        move = context.ReadValue<Vector2>();

        if (context.performed && move != Vector2.zero)
        {
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.CompleteWASD();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (isPaused || !updatingRotation)
            return;

        look = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isPaused || !jumpEnabled || isCrouching)
            return;

        if (context.performed && grounded)
        {
            Jump();

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.CompleteSpace();
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (isPaused)
            return;

        if (context.performed)
            isRunning = true;
        else if (context.canceled)
            isRunning = false;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (isPaused || !attackEnabled)
            return;

        if (context.performed)
        {
            if (playerAnimation != null)
                playerAnimation.Attack();

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.CompleteThirdLeftClick();
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (isPaused || !crouchEnabled)
        {
            isCrouching = false;
            return;
        }

        if (context.performed)
        {
            isCrouching = true;

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.CompleteCrouch();
        }
        else if (context.canceled)
        {
            isCrouching = false;
        }
    }

    public void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider>();

        if (capsuleCollider != null)
        {
            standingHeight = capsuleCollider.height;
            standingCenter = capsuleCollider.center;
        }

        currentStamina = maxStamina;

        if (staminaBar != null)
            staminaBar.SetMaxStamina(maxStamina);
    }

    void Update()
    {
        if (!updatingRotation)
            return;

        HandleStamina();
        JumpParticlesPlay();
        WalkRunParticles();
        UpdateCrouchCollider();
        HandleFootsteps();

        if (isRunning && move.y > 0f && grounded &&
            !isCrouching && currentStamina > 0f)
        {
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.CompleteLShift();
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void LateUpdate()
    {
        Look();
    }

    void Move()
    {
        if (isPaused)
        {
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );

            rb.angularVelocity = Vector3.zero;
            return;
        }

        bool sprinting =
            isRunning &&
            !isCrouching &&
            grounded &&
            currentStamina > 0f &&
            move.y > 0f;

        float currentSpeed = sprinting ? runSpeed : speed;

        Vector3 currentVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 targetVelocity =
            (forward * move.y + right * move.x) * currentSpeed;

        Vector3 velocityChange = targetVelocity - currentVelocity;

        if (!grounded)
        {
            velocityChange.x *= airControl;
            velocityChange.z *= airControl;
        }

        velocityChange =
            Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void HandleFootsteps()
    {
        bool isMoving =
            grounded &&
            !isPaused &&
            move.sqrMagnitude > 0.01f &&
            !isCrouching;

        if (!isMoving || !updatingRotation)
        {
            footstepTimer = 0f;
            return;
        }

        float interval = IsRunning
            ? runFootstepInterval
            : walkFootstepInterval;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            if (PlayerSound.instance != null)
                PlayerSound.instance.PlayFootstep();

            footstepTimer = interval;
        }
    }

    void Jump()
    {
        if (isCrouching || currentStamina < JumpDrain)
            return;

        currentStamina -= JumpDrain;

        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;

        rb.AddForce(
            Vector3.up * jumpForce,
            ForceMode.VelocityChange
        );
    }

    void Look()
    {
        if (isPaused || head == null)
            return;

        float mouseX = look.x * sensitivity * Time.deltaTime;
        float mouseY = look.y * sensitivity * Time.deltaTime;

        lookRotation -= mouseY;
        lookRotation = Mathf.Clamp(lookRotation, -80f, 70f);

        transform.Rotate(Vector3.up * mouseX);

        head.localRotation = Quaternion.Euler(
            lookRotation,
            0f,
            0f
        );
    }

    void HandleStamina()
    {
        bool sprinting =
            isRunning &&
            !isCrouching &&
            grounded &&
            move.y > 0f;

        if (sprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else if (isRunning && !grounded)
        {
            currentStamina -=
                staminaDrainRate * 0.3f * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(
            currentStamina,
            0f,
            maxStamina
        );

        if (staminaBar != null)
            staminaBar.SetStamina(currentStamina);
    }

    void JumpParticlesPlay()
    {
        if (!wasGrounded && grounded && JumpParticles != null)
            JumpParticles.Play();

        wasGrounded = grounded;
    }

    void WalkRunParticles()
    {
        bool shouldPlay =
            grounded &&
            move.y > 0.1f &&
            Mathf.Abs(move.x) < 0.1f;

        if (shouldPlay)
        {
            if (walkrunParticles != null &&
                !walkrunParticles.isPlaying)
                walkrunParticles.Play();
        }
        else
        {
            if (walkrunParticles != null &&
                walkrunParticles.isPlaying)
                walkrunParticles.Stop();
        }
    }

    private void UpdateCrouchCollider()
    {
        if (capsuleCollider == null)
            return;

        float targetHeight = isCrouching
            ? Mathf.Min(crouchHeight, standingHeight)
            : standingHeight;

        float targetCenterY = standingCenter.y -
            (standingHeight - targetHeight) / 2f;

        capsuleCollider.height = Mathf.Lerp(
            capsuleCollider.height,
            targetHeight,
            Time.deltaTime * 10f
        );

        Vector3 targetCenter = standingCenter;
        targetCenter.y = targetCenterY;

        capsuleCollider.center = Vector3.Lerp(
            capsuleCollider.center,
            targetCenter,
            Time.deltaTime * 10f
        );
    }

    public void ResetAfterRespawn()
    {
        move = Vector2.zero;
        look = Vector2.zero;

        isRunning = false;
        isCrouching = false;

        grounded = false;
        wasGrounded = false;

        isPaused = false;
        lookPaused = false;
        updatingRotation = true;

        footstepTimer = 0f;

        if (capsuleCollider != null)
        {
            capsuleCollider.height = standingHeight;
            capsuleCollider.center = standingCenter;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        // Reset the look calculation without forcing Spine1
        // or the camera's local rotation to identity.
        lookRotation = 0f;

        currentStamina = maxStamina;

        if (staminaBar != null)
            staminaBar.SetStamina(currentStamina);

        if (walkrunParticles != null)
            walkrunParticles.Stop();

        if (JumpParticles != null)
            JumpParticles.Stop();

        Debug.Log("Movement: Player movement reset after respawn.");
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }

    public void FreezeLookState()
    {
        if (head == null)
            return;

        Vector3 angles = head.localEulerAngles;

        if (angles.x > 180f)
            angles.x -= 360f;

        lookRotation = angles.x;
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }

    public void SetLookEnabled(bool state)
    {
        updatingRotation = state;

        if (!state)
            look = Vector2.zero;
    }

    public void SetAttackEnabled(bool state)
    {
        attackEnabled = state;
    }

    public void SetCrouchEnabled(bool state)
    {
        crouchEnabled = state;

        if (!state)
            isCrouching = false;
    }

    public void SetJumpEnabled(bool state)
    {
        jumpEnabled = state;
    }
}