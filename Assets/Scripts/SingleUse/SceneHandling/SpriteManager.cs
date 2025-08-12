
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


public struct SpriteData
{
    public string path;
    public Texture2D texture;

    public SpriteData(string path, Texture2D texture)
    {
        this.path = path;
        this.texture = texture;
    }
}

public struct LogoData
{
    public SpriteData spriteData;
    public Color color;

    public LogoData(SpriteData spriteData, Color color)
    {
        this.spriteData = spriteData;
        this.color = color;
    }
}

public class SpriteManager : MonoBehaviour
{
    LogoLoadingOverlay logoLoadingOverlay;

    public LogoData[] logos = new LogoData[3];
    public SpriteData[] sceneSprites = new SpriteData[3];


    public Texture2D logoDefault;
    public Color colorDefault = new Color(1f, 1f, 1f, 1f);

    public void Awake()
    {
        logoLoadingOverlay = FindFirstObjectByType<LogoLoadingOverlay>();
        logoLoadingOverlay.Initialize(logoDefault, colorDefault);
    }

    public void LoadSprite(int index, string path)
    {
        if (index < 0 || index >= sceneSprites.Length)
        {
            Debug.LogError("Index out of bounds for sceneSprites array.");
            return;
        }

        StartCoroutine(LoadTextureFromPath(path, texture =>
        {
            sceneSprites[index] = new SpriteData(path, texture);
        }));
    }

    public Texture2D GetSprite(int index)
    {
        if (index < 0 || index >= sceneSprites.Length)
        {
            Debug.LogError("Index out of bounds for sceneSprites array.");
            return null;
        }

        return sceneSprites[index].texture;
    }

    public string GetPath(int index)
    {
        if (index < 0 || index >= sceneSprites.Length)
        {
            Debug.LogError("Index out of bounds for sceneSprites array.");
            return null;
        }

        return sceneSprites[index].path;
    }

    public void UnloadSprite(int index)
    {
        if (index < 0 || index >= sceneSprites.Length)
        {
            Debug.LogError("Index out of bounds for sceneSprites array.");
            return;
        }
        sceneSprites[index] = new SpriteData(string.Empty, null);
    }

    public SpriteData[] GetSceneSprites()
    {
        return sceneSprites;
    }


    public void LoadLogo(int id, string path, string color = "")
    {
        if (id >= 0 && id < logos.Length)
        {
            logos[id].spriteData.path = path;
            StartCoroutine(LoadTextureFromPath(path, texture =>
            {
                logos[id].spriteData.texture = texture;

                var parsedColor = colorDefault;
                ColorUtility.TryParseHtmlString(color, out parsedColor);
                logos[id].color = parsedColor;
            }));
        }
    }

    public void UnloadLogo(int id)
    {
        if (id >= 0 && id < logos.Length)
        {
            logos[id] = new LogoData(new SpriteData(string.Empty, null), colorDefault);
        }
    }

    IEnumerator LoadTextureFromPath(string path, Action<Texture2D> onLoaded)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load texture: " + uwr.error);
                yield return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
            if (texture != null)
            {
                onLoaded?.Invoke(texture);
            }
            else
            {
                Debug.LogError("Failed to create texture from downloaded content.");
            }

        }
    }

    public void SetLogoFromIndex(int index)
    {
        if (index >= 0 && index < logos.Length)
        {
            logoLoadingOverlay.SetBackgroundColor(logos[index].color);
            logoLoadingOverlay.SetLogo(logos[index].spriteData.texture);
            return;
        }
        logoLoadingOverlay.SetLogo(logoDefault);
    }

    public string[] GetLogoPaths()
    {
        string[] paths = new string[logos.Length];
        for (int i = 0; i < logos.Length; i++)
        {
            paths[i] = logos[i].spriteData.path;
        }
        return paths;
    }

    public string[] GetSpritePaths()
    {
        string[] paths = new string[sceneSprites.Length];
        for (int i = 0; i < sceneSprites.Length; i++)
        {
            paths[i] = sceneSprites[i].path;
        }
        return paths;
    }

}
