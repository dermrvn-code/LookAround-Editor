using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessIndicator : MonoBehaviour
{
    public Image background;
    public GameObject loader;

    static ProcessIndicator instance;
    void Awake()
    {
        instance = this;
    }

    public static void Show()
    {
        if (instance == null) return;
        instance.background.enabled = true;
        instance.loader.SetActive(true);
    }

    public static void Hide()
    {
        if (instance == null) return;
        instance.background.enabled = false;
        instance.loader.SetActive(false);
    }

}
