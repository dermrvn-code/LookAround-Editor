using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO.Ports;
using TMPro;
using System.Text;

[ExecuteInEditMode]
public class SpriteHolder : MonoBehaviour
{

    public Sprite[] iconSprites = new Sprite[0];
    public string[] iconNames = new string[0];

    public static Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();

    void OnEnable()
    {
        if (iconSprites.Length != iconNames.Length)
        {
            Debug.LogError("Amount of icons and mapped icon names dont Work");
            return;
        }

        icons.Clear();
        for (int i = 0; i < iconNames.Length; i++)
        {
            icons.Add(
                iconNames[i],
                iconSprites[i]
            );
        }

    }
}
