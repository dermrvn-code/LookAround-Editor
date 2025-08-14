using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class SceneChanger : SceneChangerBase
{

    PanelManager panelManager;
    [SerializeField] TMP_Text sceneNameText;

    public override void Start()
    {
        base.Start();
        panelManager = FindFirstObjectByType<PanelManager>();
    }
    public override void ToMainScene()
    {
        ClearSceneElements();
        base.ToMainScene();
    }



    public bool closeSidebar = false;
    public override void SwitchScene(Scene scene, Action onLoaded = null)
    {
        base.SwitchScene(scene, onLoaded);

        sceneNameText.text = scene.Name;
        if (closeSidebar)
        {
            panelManager.CloseSidebar();
            closeSidebar = false;
        }
    }

    public void UpdateOffset(float? xOffset = null, float? yOffset = null)
    {
        if (currentScene == null) return;

        Vector2 newOffset;
        if (currentScene.Type == Scene.MediaType.Video)
        {
            newOffset = videoMaterial.mainTextureOffset;
            if (xOffset != null) newOffset.x = xOffset.Value;
            if (yOffset != null) newOffset.y = yOffset.Value;
            videoMaterial.mainTextureOffset = newOffset;
        }
        else if (currentScene.Type == Scene.MediaType.Photo)
        {

            newOffset = photoMaterial.mainTextureOffset;
            if (xOffset != null) newOffset.x = xOffset.Value;
            if (yOffset != null) newOffset.y = yOffset.Value;
            photoMaterial.mainTextureOffset = newOffset;
        }
    }

    public void UpdateMedium(string path)
    {
        if (currentScene == null) return;

        if (currentScene.Type == Scene.MediaType.Video)
        {
            SwitchToVideo();
            videoPlayer.url = path;
        }
        else if (currentScene.Type == Scene.MediaType.Photo)
        {
            StartCoroutine(textureManager.GetTexture(path, texture =>
                {
                    photoMaterial.mainTexture = texture;
                    SwitchToFoto(); ;
                }
            ));
        }
    }

    public override GameObject LoadTextElement(SceneElementText sceneElement)
    {
        var text = base.LoadTextElement(sceneElement);
        text.gameObject.AddComponent<SceneElementHolder>().sceneElement = sceneElement;
        return text;
    }
    public override GameObject LoadTextboxElement(SceneElementTextbox sceneElement)
    {
        var text = base.LoadTextboxElement(sceneElement);
        text.AddComponent<SceneElementHolder>().sceneElement = sceneElement;
        return text;
    }
    public override GameObject LoadArrow(SceneElementArrow sceneElement)
    {
        var arrow = base.LoadArrow(sceneElement);
        arrow.AddComponent<SceneElementHolder>().sceneElement = sceneElement;
        return arrow;
    }

    public override GameObject LoadModel(SceneElementModel sceneElement)
    {
        var model = base.LoadModel(sceneElement);
        model.AddComponent<SceneElementHolder>().sceneElement = sceneElement;
        return model;
    }

    public override GameObject LoadSprite(SceneElementSprite sceneElement)
    {
        var sprite = base.LoadSprite(sceneElement);
        sprite.gameObject.AddComponent<SceneElementHolder>().sceneElement = sceneElement;
        return sprite;
    }






}
