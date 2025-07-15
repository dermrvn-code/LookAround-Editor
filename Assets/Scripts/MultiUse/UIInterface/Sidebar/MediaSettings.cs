using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class MediaSettings : MonoBehaviour
{

    public MediaUploader[] logos = new MediaUploader[3];
    public ModelUploader[] models = new ModelUploader[3];


    ProjectManager projectManager;
    ModelManager modelManager;
    void Start()
    {
        projectManager = FindObjectOfType<ProjectManager>();
        modelManager = FindObjectOfType<ModelManager>();
    }

    public void Save()
    {
        List<string> logoPaths = new List<string>();
        foreach (var logo in logos)
        {
            if (!string.IsNullOrEmpty(logo.value))
            {
                if (!File.Exists(logo.value))
                {
                    InfoText.ShowInfo($"Logo-Datei '{logo.value}' existiert nicht.");
                    continue;
                }
                logoPaths.Add(logo.value);
            }
        }

        // projectManager.UpdateWorld(worldName, worldAuthor, worldDescription, newWorld, logoPaths.ToArray());
    }

    public void Initialize(string[] logoPaths = null, string[] modelNames = null)
    {
        if (logoPaths != null)
        {
            for (int i = 0; i < logos.Length && i < logoPaths.Length; i++)
            {
                logos[i].Initialize(logoPaths[i]);
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
