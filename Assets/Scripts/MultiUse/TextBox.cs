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
    public SpriteRenderer icon;
    public string iconName;
    public string textContent;

    [SerializeField]
    Vector3 closedScale = Vector3.one;

    public Vector2 scale;
    Vector3 backgroundScale;

    public bool isOpen = false;

    public Color color = Color.white;

    [SerializeField]
    TMP_Text tmptext;
    Color textColor = Color.clear;

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
        tmptext.text = textContent;
        textColor = textColor == Color.clear ? tmptext.color : textColor;
        tmptext.color = TransparentColor(textColor);
        text.sizeDelta = new Vector2(scale.x * 0.92f, scale.y * 0.92f);
        closedScale.z = background.transform.localScale.z;
        backgroundScale = new Vector3(scale.x, scale.y, background.transform.localScale.z);
        background.transform.localScale = closedScale;
    }

    public void SetText(string _text)
    {
        textContent = _text;
        if (tmptext == null)
        {
            tmptext = text.GetComponent<TMP_Text>();
        }

        tmptext.text = _text;
    }

    public void SetIcon(Sprite icon, string name)
    {
        if (this.icon != null)
        {
            this.icon.sprite = icon;
            this.iconName = name;
        }
    }

    public void SetColor(Color color)
    {
        this.color = color;
        // Choose black or white for text based on background color brightness
        float luminance = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
        Color bestTextColor = luminance > 0.7f ? Color.black : Color.white;
        textColor = bestTextColor;
        if (tmptext != null)
        {
            if (isOpen)
            {
                tmptext.color = textColor;
            }
            else
            {
                icon.color = textColor;
            }
        }
        background.GetComponent<MeshRenderer>().material.color = color;
    }



    public override void Highlight()
    {
        isOpen = true;
    }

    public override void Unhighlight()
    {
        isOpen = false;
    }

    Color TransparentColor(Color color)
    {
        return new Color(color.r, color.g, color.b, 0);
    }

    IEnumerator Open()
    {
        StartCoroutine(Fade(icon, textColor, TransparentColor(textColor), true));
        StartCoroutine(Scale(background.transform, closedScale, backgroundScale));
        yield return new WaitForSeconds(100 * 0.005f);
        if (stayOpen)
        {
            StartCoroutine(Fade(tmptext, TransparentColor(textColor), textColor));
        }
    }

    IEnumerator Close()
    {
        StartCoroutine(Fade(tmptext, textColor, TransparentColor(textColor)));
        yield return new WaitForSeconds(100 * 0.005f);
        if (stayClosed)
        {
            StartCoroutine(Scale(background.transform, backgroundScale, closedScale));
            StartCoroutine(Fade(icon, TransparentColor(textColor), textColor));
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
