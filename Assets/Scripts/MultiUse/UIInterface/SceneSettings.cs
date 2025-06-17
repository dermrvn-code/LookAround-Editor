using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSettings : MonoBehaviour
{

    string sceneName;
    bool isStartScene = false;
    string filePath;

    public TextInput sceneNameInput;
    public ToggleInput startSceneToggle;
    public ImageUploader imageUploader;

    void Start()
    {
        sceneNameInput.OnValueChanged.AddListener((value) =>
        {
            sceneName = value;
        });
        startSceneToggle.OnValueChanged.AddListener((value) =>
        {
            isStartScene = value;
        });
        imageUploader.OnValueChanged.AddListener((path) =>
        {
            filePath = path;
        });
    }

    // Update is called once per frame
    void Save()
    {
        // Hier können Sie den Code zum Speichern der Szeneinstellungen hinzufügen
        // Zum Beispiel in einer Datei oder in einem Datenbankeintrag
        Debug.Log($"Scene Name: {sceneName}, Is Start Scene: {isStartScene}, File Path: {filePath}");
    }

    public void Initialize(string sceneName, bool isStartScene, string filePath)
    {
        this.sceneName = sceneName;
        this.isStartScene = isStartScene;
        this.filePath = filePath;

        sceneNameInput.Initialize(sceneName);
        startSceneToggle.Initialize(isStartScene);
        imageUploader.Initialize(filePath);
    }
}
