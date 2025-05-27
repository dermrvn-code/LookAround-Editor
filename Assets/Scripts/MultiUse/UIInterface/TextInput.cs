using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextInput : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text label;

    public string labelText;
    public UnityEvent<string> OnValueChanged = new UnityEvent<string>();

    void Start()
    {
        label.text = labelText;
        inputField.onValueChanged.AddListener((value) => OnValueChanged?.Invoke(value));
    }


}
