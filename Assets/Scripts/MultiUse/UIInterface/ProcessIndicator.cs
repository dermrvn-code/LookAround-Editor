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

    void _Show()
    {
        background.enabled = true;
        loader.SetActive(true);
    }

    void _Hide()
    {
        background.enabled = false;
        loader.SetActive(false);
    }

    public static void Show()
    {
        if (instance == null) return;
        instance._Show();
    }

    public static void Hide()
    {
        if (instance == null) return;
        instance._Hide();
    }

}
