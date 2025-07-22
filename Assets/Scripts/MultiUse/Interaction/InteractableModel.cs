
using TMPro;
using UnityEngine;

public class InteractableModel : Interactable
{
    // Start is called before the first frame update
    private Animation anim;

    public GameObject elementContainer;


    public override void Setup()
    {
        anim = GetComponentInChildren<Animation>();
        Unhighlight();
    }


    public override void Highlight()
    {
        anim.enabled = true;
    }
    public override void Unhighlight()
    {
        anim.enabled = false;
    }
}
