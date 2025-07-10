using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;
using UnityEngine.Events;


public class ModelManager : MonoBehaviour
{
    [SerializeField]
    int maxModels = 8;

    [SerializeField]
    Dictionary<string, GameObject> loadedModels = new Dictionary<string, GameObject>();

    [SerializeField]
    GameObject sceneElementsContainer;

    [SerializeField]
    GameObject siding;

    [SerializeField]
    Renderer domeRenderer;


    void Start()
    {
        if (domeRenderer == null)
        {
            Debug.LogWarning("No Renderer found on dome.");
        }

        if (sceneElementsContainer == null)
        {
            Debug.LogWarning("sceneElementsContainer or dome is not assigned.");
        }
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

    public void LoadModel(string filepath, string modelName, UnityAction onLoaded = null)
    {
        Importer.ImportGLTFAsync(filepath, new ImportSettings(), (GameObject result, AnimationClip[] clips) =>
        {
            IntegrateModel(modelName, result);
            onLoaded?.Invoke();
            Debug.Log($"Model {modelName} loaded from {filepath}");
        });
    }

    public void UnloadModel(string modelName)
    {
        if (loadedModels.ContainsKey(modelName))
        {
            loadedModels.TryGetValue(modelName, out GameObject model);
            loadedModels.Remove(modelName);
            if (model != null)
            {
                Destroy(model);
            }
            Debug.Log($"Model {modelName} unloaded.");
            return;
        }
        Debug.LogWarning("Model not found in loaded models: " + modelName);
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
        loadedModels.Clear();
        Debug.Log("All models unloaded.");
    }

    void OnDestroy()
    {
        UnloadAllModels();
    }

}
