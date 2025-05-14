using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBox : Hoverable
{
    [SerializeField]
    RectTransform text;
    [SerializeField]
    GameObject background;
    [SerializeField]
    SpriteRenderer icon;

    [SerializeField]
    Vector3 closedScale = Vector3.one;

    public Vector2 scale;
    Vector3 backgroundScale;

    public bool isOpen = false;


    TMP_Text tmptext;
    Color textColor;
    Color iconColor;

    bool stayOpen = false;
    bool stayClosed = true;
    void Update()
    {

        if (isOpen)
        {
            stayClosed = false;
            if (stayOpen == false) StartCoroutine(Open());
            stayOpen = true;
        }
        else
        {
            stayOpen = false;
            if (stayClosed == false) StartCoroutine(Close());
            stayClosed = true;
        }
    }

    public override void Setup()
    {
        tmptext = text.GetComponent<TMP_Text>();
        textColor = tmptext.color;
        iconColor = icon.color;
        tmptext.color = Color.clear;
        text.sizeDelta = new Vector2(scale.x * 0.92f, scale.y * 0.92f);
        closedScale.z = background.transform.localScale.z;
        backgroundScale = new Vector3(scale.x, scale.y, background.transform.localScale.z);
        background.transform.localScale = closedScale;
    }

    public override void Highlight()
    {
        isOpen = true;
    }

    public override void Unhighlight()
    {
        isOpen = false;
    }

    IEnumerator Open()
    {
        StartCoroutine(Fade(icon, textColor, Color.clear));
        StartCoroutine(Scale(background.transform, closedScale, backgroundScale));
        yield return new WaitForSeconds(100 * 0.005f);
        if(stayOpen){
            StartCoroutine(Fade(tmptext, Color.clear, textColor));
        }
    }

    IEnumerator Close()
    {
        StartCoroutine(Fade(tmptext, textColor, Color.clear));
        yield return new WaitForSeconds(100 * 0.005f);
        if(stayClosed){
            StartCoroutine(Scale(background.transform, backgroundScale, closedScale));
            StartCoroutine(Fade(icon, Color.clear, textColor));
        }
    }

    IEnumerator Scale(Transform element, Vector3 from, Vector3 to)
    {
        for (int i = 0; i <= 100; i += 2)
        {
            element.localScale = Vector3.Lerp(from, to, (float)i / 100);
            yield return new WaitForSeconds(0.005f);
        }
    }

    IEnumerator Fade(TMP_Text text, Color from, Color to)
    {
        for (int i = 0; i <= 100; i += 2)
        {
            text.color = Color.Lerp(from, to, (float)i / 100);
            yield return new WaitForSeconds(0.005f);
        }
    }

    IEnumerator Fade(SpriteRenderer sprite, Color from, Color to, bool fadeIn = false)
    {
        for (int i = 0; i <= 100; i += 2)
        {
            sprite.color = Color.Lerp(from, to, (float)i / 100);
            yield return new WaitForSeconds(0.005f);
        }
    }

}
