using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Image = UnityEngine.UI.Image;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform inventoryBar;
    private Image[] slotBackgrounds = new Image[9];
    private Image[] slotImages = new Image[9];
    private bool slotsCreated = false;
    private InventorySystem localInventory;

    public Color normalColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    public Color activeColor = new Color(0.9f, 0.7f, 0.1f, 1f);

    void Start()
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    IEnumerator WaitForLocalPlayer()
    {
        // Poèkej až se spawne lokální hráè
        yield return new WaitUntil(() =>
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null);

        localInventory = NetworkManager.Singleton.LocalClient.PlayerObject
            .GetComponent<InventorySystem>();

        CreateSlots();
    }

    void CreateSlots()
    {
        foreach (Transform child in inventoryBar)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 9; i++)
        {
            GameObject slot = Instantiate(slotPrefab, inventoryBar);
            slotBackgrounds[i] = slot.GetComponent<Image>();
            slotImages[i] = slot.transform.Find("Icon").GetComponent<Image>();
        }

        slotsCreated = true;
    }

    void Update()
    {
        if (!slotsCreated) return;
        if (localInventory == null) return;

        for (int i = 0; i < 9; i++)
        {
            slotBackgrounds[i].color = (i == localInventory.activeSlot)
                ? activeColor
                : normalColor;

            Item item = localInventory.slots[i];
            if (item != null && item.icon != null)
            {
                slotImages[i].sprite = item.icon;
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].color = new Color(0, 0, 0, 0);
            }
        }
    }
}