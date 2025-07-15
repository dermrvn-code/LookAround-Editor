using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbnailMaker : MonoBehaviour
{
    [SerializeField]
    Transform modelAnchor;

    [SerializeField]
    Camera thumbnailCamera;


    public Vector2 thumbnailSize = new Vector2(256, 256);

    void Start()
    {
        DeactivateStudio();
    }

    void ActivateStudio()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    void DeactivateStudio()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public RenderTexture CreateThumbnail(GameObject model)
    {
        ActivateStudio();
        PrepareModel(model);

        RenderTexture rt = new RenderTexture((int)thumbnailSize.x, (int)thumbnailSize.y, 16);

        thumbnailCamera.targetTexture = rt;
        thumbnailCamera.Render();
        thumbnailCamera.targetTexture = null;

        ResetModel(model);
        DeactivateStudio();
        return rt;
    }

    Transform oldParent;
    Vector3 oldPosition;
    Quaternion oldRotation;
    Vector3 oldScale;
    void PrepareModel(GameObject model)
    {
        oldParent = model.transform.parent;
        oldPosition = model.transform.localPosition;
        oldRotation = model.transform.localRotation;
        oldScale = model.transform.localScale;

        Renderer modelRenderer = model.GetComponent<Renderer>();
        if (modelRenderer == null)
        {
            modelRenderer = model.GetComponentInChildren<Renderer>();
        }

        if (modelRenderer == null)
        {
            Debug.LogError("Model does not have a Renderer component.");
            return;
        }

        model.transform.SetParent(modelAnchor);
        model.transform.localRotation = Quaternion.identity;
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;

        Bounds modelBounds = modelRenderer.bounds;

        float distance = (thumbnailCamera.transform.position - modelBounds.center).magnitude;
        float maxExtent = Mathf.Max(modelBounds.extents.x, modelBounds.extents.y, modelBounds.extents.z);

        float fov = thumbnailCamera.fieldOfView * Mathf.Deg2Rad;
        float fitScale = distance * Mathf.Tan(fov / 2) / maxExtent;

        model.transform.localScale = Vector3.one * fitScale * 0.65f;

        Vector3 offset = thumbnailCamera.transform.forward * distance;
        model.transform.position = thumbnailCamera.transform.position + offset;
        model.transform.position -= modelRenderer.bounds.center - model.transform.position;
        model.SetActive(true);
    }

    void ResetModel(GameObject model)
    {
        if (oldParent != null)
        {
            model.transform.SetParent(oldParent);
        }
        else
        {
            Debug.LogError("Model reset failed: Old parent data is missing.");
        }

        if (oldPosition != null)
        {
            model.transform.localPosition = oldPosition;
        }
        else
        {
            Debug.LogError("Model reset failed: Old position data is missing.");
        }

        if (oldRotation != null)
        {
            model.transform.localRotation = oldRotation;
        }
        else
        {
            Debug.LogError("Model reset failed: Old rotation data is missing.");
        }

        if (oldScale != null)
        {
            model.transform.localScale = oldScale;
        }
        else
        {
            Debug.LogError("Model reset failed: Old scale data is missing.");
        }

        model.SetActive(false);
    }
}
