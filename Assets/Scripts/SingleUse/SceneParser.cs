using UnityEngine;

public class SceneParser : MonoBehaviour
{

    SceneManager sceneManager;
    void Start()
    {
        sceneManager = GetComponent<SceneManager>();
    }

    public void Parse()
    {
        foreach (var scene in sceneManager.sceneList.Values)
        {
            Debug.Log("Parsing scene: " + scene.Name);
        }
    }


}
