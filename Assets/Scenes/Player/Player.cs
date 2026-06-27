using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem; // DŮLEŽITÉ: Přidáno pro funkčnost nového Input Systemu

/*
    This script provides jumping and movement in Unity 3D - Gatsby (Multiplayer Edition)
    Pohyb řešen přes Rigidbody.velocity (vlastník = nekinematický, ostatní = kinematic)
*/

public class Player : NetworkBehaviour
{
    // Camera Rotation
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject cameraHolder;

    // Ground Movement
    private Rigidbody rb;
    public float MoveSpeed = 5f;
    private float moveHorizontal;
    private float moveForward;

    // Jumping
    public float jumpForce = 6f; // hodnota teď reprezentuje rychlost (m/s), nejspíš budeš muset snížit oproti původní
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private float groundCheckTimer = 0f;
    private float groundCheckDelay = 0.2f;
    private float playerHeight;
    private float raycastDistance;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.1f;

        // Jen vlastník (autoritativní strana) reálně simuluje fyziku přes
        // dynamické Rigidbody. Ostatní instance (pohled cizích klientů na tuto
        // postavu) zůstávají kinematic a jen sledují synchronizovaný transform
        // přes ClientNetworkTransform.
        rb.isKinematic = !IsOwner;
        rb.useGravity = true;

        // Zabrání převrácení postavy na stranu při kolizích. Rotaci kolem Y
        // řídíme manuálně v RotateCamera(), proto ji necháváme volnou.
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (cameraHolder != null)
            {
                cameraHolder.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // NOVÝ INPUT SYSTEM: Čtení pohybu z klávesnice (WASD / Šipky)
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

        // Detekce země pod nohama
        if (groundCheckTimer <= 0f)
        {
            isGrounded = Physics.CheckSphere(
              transform.position + Vector3.down * (raycastDistance - 0.1f), 0.2f, groundLayer);
        }
        else
        {
            groundCheckTimer -= Time.deltaTime;
        }

        // NOVÝ INPUT SYSTEM: Skok (Mezerník)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        MovePlayer();
    }

    void MovePlayer()
    {
        Vector3 moveDirection = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;

        // Zkontroluj jestli je před hráčem zeď
        bool hitsWall = Physics.Raycast(transform.position, moveDirection, 0.6f);

        if (hitsWall)
        {
            // Zastav horizontální pohyb ale nech gravitaci
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 horizontalVelocity = moveDirection * MoveSpeed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }

    void RotateCamera()
    {
        // NOVÝ INPUT SYSTEM: Čtení pohybu myši
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float horizontalRotation = mouseDelta.x * mouseSensitivity * 0.1f; // Multiplikátor 0.1f vyrovnává vyšší citlivost delty myši
        transform.Rotate(0, horizontalRotation, 0);

        verticalRotation -= mouseDelta.y * mouseSensitivity * 0.1f;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }

    void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }
}