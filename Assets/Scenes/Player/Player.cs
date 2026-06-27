using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/*
    This script provides jumping and movement in Unity 3D - Gatsby (Multiplayer Edition)
    Pohyb �e�en p�es Rigidbody.velocity (vlastn�k = nekinematick�, ostatn� = kinematic)
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
    public float jumpForce = 6f; // hodnota te� reprezentuje rychlost (m/s), nejsp� bude� muset sn�it oproti p�vodn�
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

        // Jen vlastn�k (autoritativn� strana) re�ln� simuluje fyziku p�es
        // dynamick� Rigidbody. Ostatn� instance (pohled ciz�ch klient� na tuto
        // postavu) z�st�vaj� kinematic a jen sleduj� synchronizovan� transform
        // p�es ClientNetworkTransform.
        rb.isKinematic = !IsOwner;
        rb.useGravity = true;

        // Zabr�n� p�evr�cen� postavy na stranu p�i koliz�ch. Rotaci kolem Y
        // ��d�me manu�ln� v RotateCamera(), proto ji nech�v�me volnou.
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

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        RotateCamera();

        // Detekce zem� pod nohama
        if (groundCheckTimer <= 0f)
        {
            isGrounded = Physics.CheckSphere(
              transform.position + Vector3.down * (raycastDistance - 0.1f), 0.2f, groundLayer);
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
        Vector3 moveDirection = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;

        // Zkontroluj jestli je p�ed hr��em ze�
        bool hitsWall = Physics.Raycast(transform.position, moveDirection, 0.6f);

        if (hitsWall)
        {
            // Zastav horizont�ln� pohyb ale nech gravitaci
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 horizontalVelocity = moveDirection * MoveSpeed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
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
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }
}