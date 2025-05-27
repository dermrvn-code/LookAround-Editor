using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DropdownInput : MonoBehaviour
{
    public Dropdown dropdown;
    public TMP_Text label;
    public List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

    public string labelText;
    public UnityEvent<string> OnValueChanged = new UnityEvent<string>();

    void Start()
    {
        label.text = labelText;
        dropdown.onValueChanged.AddListener((value) => OnValueChanged?.Invoke(dropdown.options[value].text));
        dropdown.options.Clear();
        dropdown.AddOptions(options);
    }


}
