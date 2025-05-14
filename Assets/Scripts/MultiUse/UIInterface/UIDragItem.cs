using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Canvas canvas;
    private GameObject dragClone;

    public void OnBeginDrag(PointerEventData eventData)
    {
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
                dropZone.OnItemDropped(dragClone);
        }

        Destroy(dragClone);
        dragClone = null;
    }
}
