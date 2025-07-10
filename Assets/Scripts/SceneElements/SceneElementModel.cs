public class SceneElementModel : SceneElement
{
    public string modelName;
    public SceneElementModel(
        string modelName, int x = 0, int y = 0,
        int distance = 0, int xRotationOffset = 0, string action = null)
        : base(x, y, distance, xRotationOffset, action)
    {
        this.modelName = modelName;
    }

    public override string ToString()
    {
        return $"Model {modelName} at x:{x} y:{y}, a distance of {distance}, action '{action}'";
    }
}