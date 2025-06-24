using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SidebarSettingsManager : MonoBehaviour
{
    public Camera cam;
    public Selector selector;
    public GameObject selectedElement;
    public GameObject sidebarContainer;

    public List<Pairs.PrefabPair> prefabs;
    private Dictionary<string, GameObject> prefabDictionary;
    public List<Pairs.SpritePair> sprites;
    private Dictionary<string, Sprite> spriteDictionary;

    PanelManager panelManager;
    SceneChanger sceneChanger;
    SceneManager sceneManager;
    ProjectManager projectManager;

    void Start()
    {
        panelManager = FindObjectOfType<PanelManager>();
        sceneChanger = FindObjectOfType<SceneChanger>();
        sceneManager = FindObjectOfType<SceneManager>();
        projectManager = FindObjectOfType<ProjectManager>();

        prefabDictionary = new Dictionary<string, GameObject>();
        foreach (var pair in prefabs)
            prefabDictionary[pair.value] = pair.prefab;

        spriteDictionary = new Dictionary<string, Sprite>();
        foreach (var pair in sprites)
            spriteDictionary[pair.value] = pair.sprite;
    }

    void ClearSidebar()
    {
        foreach (Transform child in sidebarContainer.transform)
            Destroy(child.gameObject);
    }

    void Select(GameObject target)
    {
        Highlight(target);
        ClearSidebar();
        panelManager.SidebarSetActive(true);

        if (target.TryGetComponent(out DomePosition domePosition))
            AddDomePosition(domePosition);

        if (target.TryGetComponent(out Interactable interactable))
            AddOnAction(interactable);

        if (target.TryGetComponent(out InteractableArrow arrow))
            AddArrow(arrow);

        if (target.TryGetComponent(out TextBox textbox))
            AddTextbox(textbox);

        if (target.TryGetComponent(out TMP_Text text))
            AddText(text);

        var contentSizeFitter = sidebarContainer.GetComponent<ContentSizeFitter>();
        var layoutGroup = sidebarContainer.GetComponent<VerticalLayoutGroup>();

        if (layoutGroup != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(sidebarContainer.GetComponent<RectTransform>());
        if (contentSizeFitter != null)
        {
            contentSizeFitter.enabled = false;
            contentSizeFitter.enabled = true;
        }
    }

    void Highlight(GameObject target)
    {
        selector.target = target;
    }

    public void AddArrow(InteractableArrow arrow)
    {
        var group = Instantiate(prefabDictionary["Group"], sidebarContainer.transform);
        var label = group.GetComponentInChildren<TMP_Text>();
        var elementsContainer = group.transform.Find("Elements");
        label.text = "Pfeil Einstellungen";

        var rotationInput = Instantiate(prefabDictionary["Slider"], elementsContainer).GetComponent<SliderAndInput>();
        rotationInput.Initialize(arrow.rotation, "Drehung", 0, 360);

        var colorPicker = Instantiate(prefabDictionary["ColorPicker"], elementsContainer).GetComponent<ColorPicker>();
        colorPicker.Initialize(arrow.color, "Hauptfarbe");

        var sceneElement = arrow.GetComponent<SceneElementHolder>()?.sceneElement;

        rotationInput.OnValueChanged.AddListener(value =>
        {
            arrow.SetRotation((int)value);
            if (sceneElement != null)
            {
                sceneElement.rotation = (int)value;
                UpdateSceneElement(sceneElement);
            }
        });

        colorPicker.OnColorChange.AddListener(color =>
        {
            arrow.SetColor(color);
            if (sceneElement != null)
            {
                sceneElement.color = $"#{ColorUtility.ToHtmlStringRGB(color)}";
                UpdateSceneElement(sceneElement);
            }
        });
    }

    public void AddTextbox(TextBox textbox)
    {
        var group = Instantiate(prefabDictionary["Group"], sidebarContainer.transform);
        var label = group.GetComponentInChildren<TMP_Text>();
        var elementsContainer = group.transform.Find("Elements");
        label.text = "Textbox Einstellungen";

        var sceneElement = textbox.GetComponent<SceneElementHolder>()?.sceneElement;

        var spriteSelector = Instantiate(prefabDictionary["SpriteSelector"], elementsContainer).GetComponent<SpriteSelector>();
        var spritePairs = new List<Pairs.SpritePair>();
        string[] selectedSprites = { "warning", "question", "info", "play" };
        foreach (var spriteName in selectedSprites)
            if (spriteDictionary.ContainsKey(spriteName))
                spritePairs.Add(new Pairs.SpritePair { value = spriteName, sprite = spriteDictionary[spriteName] });
        spriteSelector.Initialize(spritePairs.ToArray(), textbox.iconName);

        var textInput = Instantiate(prefabDictionary["TextArea"], elementsContainer).GetComponent<TextInput>();
        textInput.Initialize(textbox.textContent, "Text");

        var colorPicker = Instantiate(prefabDictionary["ColorPicker"], elementsContainer).GetComponent<ColorPicker>();
        colorPicker.Initialize(textbox.color, "Farbe");

        var toggleViewButton = Instantiate(prefabDictionary["Button"], elementsContainer).GetComponent<Button>();
        toggleViewButton.GetComponentInChildren<TMP_Text>().text = "Öffnen/Schließen";
        toggleViewButton.onClick.AddListener(() =>
        {
            if (textbox.isOpen) textbox.Unhighlight();
            else textbox.Highlight();
            Highlight(textbox.gameObject);
        });

        spriteSelector.OnElementSelected.AddListener(value =>
        {
            if (spriteDictionary.ContainsKey(value))
            {
                textbox.SetIcon(spriteDictionary[value], value);
                if (sceneElement != null)
                {
                    sceneElement.icon = value;
                    UpdateSceneElement(sceneElement);
                }
            }
        });

        textInput.OnValueChanged.AddListener(text =>
        {
            textbox.SetText(text);
            if (sceneElement != null)
            {
                sceneElement.text = text;
                UpdateSceneElement(sceneElement);
            }
        });

        colorPicker.OnColorChange.AddListener(color =>
        {
            textbox.SetColor(color);
            if (sceneElement != null)
            {
                sceneElement.color = $"#{ColorUtility.ToHtmlStringRGB(color)}";
                UpdateSceneElement(sceneElement);
            }
        });
    }

    public void AddText(TMP_Text text)
    {
        var group = Instantiate(prefabDictionary["Group"], sidebarContainer.transform);
        var label = group.GetComponentInChildren<TMP_Text>();
        var elementsContainer = group.transform.Find("Elements");
        label.text = "Text Einstellungen";

        var textInput = Instantiate(prefabDictionary["TextArea"], elementsContainer).GetComponent<TextInput>();
        textInput.Initialize(text.text, "Text");

        var colorPicker = Instantiate(prefabDictionary["ColorPicker"], elementsContainer).GetComponent<ColorPicker>();
        colorPicker.Initialize(text.color, "Farbe");

        var sceneElement = text.GetComponent<SceneElementHolder>()?.sceneElement;

        textInput.OnValueChanged.AddListener(newText =>
        {
            text.text = newText;
            if (sceneElement != null)
            {
                sceneElement.text = newText;
                UpdateSceneElement(sceneElement);
            }
        });

        colorPicker.OnColorChange.AddListener(color =>
        {
            text.color = color;
            if (sceneElement != null)
            {
                sceneElement.color = $"#{ColorUtility.ToHtmlStringRGB(color)}";
                UpdateSceneElement(sceneElement);
            }
        });
    }

    public void AddDomePosition(DomePosition domePosition)
    {
        var domePositionInput = Instantiate(prefabDictionary["DomePosition"], sidebarContainer.transform).GetComponent<DomePositionInput>();
        domePositionInput.Initialize((int)domePosition.position.x, (int)domePosition.position.y, domePosition.distance, (int)domePosition.xRotOffset);

        var sceneElement = domePosition.GetComponent<SceneElementHolder>()?.sceneElement;

        domePositionInput.OnInputChanged.AddListener((x, y, distance, tilt) =>
        {
            domePosition.position = new Vector2(x, y);
            domePosition.distance = distance;
            domePosition.xRotOffset = tilt;

            if (sceneElement != null)
            {
                sceneElement.x = x;
                sceneElement.y = y;
                sceneElement.distance = distance;
                sceneElement.xRotationOffset = tilt;
                UpdateSceneElement(sceneElement);
            }
        });
    }

    public void AddOnAction(Interactable interactable)
    {
        var sceneElement = interactable.GetComponent<SceneElementHolder>()?.sceneElement;
        string action = sceneElement?.action ?? "";

        var match = Regex.Match(action, @"toScene\((.*?)\)");
        if (match.Success)
        {
            string sceneName = match.Groups[1].Value;

            var group = Instantiate(prefabDictionary["Group"], sidebarContainer.transform);
            var label = group.GetComponentInChildren<TMP_Text>();
            var elementsContainer = group.transform.Find("Elements");
            label.text = "Interaktion";

            var dropdown = Instantiate(prefabDictionary["Dropdown"], elementsContainer).GetComponent<DropdownInput>();
            var actionOptions = new List<string> { "Keine" };
            foreach (var scene in sceneManager.sceneList.Values)
            {
                if (scene.Name != sceneChanger.currentScene.Name)
                {
                    actionOptions.Add(scene.Name);
                }
            }
            dropdown.Initialize(actionOptions, sceneName, "Gehe zu");

            var button = Instantiate(prefabDictionary["Button"], elementsContainer).GetComponent<Button>();
            button.GetComponentInChildren<TMP_Text>().text = "Aktion ausführen";

            dropdown.OnValueChanged.AddListener(value =>
            {
                interactable.OnInteract.RemoveAllListeners();
                if (value != "Keine")
                {
                    interactable.OnInteract.AddListener(() =>
                        sceneChanger.ActionParser(sceneElement.action)
                    );
                }
                if (sceneElement != null)
                {
                    sceneElement.action = value == "Keine" ? "" : $"toScene({value})";
                    UpdateSceneElement(sceneElement);
                }
            });

            button.onClick.AddListener(() => interactable.OnInteract.Invoke());
        }
    }

    public void UpdateSceneElement(SceneElement sceneElement)
    {
        if (sceneChanger.currentScene.SceneElements.ContainsKey(sceneElement.id))
        {
            sceneChanger.currentScene.SceneElements[sceneElement.id] = sceneElement;
            sceneChanger.currentScene.HasUnsavedChanges = true;
        }
    }

    public void Deselect(bool closeSidebar = false)
    {
        selector.target = null;
        if (closeSidebar)
            panelManager.CloseSidebar();
    }

    public void OpenSceneSettings(string sceneName)
    {
        ProcessIndicator.Show();
        panelManager.SwitchToScene();
        panelManager.SidebarSetActive(false);
        ClearSidebar();
        panelManager.SidebarSetActive(true);

        var sceneSettings = Instantiate(prefabDictionary["SceneSettings"], sidebarContainer.transform).GetComponent<SceneSettings>();

        if (sceneManager.sceneList.ContainsKey(sceneName))
        {
            var scene = sceneManager.sceneList[sceneName];
            Vector2 offset = SceneSettings.MapOffsetToDegree(scene.XOffset, scene.YOffset);

            sceneSettings.Initialize(sceneName, scene.IsStartScene, scene.Source, (int)offset.x, (int)offset.y);
        }
        ProcessIndicator.Hide();
    }
    string[] funnySceneNames = new string[]
    {
        "TatooineSundown",
        "DeathStarLounge",
        "JediCouncil",
        "EwokVillage",
        "DagobahSwamp",
        "HothHangar",
        "WookieeWorkshop",
        "SithTemple",
        "CantinaBreak",
        "MillenniumFalcon",
        "PodracerPit",
        "DroidDepot",
        "CloudCityView",
        "KesselRun",
        "YodaHut"
    };

    public void OpenCreateScene()
    {
        if (!projectManager.IsInProject())
        {
            InfoText.ShowInfo("Bitte erstelle oder lade ein Projekt, um Szenen zu erstellen.");
            return;
        }

        ProcessIndicator.Show();
        panelManager.SwitchToScene();
        panelManager.SidebarSetActive(false);
        ClearSidebar();
        panelManager.SidebarSetActive(true);

        var sceneSettings = Instantiate(prefabDictionary["SceneSettings"], sidebarContainer.transform).GetComponent<SceneSettings>();
        string sceneName = funnySceneNames[Random.Range(0, funnySceneNames.Length)];
        sceneSettings.Initialize(sceneName, sceneManager.sceneList.Count == 0, "");
        ProcessIndicator.Hide();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                Select(hit.collider.gameObject);
        }
    }
}
