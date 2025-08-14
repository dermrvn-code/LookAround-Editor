using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : ViewHandler
{
    private Camera cam;


    void Awake()
    {
        cam = GetComponent<Camera>();
        SetZoom(zoom);
    }


    public override void UpdateRotation()
    {
        currentRotation = Mathf.SmoothDampAngle(currentRotation, rotation, ref rotationVelocity, rotationSpeed * Time.deltaTime) % 360;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, currentRotation, transform.localEulerAngles.z);
    }

    public override void SetRotation(float rot)
    {
        rot = (360 + rot) % 360;
        rotation = rot;
    }


    public override void UpdateZoom()
    {
        if (zoom > 100) zoom = 100;
        if (zoom < 0) zoom = 0;

        int targetZoom = (int)Map(zoom, 0, 100, minZoom, maxZoom);
        currentZoom = (int)Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSpeed * Time.deltaTime);

        SetZoom(currentZoom);
    }


    public override void SetZoom(float newZoom)
    {
        cam.fieldOfView = newZoom;
    }
}
