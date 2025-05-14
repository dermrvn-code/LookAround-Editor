
using UnityEngine;

public class InteractableOutline : Interactable
{
    // Start is called before the first frame update
    Outline outline;

    public override void Setup()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = Color.black;
        outline.OutlineWidth = 5f;
        outline.enabled = false;
    }


    public override void Highlight()
    {
        outline.enabled = true;
    }
    public override void Unhighlight()
    {
        outline.enabled = false;
    }
}
