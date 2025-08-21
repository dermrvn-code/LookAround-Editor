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
    SpriteManager spriteManager;
    PuzzleManager puzzleManager;
    void Start()
    {
        sceneManager = GetComponent<SceneManager>();
        projectManager = FindFirstObjectByType<ProjectManager>();
        modelManager = FindFirstObjectByType<ModelManager>();
        spriteManager = FindFirstObjectByType<SpriteManager>();
        puzzleManager = FindFirstObjectByType<PuzzleManager>();
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
                scene.HasUnsavedChanges = false;
            }
        }

        string mediaFolder = Path.Combine(projectManager.currentFolderPath, ".media");

        if (!Directory.Exists(mediaFolder))
        {
            Directory.CreateDirectory(mediaFolder);
        }

        // COPY OVER LOGOS
        bool logoUpdated = false;
        for (int i = 0; i < spriteManager.logos.Length; i++)
        {
            var logo = spriteManager.logos[i];

            if (string.IsNullOrEmpty(logo.spriteData.path) || logo.spriteData.texture == null)
            {
                continue;
            }
            string dest = CopyMedium(logo.spriteData.path, Path.Combine(mediaFolder, "logos"), $"logo_{i}");
            logoUpdated = dest != null;
        }

        // COPY OVER SPRITES
        bool spritesUpdated = false;
        for (int i = 0; i < spriteManager.sceneSprites.Length; i++)
        {
            var spriteData = spriteManager.sceneSprites[i];
            if (string.IsNullOrEmpty(spriteData.path) || spriteData.texture == null)
            {
                continue;
            }
            string dest = CopyMedium(spriteData.path, Path.Combine(mediaFolder, "sprites"), $"sprite_{i}");
            spritesUpdated = dest != null;
        }

        // COPY OVER MODELS
        bool modelsUpdated = false;
        foreach (var modelName in modelManager.GetModelNames())
        {
            var modelPath = modelManager.GetModelPath(modelName);

            if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
            {
                Debug.LogWarning($"Model path for {modelName} is empty or does not exist.");
                continue;
            }

            string fileName = Path.GetFileName(modelPath);
            string modelSourceFolder = Path.GetDirectoryName(modelPath);
            string destModelFolder = Path.Combine(mediaFolder, "models", modelName, fileName);

            modelsUpdated = DirectoryCopyRecurse(modelSourceFolder, destModelFolder);
        }

        bool wasDeleted = DeleteUnusedScenes();


        SaveSceneOverview(sceneManager.sceneList.Values.ToList());
        projectManager.unsavedChanges = false;

        if (!wasDeleted && !changedScene && !logoUpdated && !modelsUpdated && !spritesUpdated)
        {
            InfoText.ShowInfo("Keine Änderungen vorhanden");
        }
        else
        {
            InfoText.ShowInfo("Projekt gespeichert in " + projectManager.currentFolderPath);
        }
    }

    bool DirectoryCopyRecurse(string sourcePath, string destPath)
    {
        if (sourcePath == destPath)
        {
            return false; // No need to copy if source and destination are the same
        }

        if (File.Exists(destPath))
        {
            destPath = Path.GetDirectoryName(destPath);
        }
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
        return true;
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
            return null; // No need to copy if the file already exists at the destination
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
            string destinationPath = Path.Combine(sceneFolderPath, scene.Name + Path.GetExtension(scene.Source));
            if (scene.Source != destinationPath)
            {
                Debug.LogWarning("Failed to copy scene file: " + scene.Name);
                return;
            }
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
        if (spriteManager.logos.Length > 0)
        {
            XElement logosElement = new XElement("Logos");

            for (int i = 0; i < spriteManager.logos.Length; i++)
            {
                var logo = spriteManager.logos[i];
                if (string.IsNullOrEmpty(logo.spriteData.path))
                {
                    continue;
                }
                string path = Path.Combine(".media", "logos", "logo_" + i + Path.GetExtension(logo.spriteData.path));
                var el = MediaBuilder("Logo", path, id: i.ToString());
                if (el != null)
                {
                    logosElement.Add(el);
                }
            }
            if (logosElement.HasElements)
            {
                root.Add(logosElement);
            }
        }

        // SPRITES
        if (spriteManager.sceneSprites.Length > 0)
        {
            XElement spritesElement = new XElement("Sprites");

            for (int i = 0; i < spriteManager.sceneSprites.Length; i++)
            {
                var spriteData = spriteManager.sceneSprites[i];
                if (string.IsNullOrEmpty(spriteData.path))
                {
                    continue;
                }
                string path = Path.Combine(".media", "sprites", "sprite_" + i + Path.GetExtension(spriteData.path));
                var el = MediaBuilder("Sprite", path, id: i.ToString());
                if (el != null)
                {
                    spritesElement.Add(el);
                }
            }
            if (spritesElement.HasElements)
            {
                root.Add(spritesElement);
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
                var el = MediaBuilder("Model", modelPath, modelName);
                if (el != null)
                {
                    modelsElement.Add(el);
                }
            }
            if (modelsElement.HasElements)
            {
                root.Add(modelsElement);
            }
        }

        if (puzzleManager.isEnabled)
        {
            XElement puzzleElement = new XElement("Minigame");
            puzzleElement.SetAttributeValue("name", "puzzle");
            puzzleElement.SetAttributeValue("sprite", puzzleManager.mainTexId.ToString());
            puzzleElement.SetAttributeValue("pieces", puzzleManager.totalPieces.ToString());
            puzzleElement.SetAttributeValue("finished", puzzleManager.finishedAction);
            root.Add(puzzleElement);
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

    XElement MediaBuilder(string tagName, string source, string name = "", string id = "")
    {
        if (string.IsNullOrEmpty(source))
        {
            Debug.LogWarning($"Model path for {(name == "" ? name : id)} is empty");
            return null;
        }

        XElement modelElement = new XElement(tagName);
        if (name != "")
        {
            modelElement.SetAttributeValue("name", name);
        }
        if (id != "")
        {
            modelElement.SetAttributeValue("id", id);
        }
        modelElement.SetAttributeValue("source", source);
        return modelElement;
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

        string sourcePath;
        if (!string.IsNullOrEmpty(newSource))
        {
            sourcePath = newSource;
        }
        else
        {
            sourcePath = scene.Source;
        }
        string relativePath = Path.GetRelativePath(Path.Combine(projectManager.currentFolderPath, scene.Name), sourcePath);
        sceneElement.SetAttributeValue("source", relativePath);
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
        Debug.Log($"Parsing {sceneElements.Count} scene elements for scene: {sceneElement.Attribute("name")?.Value}");
        foreach (var element in sceneElements.Values)
        {
            XElement elementNode = new XElement("Element");
            elementNode.SetAttributeValue("x", element.x.ToString());
            elementNode.SetAttributeValue("y", element.y.ToString());
            elementNode.SetAttributeValue("distance", element.distance.ToString());
            elementNode.SetAttributeValue("xRotationOffset", element.xRotationOffset.ToString());

            if (element is SceneElementArrow)
            {
                elementNode.SetAttributeValue("type", "directionarrow");

                SceneElementArrow arrow = (SceneElementArrow)element;
                elementNode.SetAttributeValue("rotation", arrow.rotation.ToString());
                elementNode.SetAttributeValue("action", arrow.action);
                elementNode.SetAttributeValue("color", arrow.color);
                Debug.Log($"Saving SceneElementArrow: {arrow.action} at position ({arrow.x}, {arrow.y}) with distance {arrow.distance}");
            }
            else if (element is SceneElementTextbox)
            {
                elementNode.SetAttributeValue("type", "textbox");

                SceneElementTextbox textbox = (SceneElementTextbox)element;
                elementNode.SetAttributeValue("icon", textbox.icon);
                elementNode.Add(textbox.text);
                Debug.Log($"Saving SceneElementTextbox: {textbox.text} at position ({textbox.x}, {textbox.y}) with distance {textbox.distance}");
            }
            else if (element is SceneElementText)
            {
                elementNode.SetAttributeValue("type", "text");

                SceneElementText text = (SceneElementText)element;
                elementNode.SetAttributeValue("action", text.action);
                elementNode.Add(text.text);
                Debug.Log($"Saving SceneElementText: {text.text} at position ({text.x}, {text.y}) with distance {text.distance}");
            }
            else if (element is SceneElementModel)
            {
                elementNode.SetAttributeValue("type", "model");

                SceneElementModel model = (SceneElementModel)element;
                elementNode.SetAttributeValue("name", model.modelName);
                elementNode.SetAttributeValue("scale", model.scale.ToString());
                elementNode.SetAttributeValue("rotationX", model.xRotation.ToString());
                elementNode.SetAttributeValue("rotationY", model.yRotation.ToString());
                elementNode.SetAttributeValue("rotationZ", model.zRotation.ToString());

                elementNode.SetAttributeValue("action", model.action);
                Debug.Log($"Saving SceneElementModel: {model.modelName} at position ({model.x}, {model.y}) with distance {model.distance}");
            }
            else if (element is SceneElementSprite)
            {
                elementNode.SetAttributeValue("type", "sprite");

                SceneElementSprite sprite = (SceneElementSprite)element;
                elementNode.SetAttributeValue("action", sprite.action);
                elementNode.SetAttributeValue("id", sprite.index);
                Debug.Log($"Saving SceneElementSprite: ID {sprite.index} at position ({sprite.x}, {sprite.y}) with distance {sprite.distance}");
            }
            else if (element is SceneElementPuzzle)
            {
                elementNode.SetAttributeValue("type", "puzzlepiece");

                SceneElementPuzzle sprite = (SceneElementPuzzle)element;
                Debug.Log($"Saving PuzzlePiece at position ({sprite.x}, {sprite.y}) with distance {sprite.distance}");
            }
            else
            {
                Debug.Log("SceneElement of unknown type found: " + element.GetType());
            }

            sceneElement.Add(elementNode);
        }
        return sceneElement;
    }


}
