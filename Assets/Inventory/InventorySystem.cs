using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem; // DÙLEŽITÉ: Pøidáno pro funkènost nového Input Systemu

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
        // NOVÝ INPUT SYSTEM: Naètení hodnoty scrollování myši
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

        // Pole kláves 1-9 nad písmeny
        Key[] alphaKeys = new Key[] {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
        };

        // Pole kláves 1-9 na numerické klávesnici
        Key[] keypadKeys = new Key[] {
            Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5, Key.Numpad6, Key.Numpad7, Key.Numpad8, Key.Numpad9
        };

        // Kontrola stisknutí pro všech 9 slotù
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