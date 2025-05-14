
using TMPro;
using UnityEngine;

public class InteractableArrow : Interactable
{
    [SerializeField]
    GameObject arrow;

    // Start is called before the first frame update
    private Animation anim;
    private Material mat;


    public Color color;

    [SerializeField]
    Color hoverColor;

    public override void Setup()
    {
        anim = GetComponentInChildren<Animation>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        mat.color = color;

        // Maker hoverColor a bit brighter than color
        hoverColor = new Color(color.r * 2f, color.g * 2f, color.b * 2f, color.a);
    }

    public void SetRotation(int rotation)
    {
        arrow.transform.Rotate(0, rotation, 0);
    }

    public override void Highlight()
    {
        anim.enabled = true;
        mat.color = hoverColor;
    }
    public override void Unhighlight()
    {
        anim.enabled = false;
        mat.color = color;
    }
}
