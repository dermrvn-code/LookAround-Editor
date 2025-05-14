using System.Collections.Generic;

public class Scene
{
    public enum MediaType { Video, Photo };
    public MediaType Type
    {
        get => _type;
    }
    public string Name
    {
        get => _name;
    }
    public string Source
    {
        get => _source;
    }
    public List<SceneElement> SceneElements
    {
        get => _sceneElements;
    }
    public bool IsStartScene
    {
        get => _isStartScene;
    }
    public float XOffset
    {
        get => _xOffset;
    }
    public float YOffset
    {
        get => _yOffset;
    }


    MediaType _type;
    string _name;
    string _source;
    List<SceneElement> _sceneElements;
    bool _isStartScene;
    float _xOffset;
    float _yOffset;

    public Scene(MediaType type, string name, string source, List<SceneElement> sceneElements, bool isStartScene, float xOffset = 0, float yOffset = 0)
    {
        _type = type;
        _name = name;
        _source = source;
        _sceneElements = sceneElements;
        _isStartScene = isStartScene;
        _xOffset = xOffset;
        _yOffset = yOffset;
    }

    public override string ToString()
    {
        return "Scene '" + Name + "' of type '" + Type.ToString() + "' and source '" + Source + "'";
    }
}
