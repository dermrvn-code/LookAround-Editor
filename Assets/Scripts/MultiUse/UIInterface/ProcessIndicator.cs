using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessIndicator : MonoBehaviour
{
    public Image background;
    public Image loader;

    static ProcessIndicator instance;
    void Awake()
    {
        instance = this;
    }

    public static void Show()
    {
        if (instance == null) return;
        instance.background.enabled = true;
        instance.loader.enabled = true;
    }

    public static void Hide()
    {
        if (instance == null) return;
        instance.background.enabled = false;
        instance.loader.enabled = false;
    }

}
