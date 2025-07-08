using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class SpriteSelector : MonoBehaviour
{

    public TMP_Text label;
    public GameObject elements;
    public GameObject check;
    public GameObject item;
    public string labelText;

    public Pairs.SpritePair[] elementsList;

    private GameObject currentCheck;

    public UnityEvent<string> OnElementSelected = new UnityEvent<string>();
    public string value;


    public void Initialize(Pairs.SpritePair[] _elementsList, string selectedValue, string label = "")
    {
        if (label != "")
        {
            labelText = label;
            this.label.text = label;
        }
        elementsList = _elementsList;

        if (elementsList == null || elementsList.Length == 0)
        {
            Debug.LogWarning("No elements provided for SpriteSelector.");
            return;
        }

        for (int i = 0; i < elementsList.Length; i++)
        {
            if (elementsList[i] == null)
            {
                continue;
            }
            string value = elementsList[i].value;
            GameObject newItem = Instantiate(item, elements.transform);
            newItem.name = value;
            Image image = newItem.GetComponent<Image>();
            image.sprite = elementsList[i].sprite;

            Button button = newItem.GetComponent<Button>();
            button.onClick.AddListener(() => SelectElement(value, newItem));

            if (value == selectedValue)
            {
                SelectElement(value, newItem);
            }
        }
        OnElementSelected?.AddListener((value) =>
        {
            this.value = value;
        });
        value = selectedValue;
    }

    void SelectElement(string value, GameObject selectedItem)
    {
        if (currentCheck != null)
        {
            Destroy(currentCheck);
        }
        currentCheck = Instantiate(check, selectedItem.transform);

        OnElementSelected?.Invoke(value);

    }
}
