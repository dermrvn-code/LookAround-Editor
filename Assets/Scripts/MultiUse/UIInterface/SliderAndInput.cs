using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderAndInput : MonoBehaviour
{
    public float minValue = 0;
    public float maxValue = 100;
    public float defaultValue = 50;
    public TMP_InputField inputField;
    public Slider slider;
    public TMP_Text label;
    public string labelText;
    public UnityEvent<float> OnValueChanged;
    void Start()
    {
        label.text = labelText;
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = defaultValue;
        UpdateInputFieldValue(slider.value);
        slider.onValueChanged.AddListener(delegate { UpdateInputFieldValue(slider.value); });
        inputField.onValueChanged.AddListener(delegate { UpdateSliderValue(inputField.text); });
        inputField.onEndEdit.AddListener(delegate { UpdateSliderValueOnEndEdit(); });
    }

    void UpdateSliderValue(string value)
    {
        float parsedValue = float.Parse(value);
        if (parsedValue < slider.minValue)
        {
            parsedValue = slider.minValue;
        }
        else if (parsedValue > slider.maxValue)
        {
            parsedValue = slider.maxValue;
        }
        slider.value = parsedValue;
        OnValueChanged?.Invoke(parsedValue);
    }

    void UpdateSliderValueOnEndEdit()
    {
        inputField.text = slider.value.ToString();
    }

    void UpdateInputFieldValue(float value)
    {
        inputField.text = value.ToString();
        OnValueChanged?.Invoke(value);
    }


}
