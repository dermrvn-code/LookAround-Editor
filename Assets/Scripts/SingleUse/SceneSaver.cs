using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SceneSaver : MonoBehaviour
{

    SceneManager sceneManager;
    ProjectManager projectManager;
    void Start()
    {
        sceneManager = GetComponent<SceneManager>();
        projectManager = FindObjectOfType<ProjectManager>();
    }

    public void Save()
    {
        Parse();
    }

    void Parse()
    {
        foreach (var scene in sceneManager.sceneList.Values)
        {
            // if (scene.HasUnsavedChanges)
            // {
            Debug.Log($"Scene {scene.Name} has unsaved changes. Saving...");
            SaveScene(scene);
            // }
        }
    }

    string CopyMedium(string source, string destinationFolder)
    {
        string extension = Path.GetExtension(source);
        string destFile = "medium" + extension;

        string destinationPath = Path.Combine(destinationFolder, destFile);

        if (source == destinationPath && File.Exists(destinationPath))
        {
            return destFile;
        }
        Debug.Log("source == destFIle: " + (source == destFile) + ", source: " + source + ", destFile: " + destFile);

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

        string file = CopyMedium(scene.Source, sceneFolderPath);
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
