using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A first-person camera controller.
/// </summary>
/// This script provides a first-person camera experience with features like mouse look,
/// head bobbing, dynamic field of view (FOV) adjustments, and camera sway. It's designed to be
/// attached to a camera object within a first-person player model.
public class FirstPersonCamera : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    [Tooltip("Sensitivity of mouse movement.")]
    public float mouseSensitivity = 100f;

    [Tooltip("Reference to the player's body transform for rotation.")]
    public Transform playerBody;

    [Tooltip("The camera component attached to this object.")]
    public Camera playerCamera;

    // Internal variable to track the camera's x-axis rotation.
    private float xRotation = 0f;

    [Header("Head Bobbing Settings")]
    [Tooltip("Speed of head bobbing.")]
    public float bobbingSpeed = 0.18f;

    [Tooltip("Amount of head bobbing.")]
    public float bobbingAmount = 0.2f;

    [Tooltip("Midpoint of head bobbing on the y-axis.")]
    public float midpoint = 2f;

    [Header("Field of View Settings")]
    [Tooltip("Normal field of view.")]
    public float baseFOV = 60f;

    [Tooltip("Field of view when sprinting.")]
    public float sprintFOV = 70f;

    [Tooltip("Speed of FOV change.")]
    public float fovChangeSpeed = 5f;

    [Tooltip("Flag to determine if the player is sprinting.")]
    public bool isSprinting = false;

    [Header("Camera Sway Settings")]
    [Tooltip("Amount of camera sway.")]
    public float swayAmount = 0.05f;

    [Tooltip("Speed of camera sway.")]
    public float swaySpeed = 4f;

    // Timer for head bobbing calculations.
    private float timer = 0.0f;

    // Input references
    private Mouse mouse;
    private Keyboard keyboard;

    public GameObject gun;
    public GameObject gunFiring;

    /// <summary>
    /// Initialization method.
    /// </summary>
    void Start()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned in the inspector.");
            return;
        }

        if (playerCamera.tag == "Player")
        {
            
        }
        

        // Initialize input devices
        mouse = Mouse.current;
        keyboard = Keyboard.current;

        Cursor.lockState = CursorLockMode.Locked;
        playerCamera.fieldOfView = baseFOV;
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        HandleMouseLook();
        HandleHeadBobbing();
        HandleFieldOfView();
        HandleCameraSway();
        HandleClick();
    }

    /// <summary>
    /// Handles the mouse look functionality.
    /// </summary>
    void HandleMouseLook()
    {
        if (mouse == null) return;

        Vector2 mouseDelta = mouse.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void HandleClick()
    {
        if (mouse == null) return;

        if (mouse.leftButton.isPressed)
        {
            gun.SetActive(false);
            gunFiring.SetActive(true);
        } else {
            gun.SetActive(true);
            gunFiring.SetActive(false);
        }
    }
    /// <summary>
    /// Handles the head bobbing effect.
    /// </summary>
    void HandleHeadBobbing()
    {
        if (keyboard == null) return;

        float waveslice = 0.0f;
        float horizontal = 0f;
        float vertical = 0f;

        // Check keyboard input
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal = -1f;
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal = 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical = -1f;
        else if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical = 1f;

        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
        {
            timer = 0.0f;
        }
        else
        {
            waveslice = Mathf.Sin(timer);
            timer = timer + bobbingSpeed;
            if (timer > Mathf.PI * 2)
            {
                timer = timer - (Mathf.PI * 2);
            }
        }

        Vector3 localPos = transform.localPosition;
        if (waveslice != 0)
        {
            float translateChange = waveslice * bobbingAmount;
            float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
            translateChange = totalAxes * translateChange;
            localPos.y = midpoint + translateChange;
        }
        else
        {
            localPos.y = midpoint;
        }

        transform.localPosition = localPos;
    }

    /// <summary>
    /// Handles the dynamic field of view.
    /// </summary>
    void HandleFieldOfView()
    {
        if (isSprinting)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, fovChangeSpeed * Time.deltaTime);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, baseFOV, fovChangeSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Handles the camera sway effect.
    /// </summary>
    void HandleCameraSway()
    {
        // if (mouse == null) return;

        // fixme
        // causes weird movement 
        /*
        Vector2 mouseDelta = mouse.delta.ReadValue();
        float movementX = Mathf.Clamp(mouseDelta.x * swayAmount, -swayAmount, swayAmount);
        float movementY = Mathf.Clamp(mouseDelta.y * swayAmount, -swayAmount, swayAmount);
        Vector3 finalPosition = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + finalPosition, swaySpeed * Time.deltaTime);
        */
    }
}