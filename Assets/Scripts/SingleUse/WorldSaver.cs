using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;


public class WorldSaver : MonoBehaviour
{

    bool isWindows = false;

    void Awake()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        isWindows = true;
#endif  
    }


    SceneManager sceneManager;
    ProjectManager projectManager;
    void Start()
    {
        sceneManager = GetComponent<SceneManager>();
        projectManager = FindObjectOfType<ProjectManager>();
    }

    public void Save()
    {
        if (!projectManager.IsInProject())
        {
            InfoText.ShowInfo("Bitte erstelle oder lade ein Projekt, bevor du speicherst");
            return;
        }
        if (sceneManager.sceneList.Count == 0)
        {
            InfoText.ShowInfo("Bitte füge mindestens eine Szene hinzu, bevor du speicherst");
            return;
        }

        Parse();
    }

    void Parse()
    {
        if (!Directory.Exists(projectManager.currentFolderPath))
        {
            Directory.CreateDirectory(projectManager.currentFolderPath);
        }

        bool changedScene = false;
        foreach (var scene in sceneManager.sceneList.Values)
        {
            if (scene.HasUnsavedChanges)
            {
                SaveScene(scene);
                changedScene = true;
            }
        }
        bool wasDeleted = DeleteUnusedScenes();

        SaveSceneOverview(sceneManager.sceneList.Values.ToList());
        if (wasDeleted || changedScene)
        {
        }
        else
        {
            InfoText.ShowInfo("Keine Änderungen vorhanden");
        }
    }

    void MoveToTrash(string folderPath)
    {
        try
        {
            // Move the unused scene directory to a hidden trash folder instead of deleting
            string trashFolder = Path.Combine(projectManager.currentFolderPath, ".trash");
            if (!Directory.Exists(trashFolder))
            {
                Directory.CreateDirectory(trashFolder);
                // On Windows, set the folder as hidden
                if (isWindows)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(trashFolder);
                    dirInfo.Attributes |= FileAttributes.Hidden;
                }
            }

            string destPath = Path.Combine(trashFolder, Path.GetFileName(folderPath));
            if (Directory.Exists(destPath))
            {
                Directory.Delete(destPath, true);
            }
            Directory.Move(folderPath, destPath);
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to move unused scene directory {folderPath} to trash: {e.Message}");
        }
    }

    bool DeleteUnusedScenes()
    {
        bool deleted = false;
        if (!Directory.Exists(projectManager.currentFolderPath))
        {
            return false;
        }

        foreach (var directory in Directory.GetDirectories(projectManager.currentFolderPath))
        {
            string sceneName = Path.GetFileName(directory);

            if (!sceneManager.sceneList.ContainsKey(sceneName))
            {
                MoveToTrash(directory);
                Debug.Log($"Deleting unused scene directory: {directory}");
                deleted = true;
            }
            else
            {
                Debug.Log($"Scene {sceneName} is still in use. Not deleting.");
            }
        }
        return deleted;
    }

    string CopyMedium(string source, string destinationFolder, string scenename)
    {
        string extension = Path.GetExtension(source);
        string destFile = scenename + extension;

        string destinationPath = Path.Combine(destinationFolder, destFile);

        if (source == destinationPath && File.Exists(destinationPath))
        {
            return destFile;
        }

        try
        {
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
            File.Copy(source, destinationPath);
            Debug.Log($"Copied medium from {source} to {destinationPath}");
            return destFile;
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to copy medium from {source} to {destinationPath}: {e.Message}");
            return null;
        }
    }

    void SaveScene(Scene scene)
    {

        string sceneFolderPath = Path.Combine(projectManager.currentFolderPath, scene.Name);
        if (!Directory.Exists(sceneFolderPath))
        {
            Directory.CreateDirectory(sceneFolderPath);
        }

        string file = CopyMedium(scene.Source, sceneFolderPath, scene.Name);
        if (file == null)
        {
            return;
        }


        XDocument doc = ParseScene(scene, file);
        if (doc == null)
        {
            Debug.LogError("Failed to parse scene: " + scene.Name);
            return;
        }

        string filePath = Path.Combine(sceneFolderPath, "scene.xml");
        try
        {
            doc.Save(filePath);
            Debug.Log($"Scene {scene.Name} saved successfully at {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to save scene {scene.Name}: {e.Message}");
        }
    }

    XDocument ParseSceneOverview(List<Scene> scenes)
    {
        XDocument doc = new XDocument();
        XElement root = new XElement("World");
        root.SetAttributeValue("name", projectManager.currentProjectName);
        root.SetAttributeValue("author", projectManager.currentAuthorName);

        XElement description = new XElement("Description");
        description.Add(projectManager.currentProjectDescription);
        root.Add(description);

        XElement scenesContainer = new XElement("Scenes");
        foreach (var scene in scenes)
        {
            XElement sceneElement = new XElement("Scene");
            sceneElement.SetAttributeValue("name", scene.Name);
            sceneElement.SetAttributeValue("path", Path.Combine(scene.Name, "scene.xml"));

            if (scene.IsStartScene)
            {
                sceneElement.SetAttributeValue("startScene", "true");
            }

            scenesContainer.Add(sceneElement);
        }
        root.Add(scenesContainer);

        XElement metaData = new XElement("MetaData");
        metaData.SetAttributeValue("amountOfScenes", scenes.Count.ToString());
        metaData.SetAttributeValue("editorVersion", Application.version);
        metaData.SetAttributeValue("lastSave", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        root.Add(metaData);

        doc.Add(root);
        return doc;
    }

    void SaveSceneOverview(List<Scene> scenes)
    {
        XDocument overviewDoc = ParseSceneOverview(scenes);
        if (overviewDoc != null)
        {
            string overviewPath = Path.Combine(projectManager.currentFolderPath, "ScenesOverview.xml");
            try
            {
                overviewDoc.Save(overviewPath); ;
            }
            catch (IOException e)
            {
                Debug.LogError($"Failed to save overview: {e.Message}");
            }
        }
    }

    XDocument ParseScene(Scene scene, string newSource = "")
    {
        var doc = new XDocument();
        var sceneElement = new XElement("Scene");

        var culture = CultureInfo.GetCultureInfo("de-DE");
        sceneElement.SetAttributeValue("name", scene.Name);
        if (!string.IsNullOrEmpty(newSource))
        {
            sceneElement.SetAttributeValue("source", newSource);
        }
        else
        {
            sceneElement.SetAttributeValue("source", scene.Source);
        }
        sceneElement.SetAttributeValue("xOffset", scene.XOffset.ToString(culture));
        sceneElement.SetAttributeValue("yOffset", scene.YOffset.ToString(culture));

        switch (scene.Type)
        {
            case Scene.MediaType.Video:
                sceneElement.SetAttributeValue("type", "video");
                break;
            case Scene.MediaType.Photo:
                sceneElement.SetAttributeValue("type", "image");
                break;
            default:
                Debug.LogError("Unknown media type: " + scene.Type);
                return null;
        }

        sceneElement = ParseSceneElements(sceneElement, scene.SceneElements);
        doc.Add(sceneElement);

        return doc;
    }

    XElement ParseSceneElements(XElement sceneElement, Dictionary<int, SceneElement> sceneElements)
    {
        foreach (var element in sceneElements.Values)
        {
            Debug.Log("Parsing: " + element.ToString());
            XElement elementNode = new XElement("Element");
            elementNode.SetAttributeValue("x", element.x.ToString());
            elementNode.SetAttributeValue("y", element.y.ToString());
            elementNode.SetAttributeValue("type", element.type.ToString());

            switch (element.type)
            {
                case SceneElement.ElementType.DirectionArrow:
                    elementNode.SetAttributeValue("distance", element.distance.ToString());
                    elementNode.SetAttributeValue("rotation", element.rotation.ToString());
                    elementNode.SetAttributeValue("action", element.action);
                    elementNode.SetAttributeValue("color", element.color);
                    break;

                case SceneElement.ElementType.Textbox:
                    elementNode.SetAttributeValue("distance", element.distance.ToString());
                    elementNode.SetAttributeValue("icon", element.icon);
                    elementNode.Add(element.text);
                    break;

                case SceneElement.ElementType.Text:
                    elementNode.SetAttributeValue("distance", element.distance.ToString());
                    elementNode.SetAttributeValue("action", element.action);
                    elementNode.Add(element.text);
                    break;
            }

            sceneElement.Add(elementNode);
        }
        return sceneElement;
    }


}
