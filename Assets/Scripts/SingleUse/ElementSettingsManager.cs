using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementsSettingManager : MonoBehaviour
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
    void Start()
    {
        panelManager = FindObjectOfType<PanelManager>();
        sceneChanger = FindObjectOfType<SceneChanger>();
        sceneManager = FindObjectOfType<SceneManager>();

        foreach (Pairs.PrefabPair pair in prefabs)
        {
            if (prefabDictionary == null)
            {
                prefabDictionary = new Dictionary<string, GameObject>();
            }
            prefabDictionary[pair.value] = pair.prefab;
        }

        foreach (Pairs.SpritePair pair in sprites)
        {
            if (spriteDictionary == null)
            {
                spriteDictionary = new Dictionary<string, Sprite>();
            }
            spriteDictionary[pair.value] = pair.sprite;
        }
    }

    void Select(GameObject target)
    {
        Highlight(target);
        foreach (Transform child in sidebarContainer.transform)
        {
            Destroy(child.gameObject);
        }

        var domePosition = target.GetComponent<DomePosition>();
        if (domePosition != null)
        {
            AddDomePosition(domePosition);
        }

        var interactable = target.GetComponent<Interactable>();
        if (interactable != null)
        {
            AddOnAction(interactable);
        }

        var arrow = target.GetComponent<InteractableArrow>();
        if (arrow != null)
        {
            AddArrow(arrow);
        }

        var textbox = target.GetComponent<TextBox>();
        if (textbox != null)
        {
            AddTextbox(textbox);
        }

        var text = target.GetComponent<TMP_Text>();
        if (text != null)
        {
            AddText(text);
        }


        // Force update the sidebar layout
        var contentSizeFitter = sidebarContainer.GetComponent<ContentSizeFitter>();
        var layoutGroup = sidebarContainer.GetComponent<VerticalLayoutGroup>();

        if (layoutGroup != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(sidebarContainer.GetComponent<RectTransform>());
        }
        if (contentSizeFitter != null)
        {
            // Toggle to force update
            contentSizeFitter.enabled = false;
            contentSizeFitter.enabled = true;
        }

    }

    void Highlight(GameObject target)
    {
        selector.target = target;
        panelManager.SidebarSetActive(true, target);
    }

    public void AddArrow(InteractableArrow arrow)
    {
        var group = Instantiate(prefabDictionary["Group"], sidebarContainer.transform);
        var label = group.GetComponentInChildren<TMP_Text>();
        var elementsContainer = group.transform.Find("Elements");
        label.text = "Pfeil Einstellungen";

        var rotationInput = Instantiate(prefabDictionary["Slider"], elementsContainer.transform).GetComponent<SliderAndInput>();
        rotationInput.Initialize(arrow.rotation, "Drehung", 0, 360);

        var colorPicker = Instantiate(prefabDictionary["ColorPicker"], elementsContainer.transform).GetComponent<ColorPicker>();
        colorPicker.Initialize(arrow.color, "Hauptfarbe");


        var sceneElementHolder = arrow.GetComponent<SceneElementHolder>();
        var sceneElement = sceneElementHolder.sceneElement;

        rotationInput.OnValueChanged.AddListener((value) =>
        {
            arrow.SetRotation((int)value);
            if (sceneElement != null)
            {
                sceneElement.rotation = (int)value;

                UpdateSceneElement(sceneElement);
            }
        });

        colorPicker.OnColorChange.AddListener((color) =>
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

        var sceneElementHolder = textbox.GetComponent<SceneElementHolder>();
        SceneElement sceneElement = sceneElementHolder.sceneElement;

        var spriteSelector = Instantiate(prefabDictionary["SpriteSelector"], elementsContainer.transform).GetComponent<SpriteSelector>();

        List<Pairs.SpritePair> spritePairs = new List<Pairs.SpritePair>();
        string[] selectedSprites = { "warning", "question", "info", "play" };

        foreach (string spriteName in selectedSprites)
        {
            if (spriteDictionary.ContainsKey(spriteName))
            {
                spritePairs.Add(new Pairs.SpritePair { value = spriteName, sprite = spriteDictionary[spriteName] });
            }
        }
        spriteSelector.Initialize(spritePairs.ToArray(), textbox.iconName);

        var textInput = Instantiate(prefabDictionary["TextInput"], elementsContainer.transform).GetComponent<TextInput>();
        textInput.Initialize(textbox.textContent, "Text");

        var colorPicker = Instantiate(prefabDictionary["ColorPicker"], elementsContainer.transform).GetComponent<ColorPicker>();
        colorPicker.Initialize(textbox.color, "Farbe");

        var toggleViewButton = Instantiate(prefabDictionary["Button"], elementsContainer.transform).GetComponent<Button>();
        toggleViewButton.GetComponentInChildren<TMP_Text>().text = "Öffnen/Schließen";

        toggleViewButton.onClick.AddListener(() =>
        {
            if (textbox.isOpen)
            {
                textbox.Unhighlight();
            }
            else
            {
                textbox.Highlight();
            }
            Highlight(textbox.gameObject);
        });


        spriteSelector.OnElementSelected.AddListener((value) =>
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

        textInput.OnValueChanged.AddListener((text) =>
        {
            textbox.SetText(text);
            if (sceneElement != null)
            {
                sceneElement.text = text;
                UpdateSceneElement(sceneElement);
            }
        });

        colorPicker.OnColorChange.AddListener((color) =>
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

        var textInput = Instantiate(prefabDictionary["TextInput"], elementsContainer.transform).GetComponent<TextInput>();
        textInput.Initialize(text.text, "Text");

        var colorPicker = Instantiate(prefabDictionary["ColorPicker"], elementsContainer.transform).GetComponent<ColorPicker>();
        colorPicker.Initialize(text.color, "Farbe");

        var sceneElementHolder = text.GetComponent<SceneElementHolder>();
        SceneElement sceneElement = sceneElementHolder.sceneElement;

        textInput.OnValueChanged.AddListener((newText) =>
        {
            text.text = newText;
            if (sceneElement != null)
            {
                sceneElement.text = newText;
                UpdateSceneElement(sceneElement);
            }
        });

        colorPicker.OnColorChange.AddListener((color) =>
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
        domePositionInput.Initialize(domePosition.position.x, domePosition.position.y, domePosition.distance, domePosition.xRotOffset);


        var sceneElementHolder = domePosition.GetComponent<SceneElementHolder>();
        var sceneElement = sceneElementHolder.sceneElement;

        domePositionInput.OnInputChanged.AddListener((x, y, distance, tilt) =>
        {
            domePosition.position = new Vector2(x, y);
            domePosition.distance = (int)distance;
            domePosition.xRotOffset = tilt;

            if (sceneElement != null)
            {
                sceneElement.x = (int)x;
                sceneElement.y = (int)y;
                sceneElement.distance = (int)distance;
                sceneElement.xRotationOffset = (int)tilt;

                UpdateSceneElement(sceneElement);
            }

        });
    }

    public void AddOnAction(Interactable interactable)
    {
        SceneElement sceneElement = interactable.GetComponent<SceneElementHolder>().sceneElement;
        string action = sceneElement.action;

        string pattern = @"toScene\((.*?)\)";
        Match match = Regex.Match(action, pattern);
        if (match.Success)
        {
            string sceneName = match.Groups[1].Value;

            var group = Instantiate(prefabDictionary["Group"], sidebarContainer.transform);
            var label = group.GetComponentInChildren<TMP_Text>();
            var elementsContainer = group.transform.Find("Elements");
            label.text = "Interaktion";

            var dropdown = Instantiate(prefabDictionary["Dropdown"], elementsContainer.transform).GetComponent<DropdownInput>();

            List<string> actionOptions = new List<string>();
            var scenes = sceneManager.sceneList;

            actionOptions.Add("Keine");
            foreach (Scene scene in scenes.Values)
            {
                actionOptions.Add(scene.Name);
            }
            dropdown.Initialize(actionOptions, sceneName, "Gehe zu");

            var button = Instantiate(prefabDictionary["Button"], elementsContainer.transform).GetComponent<Button>();
            button.GetComponentInChildren<TMP_Text>().text = "Aktion ausführen";

            dropdown.OnValueChanged.AddListener((value) =>
            {
                interactable.OnInteract.RemoveAllListeners();
                if (value != "Keine")
                {
                    interactable.OnInteract.AddListener(() =>
                    {
                        sceneChanger.ActionParser(sceneElement.action);
                    }
                );
                }
                if (sceneElement != null)
                {
                    if (value == "Keine")
                    {
                        sceneElement.action = "";
                    }
                    else
                    {
                        sceneElement.action = $"toScene({value})";
                    }
                    UpdateSceneElement(sceneElement);
                }
            });

            button.onClick.AddListener(() =>
            {
                interactable.OnInteract.Invoke();
            });
        }
    }

    public void UpdateSceneElement(SceneElement sceneElement)
    {
        if (sceneChanger.currentScene.SceneElements.ContainsKey(sceneElement.id))
        {
            sceneChanger.currentScene.SceneElements[sceneElement.id] = sceneElement;
        }
    }

    public void Deselect(bool closeSidebar = false)
    {
        selector.target = null;

        if (closeSidebar)
        {
            panelManager.CloseSidebar();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject target = hit.collider.gameObject;
                Select(target);
            }
        }
    }
}
