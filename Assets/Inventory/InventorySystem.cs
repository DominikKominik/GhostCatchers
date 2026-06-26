using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InventorySystem : NetworkBehaviour
{
    public static InventorySystem Instance;
    public Item[] slots = new Item[9];
    public int activeSlot = 0;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        HandleScrolling();
        HandleNumpad();
    }

    void HandleScrolling()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
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
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) ||
                Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                activeSlot = i;
            }
        }
    }

    public bool AddItem(Item item)
    {
        // Nejdøív zkus vložit do aktivního slotu
        if (slots[activeSlot] == null)
        {
            slots[activeSlot] = item;
            return true;
        }

        // Pokud je aktivní slot plný, najdi první volný
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