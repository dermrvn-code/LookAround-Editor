using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Debugger : MonoBehaviour
{

    SceneChanger sceneChanger;
    void Start()
    {
        sceneChanger = FindFirstObjectByType<SceneChanger>();
    }

    [SerializeField] Scene scene;
    [SerializeField] List<SceneElementDebug> sceneElements = new List<SceneElementDebug>();
    void Update()
    {
        scene = sceneChanger.currentScene;

        if (scene.SceneElements != null && scene.SceneElements.Count > 0) sceneElements = scene.SceneElements.Values.Select(element => new SceneElementDebug(element)).ToList();
    }
}
