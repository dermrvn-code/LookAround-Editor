using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class MediaSettings : MonoBehaviour
{

    public MediaUploader[] logos = new MediaUploader[3];
    public MediaUploader[] sprites = new MediaUploader[3];
    public ModelUploader[] models = new ModelUploader[3];


    ProjectManager projectManager;
    ModelManager modelManager;
    SpriteManager spriteManager;
    void Start()
    {
        projectManager = FindFirstObjectByType<ProjectManager>();
        modelManager = FindFirstObjectByType<ModelManager>();
        spriteManager = FindFirstObjectByType<SpriteManager>();
    }

    public void Save()
    {
        for (int i = 0; i < logos.Length; i++)
        {
            var logoInput = logos[i];
            if (string.IsNullOrEmpty(logoInput.value))
            {
                continue;
            }
            spriteManager.LoadLogo(i, logoInput.value);
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            var spriteInput = sprites[i];
            if (string.IsNullOrEmpty(spriteInput.value))
            {
                continue;
            }
            spriteManager.LoadSprite(i, spriteInput.value);
        }

        for (int i = 0; i < models.Length; i++)
        {
            var modelInput = models[i];
            if (string.IsNullOrEmpty(modelInput.modelName))
            {
                modelManager.UnloadModel("model" + (i + 1), (model) =>
            {
                modelManager.UnloadParentElement(model);
            });
                continue;
            }
            if (!modelInput.changedSinceInitialization)
            {
                continue;
            }

            projectManager.unsavedChanges = true;
            modelManager.StorePreviewModel(modelInput.modelName);
        }

        InfoText.ShowInfo("Medien wurden gespeichert.");
    }

    public void Initialize(string[] logoPaths = null, string[] spritePaths = null, string[] modelNames = null)
    {
        if (logoPaths != null)
        {
            for (int i = 0; i < logos.Length && i < logoPaths.Length; i++)
            {
                logos[i].Initialize(logoPaths[i]);
            }
        }

        if (spritePaths != null)
        {
            for (int i = 0; i < logos.Length && i < sprites.Length; i++)
            {
                sprites[i].Initialize(spritePaths[i]);
            }
        }

        if (modelNames != null)
        {
            for (int i = 0; i < models.Length && i < modelNames.Length; i++)
            {
                models[i].Initialize(modelNames[i]);
            }
        }

    }
}
