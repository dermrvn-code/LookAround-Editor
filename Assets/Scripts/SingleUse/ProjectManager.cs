using System.Text.RegularExpressions;
using SFB;
using TMPro;
using UnityEngine;

public class ProjectManager : MonoBehaviour
{
    private string projectsPath = "C:/.code/mavel/LookAround-Editor/Projects";
    public string currentSceneOverview = "";
    public string currentProjectName = "";

    public GameObject sceneTilePrefab;
    public Transform SceneOverviewList;

    public Loader loader;

    public TMP_Text projectNameText;

    private SceneManager sceneManager;
    private SceneChanger sceneChanger;
    private GraphManager graphManager;
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

        graphManager = FindObjectOfType<GraphManager>();
        panelManager = FindObjectOfType<PanelManager>();
    }

    // Update is called once per frame
    void Update()
    {

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
        var folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));
        currentProjectName = CaseToSpace(folderName);
        currentSceneOverview = path;
        loader.gameObject.SetActive(true);
        sceneManager.LoadSceneOverview(path, loader, () =>
        {
            projectNameText.text = currentProjectName;
            loader.gameObject.SetActive(false);

            foreach (var scene in sceneManager.sceneList.Values)
            {
                SceneTile sceneTile = Instantiate(sceneTilePrefab, SceneOverviewList).GetComponent<SceneTile>();
                sceneTile.Setup(scene);
            }
            sceneChanger.ToStartScene();
            graphManager.LoadGraph();
            panelManager.SwitchToScene();
        });
    }

    public void OpenProjectFolderBrowser()
    {
        StandaloneFileBrowser.OpenFilePanelAsync("Select Project Folder", projectsPath, "xml", false, (string[] paths) =>
        {
            if (paths.Length > 0)
            {
                LoadProject(paths[0]);
            }
        });
    }
}
