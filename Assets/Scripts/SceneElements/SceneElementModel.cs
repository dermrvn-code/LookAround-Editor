[System.Serializable]
public class SceneElementModel : SceneElement
{
    public string modelName;

    public int xRotation;
    public int yRotation;
    public int zRotation;
    public int scale;

    public SceneElementModel(
        string modelName, int x = 0, int y = 0,
        int distance = 0, int xRotationOffset = 0, string action = null, int xRotation = 0, int yRotation = 0, int zRotation = 0, int scale = 1)
        : base(x, y, distance, xRotationOffset, action)
    {
        this.modelName = modelName;
        this.xRotation = xRotation;
        this.yRotation = yRotation;
        this.zRotation = zRotation;
        this.scale = scale;
    }

    public override string ToString()
    {
        return $"Model {modelName} at x:{x} y:{y}, a distance of {distance}, action '{action}', rotation ({xRotation}, {yRotation}, {zRotation}), scale {scale}";
    }
}