
using TMPro;
using UnityEngine;

public class InteractableText : Interactable
{
    // Start is called before the first frame update
    public TMP_Text text;

    public override void Setup()
    {
        TryGetComponent<TMP_Text>(out text);
    }


    public override void Highlight()
    {
        if (text != null) text.fontStyle = FontStyles.Underline;
    }
    public override void Unhighlight()
    {
        if (text != null) text.fontStyle = FontStyles.Normal;
    }
}
