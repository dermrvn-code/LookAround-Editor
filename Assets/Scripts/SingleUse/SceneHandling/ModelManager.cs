using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;
using UnityEngine.Events;
using System.Linq;


public class ModelManager : MonoBehaviour
{
    [SerializeField]
    int maxModels = 3;

    [SerializeField]
    Dictionary<string, GameObject> loadedModels = new Dictionary<string, GameObject>();
    Dictionary<string, GameObject> previewModels = new Dictionary<string, GameObject>();

    Dictionary<string, RenderTexture> textures = new Dictionary<string, RenderTexture>();
    Dictionary<string, string> modelPaths = new Dictionary<string, string>();

    [SerializeField]
    GameObject sceneElementsContainer;

    [SerializeField]
    GameObject siding;

    [SerializeField]
    Renderer domeRenderer;


    ThumbnailMaker thumbnailMaker;
    void Start()
    {
        thumbnailMaker = FindObjectOfType<ThumbnailMaker>();
        if (domeRenderer == null)
        {
            Debug.LogWarning("No Renderer found on dome.");
        }

        if (sceneElementsContainer == null)
        {
            Debug.LogWarning("sceneElementsContainer or dome is not assigned.");
        }
    }

    public string GetModelPath(string modelName)
    {
        string path = modelPaths[modelName];
        Debug.Log($"{modelName}: {path}");

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("Model name is null or empty.");
            return null;
        }
        return path;
    }


    [SerializeField]
    InteractableModel containerPrefab;
    public DomePosition DisplayModel(string modelName)
    {
        if (loadedModels.TryGetValue(modelName, out GameObject model))
        {
            var container = Instantiate(containerPrefab, sceneElementsContainer.transform);

            var animContainer = container.GetComponent<InteractableModel>().animationContainer;
            model.SetActive(true);
            model.transform.SetParent(animContainer.transform, false);

            return container.GetComponent<DomePosition>();
        }

        Debug.LogWarning("Model not found in loaded models: " + modelName);
        return null;
    }

    public void HideModel(string modelName)
    {
        if (loadedModels.TryGetValue(modelName, out GameObject model))
        {
            model.transform.SetParent(siding.transform, false);
            model.SetActive(false);
            return;
        }

        Debug.LogWarning("Model not found in loaded models: " + modelName);
    }

    public void HideAllModels(string exceptModelName = null)
    {
        foreach (var model in loadedModels)
        {
            if (model.Key != exceptModelName && model.Value != null)
            {
                model.Value.transform.SetParent(siding.transform, false);
                model.Value.SetActive(false);
            }
        }
    }

    public void HideAllModels()
    {
        foreach (var model in loadedModels.Values)
        {
            if (model != null)
            {
                model.transform.SetParent(siding.transform, false);
                model.SetActive(false);
            }
        }
    }

    public void LoadModel(string filepath, string modelName, UnityAction<GameObject, RenderTexture> onLoaded = null, bool preview = false)
    {
        Importer.ImportGLTFAsync(filepath, new ImportSettings(), (GameObject result, AnimationClip[] clips) =>
        {
            modelPaths[modelName] = filepath;

            if (!preview)
            {
                IntegrateModel(modelName, result);
                Debug.Log($"Model {modelName} loaded from {filepath}");
            }
            else
            {
                PreviewModel(modelName, result);
                Debug.Log($"Preview model loaded from {filepath}");
            }

            RenderTexture rt = CreateThumbnail(result, modelName, preview);
            if (rt == null)
            {
                Debug.LogError("Failed to create thumbnail for model: " + modelName);
                return;
            }
            onLoaded?.Invoke(result, rt);
        });
    }

    public RenderTexture GetThumbnail(string modelName)
    {
        if (textures.TryGetValue(modelName, out RenderTexture rt))
        {
            return rt;
        }
        Debug.LogWarning("Thumbnail not found for model: " + modelName);
        return null;
    }

    RenderTexture CreateThumbnail(GameObject model, string modelName, bool preview = false)
    {
        RenderTexture rt = thumbnailMaker.CreateThumbnail(model);
        if (rt != null)
        {
            textures[modelName] = rt;
        }
        return rt;
    }

    public void StorePreviewModel(string modelName, string newModelName = null)
    {
        if (string.IsNullOrEmpty(newModelName))
        {
            newModelName = modelName;
        }

        if (!previewModels.ContainsKey(modelName))
        {
            Debug.LogWarning("No preview model to store.");
            return;
        }

        if (loadedModels.ContainsKey(modelName))
        {
            Debug.LogWarning("Model already exists with this name: " + modelName);
            return;
        }

        IntegrateModel(newModelName, previewModels[modelName]);
        previewModels.Remove(modelName);
    }

    public void RemovePreviewModel(string modelName)
    {
        if (previewModels.ContainsKey(modelName))
        {
            previewModels.TryGetValue(modelName, out GameObject model);
            previewModels.Remove(modelName);
            textures.Remove(modelName);
            modelPaths.Remove(modelName);
            if (model != null)
            {
                Destroy(model);
            }
            Debug.Log($"Preview model {modelName} removed.");
            return;
        }
        else
        {
            Debug.LogWarning("Preview model not found: " + modelName);
        }
    }

    public void UnloadModel(string modelName)
    {
        if (loadedModels.ContainsKey(modelName))
        {
            loadedModels.TryGetValue(modelName, out GameObject model);
            loadedModels.Remove(modelName);
            textures.Remove(modelName);
            modelPaths.Remove(modelName);
            if (model != null)
            {
                Destroy(model);
            }
            Debug.Log($"Model {modelName} unloaded.");
            return;
        }
        Debug.LogWarning("Model not found in loaded models: " + modelName);
    }

    public string[] GetModelNames(int? maxModels = null)
    {
        return loadedModels.Keys.Take(maxModels.GetValueOrDefault(this.maxModels)).ToArray();
    }

    float realismFactor = 0.3f;
    void NormalizeModel(GameObject model)
    {
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        Renderer resultRenderer = model.GetComponent<Renderer>();
        if (resultRenderer == null)
        {
            resultRenderer = model.GetComponentInChildren<Renderer>();
        }

        if (resultRenderer == null)
        {
            Debug.LogWarning("No Renderer found on result to calculate size.");
            return;
        }

        Vector3 resultSize = resultRenderer.bounds.size;
        Vector3 domeSize = domeRenderer.bounds.size;

        float resultMax = Mathf.Max(resultSize.x, resultSize.y, resultSize.z);

        if (resultMax == 0)
        {
            Debug.LogWarning("Result size is zero, cannot scale.");
            return;
        }

        float scaleFactor;
        if (resultMax == resultSize.x)
        {
            scaleFactor = domeSize.x / resultMax;
        }
        else if (resultMax == resultSize.y)
        {
            scaleFactor = domeSize.y / resultMax;
        }
        else
        {
            scaleFactor = domeSize.z / resultMax;
        }
        scaleFactor = scaleFactor * realismFactor; // realistic scaling
        model.transform.localScale = Vector3.one * scaleFactor;
    }

    void PreviewModel(string modelName, GameObject result)
    {
        if (previewModels.Count >= maxModels)
        {
            Debug.LogWarning("Maximum number of models previewed. Cannot load more.");
            return;
        }

        if (previewModels.ContainsKey(modelName))
        {
            Debug.LogWarning("Preview model already exists with this name: " + modelName);
            return;
        }

        NormalizeModel(result);
        result.transform.SetParent(siding.transform, false);
        result.gameObject.SetActive(false);

        previewModels.Add(modelName, result);
    }

    void IntegrateModel(string modelName, GameObject result)
    {
        if (loadedModels.Count >= maxModels)
        {
            Debug.LogWarning("Maximum number of models loaded. Cannot load more.");
            return;
        }

        if (loadedModels.ContainsKey(modelName))
        {
            Debug.LogWarning("Model already loaded: " + result.name);
            return;
        }

        NormalizeModel(result);
        result.transform.SetParent(siding.transform, false);
        result.gameObject.SetActive(false);

        loadedModels.Add(modelName, result);
    }

    public void UnloadAllModels()
    {
        foreach (var model in loadedModels.Values)
        {
            if (model != null)
            {
                Destroy(model);
            }
        }
        foreach (var model in previewModels.Values)
        {
            if (model != null)
            {
                Destroy(model);
            }
        }

        loadedModels.Clear();
        previewModels.Clear();
        textures.Clear();
        modelPaths.Clear();

        Debug.Log("All models unloaded.");
    }

    void OnDestroy()
    {
        UnloadAllModels();
    }

}
