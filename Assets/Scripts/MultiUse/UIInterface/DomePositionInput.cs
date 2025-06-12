using UnityEngine;
using UnityEngine.Events;

public class DomePositionInput : MonoBehaviour
{
    public SliderAndInput xPos;
    public SliderAndInput yPos;
    public SliderAndInput tilt;
    public SliderAndInput distance;

    public UnityEvent<float, float, float, float> OnInputChanged = new UnityEvent<float, float, float, float>();


    void SetupListener()
    {
        xPos.OnValueChanged.AddListener((value) =>
        {
            OnInputChanged.Invoke(
                value,
                yPos.value,
                distance.value,
                tilt.value
            );
        });

        yPos.OnValueChanged.AddListener((value) =>
        {
            OnInputChanged.Invoke(
                xPos.value,
                value,
                distance.value,
                tilt.value
            );
        });

        distance.OnValueChanged.AddListener((value) =>
        {
            OnInputChanged.Invoke(
                xPos.value,
                yPos.value,
                value,
                tilt.value
            );
        });
        tilt.OnValueChanged.AddListener((value) =>
        {
            OnInputChanged.Invoke(
                xPos.value,
                yPos.value,
                distance.value,
                value
            );
        });
    }

    public void Initialize(float x, float y, float dist, float tiltVal)
    {
        xPos.Initialize(x);
        yPos.Initialize(y);
        distance.Initialize(dist);
        tilt.Initialize(tiltVal);

        SetupListener();
    }
}
