using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSettings : MonoBehaviour
{
    public bool newWorld = false;
    public TextInput worldNameInput;
    public TextInput worldAuthorInput;
    public TextInput worldDescriptionInput;

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

    public void InitializeWorldSettings(string name, string author, string description)
    {
        worldNameInput.Initialize(name);
        worldAuthorInput.Initialize(author);
        worldDescriptionInput.Initialize(description);
    }
}
