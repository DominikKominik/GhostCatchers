using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ItemHolderScript : NetworkBehaviour
{
    private GameObject currentItemObject;
    private int lastActiveSlot = -1;
    private Item lastActiveItem = null;
    private string lastHeldItemName = "";
    private InventorySystem inventorySystem;

    public List<Item> allItems;

    public override void OnNetworkSpawn()
    {
        inventorySystem = GetComponentInParent<InventorySystem>();
    }

    void Update()
    {
        if (inventorySystem == null) return;

        if (IsOwner)
        {
            int activeSlot = inventorySystem.activeSlot;
            Item activeItem = inventorySystem.slots[activeSlot];

            if (activeSlot != lastActiveSlot || activeItem != lastActiveItem)
            {
                lastActiveSlot = activeSlot;
                lastActiveItem = activeItem;
                UpdateHeldItem(activeItem);
            }

            if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame && activeItem != null)
            {
                Vector3 dropPosition = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
                inventorySystem.RemoveItem(activeSlot);
                DropItemServerRpc(activeSlot, dropPosition, activeItem.itemName);
            }
        }
        else
        {
            string heldName = inventorySystem.HeldItemName;

            if (heldName != lastHeldItemName)
            {
                lastHeldItemName = heldName;
                Item item = allItems.Find(i => i.itemName == heldName);
                UpdateHeldItem(item);
            }
        }
    }

    void UpdateHeldItem(Item item)
    {
        if (currentItemObject != null)
        {
            Destroy(currentItemObject);
            currentItemObject = null;
        }

        if (item != null && item.prefab != null)
        {
            currentItemObject = Instantiate(item.prefab, transform);
            currentItemObject.transform.localPosition = Vector3.zero;
            currentItemObject.transform.localRotation = Quaternion.identity;
            currentItemObject.transform.localScale = Vector3.one * 0.3f;

            Collider col = currentItemObject.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Rigidbody rb = currentItemObject.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            ItemPickup pickup = currentItemObject.GetComponent<ItemPickup>();
            if (pickup != null) pickup.enabled = false;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void DropItemServerRpc(int slotIndex, Vector3 dropPosition, string itemName)
    {
        Item item = allItems.Find(i => i.itemName == itemName);
        if (item == null || item.prefab == null) return;

        GameObject dropped = Instantiate(item.prefab, dropPosition, Quaternion.identity);

        NetworkObject netObj = dropped.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.Spawn();

        UpdateHeldItemClientRpc();
    }

    [ClientRpc]
    void UpdateHeldItemClientRpc()
    {
        UpdateHeldItem(null);
    }
}