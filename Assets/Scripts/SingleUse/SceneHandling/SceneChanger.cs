using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class SceneChanger : MonoBehaviour
{
    public TMP_Text sceneNameText;
    public GameObject particlesGameobject;
    ParticleSystem particles;

    public MeshRenderer domeRenderer;

    public Material videoMaterial;
    public Material photoMaterial;

    public Texture mainScreenImage;

    public GameObject sceneElementsContainer;

    public VideoPlayer vp;
    SceneManager sm;
    InteractionHandler ih;
    PanelManager panelManager;
    TextureManager textureManager;

    public Scene currentScene;

    void Start()
    {
        sm = FindObjectOfType<SceneManager>();
        ih = FindObjectOfType<InteractionHandler>();
        textureManager = FindObjectOfType<TextureManager>();
        panelManager = FindObjectOfType<PanelManager>();

        // To prevent particles in the editor window
        particlesGameobject.SetActive(true);
        particles = particlesGameobject.GetComponent<ParticleSystem>();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ToMainScene()
    {
        photoMaterial.mainTexture = mainScreenImage;
        photoMaterial.mainTextureOffset = new Vector2(0, 0);

        SwitchToFoto();
    }

    public void ToStartScene(bool animate = true)
    {
        bool foundScene = false;
        foreach (var scene in sm.sceneList)
        {
            if (scene.Value.IsStartScene)
            {
                foundScene = true;
                if (animate)
                {
                    SwitchSceneAnimation(scene.Value);
                }
                else
                {
                    SwitchScene(scene.Value);
                }
            }
        }
        if (!foundScene) Debug.LogWarning("There is no start scene specified");
    }


    void SwitchToVideo()
    {
        domeRenderer.material = videoMaterial;
    }
    void SwitchToFoto()
    {
        domeRenderer.material = photoMaterial;
    }

    public void SwitchSceneAnimation(Scene scene)
    {
        if (scene == null || scene != currentScene)
        {
            TransitionParticles((sceneLoaded) =>
            {
                SwitchScene(scene, () =>
                {
                    sceneLoaded?.Invoke();
                });
            });
        }
    }

    public void SwitchScene(Scene scene, Action onLoaded = null)
    {
        if (scene == null || scene != currentScene)
        {
            currentScene = scene;
            LoadSceneElements(scene.SceneElements);
            if (ih != null) ih.updateElementsNextFrame = true;


            try
            {
                if (!File.Exists(scene.Source))
                {
                    Debug.LogWarning("Scene media " + scene.Source + " does not exist");
                    return;
                }


                if (scene.Type == Scene.MediaType.Video)
                {
                    SwitchToVideo();
                    videoMaterial.mainTextureOffset = new Vector2(scene.XOffset, scene.YOffset);
                    vp.url = scene.Source;
                    sceneNameText.text = scene.Name;
                    onLoaded?.Invoke();
                }
                else if (scene.Type == Scene.MediaType.Photo)
                {
                    StartCoroutine(textureManager.GetTexture(scene.Source, texture =>
                    {
                        photoMaterial.mainTexture = texture;
                        photoMaterial.mainTextureOffset = new Vector2(scene.XOffset, scene.YOffset);
                        SwitchToFoto();
                        sceneNameText.text = scene.Name;
                        onLoaded?.Invoke();
                    }));
                }
                else
                {
                    Debug.LogWarning("Scene type not supported");
                    return;
                }

                panelManager.CloseSidebar();
            }
            catch (Exception e)
            {
                Debug.LogError("Error while switching to scene " + scene.Name);
                Debug.LogError(e.Message);
            }
        }
    }



    public void LoadSceneElements(Dictionary<int, SceneElement> sceneElementsDict)
    {
        List<SceneElement> sceneElements = new List<SceneElement>(sceneElementsDict.Values);

        var children = new List<GameObject>();
        foreach (Transform child in sceneElementsContainer.transform) children.Add(child.gameObject);
        if (Application.isPlaying)
        {
            children.ForEach(child => Destroy(child));
        }
        else
        {
            children.ForEach(child => DestroyImmediate(child));
        }


        foreach (var sceneElement in sceneElements)
        {
            if (sceneElement.type == SceneElement.ElementType.Text)
            {
                LoadTextElement(sceneElement);
            }
            else if (sceneElement.type == SceneElement.ElementType.Textbox)
            {
                LoadTextboxElement(sceneElement);
            }
            else if (sceneElement.type == SceneElement.ElementType.DirectionArrow)
            {
                LoadDirectionArrow(sceneElement);
            }
        }
    }

    [SerializeField]
    TMP_Text textPrefab;
    public void LoadTextElement(SceneElement sceneElement)
    {
        var text = Instantiate(textPrefab, sceneElementsContainer.transform);
        text.name = sceneElement.text;
        text.text = sceneElement.text;
        DomePosition dp = text.GetComponent<DomePosition>();
        dp.position.x = sceneElement.x;
        dp.position.y = sceneElement.y;
        dp.distance = sceneElement.distance;
        Interactable interactable = text.GetComponent<Interactable>();
        interactable.OnInteract.AddListener(() =>
        {
            ActionParser(sceneElement.action);
        });
        text.gameObject.AddComponent<SceneElementHolder>().sceneElement = sceneElement;
    }

    [SerializeField]
    GameObject textboxPrefab;
    public Sprite info, warning, question, play;

    public void LoadTextboxElement(SceneElement sceneElement)
    {
        var text = Instantiate(textboxPrefab, sceneElementsContainer.transform);
        var textbox = text.GetComponentInChildren<TextBox>();
        textbox.SetText(sceneElement.text);
        textbox.SetColor(ColorUtility.TryParseHtmlString(sceneElement.color, out Color unityColor) ? unityColor : Color.white); textbox.iconName = sceneElement.icon;
        DomePosition dp = text.GetComponent<DomePosition>();
        dp.position.x = sceneElement.x;
        dp.position.y = sceneElement.y;
        dp.distance = sceneElement.distance;

        Sprite sprite;
        switch (sceneElement.icon)
        {
            case "info":
                sprite = info;
                break;

            case "warning":
                sprite = warning;
                break;

            case "question":
                sprite = question;
                break;

            case "play":
                sprite = play;
                break;

            default:
                sprite = null;
                break;
        }
        if (sprite != null)
        {
            textbox.SetIcon(sprite, sceneElement.icon);
        }
        text.AddComponent<SceneElementHolder>().sceneElement = sceneElement;
    }


    [SerializeField]
    GameObject arrowPrefab;
    public void LoadDirectionArrow(SceneElement sceneElement)
    {
        var arrow = Instantiate(arrowPrefab, sceneElementsContainer.transform);

        arrow.GetComponentInChildren<InteractableArrow>().SetRotation(sceneElement.rotation);

        DomePosition dp = arrow.GetComponent<DomePosition>();
        dp.position.x = sceneElement.x;
        dp.position.y = sceneElement.y;
        dp.distance = sceneElement.distance;

        InteractableArrow interactableArrow = arrow.GetComponent<InteractableArrow>();
        interactableArrow.OnInteract.AddListener(() =>
        {
            ActionParser(sceneElement.action);
        });

        Color color = ColorUtility.TryParseHtmlString(sceneElement.color, out Color unityColor) ? unityColor : Color.white;
        interactableArrow.color = color;

        arrow.AddComponent<SceneElementHolder>().sceneElement = sceneElement;

    }

    public static string[] actionTypes = { "toScene" };
    public void ActionParser(string action)
    {
        string pattern = @"toScene\((.*?)\)";
        Match match = Regex.Match(action, pattern);
        if (match.Success)
        {
            string sceneName = match.Groups[1].Value; // Extrahiere den Parameter aus der ersten Gruppe

            Scene scene = sm.sceneList[sceneName];

            if (scene != null)
            {
                SwitchSceneAnimation(scene);
            }
        }
    }

    public void TransitionParticles(Action<Action> sceneLoaded)
    {
        StartCoroutine(_StartParticles(sceneLoaded));
    }

    private IEnumerator _StartParticles(Action<Action> sceneLoaded)
    {
        particles.Play();
        yield return new WaitForSeconds(2f);
        sceneLoaded.Invoke(() =>
        {
            StartCoroutine(_StopParticles());
        });

    }

    private IEnumerator _StopParticles()
    {
        yield return new WaitForSeconds(0.5f);
        particles.Stop();
    }
}
