using UnityEngine;
using UnityEngine.UI;

public class ArrowBetweenUIElements : MonoBehaviour
{

    public Image arrowHeadImage; // Drag an arrowhead sprite here
    public float arrowHeadSize = 20f;
    public float arrowHeadOffsetRotation = 90f; // Rotation offset for the arrowhead


    public RectTransform from;
    public RectTransform to;
    public Image arrowImage; // A thin Image with pivot at (0, 0.5)
    public float lineWidth = 4f;

    private RectTransform arrowRect;

    void Start()
    {
        arrowRect = arrowImage.GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (from == null || to == null) return;

        Vector3 fromPointTemp = GetClosestEdgePoint(from, to.position);
        Vector3 toPoint = GetClosestEdgePoint(to, fromPointTemp);
        Vector3 fromPoint = GetClosestEdgePoint(from, toPoint);

        Vector3 direction = toPoint - fromPoint;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        arrowRect.sizeDelta = new Vector2(distance, lineWidth);
        arrowRect.position = fromPoint;
        arrowRect.rotation = Quaternion.Euler(0, 0, angle);

        // Place arrowhead at end
        if (arrowHeadImage != null)
        {
            arrowHeadImage.rectTransform.position = toPoint - direction.normalized * (arrowHeadSize / 2);
            arrowHeadImage.rectTransform.rotation = Quaternion.Euler(0, 0, angle + arrowHeadOffsetRotation);
            arrowHeadImage.rectTransform.sizeDelta = new Vector2(arrowHeadSize, arrowHeadSize);
        }

    }

    Vector3 GetClosestEdgePoint(RectTransform from, Vector3 toPosition)
    {
        Vector3[] corners = new Vector3[4];
        from.GetWorldCorners(corners);

        // Define edge midpoints
        Vector3[] edgeMids = new Vector3[4];
        edgeMids[0] = (corners[0] + corners[1]) * 0.5f; // Left
        edgeMids[1] = (corners[1] + corners[2]) * 0.5f; // Top
        edgeMids[2] = (corners[2] + corners[3]) * 0.5f; // Right
        edgeMids[3] = (corners[3] + corners[0]) * 0.5f; // Bottom

        Vector3 closestPoint = edgeMids[0];
        float closestDist = Vector3.Distance(toPosition, edgeMids[0]);

        for (int i = 1; i < 4; i++)
        {
            float dist = Vector3.Distance(toPosition, edgeMids[i]);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPoint = edgeMids[i];
            }
        }

        return closestPoint;
    }
}
