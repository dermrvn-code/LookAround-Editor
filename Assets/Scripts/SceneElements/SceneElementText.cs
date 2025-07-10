public class SceneElementText : SceneElement
{
    public string text;
    public string color;

    public SceneElementText(
        string text, int x = 0, int y = 0,
        int distance = 0, int xRotationOffset = 0, string action = null, string color = null)
        : base(x, y, distance, xRotationOffset, action)
    {
        this.text = text;
        this.color = color;
    }

    public override string ToString()
    {
        return $"Text with value {text} at x:{x} y:{y}, a distance of {distance}, action '{action}'";
    }
}