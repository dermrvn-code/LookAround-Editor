using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ColorPicker : MonoBehaviour
{
    public TMP_Text label;
    public FlexibleColorPicker colorPicker;

    public string labelText;
    public Color defaultColor = Color.green;

    public UnityEvent<Color> OnColorChange = new UnityEvent<Color>();
    public Color value;

    void Awake()
    {
        label.text = labelText;
        colorPicker.SetColor(defaultColor);
        colorPicker.onColorChange.AddListener((color) => OnColorChange?.Invoke(color));
    }

    public void Initialize(Color color, string label = "")
    {
        if (label != "")
        {
            labelText = label;
            this.label.text = label;
        }
        colorPicker.SetColor(color);
        value = color;
    }
}
