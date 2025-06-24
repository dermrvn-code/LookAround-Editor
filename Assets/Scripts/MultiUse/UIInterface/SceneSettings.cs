using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SceneSettings : MonoBehaviour
{
    string initialName;
    bool initialIsStartScene;
    string initialFilePath;
    Vector2 initialOffset;


    public TextInput sceneNameInput;
    public ToggleInput startSceneToggle;
    public ImageUploader imageUploader;
    public SliderAndInput xOffsetInput;
    public SliderAndInput yOffsetInput;

    SceneManager sceneManager;
    SceneChanger sceneChanger;
    PanelManager panelManager;
    ProjectManager projectManager;

    void Start()
    {
        sceneManager = FindObjectOfType<SceneManager>();
        sceneChanger = FindObjectOfType<SceneChanger>();
        panelManager = FindObjectOfType<PanelManager>();
        projectManager = FindObjectOfType<ProjectManager>();

        xOffsetInput.OnValueChanged.AddListener((value) =>
        {
            if (CheckIfLiveUpdatable()) sceneChanger.UpdateOffset(xOffset: MapOffsetToDotted(value, 0).x);
        });

        yOffsetInput.OnValueChanged.AddListener((value) =>
        {
            if (CheckIfLiveUpdatable()) sceneChanger.UpdateOffset(yOffset: MapOffsetToDotted(0, value).y);
        });

        imageUploader.OnValueChanged.AddListener((value) =>
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (CheckIfLiveUpdatable()) sceneChanger.UpateMedium(value);
            }
        });
    }

    bool CheckIfLiveUpdatable()
    {
        return sceneChanger.currentScene.Name == initialName;
    }

    public void Save()
    {
        Scene scene = _Save();
        if (scene != null)
        {
            InfoText.ShowInfo("Szeneneinstellungen gespeichert.");
        }
    }

    public void Delete()
    {
        if (sceneManager.sceneList.TryGetValue(initialName, out Scene scene))
        {
            sceneManager.sceneList.Remove(initialName);
            if (scene.IsStartScene)
            {
                sceneManager.SetStartScene(sceneNameAvoid: initialName);
            }

            if (sceneChanger.currentScene.Name == initialName)
            {
                sceneChanger.SwitchSceneAnimation(sceneManager.GetStartScene());
            }
            panelManager.UpdateSceneList();
            InfoText.ShowInfo("Szene gelöscht.");
        }
        else
        {
            InfoText.ShowInfo("Szene nicht gefunden.");
        }
    }

    public static Vector2 MapOffsetToDotted(float x, float y)
    {
        return new Vector2(
            x / 360,
            (y / 360) - 0.5f
        );
    }

    public static Vector2 MapOffsetToDegree(float x, float y)
    {
        return new Vector2(
            x * 360,
            (y + 0.5f) * 360
        );
    }

    Scene _Save()
    {
        string sceneName = sceneNameInput.value;
        bool isStartScene = startSceneToggle.value;
        string filePath = imageUploader.value;
        Vector2 offset = new Vector2(
            xOffsetInput.value,
            yOffsetInput.value
        );


        if (sceneName == initialName && isStartScene == initialIsStartScene &&
            filePath == initialFilePath && offset == initialOffset)
        {
            InfoText.ShowInfo("Es wurden keine Änderungen vorgenommen.");
            return null;
        }

        ProcessIndicator.Show();
        Vector2 offsetMapped = MapOffsetToDotted(
            offset.x,
            offset.y
        );

        if (isStartScene != initialIsStartScene)
        {
            if (isStartScene)
            {
                sceneManager.SetStartScene(sceneName: sceneName);
            }
            else
            {
                sceneManager.SetStartScene(sceneNameAvoid: sceneName);
            }
        }


        sceneManager.sceneList.TryGetValue(initialName, out Scene scene);

        if (string.IsNullOrEmpty(sceneName))
        {
            InfoText.ShowInfo("Es wurde kein Szenenname angegeben.");
            return null;
        }

        if (string.IsNullOrEmpty(filePath))
        {
            InfoText.ShowInfo("Es wurde kein Medium ausgewählt.");
            return null;
        }
        if (scene == null || sceneName != initialName)
        {
            scene = new Scene(
                Scene.MediaType.Photo,
                sceneName,
                filePath,
                new Dictionary<int, SceneElement>(),
                isStartScene,
                offsetMapped.x,
                offsetMapped.y
                );
            sceneManager.sceneList.Add(sceneName, scene);
        }
        else
        {
            scene.SetValues(sceneName, filePath, isStartScene, offsetMapped.x, offsetMapped.y);
        }

        if (sceneName != initialName)
        {
            sceneManager.sceneList.Remove(initialName);
        }
        scene.HasUnsavedChanges = true;
        projectManager.unsavedChanges = true;
        panelManager.UpdateSceneList();
        ProcessIndicator.Hide();
        SetInitials(sceneName, isStartScene, filePath, (int)offset.x, (int)offset.y);
        return scene;
    }

    public void SaveAndLoad()
    {
        Scene scene = _Save();
        if (scene != null)
        {
            InfoText.ShowInfo("Szeneneinstellungen gespeichert.");
            sceneChanger.SwitchSceneAnimation(scene, closeSidebar: false, forceReload: true);
        }
    }

    void SetInitials(string sceneName, bool isStartScene, string filePath, int xOffset, int yOffset)
    {

        initialName = sceneName;
        initialIsStartScene = isStartScene;
        initialFilePath = filePath;
        initialOffset = new Vector2(xOffset, yOffset);
    }

    public void Initialize(string sceneName, bool isStartScene, string filePath, int xOffset = 0, int yOffset = 180)
    {
        SetInitials(sceneName, isStartScene, filePath, xOffset, yOffset);

        sceneNameInput.Initialize(sceneName);
        startSceneToggle.Initialize(isStartScene);
        imageUploader.Initialize(filePath);
        xOffsetInput.Initialize(xOffset);
        yOffsetInput.Initialize(yOffset);
    }
}
