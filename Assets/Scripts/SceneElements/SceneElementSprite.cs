public class SceneElementSprite : SceneElement
{
    public string path;
    public int index;

    public SceneElementSprite(
        int index, string path, int x = 0, int y = 0,
        int distance = 0, int xRotationOffset = 0, string action = null)
        : base(x, y, distance, xRotationOffset, action)
    {
        this.index = index;
        this.path = path;
    }

    public override string ToString()
    {
        return $"Sprite {index} with path {path} at x:{x} y:{y}, a distance of {distance}, action '{action}'";
    }
}