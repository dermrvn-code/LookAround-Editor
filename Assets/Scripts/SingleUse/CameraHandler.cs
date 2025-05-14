using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{

    public float rotation = 0f;

    public int zoom = 0;
    private int minZoom = 100;
    private int maxZoom = 25;
    public float rotationSpeed = 2f;
    public float zoomSpeed = 2f;

    private Camera cam;


    void Awake()
    {
        cam = GetComponent<Camera>();
        currentZoom = cam.fieldOfView;
    }

    void Update()
    {
        UpdateRotation();
        UpdateZoom();
    }

    float currentRotation;
    float rotationVelocity = 0.0f;
    void UpdateRotation()
    {
        currentRotation = Mathf.SmoothDampAngle(currentRotation, rotation, ref rotationVelocity, rotationSpeed * Time.deltaTime) % 360;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, currentRotation, transform.localEulerAngles.z);
    }

    public void SetRotation(float rot)
    {
        rot = (360 + rot) % 360;
        rotation = rot;
    }


    float currentZoom;
    float zoomVelocity = 0.0f;
    void UpdateZoom()
    {
        if (zoom > 100) zoom = 100;
        if (zoom < 0) zoom = 0;

        int targetZoom = (int)map(zoom, 0, 100, minZoom, maxZoom);
        currentZoom = (int)Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSpeed * Time.deltaTime);

        SetZoom(currentZoom);
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public void ZoomIn()
    {
        if (zoom <= 0) return;
        zoom = zoom - 1;
        // SetZoom(zoom);
    }

    public void ZoomOut()
    {
        if (zoom >= 100) return;
        zoom = zoom + 1;
        // SetZoom(zoom);
    }

    public void LeftMove()
    {
        SetRotation(rotation - 1);
    }

    public void RightMove()
    {
        SetRotation(rotation + 1);
    }

    public void SetZoom(float newZoom)
    {
        cam.fieldOfView = newZoom;
    }
}
