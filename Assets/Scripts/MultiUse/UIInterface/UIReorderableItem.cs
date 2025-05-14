using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIReorderableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private int originalIndex;
    private GameObject placeholder;
    private RectTransform rectTransform;
    private Canvas canvas;

    public Transform container; // Set to the parent panel that holds the items

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalIndex = transform.GetSiblingIndex();

        // Create a placeholder
        placeholder = new GameObject("Placeholder");
        var layout = placeholder.AddComponent<LayoutElement>();
        var thisLayout = GetComponent<LayoutElement>();
        layout.preferredWidth = thisLayout.preferredWidth;
        layout.preferredHeight = thisLayout.preferredHeight;
        layout.flexibleWidth = 0;
        layout.flexibleHeight = 0;

        placeholder.transform.SetParent(originalParent);
        placeholder.transform.SetSiblingIndex(originalIndex);

        // Move the item to the top layer
        transform.SetParent(container.root); // make it global in canvas
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );
        rectTransform.localPosition = pos;

        // Reposition placeholder within container based on pointer position
        for (int i = 0; i < container.childCount; i++)
        {
            Transform child = container.GetChild(i);
            if (child == placeholder.transform) continue;

            if (rectTransform.position.y > child.position.y)
            {
                int newIndex = i;
                if (placeholder.transform.GetSiblingIndex() != newIndex)
                {
                    placeholder.transform.SetSiblingIndex(newIndex);
                }
                break;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Reparent the item to the container and place at new index
        transform.SetParent(container);
        transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());

        // Destroy placeholder
        Destroy(placeholder);
    }
}
