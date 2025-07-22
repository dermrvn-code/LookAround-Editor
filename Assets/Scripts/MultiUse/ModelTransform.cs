using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ModelTransform : MonoBehaviour
{

    [SerializeField]
    GameObject elementContainer;

    private Transform elementTransform;

    public Vector3 rotation;
    public float scale = 10;


    void Start()
    {
        if (elementContainer == null)
        {
            elementContainer = gameObject;
        }
        elementTransform = elementContainer.GetComponent<Transform>();
    }


    void Update()
    {
        elementTransform.localRotation = Quaternion.Euler(rotation);
        elementTransform.localScale = Vector3.one * (float)(scale / 10f);
    }

}
