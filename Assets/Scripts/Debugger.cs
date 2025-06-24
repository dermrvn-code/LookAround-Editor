using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Debugger : MonoBehaviour
{

    [SerializeField]
    List<Scene> scenes = new List<Scene>();

    [SerializeField]
    Scene scene;

    [SerializeField]
    List<SceneElement> sceneElements = new List<SceneElement>();

    SceneManager sceneManager;
    SceneChanger sceneChanger;
    void Start()
    {
        sceneManager = FindObjectOfType<SceneManager>();
        sceneChanger = FindObjectOfType<SceneChanger>();
    }

    // Update is called once per frame
    void Update()
    {
        if (sceneManager.sceneList.Count > 0)
        {
            scenes = sceneManager.sceneList.Values.ToList();
        }
        else
        {
            scenes.Clear();
        }

        if (sceneChanger.currentScene != null)
        {
            scene = sceneChanger.currentScene;
            if (sceneManager.sceneList.ContainsKey(scene.Name))
            {
                sceneElements = sceneManager.sceneList[scene.Name].SceneElements.Values.ToList();
            }
        }
    }
}
