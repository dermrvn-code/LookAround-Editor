using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{

    DomePosition pos;
    MeshRenderer meshRenderer;
    void Start()
    {
        pos = GetComponent<DomePosition>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public GameObject target;

    private GameObject lastTarget;
    private DomePosition targetPosition;
    private Renderer targetRenderer;

    void Update()
    {
        // Only update components if the target has changed
        if (target != lastTarget)
        {
            lastTarget = target;
            targetPosition = null;
            targetRenderer = null;

            if (target != null)
            {
                targetPosition = target.GetComponent<DomePosition>();

                var mr = target.GetComponentInChildren<MeshRenderer>();
                if (mr != null)
                {
                    targetRenderer = mr;
                }
                else
                {
                    var r = target.GetComponentInChildren<Renderer>();
                    if (r != null)
                    {
                        targetRenderer = r;
                    }
                }
            }
        }

        if (target != null && targetRenderer != null && targetPosition != null)
        {
            meshRenderer.enabled = true;
            Debug.Log("Selector active for " + target.name);

            // Use the mesh's local bounds size and scale it by the target's localScale to match the target's size
            MeshFilter meshFilter = targetRenderer.GetComponent<MeshFilter>();
            Vector3 size = Vector3.one;
            if (meshFilter != null)
            {
                size = Vector3.Scale(meshFilter.sharedMesh.bounds.size, targetRenderer.transform.localScale);
            }
            else
            {
                size = Vector3.Scale(targetRenderer.bounds.size, Vector3.one);
            }

            Vector3 selectorScale = transform.localScale;
            selectorScale.x = size.x * 1.2f;
            selectorScale.y = size.x * 1.2f;

            transform.localScale = selectorScale;
            pos.position = targetPosition.position;
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }
}
