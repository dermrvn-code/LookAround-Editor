using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class WorldSettings : MonoBehaviour
{
    public bool newWorld = false;
    public TextInput worldNameInput;
    public TextInput worldAuthorInput;
    public TextInput worldDescriptionInput;


    public MediaUploader[] logos = new MediaUploader[3];


    public TMP_Text title;

    public string creationTitle = "Welt erstellen";
    public string editTitle = "Welt bearbeiten";

    ProjectManager projectManager;
    void Start()
    {
        projectManager = FindObjectOfType<ProjectManager>();
    }

    public void Save()
    {
        string worldName = worldNameInput.value;
        string worldAuthor = worldAuthorInput.value;
        string worldDescription = worldDescriptionInput.value;

        if (string.IsNullOrEmpty(worldName))
        {
            InfoText.ShowInfo("Weltname darf nicht leer sein.");
            return;
        }

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

        projectManager.UpdateWorld(worldName, worldAuthor, worldDescription, newWorld, logoPaths.ToArray());
    }

    public void InitializeWorldSettings(string name, string author, string description, bool isNewWorld, string[] paths = null)
    {
        worldNameInput.Initialize(name);
        worldAuthorInput.Initialize(author);
        worldDescriptionInput.Initialize(description);

        for (int i = 0; i < paths.Length; i++)
        {
            logos[i].Initialize(paths[i]);
        }

        newWorld = isNewWorld;
        if (isNewWorld)
        {
            title.text = creationTitle;
        }
        else
        {
            title.text = editTitle;
        }
    }
}
