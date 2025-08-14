using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;
using UnityEngine.Events;
using System.Linq;

public class ModelManager : ModelManagerBase
{

    public Dictionary<string, Model> previewModels = new Dictionary<string, Model>();

    ThumbnailMaker thumbnailMaker;
    SceneChanger sceneChanger;
    public override void Start()
    {
        base.Start();
        thumbnailMaker = FindFirstObjectByType<ThumbnailMaker>();
        sceneChanger = FindFirstObjectByType<SceneChanger>();
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

    public override void HideModel(string modelName)
    {
        base.HideModel(modelName);
        SetUsed(modelName, false);
    }

    public bool preview;
    public override void OnModelLoaded(string modelName, GameObject result, string filePath, UnityAction<GameObject, Texture2D> onLoaded = null)
    {


        if (!preview)
        {
            IntegrateModel(modelName, result, filePath);
            preview = false;
        }
        else
        {
            PreviewModel(modelName, result, filePath);
        }

        Texture2D rt = CreateThumbnail(result, modelName);
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
                onLoaded?.Invoke(result, rt);
            }
        }

        StartCoroutine(Delay());
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

    Texture2D CreateThumbnail(GameObject gameObject, string modelName)
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
            UnloadModel(newModelName, (model) =>
            {
                UnloadParentElement(model);
            });
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

    public void UnloadParentElement(Model model)
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


    public override void UnloadAllModels()
    {
        foreach (var model in previewModels.Values)
        {
            if (model.gameobject != null)
            {
                Destroy(model.gameobject);
            }
        }
        previewModels.Clear();

        base.UnloadAllModels();
    }

}
