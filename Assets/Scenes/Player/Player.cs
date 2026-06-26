using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/*
    This script provides jumping and movement in Unity 3D - Gatsby (Multiplayer Edition)
    Pohyb øešen pøes Rigidbody.velocity (vlastník = nekinematický, ostatní = kinematic)
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
    public float jumpForce = 6f; // hodnota teï reprezentuje rychlost (m/s), nejspíš budeš muset snížit oproti pùvodní
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

        // Jen vlastník (autoritativní strana) reálnì simuluje fyziku pøes
        // dynamický Rigidbody. Ostatní instance (pohled cizích klientù na tuto
        // postavu) zùstávají kinematic a jen sledují synchronizovaný transform
        // pøes ClientNetworkTransform.
        rb.isKinematic = !IsOwner;
        rb.useGravity = true;

        // Zabrání pøevrácení postavy na stranu pøi kolizích. Rotaci kolem Y
        // øídíme manuálnì v RotateCamera(), proto ji necháváme volnou.
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        RotateCamera();

        // Detekce zemì pod nohama
        if (groundCheckTimer <= 0f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayer);
        }
        else
        {
            groundCheckTimer -= Time.deltaTime;
        }

        // Skok
        if (Input.GetButtonDown("Jump") && isGrounded)
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
        // Spoèítáme smìr horizontálního pohybu
        Vector3 moveDirection = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;
        Vector3 horizontalVelocity = moveDirection * MoveSpeed;

        // Pøepíšeme jen horizontální složku rychlosti, vertikální (gravitace/skok)
        // necháme na pokoji, o tu se stará fyzikální engine (useGravity).
        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
    }

    void RotateCamera()
    {
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, horizontalRotation, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
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
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }
}