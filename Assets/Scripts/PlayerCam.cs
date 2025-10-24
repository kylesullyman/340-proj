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
    [Header("References")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RigidbodyFPSController rbController;
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject gunFiring;

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Head Bobbing Settings")]
    [SerializeField] private float bobbingSpeed = 0.18f;
    [SerializeField] private float bobbingAmount = 0.2f;
    [SerializeField] private float midpoint = 2f;

    [Header("Slide Camera Settings")]
    [SerializeField] private float slidePositionOffset = -2.5f;
    [SerializeField] private float slideCameraTransitionSpeed = 10f;
    [SerializeField] private float slideCameraReturnSpeed = 5f;

    [Header("Field of View Settings")]
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float fovChangeSpeed = 5f;

    [Header("Runtime State")]
    public bool isSprinting = false;

    // Cached values
    private float xRotation = 0f;
    private float bobbingTimer = 0f;
    private float targetFOV;
    private const float TWO_PI = Mathf.PI * 2f;
    private float baseYPosition = 0f;
    private float slideRotationOffset = 0f; // Separate offset for slide rotation

    // Input references (cached for performance)
    private Mouse mouse;
    private Keyboard keyboard;

    void Awake()
    {
        // Cache input devices early
        mouse = Mouse.current;
        keyboard = Keyboard.current;
    }

    void Start()
    {
        // Validate required references
        if (rbController == null)
        {
            Debug.LogError("RigidbodyFPSController is not assigned in the inspector.", this);
            enabled = false;
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned in the inspector.", this);
            enabled = false;
            return;
        }

        if (playerBody == null)
        {
            Debug.LogError("PlayerBody is not assigned in the inspector.", this);
            enabled = false;
            return;
        }

        // Initialize state
        targetFOV = baseFOV;
        playerCamera.fieldOfView = baseFOV;
        Cursor.lockState = CursorLockMode.Locked;

        // Store base camera values for slide transitions
        baseYPosition = midpoint;
    }

    void Update()
    {
        HandleFieldOfView();
        HandleGunVisibility();
        HandleSlideCamera();

        if (rbController.isGrounded && !rbController.IsSliding)
        {
            HandleHeadBobbing();
        }
    }

    void LateUpdate()
    {
        // Camera rotation happens here to ensure smooth movement after physics
        HandleMouseLook();
    }

    void HandleMouseLook()
    {
        if (mouse == null) return;

        Vector2 mouseDelta = mouse.delta.ReadValue();
        float sensitivityMultiplier = mouseSensitivity * Time.deltaTime;

        xRotation -= mouseDelta.y * sensitivityMultiplier;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply both mouse rotation and slide offset
        float finalRotation = xRotation + slideRotationOffset;
        transform.localRotation = Quaternion.Euler(finalRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * (mouseDelta.x * sensitivityMultiplier));
    }

    void HandleGunVisibility()
    {
        if (mouse == null || gun == null || gunFiring == null) return;

        bool isFiring = mouse.leftButton.isPressed;
        if (isFiring)
        {
            gun.SetActive(false);
            gunFiring.SetActive(true);
        }
        else
        {
            gun.SetActive(true);
            gunFiring.SetActive(false);
        }
    }
    void HandleHeadBobbing()
    {
        if (keyboard == null) return;

        // Get movement input
        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal = -1f;
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal = 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical = -1f;
        else if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical = 1f;

        // Calculate movement magnitude
        bool isMoving = horizontal != 0f || vertical != 0f;

        if (!isMoving)
        {
            bobbingTimer = 0f;
            // Smoothly return to midpoint
            Vector3 localPos = transform.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, midpoint, Time.deltaTime * 5f);
            transform.localPosition = localPos;
            return;
        }

        // Update bobbing timer
        bobbingTimer += bobbingSpeed;
        if (bobbingTimer > TWO_PI)
        {
            bobbingTimer -= TWO_PI;
        }

        // Calculate bobbing offset
        float waveslice = Mathf.Sin(bobbingTimer);
        float movementMagnitude = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        float bobbingOffset = waveslice * bobbingAmount * movementMagnitude;

        // Apply bobbing
        Vector3 newLocalPos = transform.localPosition;
        newLocalPos.y = midpoint + bobbingOffset;
        transform.localPosition = newLocalPos;
    }

    void HandleSlideCamera()
    {
        if (rbController.IsSliding)
        {
            // Target position: move down by slidePositionOffset
            float targetYPosition = baseYPosition + slidePositionOffset;

            // Smoothly interpolate position (fast transition down)
            Vector3 newLocalPos = transform.localPosition;
            newLocalPos.y = Mathf.Lerp(newLocalPos.y, targetYPosition, slideCameraTransitionSpeed * Time.deltaTime);
            transform.localPosition = newLocalPos;
        }
        else
        {
            // Return to base position with smoother, slower transition
            Vector3 newLocalPos = transform.localPosition;
            newLocalPos.y = Mathf.Lerp(newLocalPos.y, midpoint, slideCameraReturnSpeed * Time.deltaTime);
            transform.localPosition = newLocalPos;
        }
    }

    void HandleFieldOfView()
    {
        targetFOV = isSprinting ? sprintFOV : baseFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }
}