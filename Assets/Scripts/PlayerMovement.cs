using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 5.0f;
    public Camera playerCamera;
    
    // You can tweak this value in the Inspector. -9.81f is a realistic starting point.
    public float gravityValue = -9.81f; 

    private CharacterController controller;
    private Vector2 moveInput;
    
    // This new Vector3 will store and track the player's falling speed.
    private Vector3 playerVelocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        // A bool to check if the controller is on the ground.
        bool isGrounded = controller.isGrounded;

        // If the player is on the ground and their vertical velocity is negative...
        if (isGrounded && playerVelocity.y < 0)
        {
            // ...reset their vertical velocity. A small value like -2f helps keep them grounded.
            playerVelocity.y = -2f;
        }

        // --- Horizontal Movement (Camera-Relative) ---
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x);
        
        // This part remains the same: it moves the player horizontally.
        controller.Move(moveDirection * playerSpeed * Time.deltaTime);

        // --- Vertical Movement (Applying Gravity) ---
        // If the player is NOT grounded, apply gravity.
        if (!isGrounded)
        {
            // Velocity increases over time based on the gravity value.
            playerVelocity.y += gravityValue * Time.deltaTime;
        }

        // Finally, apply the calculated gravity to the controller.
        // This moves the player down (or up, if velocity is positive).
        controller.Move(playerVelocity * Time.deltaTime);
    }
}