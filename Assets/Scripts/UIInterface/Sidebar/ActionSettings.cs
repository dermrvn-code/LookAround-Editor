using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ActionSettings : MonoBehaviour
{
    [SerializeField] DropdownInput dropdownInput;
    [SerializeField] ToScene toScene;
    [SerializeField] Button executeButton;

    public Action<string, string, int> OnValueChanged = null;

    string actionFunction;

    ActionManager actionManager;
    void Awake()
    {
        actionManager = FindFirstObjectByType<ActionManager>();
    }

    string[] actionOptions = new string[] { "Keine", "Gehe zu Szene", "Starte Mini-Game", "Restarte Mini-Game" };
    string[] actionFunctions = new string[] { "", "toScene", "startMiniGame", "restartMiniGame" };
    public void Initialize(int actionId, string sceneName, int currentTransition)
    {
        actionFunction = actionFunctions[actionId];
        dropdownInput.Initialize(actionOptions.ToList(), actionOptions[actionId]);
        toScene.Initialize(sceneName, currentTransition);

        dropdownInput.OnValueChanged.AddListener((value) =>
        {
            actionFunction = actionFunctions[Array.IndexOf(actionOptions, value)];
            OnValueChanged?.Invoke(actionFunction, toScene.currentScene, toScene.currentTransition);
        });
        toScene.OnValueChanged += (scene, transition) =>
        {
            OnValueChanged?.Invoke(actionFunction, toScene.currentScene, toScene.currentTransition);
        };

        executeButton.onClick.AddListener(() =>
        {
            actionManager.ActionParser(GenerateAction());
        });
    }

    public void Initialize(string action)
    {
        ActionType actionType = actionManager.GetFunction(action, out Match match);

        string sceneName = "";
        int animationIndex = -1;
        if (actionType == ActionType.ToScene)
        {
            sceneName = match.Groups[1].Value;
            if (match.Groups.Count > 2 && match.Groups[2].Success)
                int.TryParse(match.Groups[2].Value.Trim(), out animationIndex);
        }
        else if (actionType == ActionType.StartMiniGame || actionType == ActionType.RestartMiniGame)
        {
            if (match.Groups.Count > 2 && match.Groups[2].Success)
                sceneName = match.Groups[2].Value;

            animationIndex = -1;
            if (match.Groups.Count > 3 && match.Groups[3].Success)
                int.TryParse(match.Groups[3].Value.Trim(), out animationIndex);
        }

        Initialize((int)actionType, sceneName, animationIndex);
    }

    public string GenerateAction()
    {
        if (string.IsNullOrEmpty(actionFunction)) return "";

        switch (actionFunction)
        {
            case "toScene":
                var parameters = SceneParameters();
                if (parameters != "")
                {
                    return $"toScene({parameters})";
                }
                return "";
            case "startMiniGame":
                return $"startMiniGame(puzzle{SceneParameters(seperatingComma: true)})";
            case "restartMiniGame":
                return $"restartMiniGame(puzzle{SceneParameters(seperatingComma: true)})";
            default:
                return "";
        }
    }

    string SceneParameters(bool seperatingComma = false)
    {
        if (toScene.currentScene == "" || toScene.currentScene == "Keine") return "";

        if (toScene.currentTransition == -1)
        {
            return $"{(seperatingComma ? "," : "")}{toScene.currentScene}";
        }
        else
        {
            return $"{(seperatingComma ? "," : "")}{toScene.currentScene},{toScene.currentTransition}";
        }
    }
}
