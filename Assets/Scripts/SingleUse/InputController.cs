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
            Debug.LogError("No eyes were given in the Hardware Emulator");

    }

    void Update()
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
