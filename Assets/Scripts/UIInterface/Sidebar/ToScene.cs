using System;
using System.Collections.Generic;
using UnityEngine;

public class ToScene : MonoBehaviour
{
    [SerializeField] DropdownInput dropdownInput;
    [SerializeField] SpriteSelector spriteSelector;

    public Action<string, int> OnValueChanged;

    public string currentScene = "";
    public int currentTransition = -1;

    SceneManager sceneManager;
    SceneChanger sceneChanger;
    SpriteManager spriteManager;
    void Awake()
    {
        sceneManager = FindFirstObjectByType<SceneManager>();
        sceneChanger = FindFirstObjectByType<SceneChanger>();
        spriteManager = FindFirstObjectByType<SpriteManager>();

        dropdownInput.OnValueChanged.AddListener((value) =>
        {
            currentScene = value;
            OnValueChanged?.Invoke(currentScene, currentTransition);
        });
        spriteSelector.OnElementSelected.AddListener((value) =>
        {
            currentTransition = int.Parse(value);
            OnValueChanged?.Invoke(currentScene, currentTransition);
        });
    }

    [SerializeField] Sprite smoke;
    public void Initialize(string currentScene, int currentTransition)
    {
        this.currentScene = currentScene;
        this.currentTransition = currentTransition;

        var actionOptions = new List<string> { "Keine" };
        foreach (var scene in sceneManager.sceneList.Values)
        {
            if (scene.Name != sceneChanger.currentScene.Name)
            {
                actionOptions.Add(scene.Name);
            }
        }

        dropdownInput.Initialize(actionOptions, currentScene);

        Sprite[] logos = new Sprite[3];
        for (int i = 0; i < spriteManager.logos.Length; i++)
        {
            var logo = spriteManager.logos[i].spriteData.texture;
            if (logo != null)
            {
                logos[i] = Sprite.Create(logo, new Rect(0, 0, logo.width, logo.height), new Vector2(0.5f, 0.5f));
            }
        }

        var spritePairs = new Pairs.SpritePair[4];
        spritePairs[0] = new Pairs.SpritePair { value = "-1", sprite = smoke };
        for (int i = 1; i < spritePairs.Length; i++)
        {
            var logo = logos[i - 1];
            if (logo != null)
            {
                spritePairs[i] = new Pairs.SpritePair { value = (i - 1).ToString(), sprite = logo };
            }
        }

        string selectedValue = currentTransition.ToString();
        if (currentTransition >= 1 && currentTransition < spritePairs.Length)
        {
            selectedValue = currentTransition.ToString();
        }
        spriteSelector.Initialize(spritePairs, selectedValue);

    }
}
