using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SFB;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldBundleManager : MonoBehaviour
{

    [SerializeField]
    Transform worldsContainer;
    [SerializeField]
    Transform bundlesContainer;

    [SerializeField]
    Button moveLeftButton;
    [SerializeField]
    Button moveRightButton;

    [SerializeField]
    TMP_InputField bundleNameInput;

    [SerializeField]
    Button saveButton;

    [SerializeField]
    GameObject worldListItemPrefab;

    ProjectManager projectManager;

    WorldListItem selectedWorldItem;

    void Start()
    {
        projectManager = FindObjectOfType<ProjectManager>();

        LoadWorldsFromProjectsFolder();
        ReloadWorldsDisplay();

        moveRightButton.onClick.AddListener(() => MoveRight(selectedWorldItem));
        moveLeftButton.onClick.AddListener(() => MoveLeft(selectedWorldItem));
        saveButton.onClick.AddListener(Save);
    }

    void Save()
    {
        if (bundleItems.Count == 0)
        {
            Debug.LogWarning("No worlds selected for bundling.");
            return;
        }

        string bundleName = ProjectManager.SpaceToCase(bundleNameInput.text).Trim();
        if (string.IsNullOrEmpty(bundleNameInput.text))
        {
            bundleName = "bundle_";
            bundleName += System.DateTime.Now.ToString("ddMMyyyy_HHmmss");
        }

        StandaloneFileBrowser.OpenFolderPanelAsync("Zielordner auswählen", projectManager.ProjectsPath, false, (string[] paths) =>
        {
            if (paths.Length == 1)
            {
                string destinationLocation = paths[0];

                StartCoroutine(SaveBundle(destinationLocation, bundleName));
            }
            else
            {
                Debug.LogWarning("No folder selected.");
            }
        });
    }

    IEnumerator SaveBundle(string destinationLocation, string bundleName)
    {

        ProcessIndicator.Show();
        yield return null;
        yield return new WaitForEndOfFrame();

        string destinationFolder = Path.Combine(destinationLocation, bundleName);

        if (Directory.Exists(destinationFolder))
        {
            Dialog.ShowDialogOkay("Dieses Bundle existiert bereits in dem Ordner");
            yield return null;
        }
        Directory.CreateDirectory(destinationFolder);

        string bundlePath = Path.Combine(destinationFolder, "bundle.xml");

        XDocument bundleDoc = new XDocument();
        XElement worlds = new XElement("Worlds");
        foreach (var item in bundleItems.Values)
        {
            string sourceFolder = item.location;
            string destinationPath = Path.Combine(destinationFolder, item.worldName);

            if (Directory.Exists(sourceFolder))
            {
                if (Directory.Exists(destinationPath))
                {
                    Debug.LogWarning($"Destination folder already exists: {destinationPath}");
                    destinationPath = RenameDuplicate(destinationPath);
                }

                yield return CopyFilesRecursively(sourceFolder, destinationPath);

                string relativePath = Path.GetRelativePath(destinationFolder, destinationPath);
                string path = Path.Combine(relativePath, "ScenesOverview.xml");

                XElement worldElement = new XElement("World",
                    new XAttribute("name", item.worldName),
                    new XAttribute("path", path));

                worlds.Add(worldElement);
            }
            else
            {
                Debug.LogWarning($"Source folder does not exist: {sourceFolder}");
            }
            yield return new WaitForEndOfFrame();
        }

        bundleDoc.Add(worlds);
        bundleDoc.Save(bundlePath);
        ProcessIndicator.Hide();

        Dialog.ShowDialogOkay("Bundle erfolgreich erstellt!", () =>
        {
            ClearBundle();
            bundleNameInput.text = string.Empty;
            UpdateArrows();
        });
    }

    string RenameDuplicate(string path)
    {
        int count = 1;
        string newPath = path;

        while (Directory.Exists(newPath))
        {
            newPath = $"{path}_{count}";
            count++;
        }

        return newPath;
    }

    void MoveLeft(WorldListItem item)
    {
        if (item != null && item.side == Side.Bundle)
        {
            overviewItems.Add(item.Id, item);
            bundleItems.Remove(item.Id);

            item.side = Side.Overview;
            item.transform.SetParent(worldsContainer);
            UpdateArrows();
        }
    }

    void MoveRight(WorldListItem item)
    {
        if (item != null && item.side == Side.Overview)
        {
            overviewItems.Remove(item.Id);
            bundleItems.Add(item.Id, item);

            item.side = Side.Bundle;
            item.transform.SetParent(bundlesContainer);
            Deselect();
        }
    }

    void Deselect()
    {
        if (selectedWorldItem != null)
        {
            selectedWorldItem.isSelected = false;
            selectedWorldItem = null;
            UpdateArrows();
        }
    }

    void ClearBundle()
    {
        var items = bundleItems.Values.ToList();
        foreach (var child in items)
        {
            MoveLeft(child);
        }
    }


    public void Select(int id, bool unselect = false)
    {
        selectedWorldItem = null;
        IEnumerable<KeyValuePair<int, WorldListItem>> items = overviewItems.Concat(bundleItems);

        foreach (var item in items)
        {
            if (item.Value.Id == id && !unselect)
            {
                item.Value.isSelected = true;
                selectedWorldItem = item.Value;
            }
            else
            {
                item.Value.isSelected = false;
            }
        }
        UpdateArrows();
    }

    public void UpdateArrows()
    {
        if (selectedWorldItem != null)
        {
            moveLeftButton.interactable = selectedWorldItem.side == Side.Bundle;
            moveRightButton.interactable = selectedWorldItem.side == Side.Overview;
        }
        else
        {
            moveLeftButton.interactable = false;
            moveRightButton.interactable = false;
        }
    }



    List<string> allFolders = new List<string>();
    void LoadWorldsFromProjectsFolder()
    {
        var projectPath = projectManager.ProjectsPath;
        if (Directory.Exists(projectPath))
        {
            string[] folders = Directory.GetDirectories(projectPath);

            foreach (string folder in folders)
            {
                allFolders.Add(folder);
            }
        }
    }

    Dictionary<int, WorldListItem> bundleItems = new Dictionary<int, WorldListItem>();
    Dictionary<int, WorldListItem> overviewItems = new Dictionary<int, WorldListItem>();
    public void ReloadWorldsDisplay()
    {
        foreach (Transform child in worldsContainer)
        {
            Destroy(child.gameObject);
        }

        ClearBundle();

        overviewItems.Clear();
        int id = 0;
        foreach (string folder in allFolders)
        {
            var item = DisplayWorld(folder, id);

            if (item != null)
            {
                overviewItems.Add(id, item);
                id++;
            }
        }
    }

    public void UploadWorld()
    {
        StandaloneFileBrowser.OpenFolderPanelAsync("Welt hochladen", projectManager.ProjectsPath, false, (string[] paths) =>
        {
            if (paths.Length == 1)
            {
                string folder = paths[0];

                if (Directory.Exists(folder))
                {
                    if (allFolders.Contains(folder))
                    {
                        Dialog.ShowDialogOkay("Die Welt ist bereits in der Liste.");
                        return;
                    }


                    allFolders.Add(folder);

                    var item = DisplayWorld(folder, overviewItems.Count);

                    if (item != null)
                    {
                        Dialog.ShowDialogOkay($"Welt '{item.worldName}' geladen");
                    }
                    else
                    {
                        Dialog.ShowDialogOkay("Die ausgewählte Welt konnte nicht geladen werden.");
                    }
                }
                else
                {
                    Debug.LogWarning("Der ausgewählte Pfad ist kein gültiger Ordner.");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("No folder selected.");
            }
        });
    }

    WorldListItem DisplayWorld(string folder, int id)
    {
        var scenesOverview = Path.Combine(folder, "ScenesOverview.xml");

        if (File.Exists(scenesOverview))
        {
            var xdoc = XDocument.Load(scenesOverview);
            var worldNode = xdoc.Element("World");

            string worldName = "Unbekannt";
            if (worldNode.Attribute("name") != null)
            {
                worldName = worldNode.Attribute("name").Value;
                if (string.IsNullOrEmpty(worldName))
                {
                    worldName = "Unbekannt";
                }
            }

            string author = "Unbekannt";
            if (worldNode.Attribute("author") != null)
            {
                author = worldNode.Attribute("author").Value;
                if (string.IsNullOrEmpty(author))
                {
                    author = "Unbekannt";
                }
            }

            var scenesNode = worldNode.Element("Scenes");
            string startScenePath = string.Empty;
            if (scenesNode != null)
            {
                foreach (var sceneElement in scenesNode.Elements("Scene"))
                {
                    var isStartSceneAttr = sceneElement.Attribute("startScene");
                    if (isStartSceneAttr != null && isStartSceneAttr.Value == "true")
                    {
                        var pathAttr = sceneElement.Attribute("path");
                        if (pathAttr != null && !string.IsNullOrEmpty(pathAttr.Value))
                        {
                            startScenePath = pathAttr.Value;
                        }
                        break;
                    }
                }
            }

            string mediumPath = null;
            if (!string.IsNullOrEmpty(startScenePath))
            {
                var scene = Path.Combine(folder, startScenePath);

                if (File.Exists(scene))
                {
                    var sceneDoc = XDocument.Load(scene);
                    var sceneNode = sceneDoc.Element("Scene");

                    if (sceneNode != null)
                    {
                        var pathAttr = sceneNode.Attribute("source");
                        if (pathAttr != null && !string.IsNullOrEmpty(pathAttr.Value))
                        {
                            // Get the path to the parent folder of the scene file
                            var parentFolder = Directory.GetParent(scene).FullName;
                            var mediumPathTemp = Path.Combine(parentFolder, pathAttr.Value);
                            mediumPath = mediumPathTemp;
                        }
                    }
                }

            }

            var worldListItem = Instantiate(worldListItemPrefab, worldsContainer);
            var itemComponent = worldListItem.GetComponent<WorldListItem>();
            itemComponent.Initialize(id, worldName, author, folder, mediumPath);

            return itemComponent;
        }
        return null;
    }

    IEnumerator CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            yield return new WaitForEndOfFrame(); // Yield to allow UI updates
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            yield return new WaitForEndOfFrame(); // Yield to allow UI updates
        }
    }
}

