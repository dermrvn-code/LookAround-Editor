using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;
using UnityEngine.Events;
using System.Linq;

public struct Model
{
    public string name;
    public string path;
    public GameObject gameobject;
    public Texture2D preview;

    public bool used;

    public Model(string name, string path, GameObject gameobject)
    {
        this.name = name;
        this.path = path;
        this.gameobject = gameobject;
        preview = null;
        used = false;
    }
}

public class ModelManager : MonoBehaviour
{
    [SerializeField]
    int maxModels = 3;

    [SerializeField]
    public Dictionary<string, Model> loadedModels = new Dictionary<string, Model>();
    public Dictionary<string, Model> previewModels = new Dictionary<string, Model>();

    [SerializeField]
    GameObject sceneElementsContainer;

    [SerializeField]
    GameObject siding;

    [SerializeField]
    Renderer domeRenderer;


    ThumbnailMaker thumbnailMaker;
    SceneChanger sceneChanger;
    void Start()
    {
        thumbnailMaker = FindObjectOfType<ThumbnailMaker>();
        sceneChanger = FindObjectOfType<SceneChanger>();
        if (domeRenderer == null)
        {
            Debug.LogWarning("No Renderer found on dome.");
        }

        if (sceneElementsContainer == null)
        {
            Debug.LogWarning("sceneElementsContainer or dome is not assigned.");
        }
    }

    public string GetFirstModel()
    {
        foreach (var model in loadedModels.Values)
        {
            if (model.name != null && !model.used)
            {
                return model.name;
            }
        }
        return null;
    }

    public int AmountOfModels()
    {
        return loadedModels.Count + previewModels.Count;
    }

    public List<Model> GetAllModels()
    {
        List<Model> allModels = new List<Model>();
        allModels.AddRange(loadedModels.Values);
        allModels.AddRange(previewModels.Values);
        return allModels;
    }

    public string GetModelPath(string modelName)
    {
        var models = GetAllModels();
        foreach (var model in models)
        {
            if (model.name == modelName)
            {
                return model.path;
            }
        }
        return null;
    }

    void SetUsed(string modelName, bool used = true)
    {
        if (loadedModels.ContainsKey(modelName))
        {
            Model model = loadedModels[modelName];
            model.used = used;
            loadedModels[modelName] = model;
        }
        else if (previewModels.ContainsKey(modelName))
        {
            Model model = previewModels[modelName];
            model.used = used;
            previewModels[modelName] = model;
        }
    }



    [SerializeField]
    InteractableModel containerPrefab;
    public GameObject DisplayModel(string modelName)
    {
        if (!loadedModels.TryGetValue(modelName, out Model model))
        {
            Debug.LogWarning("Model not found in loaded models: " + modelName);
        }

        if (model.gameobject != null)
        {
            var container = Instantiate(containerPrefab, sceneElementsContainer.transform);

            var animContainer = container.GetComponent<InteractableModel>().elementContainer;
            model.gameobject.SetActive(true);
            model.gameobject.transform.SetParent(animContainer.transform, false);

            SetUsed(modelName);
            return container.gameObject;
        }
        return null;
    }



    public void SwitchModel(GameObject container, string oldModelName, string newModelName)
    {
        if (!loadedModels.TryGetValue(newModelName, out Model model))
        {
            Debug.LogWarning("Model not found in loaded models: " + newModelName);
            return;
        }

        if (model.gameobject != null)
        {
            HideModel(oldModelName);

            var animContainer = container.GetComponent<InteractableModel>().elementContainer;
            model.gameobject.SetActive(true);
            model.gameobject.transform.SetParent(animContainer.transform, false);
            SetUsed(newModelName);
        }

    }

    public void HideModel(string modelName)
    {
        if (loadedModels.TryGetValue(modelName, out Model model))
        {
            model.gameobject.transform.SetParent(siding.transform, false);
            model.gameobject.SetActive(false);
            SetUsed(modelName, false);
            return;
        }

        Debug.LogWarning("Model not found in loaded models: " + modelName);
    }

    public void HideAllModels(string exceptModelName = null)
    {
        var models = GetAllModels();
        foreach (var model in models)
        {
            if (model.name != exceptModelName && model.gameobject != null)
            {
                model.gameobject.transform.SetParent(siding.transform, false);
                model.gameobject.SetActive(false);
                SetUsed(model.name, false);
            }
        }
    }

    public void LoadModel(string filepath, string modelName, UnityAction<GameObject, Texture2D> onLoaded = null, bool preview = false)
    {
        Importer.ImportGLTFAsync(filepath, new ImportSettings(), (GameObject result, AnimationClip[] clips) =>
        {
            foreach (var meshFilter in result.GetComponentsInChildren<MeshFilter>())
            {
                var collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = meshFilter.sharedMesh;
            }
            ;
            if (loadedModels.ContainsKey(modelName) || previewModels.ContainsKey(modelName))
            {
                Debug.LogWarning($"Model {modelName} already found in loaded models");
                return;
            }

            result.name = modelName;
            NormalizeModel(result);


            if (!preview)
            {
                IntegrateModel(modelName, result, filepath);
            }
            else
            {
                PreviewModel(modelName, result, filepath);
            }

            Texture2D rt = CreateThumbnail(result, modelName, preview);
            if (rt == null)
            {
                Debug.LogError("Failed to create thumbnail for model: " + modelName);
                return;
            }

            IEnumerator Delay()
            {
                yield return new WaitForSeconds(1.5f);
                if (onLoaded != null)
                {
                    onLoaded.Invoke(result, rt);
                }
            }

            StartCoroutine(Delay());
        });
    }

