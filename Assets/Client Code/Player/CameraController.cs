using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private LocalPlayerController player;

    [SerializeField]
    private float mouseSensitivity;

    [SerializeField]
    private float yRotationClamp = 90f;

    private const string MOUSE_X = "Mouse X";
    private const string MOUSE_Y = "Mouse Y";

    private Vector3 playerRotation;
    private Vector3 cameraRotation;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor();
     
        RotateCamera();
    }

    bool lockCursor;

    void LockCursor()
    {
        lockCursor = !lockCursor;

        if (lockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            return;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void RotateCamera()
    {
        if (!lockCursor) return;

        float x = Input.GetAxisRaw(MOUSE_X) * mouseSensitivity;
        float y = Input.GetAxisRaw(MOUSE_Y) * mouseSensitivity;

        playerRotation.y += x;

        cameraRotation.x -= y;
        cameraRotation.y += x;

        player.transform.eulerAngles = playerRotation;
        transform.eulerAngles = cameraRotation;
    }
}
