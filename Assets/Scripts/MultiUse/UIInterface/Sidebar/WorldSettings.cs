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


        projectManager.UpdateWorld(worldName, worldAuthor, worldDescription, newWorld);
    }

    public void Initialize(string name, string author, string description, bool isNewWorld)
    {
        worldNameInput.Initialize(name);
        worldAuthorInput.Initialize(author);
        worldDescriptionInput.Initialize(description);

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
