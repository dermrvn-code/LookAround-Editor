
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
    public Color hoverColor;
    public int rotation = 0;

    public override void Setup()
    {
        anim = GetComponentInChildren<Animation>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        SetColor(color);
    }

    public void SetRotation(int rotation)
    {
        arrow.transform.localRotation = Quaternion.Euler(0, rotation, 0);
        this.rotation = rotation;

    }

    public void SetColor(Color newColor)
    {
        color = newColor;
        mat.color = color;
        hoverColor = new Color(color.r * 2f, color.g * 2f, color.b * 2f, color.a);
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
