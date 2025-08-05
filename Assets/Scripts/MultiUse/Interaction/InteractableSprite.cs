
using TMPro;
using UnityEngine;

public class InteractableSprite : Interactable
{
    private Animation anim;
    public Texture2D texture;
    public int id;

    MeshRenderer meshRenderer;


    public override void Setup()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        anim = GetComponentInChildren<Animation>();

        if (texture == null)
        {
            Debug.LogError("InteractableSprite: Image is not set.");
            return;
        }
        SetImage(texture);
        Unhighlight();
    }

    public void SetImage(Texture2D newTexture)
    {
        texture = newTexture;
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material = new Material(meshRenderer.material);
            meshRenderer.material.mainTexture = texture;

            Vector2 logoSize = new Vector2(texture.width, texture.height);
            float maxDimension = Mathf.Max(logoSize.x, logoSize.y);
            float aspectWidth = logoSize.x / maxDimension;
            float aspectHeight = logoSize.y / maxDimension;

            meshRenderer.material.SetFloat("_TexWidth", aspectWidth);
            meshRenderer.material.SetFloat("_TexHeight", aspectHeight);
        }
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
