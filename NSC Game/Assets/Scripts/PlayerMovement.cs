using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 14f;
    public float airControlMultiplier = 0.6f;
    public float gravity = -16f;
    public float jumpHeight = 2.5f;
    private Vector3 currentVelocity;
    public float acceleration = 10f;
    public float deceleration = 15f;

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 150f;

    [Header("Camera Tilt")]
    public float tiltAmount = 10f;
    public float tiltSpeed = 8f;
    private float currentTilt = 0f;

    [Header("Ground Check")]
    public float groundRayLength = 1.1f;
    public LayerMask groundMask;

    [Header("Head Bobbing (Minimal)")]
    public float bobSpeed = 10f;
    public float bobAmount = 0.02f;
    private float defaultYPos;
    private float bobTimer = 0f;
    private bool wasBobbingDown = false;

    [Header("Falling")]
    public float fallThresholdY = -20f;
    private Vector3 lastSafePosition;

    [Header("Footstep Sound")]
    public AudioSource audioSource;
    public AudioClip[] footstepClips;
    public float footstepCooldown = 0.4f;
    private float nextFootstepTime = 0f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    public bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        defaultYPos = cameraTransform.localPosition.y;
        lastSafePosition = transform.position;
    }

    void Update()
    {
        GroundCheck();
        HandleMovement();
        HandleLook();
        HandleCameraTilt();
        HandleHeadBobbing();

        if (controller.isGrounded)
        {
            lastSafePosition = transform.position;
        }

        if (transform.position.y < fallThresholdY)
        {
            Teleport();
        }
    }

    void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundRayLength, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = transform.right * moveX + transform.forward * moveZ;
        inputDir.Normalize();

        Vector3 targetVelocity;

        if (isGrounded)
        {
            targetVelocity = inputDir * moveSpeed;

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            targetVelocity = inputDir * moveSpeed * airControlMultiplier;
        }

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, (isGrounded ? acceleration : acceleration * airControlMultiplier) * Time.deltaTime);

        Vector3 move = currentVelocity + Vector3.up * velocity.y;
        controller.Move(move * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleCameraTilt()
    {
        float tiltTarget = 0f;
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        tiltTarget = -horizontalInput * tiltAmount;

        currentTilt = Mathf.Lerp(currentTilt, tiltTarget, Time.deltaTime * tiltSpeed);
    }

    void HandleHeadBobbing()
    {
        bool isMoving = (Mathf.Abs(currentVelocity.x) > 0.1f || Mathf.Abs(currentVelocity.z) > 0.1f) && isGrounded;

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobSin = Mathf.Sin(bobTimer);
            float bobY = defaultYPos + bobSin * bobAmount;
            float bobX = Mathf.Cos(bobTimer / 2) * bobAmount / 2;
            cameraTransform.localPosition = new Vector3(bobX, bobY, cameraTransform.localPosition.z);

            // Play footstep sound when bobbing passes downward through 0
            if (bobSin < 0 && !wasBobbingDown && Time.time >= nextFootstepTime)
            {
                PlayFootstepSound();
                nextFootstepTime = Time.time + footstepCooldown;
            }

            wasBobbingDown = bobSin < 0;
        }
        else
        {
            bobTimer = 0;
            wasBobbingDown = false;
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                new Vector3(0, defaultYPos, cameraTransform.localPosition.z),
                Time.deltaTime * bobSpeed);
        }
    }

    void PlayFootstepSound()
    {
        if (footstepClips.Length > 0 && audioSource != null)
        {
            int index = Random.Range(0, footstepClips.Length);
            audioSource.PlayOneShot(footstepClips[index]);
        }
    }

    void Teleport()
    {
        controller.enabled = false;
        transform.position = lastSafePosition + Vector3.up * 2f;
        controller.enabled = true;
    }
}
