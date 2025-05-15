using UnityEngine;

public class WeaponSwayAndBob : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float swayMaxAmount = 0.06f;
    [SerializeField] private float swaySmoothness = 8.0f;
    [SerializeField] private float swayResetSmoothness = 6.0f;
    [SerializeField] private bool invertXSway = false;
    [SerializeField] private bool invertYSway = false;

    [Header("Bob Settings")]
    [SerializeField] private float bobSpeed = 10.0f;
    [SerializeField] private float bobAmount = 0.05f;
    [SerializeField] private float sprintBobMultiplier = 1.5f;
    [SerializeField] private bool enableBobbing = true;

    [Header("Breathing")]
    [SerializeField] private float breathingSpeed = 1.0f;
    [SerializeField] private float breathingAmount = 0.02f;
    [SerializeField] private bool enableBreathing = true;

    [Header("Position Adjustment")]
    [SerializeField] private Vector3 weaponPositionOffset = new Vector3(0.0f, 0.0f, 0.0f);
    [SerializeField] private Vector3 aimingPositionOffset = new Vector3(0.0f, 0.0f, 0.0f);
    [SerializeField] private float aimSmoothing = 10.0f;

    // References
    private PlayerMovement playerMovement; // Reference to your PlayerMovement script 
    private Transform playerCamera;
    
    // Internal variables
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float bobTimer = 0.0f;
    private Vector3 targetSwayPosition;
    private Vector3 targetSwayRotation;
    private Vector3 currentSwayPosition;
    private Vector3 currentSwayRotation;
    private bool isAiming = false;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        playerMovement = GetComponentInParent<PlayerMovement>();
        
        // Find the player camera
        playerCamera = transform.parent;
        while (playerCamera != null && !playerCamera.GetComponent<Camera>())
        {
            playerCamera = playerCamera.parent;
            if (playerCamera.GetComponent<Camera>())
                break;
        }
        
        if (playerCamera == null)
        {
            Debug.LogError("WeaponSwayAndBob: Could not find camera in parent hierarchy!");
            playerCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        // Check for aiming input (e.g., right mouse button)
        if (Input.GetMouseButton(1))
        {
            isAiming = true;
        }
        else
        {
            isAiming = false;
        }
        
        // Apply all effects
        CalculateWeaponSway();
        CalculateWeaponBobbing();
        CalculateBreathing();
        ApplyPositionAdjustment();
    }

    private void CalculateWeaponSway()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * (invertXSway ? -1 : 1);
        float mouseY = Input.GetAxis("Mouse Y") * (invertYSway ? -1 : 1);

        // Reduce sway when aiming
        float aimSwayMultiplier = isAiming ? 0.3f : 1.0f;
        
        // Calculate target position based on mouse movement
        Vector3 targetPositionSway = new Vector3(mouseX * swayAmount * aimSwayMultiplier, mouseY * swayAmount * aimSwayMultiplier, 0);
        targetPositionSway = Vector3.ClampMagnitude(targetPositionSway, swayMaxAmount * aimSwayMultiplier);

        // Calculate target rotation based on mouse movement
        Vector3 targetRotationSway = new Vector3(-mouseY * swayAmount * 20.0f * aimSwayMultiplier, mouseX * swayAmount * 20.0f * aimSwayMultiplier, mouseX * swayAmount * 10.0f * aimSwayMultiplier);
        
        // Smoothly interpolate to the target sway
        currentSwayPosition = Vector3.Lerp(currentSwayPosition, targetPositionSway, Time.deltaTime * swaySmoothness);
        currentSwayRotation = Vector3.Lerp(currentSwayRotation, targetRotationSway, Time.deltaTime * swaySmoothness);
        
        // Reset when no input
        if (Mathf.Approximately(mouseX, 0) && Mathf.Approximately(mouseY, 0))
        {
            currentSwayPosition = Vector3.Lerp(currentSwayPosition, Vector3.zero, Time.deltaTime * swayResetSmoothness);
            currentSwayRotation = Vector3.Lerp(currentSwayRotation, Vector3.zero, Time.deltaTime * swayResetSmoothness);
        }
    }

    private void CalculateWeaponBobbing()
    {
        if (!enableBobbing || playerMovement == null)
            return;
            
        // Get horizontal and vertical movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
        
        // Check if player is sprinting - modify based on your movement system
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && verticalInput > 0;
        float bobMultiplier = isSprinting ? sprintBobMultiplier : 1.0f;
        
        if (isMoving && playerMovement.isGrounded)
        {
            // Increment timer for bob cycle
            bobTimer += Time.deltaTime * bobSpeed * bobMultiplier;
            
            // Calculate bob effect - different frequency for x and y for more natural movement
            float xBob = Mathf.Sin(bobTimer) * bobAmount * bobMultiplier;
            float yBob = Mathf.Sin(bobTimer * 2) * bobAmount * bobMultiplier;
            
            targetSwayPosition += new Vector3(xBob, yBob, 0);
        }
        else
        {
            // Reset bob timer when not moving
            bobTimer = 0;
        }
    }
    
    private void CalculateBreathing()
    {
        if (!enableBreathing)
            return;
            
        // Simple breathing effect
        float breathingY = Mathf.Sin(Time.time * breathingSpeed) * breathingAmount;
        float breathingX = Mathf.Sin(Time.time * breathingSpeed * 0.5f) * breathingAmount * 0.5f;
        
        Vector3 breathingEffect = new Vector3(breathingX, breathingY, 0);
        
        // Apply breathing effect to target sway
        targetSwayPosition += breathingEffect;
    }
    
    private void ApplyPositionAdjustment()
    {
        // Calculate base position
        Vector3 basePosition = initialPosition + weaponPositionOffset;
        
        // Apply aiming position if aiming
        Vector3 targetPosition = isAiming ? initialPosition + aimingPositionOffset : basePosition;
        
        // Apply all effects
        Vector3 finalPosition = targetPosition + currentSwayPosition + targetSwayPosition;
        Quaternion finalRotation = initialRotation * Quaternion.Euler(currentSwayRotation);
        
        // Smooth the aiming transition
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, Time.deltaTime * aimSmoothing);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation, Time.deltaTime * aimSmoothing);
        
        // Reset target sway position for next frame
        targetSwayPosition = Vector3.zero;
    }
}