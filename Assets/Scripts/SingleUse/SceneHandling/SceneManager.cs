using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

public class SceneManager : MonoBehaviour
{

    SceneChanger sc;
    TextureManager textureManager;

    XDocument sceneOverview;
    LogoLoadingOverlay lolo;

    public Dictionary<string, Scene> sceneList = new Dictionary<string, Scene>();

    void Start()
    {
        sc = FindObjectOfType<SceneChanger>();
        textureManager = FindObjectOfType<TextureManager>();
        lolo = FindObjectOfType<LogoLoadingOverlay>();

        sc.ToMainScene();
    }

    List<string> texturePaths = new List<string>();
    public bool LoadSceneOverview(string sceneOverviewPath, Loader loadingBar, Action onComplete)
    {
        texturePaths.Clear();
        textureManager.ReleaseAllTextures();

        sceneList.Clear();
        if (!File.Exists(sceneOverviewPath)) Debug.LogWarning("The scene overview file does not exist: " + sceneOverviewPath);
        sceneOverview = XDocument.Load(sceneOverviewPath);

        try
        {
            var scenesList = sceneOverview.Root.Element("Scenes");
            var scenes = scenesList.Descendants("Scene");

            int counter = 0;
            foreach (var scene in scenes)
            {
                string scenePath = scene.Attribute("path").Value;
                string sceneName = scene.Attribute("name").Value;

                var startScene = scene.Attribute("startScene");
                bool isStartScene = false;
                if (startScene != null)
                {
                    if (startScene.Value.ToLower() == "true") isStartScene = true;
                }

                string sceneFolder = Path.GetDirectoryName(sceneOverviewPath);
                Scene s = LoadScene(sceneName, sceneFolder, scenePath, isStartScene);

                if (s.Type == Scene.MediaType.Photo)
                {
                    if (s.IsStartScene)
                    {
                        texturePaths.Insert(0, s.Source);
                    }
                    else
                    {
                        texturePaths.Add(s.Source);
                    }
                }

                counter++;
            }


            var logoList = sceneOverview.Root.Element("Logos");
            if (logoList != null)
            {
                var logos = logoList.Descendants("Logo");
                foreach (var logo in logos)
                {
                    string logoSource = logo.Attribute("source").Value;
                    string id_str = logo.Attribute("id").Value;
                    string backgroundColor = logo.Attribute("backgroundColor")?.Value ?? "";

                    if (int.TryParse(id_str, out int id))
                    {
                        string logoPath = Path.Combine(Path.GetDirectoryName(sceneOverviewPath), logoSource);
                        if (File.Exists(logoPath))
                        {
                            lolo.LoadLogo(id, logoPath, backgroundColor);
                        }
                        else
                        {
                            Debug.LogWarning("Logo file does not exist: " + logoPath);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading scene overview: " + e.Message);
            return false;
        }

        StartCoroutine(textureManager.LoadAllTextures(texturePaths, loadingBar, () =>
        {
            Debug.Log("Textures preloaded!");
            onComplete?.Invoke();
        }));
        return true;
    }

    Scene LoadScene(string sceneName, string mainFolder, string scenePath, bool isStartScene)
    {
        var sceneXML = XDocument.Load(mainFolder + "/" + scenePath);

        var sceneTag = sceneXML.Element("Scene");
        string type = sceneTag.Attribute("type").Value;
        string source = sceneTag.Attribute("source").Value;


        float xOffset = 0;
        float yOffset = 0;
        if (sceneTag.Attribute("xOffset") != null)
        {
            xOffset = float.Parse(sceneTag.Attribute("xOffset").Value);
        }
        if (sceneTag.Attribute("yOffset") != null)
        {
            yOffset = float.Parse(sceneTag.Attribute("yOffset").Value);
        }

        string sceneFolder = Path.GetDirectoryName(mainFolder + "/" + scenePath);
        source = Path.Combine(sceneFolder, source);


        var elements = sceneTag.Descendants("Element");

        var sceneElements = new Dictionary<int, SceneElement>();

        int idCounter = 0;
        foreach (var element in elements)
        {
            string elementType = element.Attribute("type").Value.ToLower();

            string text = element.Value.Trim();
            if (text == "")
            {
                text = "No Text given";
            }

            int x = int.Parse(element.Attribute("x").Value);
            int y = int.Parse(element.Attribute("y").Value);

            int distance = 10;
            if (element.Attribute("distance") != null)
            {
                distance = int.Parse(element.Attribute("distance").Value);
            }

            int xRotationOffset = 0;
            if (element.Attribute("xRotationOffset") != null)
            {
                xRotationOffset = int.Parse(element.Attribute("xRotationOffset").Value);
            }


            SceneElement se;
            if (elementType == "text")
            {
                string action = element.Attribute("action").Value;
                se = new SceneElement(
                        SceneElement.ElementType.Text,
                        text, x, y,
                        distance, xRotationOffset,
                        action: action
                    );
            }
            else if (elementType == "textbox")
            {
                string icon = element.Attribute("icon").Value;
                se = new SceneElement(
                        SceneElement.ElementType.Textbox,
                        text, x, y,
                        distance, xRotationOffset,
                        icon: icon
                    );
            }
            else if (elementType == "directionarrow")
            {
                string action = element.Attribute("action").Value;
                int rotation = int.Parse(element.Attribute("rotation").Value);

                string color = "";
                if (element.Attribute("color") != null)
                {
                    color = element.Attribute("color").Value;
                }

                se = new SceneElement(
                        SceneElement.ElementType.DirectionArrow,
                        text, x, y,
                        distance, xRotationOffset,
                        action: action, rotation: rotation, color: color
                    );

            }
            else
            {
                Debug.Log("Element doesnt match any type : " + elementType);
                se = null;
            }
            if (se != null)
            {
                se.id = idCounter;
                sceneElements.Add(idCounter, se);
            }
            idCounter++;
        }
        Scene sceneObj = new Scene(type == "video" ? Scene.MediaType.Video : Scene.MediaType.Photo, sceneName, source, sceneElements, isStartScene, xOffset, yOffset);

        Debug.Log("Scene loaded" + sceneName);
        sceneList.Add(sceneName, sceneObj);
        return sceneObj;
    }

    public Scene GetStartScene()
    {
        foreach (var scene in sceneList.Values)
        {
            if (scene.IsStartScene)
            {
                return scene;
            }
        }
        return null;
    }

    public void SetStartScene(string sceneName = "", string sceneNameAvoid = "")
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (sceneList.ContainsKey(sceneName))
            {
                foreach (var scene in sceneList.Values)
                {
                    scene.SetStartScene(false); // Reset all scenes
                }
                sceneList[sceneName].SetStartScene(true); // Set the specified scene as start scene
            }
            else
            {
                Debug.LogWarning("Scene not found: " + sceneName);
            }
        }
        else
        {
            foreach (var scene in sceneList.Values)
            {
                scene.SetStartScene(false); // Reset all scenes
            }

            sceneList.FirstOrDefault(x => x.Key != sceneNameAvoid).Value?.SetStartScene(true);
        }


    }


    void OnDestroy()
    {
        // Release all textures when done
        textureManager.ReleaseAllTextures();
    }
}
