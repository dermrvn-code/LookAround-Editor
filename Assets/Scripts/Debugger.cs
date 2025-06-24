using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Debugger : MonoBehaviour
{
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
        scene = sceneChanger.currentScene;
        sceneElements = sceneManager.sceneList[scene.Name].SceneElements.Values.ToList();
    }
}
