using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    // Camera Rotation
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject cameraHolder;

    // Ground Movement
    private Rigidbody rb;
    private PlayerTeam playerTeam;
    public float MoveSpeed = 5f;
    private float moveHorizontal;
    private float moveForward;

    // Jumping
    public float jumpForce = 6f;
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private float groundCheckTimer = 0f;
    private float groundCheckDelay = 0.2f;
    private float playerHeight;
    private float raycastDistance;

    // Landing impact
    private bool wasGrounded = true;
    public float landingImpactAmount = 0.15f;
    public float landingImpactSpeed = 8f;
    private float cameraYOffset = 0f;
    private float targetCameraYOffset = 0f;
    private Vector3 cameraDefaultLocalPosition;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        playerTeam = GetComponent<PlayerTeam>();

        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.1f;

        rb.isKinematic = !IsOwner;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        if (cameraTransform != null)
            cameraDefaultLocalPosition = cameraTransform.localPosition;

        if (!IsOwner && cameraHolder != null)
        {
            cameraHolder.SetActive(false);
        }
    }

    private bool CanControl()
    {
        return IsOwner && playerTeam != null && playerTeam.CurrentTeam.Value != Team.None;
    }

    void Update()
    {
        if (!CanControl()) return;

        moveHorizontal = 0f;
        moveForward = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveForward = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveForward = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveHorizontal = 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveHorizontal = -1f;
        }

        RotateCamera();
        LandingImpact();

        if (groundCheckTimer <= 0f)
        {
            isGrounded = Physics.CheckSphere(
                transform.position + Vector3.down * (raycastDistance - 0.1f), 0.2f, groundLayer);
        }
        else
        {
            groundCheckTimer -= Time.deltaTime;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (!CanControl()) return;
        MovePlayer();
    }

    void MovePlayer()
    {
        Vector3 moveDirection = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;

        bool hitsWall = Physics.Raycast(transform.position, moveDirection, 0.6f);

        if (hitsWall && isGrounded)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        if (!isGrounded)
        {
            if (moveDirection.magnitude > 0)
            {
                Vector3 airControl = moveDirection * MoveSpeed * 0.8f;
                rb.linearVelocity = new Vector3(
                    Mathf.Lerp(rb.linearVelocity.x, airControl.x, Time.fixedDeltaTime * 8f),
                    rb.linearVelocity.y,
                    Mathf.Lerp(rb.linearVelocity.z, airControl.z, Time.fixedDeltaTime * 8f)
                );
            }
            return;
        }

        Vector3 horizontalVelocity = moveDirection * MoveSpeed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }

    void RotateCamera()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float horizontalRotation = mouseDelta.x * mouseSensitivity * 0.1f;
        transform.Rotate(0, horizontalRotation, 0);

        verticalRotation -= mouseDelta.y * mouseSensitivity * 0.1f;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }

    void LandingImpact()
    {
        if (!wasGrounded && isGrounded)
        {
            targetCameraYOffset = -landingImpactAmount;
        }
        wasGrounded = isGrounded;

        targetCameraYOffset = Mathf.Lerp(targetCameraYOffset, 0f, Time.deltaTime * landingImpactSpeed);
        cameraYOffset = Mathf.Lerp(cameraYOffset, targetCameraYOffset, Time.deltaTime * landingImpactSpeed);

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = new Vector3(
                cameraDefaultLocalPosition.x,
                cameraDefaultLocalPosition.y + cameraYOffset,
                cameraDefaultLocalPosition.z
            );
        }
    }

    void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }
}