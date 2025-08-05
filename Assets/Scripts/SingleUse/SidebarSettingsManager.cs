using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
    ModelManager modelManager;
    SpriteManager spriteManager;


    public bool notAutomaticSave = false;

    void Start()
    {
        panelManager = FindObjectOfType<PanelManager>();
        sceneChanger = FindObjectOfType<SceneChanger>();
        sceneManager = FindObjectOfType<SceneManager>();
        projectManager = FindObjectOfType<ProjectManager>();
        modelManager = FindObjectOfType<ModelManager>();
        spriteManager = FindObjectOfType<SpriteManager>();

        prefabDictionary = new Dictionary<string, GameObject>();
        foreach (var pair in prefabs)
            prefabDictionary[pair.value] = pair.prefab;

        spriteDictionary = new Dictionary<string, Sprite>();
        foreach (var pair in sprites)
            spriteDictionary[pair.value] = pair.sprite;
    }

    public void ClearSidebar(UnityAction onCleared)
    {
        if (notAutomaticSave)
        {
            Dialog.ShowDialogConfirm("Möchtest du das wirklich schließen?", () =>
            {
                notAutomaticSave = false;
                foreach (Transform child in sidebarContainer.transform)
                    Destroy(child.gameObject);

                onCleared?.Invoke();
            });
            return;
        }
        foreach (Transform child in sidebarContainer.transform)
            Destroy(child.gameObject);
        onCleared?.Invoke();
    }

    void Select(GameObject target)
    {
        Highlight(target);
        ClearSidebar(() =>
        {
            notAutomaticSave = false;
            panelManager.SidebarSetActive(true);

            if (target.TryGetComponent(out DomePosition domePosition))
                AddDomePosition(domePosition);

            if (target.TryGetComponent(out ModelTransform modelTransform))
                AddModelTransform(modelTransform);

            if (target.TryGetComponent(out Interactable interactable))
                AddOnAction(interactable);

            if (target.TryGetComponent(out InteractableArrow arrow))
                AddArrow(arrow);

            if (target.TryGetComponent(out TextBox textbox))
                AddTextbox(textbox);

            if (target.TryGetComponent(out TMP_Text text))
                AddText(text);

            if (target.TryGetComponent(out InteractableModel model))
                AddModel(model);

            if (target.TryGetComponent(out InteractableSprite sprite))
                AddSprite(sprite);

            if (target.TryGetComponent(out SceneElementHolder holder)) // always true, as functions checks target for SceneElementHolder
                AddDeleteElement(holder);                              // maybe add other check later, it needed, for now every element can be deleted

            ReloadLayout();
        });
    }

    void ReloadLayout()
    {
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

    public void AddDeleteElement(SceneElementHolder holder)
    {
        var group = Instantiate(prefabDictionary["Group"], sidebarContainer.transform);
        var label = group.GetComponentInChildren<TMP_Text>();
        var elementsContainer = group.transform.Find("Elements");
        label.text = "Element Einstellungen";

        var deleteButton = Instantiate(prefabDictionary["Button"], elementsContainer).GetComponent<Button>();
        deleteButton.GetComponentInChildren<TMP_Text>().text = "Löschen";
        deleteButton.GetComponent<Image>().color = new Color(1f, 0.5f, 0.5f, 1f); // light red
        deleteButton.onClick.AddListener(() =>
        {
            Dialog.ShowDialogConfirm("Möchtest du das Element wirklich löschen?", () =>
            {
                if (holder.TryGetComponent(out InteractableModel interactableModel))
                {
                    string modelName = interactableModel.elementContainer.transform.childCount > 0
                        ? interactableModel.elementContainer.transform.GetChild(0).gameObject.name
                        : "";
                    modelManager.HideModel(modelName);
                }
                Destroy(holder.gameObject);
                sceneChanger.currentScene.SceneElements.Remove(holder.sceneElement.list_id);
                Deselect();
                panelManager.CloseSidebar();
            });
        });
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

        SceneElementArrow sceneElement = (SceneElementArrow)arrow.GetComponent<SceneElementHolder>()?.sceneElement;

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

        SceneElementTextbox sceneElement = (SceneElementTextbox)textbox.GetComponent<SceneElementHolder>()?.sceneElement;

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

        SceneElementText sceneElement = (SceneElementText)text.GetComponent<SceneElementHolder>()?.sceneElement;

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

    public void AddModel(InteractableModel interactableModel)
    {
        GameObject gameObject = interactableModel.elementContainer.transform.childCount > 0
            ? interactableModel.elementContainer.transform.GetChild(0).gameObject
            : null;

        if (gameObject == null)
        {
            Debug.LogError("InteractableModel has no model assigned.");
            return;
        }

        SceneElementModel sceneElement = (SceneElementModel)interactableModel.GetComponent<SceneElementHolder>()?.sceneElement;


        var modelSelection = Instantiate(prefabDictionary["SpriteSelector"], sidebarContainer.transform).GetComponent<SpriteSelector>();

        var selectedModelName = gameObject.name;
        var allModels = modelManager.GetAllModels();
        var spritePairs = new Pairs.SpritePair[allModels.Count];

        int index = 0;
        for (int i = 0; i < allModels.Count; i++)
        {
            var model = allModels.ElementAt(i);

            if (model.name != selectedModelName && model.used)
            {
                continue;
            }

            var texture = model.preview;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            spritePairs[index] = new Pairs.SpritePair { value = model.name, sprite = sprite };
            index++;
        }

        modelSelection.Initialize(spritePairs, selectedModelName, "Model-Auswahl");


        modelSelection.OnElementSelected.AddListener(value =>
        {
            string modelName = interactableModel.elementContainer.transform.childCount > 0
                ? interactableModel.elementContainer.transform.GetChild(0).gameObject.name
                : "null";

            modelManager.SwitchModel(interactableModel.gameObject, modelName, value);

            sceneElement.modelName = value;
            UpdateSceneElement(sceneElement);
        });
    }

    public void AddSprite(InteractableSprite sprite)
    {
        SceneElementSprite sceneElement = (SceneElementSprite)sprite.GetComponent<SceneElementHolder>()?.sceneElement;


        var group = Instantiate(prefabDictionary["Group"], sidebarContainer.transform);
        var label = group.GetComponentInChildren<TMP_Text>();
        var elementsContainer = group.transform.Find("Elements");
        label.text = "Sprite Einstellungen";

        var spriteSelector = Instantiate(prefabDictionary["SpriteSelector"], elementsContainer).GetComponent<SpriteSelector>();

        var selectedSprite = (sprite.id + 1).ToString();

        var allSprites = spriteManager.GetSceneSprites();

        var spritePairs = new Pairs.SpritePair[allSprites.Length];

        for (int i = 0; i < allSprites.Length; i++)
        {
            var currSprite = allSprites[i];
            var displaySprite = currSprite.texture != null
                ? Sprite.Create(currSprite.texture, new Rect(0, 0, currSprite.texture.width, currSprite.texture.height), new Vector2(0.5f, 0.5f))
                : null;

            if (displaySprite == null) continue;

            spritePairs[i] = new Pairs.SpritePair { value = (i + 1).ToString(), sprite = displaySprite };
        }

        spriteSelector.Initialize(spritePairs, selectedSprite, "Sprite-Auswahl");


        spriteSelector.OnElementSelected.AddListener(value =>
        {
            int id = int.Parse(value) - 1;

            string newPath = spriteManager.GetSpritePaths()[id];
            sprite.SetImage(spriteManager.GetSprite(id));
            sprite.id = id;

            sceneElement.index = id;
            sceneElement.path = newPath;
            UpdateSceneElement(sceneElement);
        });
    }

    public void AddDomePosition(DomePosition domePosition)
    {
        bool tiltEnabled = true;
        if (domePosition.TryGetComponent(out ModelTransform _))
        {
            tiltEnabled = false;
        }

        var domePositionInput = Instantiate(prefabDictionary["DomePosition"], sidebarContainer.transform).GetComponent<DomePositionInput>();
        domePositionInput.Initialize((int)domePosition.position.x, (int)domePosition.position.y, domePosition.distance, (int)domePosition.xRotOffset, tiltEnabled: tiltEnabled);

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

    public void AddModelTransform(ModelTransform modelTransform)
    {
        var modelTransformInput = Instantiate(prefabDictionary["ModelTransform"], sidebarContainer.transform).GetComponent<ModelTransformInput>();
        modelTransformInput.Initialize((int)modelTransform.rotation.x, (int)modelTransform.rotation.y, (int)modelTransform.rotation.z, (int)modelTransform.scale);

        var sceneElement = (SceneElementModel)modelTransform.GetComponent<SceneElementHolder>()?.sceneElement;

        modelTransformInput.OnInputChanged.AddListener((x, y, z, scale) =>
        {
            modelTransform.rotation = new Vector3(x, y, z);
            modelTransform.scale = scale;

            if (sceneElement != null)
            {
                sceneElement.xRotation = x;
                sceneElement.yRotation = y;
                sceneElement.zRotation = z;
                sceneElement.scale = scale;
                UpdateSceneElement(sceneElement);
            }
        });
    }

    public void AddOnAction(Interactable interactable)
    {
        var sceneElement = interactable.GetComponent<SceneElementHolder>()?.sceneElement;

        string action = sceneElement?.action ?? "";

        string pattern = @"toScene\(([^,]*?)(?:,(-?\d))*\)";
        Match match = Regex.Match(action, pattern);
        if (match.Success)
        {
            string sceneName = match.Groups[1].Value;

            int animationIndex = -1; // -1 = particle, 0,1,2,... = logo index
            if (match.Groups.Count > 2 && match.Groups[2].Success)
            {
                int.TryParse(match.Groups[2].Value.Trim(), out animationIndex);
            }

            ActionToScene(interactable, sceneElement, sceneName, animationIndex);
        }
    }

    public void ActionToScene(Interactable interactable, SceneElement sceneElement, string sceneName, int animationIndex)
    {
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

        var spriteSelector = Instantiate(prefabDictionary["SpriteSelector"], elementsContainer).GetComponent<SpriteSelector>();


        Sprite[] logos = new Sprite[3];
        for (int i = 0; i < spriteManager.logos.Length; i++)
        {
            var logo = spriteManager.logos[i].spriteData.texture;
            if (logo != null)
            {
                logos[i] = Sprite.Create(logo, new Rect(0, 0, logo.width, logo.height), new Vector2(0.5f, 0.5f));
            }
        }

        var spritePairs = new Pairs.SpritePair[4];
        spritePairs[0] = new Pairs.SpritePair { value = "-1", sprite = spriteDictionary["smoke"] };
        for (int i = 1; i < spritePairs.Length; i++)
        {
            var logo = logos[i - 1];
            if (logo != null)
            {
                spritePairs[i] = new Pairs.SpritePair { value = (i - 1).ToString(), sprite = logo };
            }
        }

        string selectedValue = animationIndex.ToString();
        if (animationIndex >= 1 && animationIndex < spritePairs.Length)
        {
            selectedValue = animationIndex.ToString();
        }
        spriteSelector.Initialize(spritePairs, selectedValue, "Übergang");

        var button = Instantiate(prefabDictionary["Button"], elementsContainer).GetComponent<Button>();
        button.GetComponentInChildren<TMP_Text>().text = "Aktion ausführen";

        dropdown.OnValueChanged.AddListener(value =>
        {
            UpdateToScene(sceneElement, interactable, dropdown, spriteSelector);
        });

        spriteSelector.OnElementSelected.AddListener(value =>
        {
            UpdateToScene(sceneElement, interactable, dropdown, spriteSelector);
        });

        button.onClick.AddListener(() => interactable.OnInteract.Invoke());
    }

    void UpdateToScene(SceneElement sceneElement, Interactable interactable, DropdownInput sceneInput, SpriteSelector animIndexInput)
    {
        if (sceneElement != null)
        {
            string newAction = "";
            if (sceneInput.value.ToLower() != "keine" && !string.IsNullOrEmpty(sceneInput.value.ToLower()))
            {
                newAction = UpdateAction(sceneElement.action, "toScene", new string[] { sceneInput.value, animIndexInput.value });
            }
            sceneElement.action = newAction;
            UpdateSceneElement(sceneElement);
        }

        interactable.OnInteract.RemoveAllListeners();
        if (sceneInput.value.ToLower() != "keine" && !string.IsNullOrEmpty(sceneInput.value.ToLower()))
        {
            interactable.OnInteract.AddListener(() =>
                sceneChanger.ActionParser(sceneElement.action)
            );
        }
    }

    public string UpdateAction(string action, string function = "", string[] parameters = null)
    {
        string newFunctionName = function;
        string[] newParameters = parameters ?? new string[0];

        string pattern = @"([a-zA-Z_]+)\((?:(?:(?:([^,()]*),)*)|(?:(?:([^,()]*),)*([^,()]){1}))\)";
        Match match = Regex.Match(action, pattern);

        if (match.Success)
        {
            // Extract old function and params
            string functionName = match.Groups[1].Value;

            List<string> extractedParams = new List<string>();
            for (int i = 0; i < match.Groups.Count - 2; i++)
            {
                var paramMatch = match.Groups[i + 2];
                if (paramMatch.Success && !string.IsNullOrEmpty(paramMatch.Value))
                {
                    string value = paramMatch.Value.Trim();
                    extractedParams.Add(value);
                }
            }

            // Override with with new values
            if (!string.IsNullOrEmpty(functionName))
            {
                newFunctionName = functionName;
            }


            for (int i = 0; i < newParameters.Length; i++)
            {
                if (newParameters[i] == "" && i < extractedParams.Count)
                {
                    newParameters[i] = extractedParams[i].Trim();
                }
            }


        }
        if (newFunctionName == "")
        {
            return "";
        }
        return $"{newFunctionName}({string.Join(",", newParameters)})";

    }

    public void UpdateSceneElement(SceneElement sceneElement)
    {
        if (sceneChanger.currentScene.SceneElements.ContainsKey(sceneElement.list_id))
        {
            sceneChanger.currentScene.SceneElements[sceneElement.list_id] = sceneElement;
            sceneChanger.currentScene.HasUnsavedChanges = true;
            projectManager.unsavedChanges = true;
        }
        else
        {
            Debug.LogError("SceneElement with ID " + sceneElement.list_id + " not found in current scene.");
            return;
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
        notAutomaticSave = true;
        ProcessIndicator.Show();
        panelManager.SwitchToScene();
        ClearSidebar(() =>
        {
            panelManager.SidebarSetActive(true);

            var sceneSettings = Instantiate(prefabDictionary["SceneSettings"], sidebarContainer.transform).GetComponent<SceneSettings>();

            if (sceneManager.sceneList.ContainsKey(sceneName))
            {
                var scene = sceneManager.sceneList[sceneName];
                Vector2 offset = SceneSettings.MapOffsetToDegree(scene.XOffset, scene.YOffset);

                sceneSettings.Initialize(sceneName, scene.IsStartScene, scene.Source, (int)offset.x, (int)offset.y);
            }
            ReloadLayout();
            ProcessIndicator.Hide();
        });
    }

    public void OpenWorldSettings(bool newWorld = false)
    {
        ProcessIndicator.Show();
        panelManager.SwitchToScene();
        ClearSidebar(() =>
        {
            notAutomaticSave = true;
            panelManager.SidebarSetActive(true);

            var worldSettings = Instantiate(prefabDictionary["WorldSettings"], sidebarContainer.transform).GetComponent<WorldSettings>();
            worldSettings.newWorld = newWorld;

            if (!newWorld)
            {
                worldSettings.Initialize(
                    projectManager.currentProjectName,
                    projectManager.currentAuthorName,
                    projectManager.currentProjectDescription,
                    newWorld
                );
            }
            ReloadLayout();
            ProcessIndicator.Hide();
        });
    }

    public void OpenAppSettings()
    {
        ProcessIndicator.Show();
        ClearSidebar(() =>
        {
            notAutomaticSave = true;
            panelManager.SidebarSetActive(true);

            var appSettings = Instantiate(prefabDictionary["AppSettings"], sidebarContainer.transform).GetComponent<AppSettings>();

            appSettings.Initialize(projectManager.ProjectsPath);

            ReloadLayout();
            ProcessIndicator.Hide();
        });
    }

    public void OpenMediaSettings()
    {
        ProcessIndicator.Show();
        panelManager.SwitchToScene();
        ClearSidebar(() =>
        {
            notAutomaticSave = true;
            panelManager.SidebarSetActive(true);

            var mediaSettings = Instantiate(prefabDictionary["MediaSettings"], sidebarContainer.transform).GetComponent<MediaSettings>();

            mediaSettings.Initialize(
                spriteManager.GetLogoPaths(),
                spriteManager.GetSpritePaths(),
                modelManager.GetModelNames()
            );

            ReloadLayout();
            ProcessIndicator.Hide();
        });
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
        ClearSidebar(() =>
        {
            notAutomaticSave = true;
            panelManager.SidebarSetActive(true);

            var sceneSettings = Instantiate(prefabDictionary["SceneSettings"], sidebarContainer.transform).GetComponent<SceneSettings>();
            string sceneName = funnySceneNames[Random.Range(0, funnySceneNames.Length)];
            sceneSettings.Initialize(sceneName, sceneManager.sceneList.Count == 0, "");
            ProcessIndicator.Hide();
        });
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
            {
                GameObject target = hit.collider.gameObject;

                if (target.TryGetComponent(out SceneElementHolder _))
                {
                    Select(target);
                }
                else
                {
                    var parent = target.transform.parent;
                    while (parent != null && !parent.TryGetComponent<SceneElementHolder>(out _))
                    {
                        parent = parent.parent;
                    }
                    if (parent != null)
                    {
                        Select(parent.gameObject);
                    }
                }
            }

        }
    }
}
