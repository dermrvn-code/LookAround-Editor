public class SceneElementArrow : SceneElement
{
    public int rotation;
    public string color;

    public SceneElementArrow(
        int x = 0, int y = 0,
        int distance = 0, int xRotationOffset = -20,
        int rotation = 0,
        string color = null, string action = null)
        : base(x, y, distance, xRotationOffset, action)
    {
        this.rotation = rotation;
        this.color = color;
    }

    public override string ToString()
    {
        return $"Arrow with color {color} at x:{x} y:{y}, a distance of {distance}, and rotation of '{rotation}' and action '{action}'";
    }
}