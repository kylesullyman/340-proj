using UnityEngine;

public class RigidbodyFPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 2f;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    
    
    // Private variables
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 mouseInput;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool jumpRequested;
    
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
    }
    
    void FixedUpdate()
    {
        // STEP 4: Apply all physics-based movement
        HandleMovement();
        HandleJump();
        ApplyDrag();
    }
    
    void CaptureInput()
    {
        // Get movement input (WASD or arrow keys)
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
        
        // Get mouse input
        mouseInput.x = Input.GetAxis("Mouse X");
        mouseInput.y = Input.GetAxis("Mouse Y");
        
        // Check for jump input (queue it for FixedUpdate)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
        
        // Allow unlocking cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
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
        // Calculate movement direction relative to where player is looking
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        // Normalize to prevent faster diagonal movement
        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();
        
        // Calculate target velocity (only horizontal movement)
        Vector3 targetVelocity = moveDirection * moveSpeed;
        
        // Keep the current vertical velocity (gravity/jumping)
        targetVelocity.y = rb.linearVelocity.y;
        
        // Apply the velocity
        rb.linearVelocity = targetVelocity;
    }
    
    void HandleJump()
    {
        if (jumpRequested)
        {
            // Apply instant upward velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            jumpRequested = false; // Reset the flag
        }
    }
    
    void ApplyDrag()
    {
        // Apply more drag when grounded for responsive stopping
        rb.linearDamping = isGrounded ? groundDrag : airDrag;
    }
}