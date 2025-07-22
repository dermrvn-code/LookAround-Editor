using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ModelTransformInput : MonoBehaviour
{

    [SerializeField]
    SliderAndInput rotationX;

    [SerializeField]
    SliderAndInput rotationY;

    [SerializeField]
    SliderAndInput rotationZ;

    [SerializeField]
    SliderAndInput scale;

    public UnityEvent<int, int, int, int> OnInputChanged = new UnityEvent<int, int, int, int>();


    void SetupListener()
    {
        rotationX.OnValueChanged.AddListener((value) =>
        {
            OnInputChanged.Invoke(
                value,
                rotationY.value,
                rotationZ.value,
                scale.value
            );
        });
        rotationY.OnValueChanged.AddListener((value) =>
        {
            OnInputChanged.Invoke(
                rotationX.value,
                value,
                rotationZ.value,
                scale.value
            );
        });
        rotationZ.OnValueChanged.AddListener((value) =>
        {
            OnInputChanged.Invoke(
                rotationX.value,
                rotationY.value,
                value,
                scale.value
            );
        });
        scale.OnValueChanged.AddListener((value) =>
        {
            OnInputChanged.Invoke(
                rotationX.value,
                rotationY.value,
                rotationZ.value,
                value
            );
        });

    }

    public void Initialize(int x, int y, int z, int scale)
    {
        rotationX.Initialize(x);
        rotationY.Initialize(y);
        rotationZ.Initialize(z);
        this.scale.Initialize(scale);

        SetupListener();
    }
}
