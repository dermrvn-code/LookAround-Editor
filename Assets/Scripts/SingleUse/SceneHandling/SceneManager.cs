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
    SpriteManager spriteManager;
    ModelManager modelManager;

    public Dictionary<string, Scene> sceneList = new Dictionary<string, Scene>();

    void Start()
    {
        sc = FindObjectOfType<SceneChanger>();
        textureManager = FindObjectOfType<TextureManager>();
        modelManager = FindObjectOfType<ModelManager>();
        spriteManager = FindObjectOfType<SpriteManager>();

        sc.ToMainScene();
    }

    List<string> texturePaths = new List<string>();
    Dictionary<int, string> spritePaths = new Dictionary<int, string>();
    Dictionary<string, string> modelPaths = new Dictionary<string, string>();
    public bool LoadSceneOverview(string sceneOverviewPath, Loader loadingBar, Action onComplete)
    {
        texturePaths.Clear();
        textureManager.ReleaseAllTextures();

        modelPaths = new Dictionary<string, string>();
        modelManager.UnloadAllModels();

        sceneList.Clear();
        if (!File.Exists(sceneOverviewPath)) Debug.LogWarning("The scene overview file does not exist: " + sceneOverviewPath);
        sceneOverview = XDocument.Load(sceneOverviewPath);

        try
        {
            LoadScenes(sceneOverviewPath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading scene overview: " + e.Message);
            return false;
        }

        LoadLogos(sceneOverviewPath);
        LoadModels(sceneOverviewPath);
        LoadSprites(sceneOverviewPath);

        int maxLoadingSteps = texturePaths.Count + modelPaths.Count + spritePaths.Count;

        loadingBar.OnFull(() =>
        {
            onComplete?.Invoke();
        });

        StartCoroutine(textureManager.LoadAllTextures(texturePaths,
        (texturePath) => // onProgress
        {
            loadingBar.IncreaseLoader(maxLoadingSteps, "Lädt Textur: " + Path.GetFileName(texturePath));
        }, () => // onComplete
        {
            Debug.Log("Textures preloaded!");
            onComplete?.Invoke();
        }));

        foreach (var spritePath in spritePaths)
        {
            spriteManager.LoadSprite(spritePath.Key, spritePath.Value);
            loadingBar.IncreaseLoader(maxLoadingSteps, "Lädt Sprite: " + Path.GetFileName(spritePath.Value));
            Debug.Log($"Loaded sprite: {spritePath.Value}");
        }

        foreach (var model in modelPaths)
        {
            string modelName = model.Key;
            string modelPath = model.Value;

            try
            {
                modelManager.LoadModel(modelPath, modelName, (GameObject obj, RenderTexture rt) =>
                {
                    loadingBar.IncreaseLoader(maxLoadingSteps, "Lädt Model: " + Path.GetFileName(modelPath));
                });
            }
            catch
            {
                InfoText.ShowInfo($"Model '{modelName}' konnte nicht geladen werden.");
            }
        }
        return true;
    }

    void LoadScenes(string sceneOverviewPath)
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
    }

    void LoadLogos(string sceneOverviewPath)
    {
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
                        spriteManager.LoadLogo(id, logoPath, backgroundColor);
                    }
                    else
                    {
                        Debug.LogWarning("Logo file does not exist: " + logoPath);
                    }
                }
            }
        }
    }

    void LoadSprites(string scenesOverviewPath)
    {
        var spritesList = sceneOverview.Root.Element("Sprites");
        if (spritesList != null)
        {
            var sprites = spritesList.Descendants("Sprite");
            foreach (var sprite in sprites)
            {
                string spriteSource = sprite.Attribute("source").Value;
                int index = int.Parse(sprite.Attribute("id").Value);

                string spritePath = Path.Combine(Path.GetDirectoryName(scenesOverviewPath), spriteSource);
                if (File.Exists(spritePath))
                {
                    spritePaths.Add(index, spritePath); ;
                }
                else
                {
                    Debug.LogWarning("Sprite file does not exist: " + spritePath);
                }
            }
        }
    }

    void LoadModels(string scenesOverviewPath)
    {
        var modelsList = sceneOverview.Root.Element("Models");
        if (modelsList != null)
        {
            var models = modelsList.Descendants("Model");
            foreach (var model in models)
            {
                string modelSource = model.Attribute("source").Value;
                string modelName = model.Attribute("name").Value;

                string modelPath = Path.Combine(Path.GetDirectoryName(scenesOverviewPath), modelSource);

                if (File.Exists(modelPath))
                {
                    modelPaths.Add(modelName, modelPath);
                    return;
                }
                Debug.LogWarning("Model file does not exist: " + modelPath);
            }
        }
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
                se = new SceneElementText(
                    text: text,
                    x: x, y: y,
                    distance: distance,
                    xRotationOffset: xRotationOffset,
                    action: action
                );
            }
            else if (elementType == "textbox")
            {
                string icon = element.Attribute("icon").Value;
                se = new SceneElementTextbox(
                    text: text, icon: icon,
                    x: x, y: y,
                    distance: distance,
                    xRotationOffset: xRotationOffset
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

                se = new SceneElementArrow(
                    x: x, y: y,
                    distance: distance,
                    xRotationOffset: xRotationOffset,
                    rotation: rotation,
                    color: color, action: action
                );

            }
            else if (elementType == "model")
            {
                string name = element.Attribute("name").Value;
                string action = element.Attribute("action").Value;

                se = new SceneElementModel(
                    modelName: name,
                    x: x, y: y,
                    distance: distance,
                    xRotationOffset: xRotationOffset,
                    action: action
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
