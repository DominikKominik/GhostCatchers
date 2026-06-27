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
                HideItemRpc(); // Zmìnìno na nový název metody
            }
        }
        else
        {
            outline.enabled = false;
        }
    }

    // OPRAVA PRO UNITY 6: Použití nového Rpc atributu místo ServerRpc
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void HideItemRpc() // Pøejmenováno, aby název konèil pouze "Rpc"
    {
        HideItemClientRpc();
    }

    // Tady mùže zùstat ClientRpc atribut, ale pro jednotnost v Unity 6 
    // se doporuèuje používat [Rpc(SendTo.Everyone)] - pro teï ale staèí takto:
    [ClientRpc]
    void HideItemClientRpc()
    {
        gameObject.SetActive(false);
    }
}