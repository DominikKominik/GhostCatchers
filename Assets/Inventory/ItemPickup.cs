using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ItemPickup : NetworkBehaviour
{
    public Item item;
    public float pickupRange = 2.5f;
    private Camera cam;
    private Outline outline;

    void Start()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 8f;
        outline.enabled = false;
    }

    void Update()
    {
        if (cam == null)
        {
            cam = Camera.main;
            return;
        }

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            bool isLookingAtThis = hit.collider.gameObject == gameObject;
            outline.enabled = isLookingAtThis;

            if (isLookingAtThis && Input.GetKeyDown(KeyCode.E))
            {
                InventorySystem.Instance.AddItem(item);
                HideItemServerRpc();
            }
        }
        else
        {
            outline.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void HideItemServerRpc()
    {
        HideItemClientRpc();
    }

    [ClientRpc]
    void HideItemClientRpc()
    {
        gameObject.SetActive(false);
    }
}