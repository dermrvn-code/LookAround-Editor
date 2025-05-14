using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : Hoverable
{
    // Start is called before the first frame update
    public UnityEvent OnInteract;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        Setup();
    }

    public void Interact()
    {
        OnInteract.Invoke();
    }
}
