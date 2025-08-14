using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : ProgressBarBase
{
    public GameObject bar;

    Image background;
    void Start()
    {
        background = GetComponent<Image>();
    }

    public void SetActive(bool isActive)
    {
        background.enabled = isActive;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(isActive);
        }
    }

    public override void Update()
    {
        base.Update();

        if (!show) return;

        RectTransform rt = bar.GetComponent<RectTransform>();
        rt.anchorMax = new Vector2((float)currentStep / totalSteps, rt.anchorMax.y);

        if (currentStep + 1 >= totalSteps)
        {
            onFull?.Invoke();
        }
    }
}
