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

    [Header("Stamina Settings")]
    public StaminaBar staminaBar;
    public float maxStamina = 100f;
    public float staminaDrainRate = 15f;
     public float BoostDrain = 30f;
    public float staminaRegenRate = 10f;
    private float currentStamina;
    private bool isRunning;

    public bool isPaused;
    

    // Input Callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        if (isPaused) return; // Ignore input when paused
        move = context.ReadValue<Vector2>();
        //Debug.Log(move);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (isPaused) return; // Ignore input when paused
        look = context.ReadValue<Vector2>();
        //Debug.Log(look);
    }

   public void OnJump(InputAction.CallbackContext context)
    {
        if (isPaused) return;

        if (context.performed)
        {
            // Don't allow jump in air
            if (!grounded) return;

            Jump();
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed) isRunning = true;
        else if (context.canceled) isRunning = false;
    }

    // Unity Methods
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        currentStamina = maxStamina;
        if (staminaBar != null)
            staminaBar.SetMaxStamina(maxStamina);
    }

    void Update()
    {
        HandleStamina();
    }
    void LateUpdate()
    {
       Look();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
                if (isPaused)
                {
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    return;
                }

        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            // Determine speed
            float currentSpeed;

                if (isRunning && grounded && currentStamina > 0 && move.y > 0)
                {
                    // If the player is holding run
                    currentSpeed = runSpeed;
                }
                else
                {
                    // normal walking speed
                    currentSpeed = speed;
                }
                
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Flatten so you don't go upward/downward
            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 targetVelocity = (forward * move.y + right * move.x) * currentSpeed;
            
            Vector3 moveDir = targetVelocity;
            moveDir.y = 0f;

            Vector3 velocityChange = targetVelocity - currentVelocity;
            velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

            // Air Control Restriction
            if (!grounded)
            {

                velocityChange.x *= airControl;
                velocityChange.z *= airControl;

            }
                // Only allow forward movement in air
               /* if (move.y > 0) // W pressed
                {
                    // Apply air control normally
                    velocityChange.x *= 0f; // block sideways
                    velocityChange.z *= airControl; // allow forward
                }
                else
                {
                    // Not moving forward → block all air movement
                    velocityChange.x = 0f;
                    velocityChange.z = 0f;
                }
            }*/

            velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Jump()
    {
        // Reset vertical velocity for consistency
        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;

        // Normal jump only
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }


    void Look()
    {
        if (isPaused) return;
        
        float mouseX = look.x * sensitivity * Time.deltaTime;
        float mouseY = look.y * sensitivity * Time.deltaTime;

        // Pitch
        lookRotation -= mouseY;
        lookRotation = Mathf.Clamp(lookRotation, -80f, 80f);

        // Rotate BODY left/right (yaw)
        transform.Rotate(Vector3.up * mouseX);

        head.localRotation = Quaternion.Euler(lookRotation, 0f, 0f);

    }

        void HandleStamina()
    {
        // Sprinting on ground → drain
        if (isRunning && grounded && move.y > 0)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isRunning = false;
            }
        }
        // Sprinting in air → small drain (THIS is where your line goes)
        else if (isRunning && !grounded)
        {
            currentStamina -= (staminaDrainRate * 0.3f) * Time.deltaTime;
        }
        // Not sprinting → regen
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;

            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }

        if (staminaBar != null)
            staminaBar.SetStamina(currentStamina);
    }

    // Grounded Setter
    public void SetGrounded(bool state)
    {
        grounded = state;
    }
    public void FreezeLookState()
    {
        Vector3 angles = head.localEulerAngles;

        if (angles.x > 180f) angles.x -= 360f;

        lookRotation = angles.x;
    }
}