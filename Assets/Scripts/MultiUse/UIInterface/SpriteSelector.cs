using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[System.Serializable]
public class Element
{
    public string value;
    public Sprite sprite;
}

public class SpriteSelector : MonoBehaviour
{

    public TMP_Text label;
    public GameObject elements;
    public GameObject check;
    public GameObject item;
    public string labelText;

    public Element[] elementsList;

    private GameObject currentCheck;
    private Vector3 checkPosition;

    public UnityEvent<string> OnElementSelected = new UnityEvent<string>();

    void Start()
    {
        checkPosition = check.transform.localPosition;
        label.text = labelText;
        for (int i = 0; i < elementsList.Length; i++)
        {
            string value = elementsList[i].value;
            GameObject newItem = Instantiate(item, elements.transform);
            newItem.name = value;
            Image image = newItem.GetComponent<Image>();
            image.sprite = elementsList[i].sprite;

            Button button = newItem.GetComponent<Button>();
            button.onClick.AddListener(() => SelectElement(value, newItem));
        }
    }

    void SelectElement(string value, GameObject selectedItem)
    {
        if (currentCheck != null)
        {
            Destroy(currentCheck);
        }
        currentCheck = Instantiate(check, selectedItem.transform);
        Debug.Log("Selected Element ID: " + value);

        OnElementSelected?.Invoke(value);

    }
}
