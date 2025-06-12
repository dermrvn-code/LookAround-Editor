using UnityEngine;

public class UIDropZone : MonoBehaviour
{
    [SerializeField] private GameObject hint;

    Camera cam;
    ElementInitManager elementInitManager;
    SceneChanger sceneChanger;
    InteractionHandler interactionHandler;
    private void Start()
    {
        SetHintActive(false);
        cam = FindObjectOfType<Camera>();
        elementInitManager = FindObjectOfType<ElementInitManager>();
        sceneChanger = FindObjectOfType<SceneChanger>();
        interactionHandler = FindObjectOfType<InteractionHandler>();
    }

    public void OnHoverEnter()
    {
        SetHintActive(true);
    }

    public void OnHoverExit()
    {
        SetHintActive(false);
    }

    public void OnItemDropped(UIDragItem droppedItem)
    {
        HandleItemDropped(droppedItem);
        SetHintActive(false);
    }

    private void SetHintActive(bool isActive)
    {
        if (hint != null)
            hint.SetActive(isActive);
    }

    private void HandleItemDropped(UIDragItem droppedItem)
    {
        if (sceneChanger.currentScene == null)
        {
            InfoText.ShowInfo("Elemente können nur in validen Szenen platziert werden.");
            return;
        }

        float pos = cam.transform.eulerAngles.y;
        switch (droppedItem.itemType)
        {
            case DragItemType.Text:
                elementInitManager.InitText(pos);
                break;
            case DragItemType.Arrow:
                elementInitManager.InitArrow(pos);
                break;
            case DragItemType.Textbox:
                elementInitManager.InitTextbox(pos);
                break;
            default:
                Debug.LogWarning("Unknown item type dropped");
                break;
        }

        interactionHandler.UpdateElements();

    }
}
