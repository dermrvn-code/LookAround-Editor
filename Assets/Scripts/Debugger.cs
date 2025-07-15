using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Debugger : MonoBehaviour
{

    [SerializeField]
    List<string> previewModels = new List<string>();


    ModelManager modelManager;
    void Start()
    {
        modelManager = FindAnyObjectByType<ModelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        previewModels.Clear();
        foreach (var m in modelManager.previewModels.Keys)
        {
            previewModels.Add(m);
        }
    }
}
