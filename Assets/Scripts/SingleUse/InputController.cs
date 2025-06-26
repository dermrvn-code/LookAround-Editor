using UnityEngine;

public class InputController : MonoBehaviour
{
    public CameraHandler cam;
    public InteractionHandler interaction;
    public SceneChanger sc;

    void Start()
    {
        sc = FindObjectOfType<SceneChanger>();

        if (cam == null)
            Debug.LogError("No camera was given in the Hardware Emulator");

    }

    void Update()
    {
        // Only trigger input if not over a UI element
        if (!UnityEngine.EventSystems.EventSystem.current || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                cam.LeftMove();
            else if (Input.GetKey(KeyCode.RightArrow))
                cam.RightMove();

            if (Input.GetKey(KeyCode.DownArrow))
                cam.ZoomIn();
            else if (Input.GetKey(KeyCode.UpArrow))
                cam.ZoomOut();

            if (Input.GetKeyDown(KeyCode.Space))
                interaction.Interact();

            if (Input.GetKeyDown(KeyCode.H))
                sc.ToStartScene();
        }
    }
}
