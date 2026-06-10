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

    [Header("Look Settings")]
    public float sensitivity = 100f;
    public Vector2 move, look;
    private float lookRotation;

    [Header("Grounded")]
    public bool grounded;
    public PlayerAnimation playerAnimation;

    [Header("Stamina Settings")]
    public StaminaBar staminaBar;
    public float maxStamina = 100f;
    public float staminaDrainRate = 10f;
    public float JumpDrain = 7f;
    public float staminaRegenRate = 15f;

    private float currentStamina;
    private bool isRunning;

    [Header("Pause")]
    public bool isPaused;

    public bool IsRunning
        {
            get
            {
                return isRunning && grounded && currentStamina > 0f && move.y > 0f;
            }
        }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isPaused) return;
        move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (isPaused) return;
        look = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isPaused) return;

        if (context.performed && grounded)
        {
            Jump();
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
            isRunning = true;
        else if (context.canceled)
            isRunning = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina;

        if (staminaBar != null)
            staminaBar.SetMaxStamina(maxStamina);
    }

    void Update()
    {
        HandleStamina();
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
        float currentSpeed;

        if (isPaused)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 currentVelocity =
            new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        bool sprinting =
            isRunning &&
            grounded &&
            currentStamina > 0f &&
            move.y > 0f;

        if (sprinting)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = speed;
        }

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

        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Jump()
    {
        if (currentStamina < JumpDrain)
            return;

        currentStamina -= JumpDrain;

        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

        if (playerAnimation != null)
            playerAnimation.ChangeAnimation("JumpUp");
    }

    void Look()
    {
        if (isPaused) return;

        float mouseX = look.x * sensitivity * Time.deltaTime;
        float mouseY = look.y * sensitivity * Time.deltaTime;

        lookRotation -= mouseY;
        lookRotation = Mathf.Clamp(lookRotation, -80f, 80f);

        transform.Rotate(Vector3.up * mouseX);

        head.localRotation =
            Quaternion.Euler(lookRotation, 0f, 0f);
    }

    void HandleStamina()
    {
        bool sprinting = isRunning && grounded && move.y > 0f;

        if (sprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else if (isRunning && !grounded)
        {
            currentStamina -= staminaDrainRate * 0.3f * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        if (staminaBar != null)
            staminaBar.SetStamina(currentStamina);
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }

    public void FreezeLookState()
    {
        Vector3 angles = head.localEulerAngles;

        if (angles.x > 180f)
            angles.x -= 360f;

        lookRotation = angles.x;
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }
}