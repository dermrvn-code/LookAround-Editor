using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DropdownInput : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public TMP_Text label;
    public List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

    public string labelText;
    public UnityEvent<string> OnValueChanged = new UnityEvent<string>();

    public string value;

    void Start()
    {
        if (labelText != "")
        {
            label.text = labelText;
        }

        dropdown.onValueChanged.AddListener((value) => this.value = dropdown.options[value].text);
        dropdown.onValueChanged.AddListener((value) => OnValueChanged?.Invoke(dropdown.options[value].text));
        dropdown.options.Clear();
        dropdown.AddOptions(options);

    }

    public void Initialize(List<string> optionTexts, string selectedValue, string label = "")
    {
        if (label != "")
        {
            labelText = label;
            this.label.text = label;
        }

        options.Clear();
        foreach (var text in optionTexts)
        {
            options.Add(new TMP_Dropdown.OptionData(text));
        }

        dropdown.options.Clear();
        dropdown.AddOptions(options);


        int index = options.FindIndex(o => o.text == selectedValue);
        if (index >= 0)
        {
            dropdown.value = index;
        }

        value = selectedValue;
    }


}
