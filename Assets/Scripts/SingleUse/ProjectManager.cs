using System.IO;
using System.Text.RegularExpressions;
using SFB;
using TMPro;
using UnityEngine;

public class ProjectManager : MonoBehaviour
{
    private string projectsPath = "C:/.code/mavel/LookAround-Editor/Projects";
    public string currentSceneOverview = "";
    public string currentProjectName = "";
    public string currentFolderPath = "";

    private bool isInProject = false;

    public Loader loader;

    public TMP_Text projectNameText;

    private SceneManager sceneManager;
    private SceneChanger sceneChanger;
    private PanelManager panelManager;

    void Start()
    {
        sceneManager = FindObjectOfType<SceneManager>();
        if (sceneManager == null)
        {
            Debug.LogError("SceneManager not found in the scene.");
            return;
        }

        sceneChanger = FindObjectOfType<SceneChanger>();
        if (sceneChanger == null)
        {
            Debug.LogError("SceneChanger not found in the scene.");
            return;
        }
        panelManager = FindObjectOfType<PanelManager>();
    }

    public bool IsInProject()
    {
        return isInProject;
    }

    string CaseToSpace(string str)
    {
        // Convert various casings (camelCase, PascalCase, snake_case, kebab-case, etc.) to space-separated words
        string result = str
            .Replace("_", " ")
            .Replace("-", " ");
        result = Regex.Replace(result, @"(?<!^)(?=[A-Z])", " ");
        result = Regex.Replace(result, @"\s+", " ").Trim();
        return result;
    }

    void LoadProject(string path)
    {
        currentFolderPath = Path.GetDirectoryName(path);
        var folderName = Path.GetFileName(currentFolderPath);
        currentProjectName = CaseToSpace(folderName);
        currentSceneOverview = path;
        loader.gameObject.SetActive(true);
        sceneManager.LoadSceneOverview(path, loader, () =>
        {
            projectNameText.text = currentProjectName;
            panelManager.UpdateSceneList();
            loader.gameObject.SetActive(false);
            sceneChanger.ToStartScene();
            panelManager.SwitchToScene();
            isInProject = true;
        });

    }

    public void OpenProjectFolderBrowser()
    {
        StandaloneFileBrowser.OpenFilePanelAsync("Select Project Folder", projectsPath, "xml", false, (string[] paths) =>
        {
            if (paths.Length == 1)
            {
                LoadProject(paths[0]);
            }
        });
    }
}
