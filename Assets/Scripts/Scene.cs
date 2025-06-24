using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Scene
{
    public bool HasUnsavedChanges { get; set; } = false;

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
    public Dictionary<int, SceneElement> SceneElements
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

    [SerializeField]
    MediaType _type;
    [SerializeField]
    string _name;
    [SerializeField]
    string _source;
    Dictionary<int, SceneElement> _sceneElements;
    [SerializeField]
    bool _isStartScene;
    [SerializeField]
    float _xOffset;
    [SerializeField]
    float _yOffset;

    public Scene(MediaType type, string name, string source, Dictionary<int, SceneElement> sceneElements, bool isStartScene, float xOffset = 0, float yOffset = 0)
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
        return "Scene '" + Name + "' of type '" + Type.ToString() + "' and source '" + Source + "', Offset(" + XOffset + ", " + YOffset + ")";
    }

    public void SetValues(string name, string source, bool isStartScene, float xOffset = 0, float yOffset = 0)
    {
        _name = name;
        _source = source;
        _isStartScene = isStartScene;
        _xOffset = xOffset;
        _yOffset = yOffset;
    }

    public void SetStartScene(bool isStartScene)
    {
        _isStartScene = isStartScene;
    }

    public void SetSource(string source)
    {
        _source = source;
    }
}
