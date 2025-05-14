using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject bar;
    public TMP_Text loadingText;

    public void UpdateLoader(float progress, string text = "")
    {
        RectTransform rt = bar.GetComponent<RectTransform>();
        rt.anchorMax = new Vector2(progress, rt.anchorMax.y);

        if (!string.IsNullOrEmpty(text))
        {
            loadingText.text = text;
        }
    }
}
