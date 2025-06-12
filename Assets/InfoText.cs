using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoText : MonoBehaviour
{

    static InfoText instance;

    TMP_Text text;
    Animator animator;
    void Awake()
    {
        instance = this;
        text = GetComponentInChildren<TMP_Text>();
        animator = GetComponent<Animator>();
    }

    IEnumerator EnumShowInfo(string text)
    {
        float delay = 3f;
        if (text.Length > 100)
        {
            delay = text.Length / 20f; // Adjust delay based on text length
        }
        this.text.text = text;
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Visible", true);
        yield return new WaitForSeconds(delay);
        animator.SetBool("Visible", false);
    }

    void _ShowInfo(string text)
    {
        StartCoroutine(EnumShowInfo(text));
    }

    public static void ShowInfo(string text)
    {
        instance._ShowInfo(text);
    }




}
