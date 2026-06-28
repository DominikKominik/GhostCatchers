using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class ItemPickup : NetworkBehaviour
{
    public Item item;
    public float pickupRange = 2.5f;
    private Camera cam;
    private Outline outline;
    private InventorySystem inventorySystem;

    public override void OnNetworkSpawn()
    {
        // Nic tady
    }

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

        if (inventorySystem == null)
        {
            if (NetworkManager.Singleton?.LocalClient?.PlayerObject != null)
            {
                inventorySystem = NetworkManager.Singleton.LocalClient.PlayerObject
                    .GetComponent<InventorySystem>();
            }
            return;
        }

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            bool isLookingAtThis = hit.collider.gameObject == gameObject;
            outline.enabled = isLookingAtThis;

            if (isLookingAtThis && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                bool added = inventorySystem.AddItem(item);
                if (added)
                {
                    HideItemRpc();
                }
            }
        }
        else
        {
            outline.enabled = false;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void HideItemRpc()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned && !netObj.InScenePlaced)
        {
            netObj.Despawn(true);
        }
        else
        {
            HideStaticItemClientRpc();
        }
    }

    [ClientRpc]
    void HideStaticItemClientRpc()
    {
        gameObject.SetActive(false);
    }
}