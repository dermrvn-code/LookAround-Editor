using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public enum DragItemType
{
    Text,
    Arrow,
    Textbox
}
public class UIDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private UIDropZone dropLayer;
    private GameObject dragClone;

    public DragItemType itemType;


    public void OnBeginDrag(PointerEventData eventData)
    {
        dropLayer.gameObject.SetActive(true);
        dragClone = Instantiate(gameObject, canvas.transform);
        dragClone.transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragClone == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos))
        {
            dragClone.GetComponent<RectTransform>().localPosition = pos;
        }

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool hoveringDropZone = false;
        foreach (var result in results)
        {
            var dropZone = result.gameObject.GetComponent<UIDropZone>();
            if (dropZone != null)
            {
                dropZone.OnHoverEnter();
                hoveringDropZone = true;
            }
        }

        if (!hoveringDropZone)
        {
            foreach (var dropZone in FindObjectsOfType<UIDropZone>())
                dropZone.OnHoverExit();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragClone == null) return;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var dropZone = result.gameObject.GetComponent<UIDropZone>();
            if (dropZone != null)
            {
                var dragItem = dragClone.GetComponent<UIDragItem>();
                dropZone.OnItemDropped(dragItem);
            }
        }

        Destroy(dragClone);
        dragClone = null;
        dropLayer.gameObject.SetActive(false);
    }
}
