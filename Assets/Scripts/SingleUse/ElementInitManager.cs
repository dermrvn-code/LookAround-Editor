using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ElementInitManager : MonoBehaviour
{


    public GameObject sceneElementsContainer;
    public List<Pairs.PrefabPair> prefabs;
    private Dictionary<string, GameObject> prefabDictionary;

    SceneChanger sceneChanger;

    void Start()
    {
        sceneChanger = FindObjectOfType<SceneChanger>();

        foreach (Pairs.PrefabPair pair in prefabs)
        {
            if (prefabDictionary == null)
            {
                prefabDictionary = new Dictionary<string, GameObject>();
            }
            prefabDictionary[pair.value] = pair.prefab;
        }
    }

    string[] texts = new string[]
    {
        "TODO: Inhalt kommt gleich",
        "Hier könnte Ihr Text stehen.",
        "Nicht löschen - könnte wichtig sein!",
        "Compiler fand's witzig.",
        "Text geladen. Sinn folgt eventuell.",
        "Debug-Modus für Menschen.",
        "Platzhalter mit Persönlichkeit.",
        "Binär, aber mit Gefühl.",
        "Wortsalat erfolgreich kompiliert.",
        "Schöner Code beginnt hier.",
        "Text steht unter Beobachtung.",
        "Hier war mal ein guter Gedanke.",
        "Noch unformatiert, aber ambitioniert.",
        "Text v1.0 - Beta forever.",
        "Nichts zu sehen, bitte weitergehen."
    };
    public void InitText(float position)
    {
        TMP_Text text = Instantiate(prefabDictionary["Text"], sceneElementsContainer.transform).GetComponent<TMP_Text>();

        var color = new Color(Random.value, Random.value, Random.value);
        text.color = color;
        text.text = texts[Random.Range(0, texts.Length)];


        var dp = text.GetComponent<DomePosition>();
        dp.position = new Vector2(position, 0);
        dp.distance = 8;

        var holder = text.gameObject.AddComponent<SceneElementHolder>();

        SceneElement sceneElement = new SceneElement
        {
            type = SceneElement.ElementType.Text,
            x = (int)position,
            y = 0,
            distance = 8,
            rotation = 0,
            color = $"#{ColorUtility.ToHtmlStringRGBA(color)}",
            action = "toScene()",
            text = text.text
        };

        AddSceneElement(sceneElement);
        holder.sceneElement = sceneElement;
    }

    public void InitArrow(float position)
    {
        InteractableArrow arrow = Instantiate(prefabDictionary["Arrow"], sceneElementsContainer.transform).GetComponent<InteractableArrow>();

        var color = new Color(Random.value, Random.value, Random.value);
        arrow.color = color;
        arrow.SetRotation(180);

        var dp = arrow.GetComponent<DomePosition>();
        dp.position = new Vector2(position, 0);
        dp.distance = 8;

        var holder = arrow.gameObject.AddComponent<SceneElementHolder>();

        SceneElement sceneElement = new SceneElement
        {
            type = SceneElement.ElementType.DirectionArrow,
            x = (int)position,
            y = 0,
            distance = 8,
            rotation = 180,
            color = $"#{ColorUtility.ToHtmlStringRGBA(color)}",
            action = "toScene()"
        };

        AddSceneElement(sceneElement);
        holder.sceneElement = sceneElement;
    }

    public void InitTextbox(float position)
    {
        TextBox textbox = Instantiate(prefabDictionary["Textbox"], sceneElementsContainer.transform).GetComponent<TextBox>();

        var color = new Color(Random.value, Random.value, Random.value);
        textbox.SetColor(color);
        var text = texts[Random.Range(0, texts.Length)];
        textbox.SetText(text);
        textbox.iconName = "info";

        var dp = textbox.GetComponent<DomePosition>();
        dp.position = new Vector2(position, 0);
        dp.distance = 8;

        var holder = textbox.gameObject.AddComponent<SceneElementHolder>();

        SceneElement sceneElement = new SceneElement
        {
            type = SceneElement.ElementType.Textbox,
            x = (int)position,
            y = 0,
            distance = 8,
            rotation = 0,
            color = $"#{ColorUtility.ToHtmlStringRGBA(color)}",
            icon = "info",
            text = text
        };

        AddSceneElement(sceneElement);
        holder.sceneElement = sceneElement;
    }

    void AddSceneElement(SceneElement sceneElement)
    {
        if (sceneChanger.currentScene == null)
        {
            Debug.LogError("Current scene is null, cannot add scene element.");
            return;
        }

        var elements = sceneChanger.currentScene.SceneElements;
        int maxKey = 0;
        if (elements.Count > 0)
        {
            maxKey = Mathf.Max(elements.Keys.ToArray());
            foreach (var i in elements.Values)
            {
                Debug.Log(i);
            }
        }
        int id = maxKey + 1;
        sceneElement.id = id;
        elements[id] = sceneElement;
    }
}
