using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderAndInput : MonoBehaviour
{
    public int minValue = 0;
    public int maxValue = 100;
    public int defaultValue = 50;
    public TMP_InputField inputField;
    public Slider slider;
    public TMP_Text label;
    public string labelText;
    public UnityEvent<int> OnValueChanged;
    public int value;

    void Awake()
    {
        label.text = labelText;
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = defaultValue;
        UpdateInputFieldValue((int)slider.value);
        slider.onValueChanged.AddListener(delegate { UpdateInputFieldValue((int)slider.value); });
        inputField.onValueChanged.AddListener(delegate { UpdateSliderValue(inputField.text); });
        inputField.onEndEdit.AddListener(delegate { UpdateInputValueOnEndEdit(); });
    }

    void UpdateSliderValue(string value)
    {
        int parsedValue = 0;
        int.TryParse(value, out parsedValue);
        if (parsedValue < minValue)
        {
            parsedValue = minValue;
        }
        else if (parsedValue > maxValue)
        {
            parsedValue = maxValue;
        }
        this.value = parsedValue;
        slider.value = parsedValue;
        OnValueChanged?.Invoke(parsedValue);
    }

    void UpdateInputValueOnEndEdit()
    {
        inputField.text = slider.value.ToString();
    }

    void UpdateInputFieldValue(int value)
    {
        inputField.text = value.ToString();
        OnValueChanged?.Invoke(value);
        this.value = value;
    }

    public void Initialize(int value, string label = "", int minValue = 0, int maxValue = 100)
    {
        if (minValue != 0)
        {
            this.minValue = minValue;
            slider.minValue = minValue;
        }
        if (maxValue != 100)
        {
            this.maxValue = maxValue;
            slider.maxValue = maxValue;
        }

        if (!string.IsNullOrEmpty(label))
        {
            labelText = label;
            this.label.text = labelText;
        }

        UpdateSliderValue(value.ToString());
        UpdateInputFieldValue(value);
        OnValueChanged?.Invoke(value);
        this.value = value;
    }


}
