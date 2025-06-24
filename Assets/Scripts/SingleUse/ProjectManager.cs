using System.Globalization;
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
    public string currentAuthorName = "Default Author";
    public string currentProjectDescription = "Default Description";
    public string currentFolderPath = "";
    public bool unsavedChanges = false;

    private bool isInProject = false;

    public Loader loader;

    public TMP_Text projectNameText;
    public TMP_Text sceneNameText;

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

    public static string CaseToSpace(string str)
    {
        // Convert various casings (camelCase, PascalCase, snake_case, kebab-case, etc.) to space-separated words
        string result = str
            .Replace("_", " ")
            .Replace("-", " ");
        result = Regex.Replace(result, @"(?<!^)(?=[A-Z])", " ");
        result = Regex.Replace(result, @"\s+", " ").Trim();
        return result;
    }

    public static string SpaceToCase(string str)
    {
        string result = str;
        if (str.Contains(" "))
        {
            var words = str.Split(' ');
            foreach (var word in words)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    result += char.ToUpper(word[0]) + word.Substring(1);
                }
            }
        }
        return result;
    }

    string CleanUpString(string str)
    {
        // Remove any unwanted characters and trim whitespace
        return Regex.Replace(str, @"[^\w ]", "").Trim();
    }

    public void UpdateWorld(string name, string author, string description, bool newWorld = false)
    {
        string initialProjectName = currentProjectName;

        string cleanName = CleanUpString(name);
        string cleanAuthor = CleanUpString(author);

        string worldFolderPath = currentFolderPath;
        if (newWorld)
        {
            worldFolderPath = Path.Combine(projectsPath, SpaceToCase(name));

            Debug.Log(SpaceToCase(name));
            if (Directory.Exists(worldFolderPath))
            {
                InfoText.ShowInfo("Eine Welt mit diesem Namen existiert bereits.");
                return;
            }

            InfoText.ShowInfo("Welt erstellt: " + cleanName);
            panelManager.CloseSidebar();
            ClearWindow();
        }
        else
        {
            if (initialProjectName != cleanName)
            {
                // Get the parent directory of the current folder path
                string parentFolderPath = Directory.GetParent(currentFolderPath)?.FullName;
                worldFolderPath = Path.Combine(parentFolderPath, SpaceToCase(cleanName));
                if (Directory.Exists(worldFolderPath))
                {
                    InfoText.ShowInfo("Eine Welt mit diesem Namen existiert bereits.");
                    return;
                }

                string oldFolderPath = Path.Combine(parentFolderPath, SpaceToCase(initialProjectName));
                Debug.Log(Directory.Exists(oldFolderPath));
                if (Directory.Exists(oldFolderPath))
                {
                    Directory.Move(oldFolderPath, worldFolderPath);
                }
            }
            InfoText.ShowInfo("Welt aktualisiert");
        }


        currentProjectName = cleanName;
        currentAuthorName = cleanAuthor;
        currentProjectDescription = description;
        currentFolderPath = worldFolderPath;
        isInProject = true;
        unsavedChanges = true;

        if (projectNameText != null)
        {
            projectNameText.text = currentProjectName;
        }

    }

    void LoadProject(string path)
    {
        if (!File.Exists(path) || Path.GetExtension(path).ToLower() != ".xml")
        {
            InfoText.ShowInfo("Bitte wählen Sie eine gültige XML-Datei aus.");
            return;
        }
        currentFolderPath = Path.GetDirectoryName(path);
        var folderName = Path.GetFileName(currentFolderPath);
        currentProjectName = CaseToSpace(folderName);
        currentSceneOverview = path;
        loader.gameObject.SetActive(true);
        bool success = sceneManager.LoadSceneOverview(path, loader, () =>
        {
            projectNameText.text = currentProjectName;
            panelManager.UpdateSceneList();
            loader.gameObject.SetActive(false);
            sceneChanger.ToStartScene();
            panelManager.SwitchToScene();
            isInProject = true;
        });
        if (!success)
        {
            InfoText.ShowInfo("Fehler beim Laden der Szenenübersicht. Bitte überprüfe die Datei.");
            loader.gameObject.SetActive(false);
        }

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

    public void ClearWindow()
    {
        currentSceneOverview = "";
        currentProjectName = "";
        currentAuthorName = "";
        currentProjectDescription = "";
        currentFolderPath = "";
        unsavedChanges = false;
        isInProject = false;

        if (projectNameText != null)
        {
            projectNameText.text = "";
        }
        if (sceneNameText != null)
        {
            sceneNameText.text = "";
        }

        sceneManager.sceneList.Clear();
        sceneChanger.currentScene = null;
        sceneChanger.ToMainSceneAnimation();
        panelManager.UpdateSceneList();
    }
}
