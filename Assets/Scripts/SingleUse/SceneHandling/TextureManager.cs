using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class TextureManager : MonoBehaviour
{
    public static int maxTexturesToKeep = 15;
    public static int maxMemoryUsageMB = 2000;
    Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    LinkedList<string> lruList = new LinkedList<string>();
    int currentMemoryUsage = 0;


    public IEnumerator LoadAllTextures(List<string> texturePaths, Loader loadingBar, Action onComplete = null)
    {
        textureCache.Clear();
        lruList.Clear();

        for (int i = 0; i < texturePaths.Count; i++)
        {
            if (i >= maxTexturesToKeep)
            {
                Debug.LogWarning($"Max number of textures ({maxTexturesToKeep}) to keep loaded reached. Stopping preload.");
                break;
            }
            var texturePath = texturePaths[i];

            int max = texturePaths.Count;
            if (maxTexturesToKeep < max)
            {
                max = maxTexturesToKeep;
            }
            string fileName = Path.GetFileName(texturePath);
            loadingBar.UpdateLoader((float)i / (max - 1), "Textur \"" + fileName + "\" wird geladen...");

            yield return StartCoroutine(LoadTextureWithEviction(texturePath, null));
        }

        onComplete?.Invoke();
    }


    public IEnumerator GetTexture(string filePath, Action<Texture2D> onLoaded)
    {
        if (textureCache.TryGetValue(filePath, out var cachedTexture))
        {
            lruList.Remove(filePath);
            lruList.AddFirst(filePath);
            onLoaded?.Invoke(cachedTexture);
        }
        else
        {
            yield return StartCoroutine(LoadTextureWithEviction(filePath, onLoaded));
        }
    }

    private IEnumerator LoadTextureWithEviction(string filePath, Action<Texture2D> onLoaded)
    {
        yield return StartCoroutine(LoadTextureAsync(filePath, texture =>
        {
            if (texture == null) return;

            int textureMemoryUsage = EstimateTextureMemoryUsage(texture);
            if (textureCache.Count >= maxTexturesToKeep || currentMemoryUsage + textureMemoryUsage > maxMemoryUsageMB)
            {
                EvictLeastRecentlyUsedTexture();
            }

            textureCache[filePath] = texture;
            lruList.AddFirst(filePath); // Add to the front of the LRU list
            currentMemoryUsage += textureMemoryUsage;
            onLoaded?.Invoke(texture);

            Debug.Log($"Loaded texture from {filePath}. Current memory usage: {currentMemoryUsage} MB");
        }));
    }

    private void EvictLeastRecentlyUsedTexture()
    {
        var leastRecentlyUsedPath = lruList.Last?.Value;
        if (leastRecentlyUsedPath != null)
        {
            if (textureCache.TryGetValue(leastRecentlyUsedPath, out var textureToEvict))
            {
                int textureMemoryUsage = EstimateTextureMemoryUsage(textureToEvict);
                Destroy(textureToEvict);
                textureCache.Remove(leastRecentlyUsedPath);
                lruList.RemoveLast(); // Remove from the LRU list
                currentMemoryUsage -= textureMemoryUsage;

                Debug.Log($"Evicted texture: {leastRecentlyUsedPath}. Freed memory: {textureMemoryUsage} MB");
            }
        }
    }

    private IEnumerator LoadTextureAsync(string filePath, Action<Texture2D> onLoaded)
    {
        if (File.Exists(filePath))
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + filePath))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to load texture: " + uwr.error);
                    onLoaded?.Invoke(null);
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    onLoaded?.Invoke(texture);
                }
            }
        }
        else
        {
            Debug.LogError($"File not found:{filePath}");
            onLoaded?.Invoke(null);
        }
    }

    private int EstimateTextureMemoryUsage(Texture2D texture)
    {
        // Calculate memory usage: width * height * 4 (RGBA) / (1024 * 1024) to get MB
        return (texture.width * texture.height * 4) / (1024 * 1024);
    }

    public void ReleaseAllTextures()
    {
        foreach (var texture in textureCache.Values)
        {
            Destroy(texture);
        }
        textureCache.Clear();
        lruList.Clear();
        currentMemoryUsage = 0;
        Debug.Log("Released all textures");
    }

    public void UpdateMaxTexturesToKeep(float value)
    {
        maxTexturesToKeep = (int)value;
    }

    public void UpdateMaxMemoryUsage(float value)
    {
        maxMemoryUsageMB = (int)value;
    }
}
