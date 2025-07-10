public class SceneElementTextbox : SceneElement
{
    public string text;
    public string icon;
    public string color;

    public SceneElementTextbox(
        string text, string icon, int x = 0, int y = 0,
        int distance = 0, int xRotationOffset = 0, string color = null)
        : base(x, y, distance, xRotationOffset)
    {
        this.text = text;
        this.icon = icon;
        this.color = color;
    }

    public override string ToString()
    {
        return $"Textbox with value {text} at x:{x} y:{y}, a distance of {distance}, and icon '{icon}'";
    }
}