    public Texture2D GetThumbnail(string modelName)
    {
        if (loadedModels.TryGetValue(modelName, out Model model) && model.preview != null)
        {
            return model.preview;
        }
        if (previewModels.TryGetValue(modelName, out model) && model.preview != null)
        {
            return model.preview;
        }
        return null;
    }

    Texture2D CreateThumbnail(GameObject gameObject, string modelName, bool preview = false)
    {
        Texture2D rt = thumbnailMaker.CreateThumbnail(gameObject);
        if (rt != null)
        {

            if (loadedModels.ContainsKey(modelName))
            {
                Model m = loadedModels[modelName];
                m.preview = rt;
                loadedModels[modelName] = m;
            }
            else if (previewModels.ContainsKey(modelName))
            {
                Model m = previewModels[modelName];
                m.preview = rt;
                previewModels[modelName] = m;
            }
            else
            {
                Debug.LogWarning($"Model {modelName} not found in either loaded or preview models.");
                return null;
            }
            return rt;
        }
        return null;
    }

    public void StorePreviewModel(string modelName, string newModelName = null)
    {
        if (string.IsNullOrEmpty(newModelName))
        {
            newModelName = modelName;
        }

        if (!previewModels.ContainsKey(modelName))
        {
            Debug.LogWarning($"No preview model with name {modelName} to store.");
            return;
        }

        if (loadedModels.ContainsKey(newModelName))
        {
            UnloadModel(newModelName);
        }

        if (loadedModels.Count >= maxModels)
        {
            Debug.LogWarning("Maximum number of models loaded. Cannot store more.");
            return;
        }

        Model model = previewModels[modelName];
        IntegrateModel(newModelName, model.gameobject, model.path);
        previewModels.Remove(modelName);
        Debug.Log($"Preview model {modelName} stored as {newModelName}.");
    }

    public void RemovePreviewModel(string modelName)
    {
        if (previewModels.ContainsKey(modelName))
        {
            previewModels.TryGetValue(modelName, out Model model);
            previewModels.Remove(modelName);


            if (model.gameobject != null)
            {
                Destroy(model.gameobject);
            }
            Debug.Log($"Preview model {modelName} removed.");
            return;
        }
    }

    public void UnloadModel(string modelName)
    {
        if (loadedModels.ContainsKey(modelName))
        {
            loadedModels.TryGetValue(modelName, out Model model);
            loadedModels.Remove(modelName);

            if (model.gameobject != null)
            {
                UnloadParentElement(model);
                Destroy(model.gameobject);
            }

            return;
        }
        Debug.LogWarning("Model not found in loaded models: " + modelName);
    }

    void UnloadParentElement(Model model)
    {
        if (model.gameobject != null)
        {
            Transform parent = model.gameobject.transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<InteractableModel>() != null)
                {
                    sceneChanger.currentScene.SceneElements.Remove(parent.GetComponent<SceneElementHolder>().sceneElement.list_id);
                    Destroy(parent.gameObject);
                    break;
                }
                parent = parent.parent;
            }
        }
    }

    public string[] GetModelNames(int? maxModels = null)
    {
        return loadedModels.Keys.Take(maxModels.GetValueOrDefault(this.maxModels)).ToArray();
    }

    float realismFactor = 0.2f;
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

        Bounds meshBounds = new Bounds();
        bool hasBounds = false;
        foreach (var meshFilter in model.GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter.sharedMesh != null)
            {
                if (!hasBounds)
                {
                    meshBounds = meshFilter.sharedMesh.bounds;
                    meshBounds.center = meshFilter.transform.TransformPoint(meshBounds.center);
                    hasBounds = true;
                }
                else
                {
                    Bounds transformedBounds = meshFilter.sharedMesh.bounds;
                    transformedBounds.center = meshFilter.transform.TransformPoint(transformedBounds.center);
                    meshBounds.Encapsulate(transformedBounds);
                }
            }
        }
        Vector3 resultSize = hasBounds ? meshBounds.size : Vector3.zero;
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

    void PreviewModel(string modelName, GameObject result, string filepath)
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

        result.transform.SetParent(siding.transform, false);
        result.gameObject.SetActive(false);

        Model model = new Model(modelName, filepath, result);

        previewModels.Add(modelName, model);
    }

    void IntegrateModel(string modelName, GameObject result, string filepath)
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

        result.transform.SetParent(siding.transform, false);
        result.gameObject.SetActive(false);

        Model model = new Model(modelName, filepath, result);

        loadedModels.Add(modelName, model);
    }

    public void UnloadAllModels()
    {
        foreach (var model in loadedModels.Values)
        {
            if (model.gameobject != null)
            {
                Destroy(model.gameobject);
            }
        }
        foreach (var model in previewModels.Values)
        {
            if (model.gameobject != null)
            {
                Destroy(model.gameobject);
            }
        }

        loadedModels.Clear();
        previewModels.Clear();

        Debug.Log("All models unloaded.");
    }

    void OnDestroy()
    {
        UnloadAllModels();
    }

}
