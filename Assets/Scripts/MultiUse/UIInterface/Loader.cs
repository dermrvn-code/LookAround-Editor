using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject bar;
    public TMP_Text loadingText;

    Action onFull = null;

    int maxSteps = 0;
    int currentStep = 0;

    public void IncreaseLoader(int maxSteps, string text = "", int stepSize = 1)
    {
        currentStep += stepSize;

        if (currentStep > maxSteps)
        {
            currentStep = maxSteps;
        }

        UpdateLoader(currentStep, maxSteps, text);
    }

    public void UpdateLoader(int currentStep, int maxSteps, string text = "")
    {
        this.maxSteps = maxSteps;
        this.currentStep = currentStep;

        RectTransform rt = bar.GetComponent<RectTransform>();
        rt.anchorMax = new Vector2((float)currentStep / maxSteps, rt.anchorMax.y);

        if (!string.IsNullOrEmpty(text))
        {
            loadingText.text = text;
        }

        if (currentStep >= maxSteps)
        {
            onFull?.Invoke();
        }
    }

    public void OnFull(Action onFull)
    {
        onFull = () =>
        {
            onFull?.Invoke();
            onFull = null;
            currentStep = 0;
        };
    }
}
