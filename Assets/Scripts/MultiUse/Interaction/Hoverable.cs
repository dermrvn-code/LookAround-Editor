using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Hoverable : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        Setup();
    }

    public abstract void Setup();

    public abstract void Highlight();

    public abstract void Unhighlight();

}
