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
    ModelManager modelManager;
    LogoLoadingOverlay logoLoadingOverlay;
    void Start()
    {
        sceneManager = GetComponent<SceneManager>();
        projectManager = FindObjectOfType<ProjectManager>();
        modelManager = FindObjectOfType<ModelManager>();
        logoLoadingOverlay = FindObjectOfType<LogoLoadingOverlay>();
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

        // SAVE SCENES
        bool changedScene = false;
        foreach (var scene in sceneManager.sceneList.Values)
        {
            if (scene.HasUnsavedChanges)
            {
                SaveScene(scene);
                changedScene = true;
            }
        }

        string mediaFolder = Path.Combine(projectManager.currentFolderPath, ".media");

        if (!Directory.Exists(mediaFolder))
        {
            Directory.CreateDirectory(mediaFolder);
        }

        // COPY OVER LOGOS
        bool logoUpdated = false;
        for (int i = 0; i < logoLoadingOverlay.logoPaths.Length; i++)
        {
            string logoPath = logoLoadingOverlay.logoPaths[i];
            if (string.IsNullOrEmpty(logoPath) || logoLoadingOverlay.logoTextures[i] == null)
            {
                continue;
            }
            CopyMedium(logoPath, Path.Combine(mediaFolder, "logos"), $"logo_{i}");
            logoUpdated = true;
        }

        // COPY OVER MODELS
        bool modelsUpdated = false;
        foreach (var modelName in modelManager.GetModelNames())
        {
            Debug.Log(modelName);
            var modelPath = modelManager.GetModelPath(modelName);
            if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
            {
                Debug.LogWarning($"Model path for {modelName} is empty or does not exist.");
                continue;
            }

            string modelSourceFolder = Path.GetDirectoryName(modelPath);
            string destModelFolder = Path.Combine(mediaFolder, "models", modelName);
            if (Directory.Exists(destModelFolder))
            {
                Directory.Delete(destModelFolder, true);
            }
            DirectoryCopyRecurse(modelSourceFolder, destModelFolder);
            modelsUpdated = true;
        }


        bool wasDeleted = DeleteUnusedScenes();


        SaveSceneOverview(sceneManager.sceneList.Values.ToList());
        if (!wasDeleted && !changedScene && !logoUpdated && !modelsUpdated)
        {
            InfoText.ShowInfo("Keine Änderungen vorhanden");
        }
        else
        {
            InfoText.ShowInfo("Projekt gespeichert in " + projectManager.currentFolderPath);
        }
    }

    void DirectoryCopyRecurse(string sourcePath, string destPath)
    {
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        foreach (string filePath in Directory.GetFiles(sourcePath))
        {
            string fileName = Path.GetFileName(filePath);
            string destFile = Path.Combine(destPath, fileName);
            File.Copy(filePath, destFile, true);
        }

        foreach (string directoryPath in Directory.GetDirectories(sourcePath))
        {
            string dirName = Path.GetFileName(directoryPath);
            string destDir = Path.Combine(destPath, dirName);
            DirectoryCopyRecurse(directoryPath, destDir);
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
            Debug.LogWarning($"Failed to move unused scene directory {folderPath} to trash: {e.Message}");
        }
    }

    string[] excludeFolders = new string[] { ".media", ".trash" };
    bool DeleteUnusedScenes()
    {
        bool deleted = false;
        if (!Directory.Exists(projectManager.currentFolderPath))
        {
            return false;
        }

        foreach (var directory in Directory.GetDirectories(projectManager.currentFolderPath))
        {
            if (excludeFolders.Any(exclude => directory.EndsWith(exclude, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            string sceneName = Path.GetFileName(directory);

            if (!sceneManager.sceneList.ContainsKey(sceneName))
            {
                MoveToTrash(directory);
                deleted = true;
            }
        }
        return deleted;
    }



    string CopyMedium(string source, string destinationFolder, string filename)
    {
        string extension = Path.GetExtension(source);
        string destFile = filename + extension;

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

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
            return destFile;
        }
        catch (IOException e)
        {
            Debug.LogWarning($"Failed to copy medium from {source} to {destinationPath}: {e.Message}");
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
            Debug.LogWarning("Failed to parse scene: " + scene.Name);
            return;
        }

        string filePath = Path.Combine(sceneFolderPath, "scene.xml");
        try
        {
            doc.Save(filePath);
        }
        catch (IOException e)
        {
            Debug.LogWarning($"Failed to save scene {scene.Name}: {e.Message}");
        }
    }

    XDocument ParseSceneOverview(List<Scene> scenes)
    {
        XDocument doc = new XDocument();

        // HEADER
        XElement root = new XElement("World");
        root.SetAttributeValue("name", projectManager.currentProjectName);
        root.SetAttributeValue("author", projectManager.currentAuthorName);

        // DESCRIPTION
        XElement description = new XElement("Description");
        description.Add(projectManager.currentProjectDescription);
        root.Add(description);

        // SCENES
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

        // LOGOS
        if (logoLoadingOverlay.logoPaths.Length > 0)
        {
            XElement logosElement = new XElement("Logos");

            for (int i = 0; i < logoLoadingOverlay.logoPaths.Length; i++)
            {
                if (string.IsNullOrEmpty(logoLoadingOverlay.logoPaths[i]))
                {
                    continue;
                }
                string path = Path.Combine(".media", "logos", "logo_" + i + Path.GetExtension(logoLoadingOverlay.logoPaths[i]));
                XElement logoElement = new XElement("Logo");
                logoElement.SetAttributeValue("id", i);
                logoElement.SetAttributeValue("source", path);
                logosElement.Add(logoElement);
            }
            if (logosElement.HasElements)
            {
                root.Add(logosElement);
            }
        }

        // MODELS
        XElement modelsElement = new XElement("Models");
        var modelNames = modelManager.GetModelNames();
        if (modelNames.Length > 0)
        {
            foreach (var modelName in modelNames)
            {
                var path = modelManager.GetModelPath(modelName);
                var modelFileName = Path.GetFileName(path);

                var modelPath = Path.Combine(".media", "models", modelName, modelFileName);
                if (string.IsNullOrEmpty(modelPath))
                {
                    Debug.LogWarning($"Model path for {modelName} is empty");
                    continue;
                }

                XElement modelElement = new XElement("Model");
                modelElement.SetAttributeValue("name", modelName);
                modelElement.SetAttributeValue("source", modelPath);
                modelsElement.Add(modelElement);
            }
            if (modelsElement.HasElements)
            {
                root.Add(modelsElement);
            }
        }


        // METADATA
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
                Debug.LogWarning($"Failed to save overview: {e.Message}");
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
                Debug.LogWarning("Unknown media type: " + scene.Type);
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
            XElement elementNode = new XElement("Element");
            elementNode.SetAttributeValue("x", element.x.ToString());
            elementNode.SetAttributeValue("y", element.y.ToString());

            if (element is SceneElementArrow)
            {
                elementNode.SetAttributeValue("type", "directionarrow");

                SceneElementArrow arrow = (SceneElementArrow)element;
                elementNode.SetAttributeValue("distance", arrow.distance.ToString());
                elementNode.SetAttributeValue("rotation", arrow.rotation.ToString());
                elementNode.SetAttributeValue("action", arrow.action);
                elementNode.SetAttributeValue("color", arrow.color);
            }
            else if (element is SceneElementTextbox)
            {
                elementNode.SetAttributeValue("type", "textbox");

                SceneElementTextbox textbox = (SceneElementTextbox)element;
                elementNode.SetAttributeValue("distance", textbox.distance.ToString());
                elementNode.SetAttributeValue("icon", textbox.icon);
                elementNode.Add(textbox.text);
            }
            else if (element is SceneElementText)
            {
                elementNode.SetAttributeValue("type", "text");

                SceneElementText text = (SceneElementText)element;
                elementNode.SetAttributeValue("distance", text.distance.ToString());
                elementNode.SetAttributeValue("action", text.action);
                elementNode.Add(text.text);
            }

            sceneElement.Add(elementNode);
        }
        return sceneElement;
    }


}
