using UnityEngine;

public class AppSettings : MonoBehaviour
{

    public SelectFolder selectFolder;

    ProjectManager projectManager;
    void Start()
    {
        projectManager = FindObjectOfType<ProjectManager>();
    }

    public void Save()
    {
        string path = selectFolder.value;
        PlayerPrefs.SetString("ProjectsFolder", path);
        PlayerPrefs.Save();
        projectManager.UpdateProjectsFolder();

        InfoText.ShowInfo("Einstellungen gespeichert");
    }

    public void Initialize(string projectFolder)
    {
        selectFolder.Initialize(projectFolder);
    }

}
