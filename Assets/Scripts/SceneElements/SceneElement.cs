public abstract class SceneElement
{
    public int id;
    public int x;
    public int y;
    public int distance;
    public int xRotationOffset;
    public string action;

    protected SceneElement(int x = 0, int y = 0, int distance = 0, int xRotationOffset = 0, string action = null)
    {
        this.x = x;
        this.y = y;
        this.distance = distance;
        this.xRotationOffset = xRotationOffset;
        this.action = action;
    }

    public abstract override string ToString();
}
