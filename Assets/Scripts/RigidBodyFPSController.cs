using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RigidbodyFPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 2f;
    [SerializeField] [Range(0f, 5f)] private float gravityMultiplier = 1f;

    [Header("Slide Settings")]
    [SerializeField] private float slideSpeed = 12f;
    [SerializeField] private float slideDuration = 2f;
    [SerializeField] private float slideMultiplier = 3f;
    [SerializeField] private float slideDrag = 15f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private float playerSpeed;

    // Private variables
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 mouseInput;
    // private float xRotation = 0f;
    private bool jumpRequested;
    private bool isSliding;
    private Vector3 slideDirection;
    private Coroutine slideCoroutine;

    // public variables
    public bool isGrounded;
    public bool IsSliding => isSliding;

    // Input references (cached for performance)
    private Keyboard keyboard;
    private Mouse mouse;

    void Awake()
    {
        // Cache input devices early
        keyboard = Keyboard.current;
        mouse = Mouse.current;
    }

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Validate references
        if (groundCheck == null)
            Debug.LogError("GroundCheck not assigned!");
    }
    
    void Update()
    {
        // STEP 1: Capture input (do this in Update, not FixedUpdate)
        CaptureInput();

        // STEP 3: Check if we're on the ground
        CheckGrounded();

        // Display player speed
        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f; 
        playerSpeed = horizontalVelocity.magnitude;
        // Debug.Log($"Player Speed: {playerSpeed:F2} units/s");
    }
    
    void FixedUpdate()
    {
        // STEP 4: Apply all physics-based movement
        HandleMovement();
        HandleJump();
        ApplyDrag();
        ApplyGravity();
    }
    
    void CaptureInput()
    {
        if (keyboard == null) return;

        // Get movement input (WASD or arrow keys)
        moveInput.x = 0f;
        moveInput.y = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput.x = -1f;
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput.x = 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveInput.y = -1f;
        else if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveInput.y = 1f;

        // Get mouse input
        if (mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            mouseInput = mouseDelta;
        }

        // Check for slide input (LCtrl) - only trigger on key DOWN, not held
        if (keyboard.leftCtrlKey.wasPressedThisFrame && isGrounded)
        {
            StartSlide();
        }

        // Cancel slide if key is released early
        if (keyboard.leftCtrlKey.wasReleasedThisFrame && isSliding)
        {
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }
            EndSlide();
        }

        // Check for jump input (queue it for FixedUpdate)
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
        {
            jumpRequested = true;
        }

        // Allow unlocking cursor with Escape
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void CheckGrounded()
    {
        // Use a small sphere to check if we're touching ground
        isGrounded = Physics.CheckSphere(
            groundCheck.position, 
            groundDistance, 
            groundLayer
        );
    }
    
    void HandleMovement()
    {
        Vector3 targetVelocity;

        if (isSliding && playerSpeed > 10f)
        {
            // During slide, boost current velocity in the slide direction
            targetVelocity = slideDirection * (slideSpeed*slideMultiplier);
        }
        else
        {
            // Calculate movement direction relative to where player is looking
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

            // Normalize to prevent faster diagonal movement
            if (moveDirection.magnitude > 0f)
            {
                moveDirection.Normalize();
            }

            // Calculate target velocity (only horizontal movement)
            targetVelocity = moveDirection * moveSpeed;
        }

        // Keep the current vertical velocity (gravity/jumping)
        targetVelocity.y = rb.linearVelocity.y;

        // Smoothly interpolate to target velocity for less jitter
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 0.5f);
    }
    
    void HandleJump()
    {
        if (jumpRequested)
        {
            if (isSliding)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce*2, rb.linearVelocity.z);
                jumpRequested = false; // Reset the flag

                return; 
            }
            // Apply instant upward velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            jumpRequested = false; // Reset the flag

        }
    }
    
    void ApplyDrag()
    {
        // Apply specific drag based on state
        if (isSliding)
        {
            rb.linearDamping = slideDrag;
        }
        else
        {
            // Apply more drag when grounded for responsive stopping
            rb.linearDamping = isGrounded ? groundDrag : airDrag;
        }
    }

    void ApplyGravity()
    {
        // Apply additional gravity force based on multiplier
        if (gravityMultiplier > 1f)
        {
            Vector3 extraGravity = Physics.gravity * (gravityMultiplier - 1f);
            rb.AddForce(extraGravity, ForceMode.Acceleration);
        }
    }

    void StartSlide()
    {
        // Debug.Log("SLIDE STARTED");
        isSliding = true;

        // Get current horizontal velocity
        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0f; // Ignore vertical component

        // If player is moving, use their movement direction; otherwise use forward
        if (currentVelocity.magnitude > 0.1f)
        {
            slideDirection = currentVelocity.normalized;
        }
        else
        {
            // If stationary, slide forward
            slideDirection = transform.forward;
        }

        // Start the slide timer coroutine
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }
        slideCoroutine = StartCoroutine(SlideTimer());
    }

    IEnumerator SlideTimer()
    {
        float elapsed = 0f;

        while (elapsed < slideDuration/2)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        //  Debug.Log("SLIDE ENDED");
        EndSlide();
    }

    void EndSlide()
    {
        isSliding = false;
        slideCoroutine = null;

        // Immediately stop horizontal momentum
        Vector3 currentVel = rb.linearVelocity;
        currentVel.x = 0f;
        currentVel.z = 0f;
        rb.linearVelocity = currentVel;
    }
}