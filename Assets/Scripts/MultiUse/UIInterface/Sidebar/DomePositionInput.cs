using UnityEngine;
using UnityEngine.Events;

public class DomePositionInput : MonoBehaviour
{
    public SliderAndInput xPos;
    public SliderAndInput yPos;
    public SliderAndInput tilt;
    public SliderAndInput distance;

    public UnityEvent<int, int, int, int> OnInputChanged = new UnityEvent<int, int, int, int>();

    public bool tiltEnabled = true;

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
        if (tiltEnabled)
        {
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
    }

    public void Initialize(int x, int y, int dist, int tiltVal, bool tiltEnabled = true)
    {
        this.tiltEnabled = tiltEnabled;

        xPos.Initialize(x);
        yPos.Initialize(y);
        distance.Initialize(dist);

        if (tiltEnabled)
        {
            tilt.Initialize(tiltVal);
        }
        else
        {
            tilt.gameObject.SetActive(false);
        }

        SetupListener();
    }
}
