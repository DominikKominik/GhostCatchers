using UnityEngine;

public class Door : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 3f;
    public float interactRange = 2.5f;

    private bool isOpen = false;
    private bool isMoving = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        // Otevøení/zavøení
        if (Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isOpen = !isOpen;
                }
            }
        }

        // Animace
        Quaternion target = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * speed);
    }
}
