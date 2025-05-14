using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DomePosition : MonoBehaviour
{

    private Transform elementTransform;

    public Vector2 position;
    public int distance = 10;
    public float xRotOffset = 0;


    void Start()
    {
        elementTransform = GetComponent<Transform>();
    }


    void Update()
    {
        if(position.x < 0){
            position.x = 360+position.x;
        }
        if(position.y < 0){
            position.y = 360+position.y;
        }
        Vector3 cartesianPosition = PolarToCartesian(position);
        SetRectPosition(cartesianPosition);
    }

    void SetRectPosition(Vector3 newPosition)
    {
        elementTransform.localPosition = newPosition;
        elementTransform.rotation = Quaternion.Euler(position.y + xRotOffset, position.x, 0);
    }

    public Vector3 PolarToCartesian(Vector2 polar)
    {

        Vector3 origin = new Vector3(0, 0, distance);
        Quaternion rotation = Quaternion.Euler(polar.y, polar.x, 1);
        Vector3 point = rotation * origin;

        return point;
    }

}
