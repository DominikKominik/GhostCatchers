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
        closedRotation = transform.rotation;
    }

    void Update()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        if (!isMoving && Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                if (hit.collider.transform.IsChildOf(transform) || hit.collider.gameObject == gameObject)
                {
                    if (!isOpen)
                    {
                        Vector3 directionToPlayer = (cam.transform.position - transform.position).normalized;
                        float side = Vector3.Dot(transform.right, directionToPlayer);

                        float angle = side >= 0 ? openAngle : -openAngle;
                        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, angle, 0));
                    }

                    isOpen = !isOpen;
                    isMoving = true;
                }
            }
        }

        if (isMoving)
        {
            Quaternion target = isOpen ? openRotation : closedRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * speed);

            if (Quaternion.Angle(transform.rotation, target) < 0.1f)
            {
                transform.rotation = target;
                isMoving = false;
            }
        }
    }
}