using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleInput : MonoBehaviour
{
    public Toggle toggle;
    public TMP_Text label;

    public string labelText;
    public UnityEvent<bool> OnValueChanged = new UnityEvent<bool>();
    public bool value;

    void Start()
    {
        label.text = labelText;
        toggle.onValueChanged.AddListener((value) => this.value = value);
        toggle.onValueChanged.AddListener((value) => OnValueChanged?.Invoke(value));
    }


    public void Initialize(bool initialValue, string label = "")
    {
        if (label != "")
        {
            labelText = label;
            this.label.text = label;
        }

        toggle.isOn = initialValue;
        value = initialValue;
    }


}
