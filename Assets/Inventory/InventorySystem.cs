using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class InventorySystem : NetworkBehaviour
{
    public static InventorySystem Instance;
    public Item[] slots = new Item[9];
    public int activeSlot = 0;

    private NetworkVariable<Unity.Collections.FixedString64Bytes> heldItemName =
        new NetworkVariable<Unity.Collections.FixedString64Bytes>(
            "",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    public string HeldItemName => heldItemName.Value.ToString();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public override void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        base.OnDestroy();
    }

    void Update()
    {
        if (!IsOwner) return;
        HandleScrolling();
        HandleNumpad();
        UpdateHeldItemName();
    }

    void UpdateHeldItemName()
    {
        Item activeItem = slots[activeSlot];
        string name = activeItem != null ? activeItem.itemName : "";
        if (heldItemName.Value.ToString() != name)
        {
            heldItemName.Value = name;
        }
    }

    void HandleScrolling()
    {
        if (Mouse.current == null) return;
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0f)
        {
            activeSlot--;
            if (activeSlot < 0) activeSlot = 8;
        }
        else if (scroll < 0f)
        {
            activeSlot++;
            if (activeSlot > 8) activeSlot = 0;
        }
    }

    void HandleNumpad()
    {
        if (Keyboard.current == null) return;
        Key[] alphaKeys = new Key[] {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
            Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
        };
        Key[] keypadKeys = new Key[] {
            Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4,
            Key.Numpad5, Key.Numpad6, Key.Numpad7, Key.Numpad8, Key.Numpad9
        };
        for (int i = 0; i < 9; i++)
        {
            if (Keyboard.current[alphaKeys[i]].wasPressedThisFrame ||
                Keyboard.current[keypadKeys[i]].wasPressedThisFrame)
            {
                activeSlot = i;
            }
        }
    }

    public bool AddItem(Item item)
    {
        if (slots[activeSlot] == null)
        {
            slots[activeSlot] = item;
            return true;
        }
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                return true;
            }
        }
        Debug.Log("Inventáø je plný!");
        return false;
    }

    public void RemoveItem(int slotIndex)
    {
        slots[slotIndex] = null;
    }
}