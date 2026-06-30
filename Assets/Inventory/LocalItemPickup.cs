using UnityEngine;

public class LocalItemPickup : MonoBehaviour
{
    public Item item;
    public float pickupRange = 2.5f;
    private Camera cam;
    private Outline outline;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
            cam = FindAnyObjectByType<Camera>();

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

            if (isLookingAtThis && Input.GetKeyDown(KeyCode.F))
            {
                InventorySystem.Instance.AddItem(item);
                Destroy(gameObject);
            }
        }
        else
        {
            outline.enabled = false;
        }
    }
}
