using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform inventoryBar;
    private Image[] slotBackgrounds = new Image[9];
    private Image[] slotImages = new Image[9];
    private bool slotsCreated = false;

    public Color normalColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    public Color activeColor = new Color(0.9f, 0.7f, 0.1f, 1f);

    void Start()
    {
        if (InventorySystem.Instance == null || !InventorySystem.Instance.IsOwner)
        {
            StartCoroutine(WaitForLocalPlayer());
            return;
        }
        CreateSlots();
    }

    IEnumerator WaitForLocalPlayer()
    {
        yield return new WaitUntil(() =>
            InventorySystem.Instance != null && InventorySystem.Instance.IsOwner);
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
        if (InventorySystem.Instance == null) return;

        for (int i = 0; i < 9; i++)
        {
            slotBackgrounds[i].color = (i == InventorySystem.Instance.activeSlot)
                ? activeColor
                : normalColor;

            Item item = InventorySystem.Instance.slots[i];
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