using System.Collections;
using System.Collections.Generic;
using Unity.Netcode; // <-- 1. DÙLEŽITÉ: Pøidali jsme knihovnu pro Netcode
using UnityEngine;

/*
    This script provides jumping and movement in Unity 3D - Gatsby (Multiplayer Edition)
*/

// <-- 2. ZMÌNA: Tøída teï dìdí z NetworkBehaviour místo MonoBehaviour
public class Player : NetworkBehaviour
{
    // Camera Rotation
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    // <-- 3. ZMÌNA: Kameru si pøiøadíme z Inspectoru, abychom ji mohli cizím hráèùm vypnout
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject cameraHolder; // Objekt, na kterém drží kamera a audio listener

    // Ground Movement
    private Rigidbody rb;
    public float MoveSpeed = 5f;
    private float moveHorizontal;
    private float moveForward;

    // Jumping
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float ascendMultiplier = 2f;
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private float groundCheckTimer = 0f;
    private float groundCheckDelay = 0.3f;
    private float playerHeight;
    private float raycastDistance;

    // <-- 4. ZMÌNA: V multiplayeru se místo Start používá OnNetworkSpawn
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Nastavení raycastu pod nohy
        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;

        // Pokud tato postava PATØÍ MNÌ (já jsem ten, kdo ji ovládá)
        if (IsOwner)
        {
            // Skryjeme myš
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Pokud postava PATØÍ NÌKOMU JINÉMU pøes internet:
            // Vypneme její kameru, abychom se nekoukali z cizích oèí!
            if (cameraHolder != null)
            {
                cameraHolder.SetActive(false);
            }

            // Pro jistotu vypneme fyziku na cizím hráèi, aby nám s ním netrhalo naše gravitace
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        // <-- 5. KLÍÈOVÁ ZMÌNA: Pokud tahle postava není moje, neèti moji klávesnici ani myš!
        if (!IsOwner) return;

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        RotateCamera();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        if (!isGrounded && groundCheckTimer <= 0f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayer);
        }
        else
        {
            groundCheckTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // <-- Pokud postava není moje, nehýbej s ní na mém PC
        if (!IsOwner) return;

        MovePlayer();
        ApplyJumpPhysics();
    }

    void MovePlayer()
    {
        Vector3 movement = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;
        Vector3 targetVelocity = movement * MoveSpeed;

        Vector3 velocity = rb.velocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.velocity = velocity;

        if (isGrounded && moveHorizontal == 0 && moveForward == 0)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void RotateCamera()
    {
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, horizontalRotation, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    void ApplyJumpPhysics()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * ascendMultiplier * Time.fixedDeltaTime;
        }
    }
}