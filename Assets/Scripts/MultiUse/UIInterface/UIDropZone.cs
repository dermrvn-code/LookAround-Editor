using UnityEngine;

public class UIDropZone : MonoBehaviour
{
    [SerializeField] private GameObject hint;

    private void Start()
    {
        SetHintActive(false);
    }

    public void OnHoverEnter()
    {
        SetHintActive(true);
    }

    public void OnHoverExit()
    {
        SetHintActive(false);
    }

    public void OnItemDropped(GameObject droppedItem)
    {
        HandleItemDropped(droppedItem);
        SetHintActive(false);
    }

    private void SetHintActive(bool isActive)
    {
        if (hint != null)
            hint.SetActive(isActive);
    }

    private void HandleItemDropped(GameObject droppedItem)
    {
        Debug.Log($"Item dropped on {gameObject.name}: {droppedItem.name}");
        // Add item drop logic here
    }
}
