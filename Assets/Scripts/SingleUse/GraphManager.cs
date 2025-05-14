using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject connectionPrefab;

    public GameObject startPoint;


    SceneManager sceneManager;
    TextureManager textureManager;

    Vector2 centerPosition = Vector2.zero;
    Vector2 offsetRange = Vector2.zero;

    void Start()
    {
        sceneManager = FindObjectOfType<SceneManager>();
        textureManager = FindObjectOfType<TextureManager>();

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 size = rectTransform != null ? rectTransform.rect.size : Vector2.zero;
        centerPosition = new Vector2(size.x / 2, size.y / 2);
        offsetRange = new Vector2(size.x / 3, size.y / 3);
    }

    public Dictionary<string, GameObject> nodes;
    public List<string[]> sceneConnections;
    public List<ArrowBetweenUIElements> connectionList;

    public void LoadGraph()
    {
        sceneConnections = new List<string[]>();
        nodes = new Dictionary<string, GameObject>();

        connectionList = new List<ArrowBetweenUIElements>();

        foreach (var scene in sceneManager.sceneList.Values)
        {
            foreach (var element in scene.SceneElements)
            {
                if (!string.IsNullOrEmpty(element.action))
                {
                    if (element.action.Contains("toScene"))
                    {
                        string sceneName = element.action.Replace("toScene(", "").Replace(")", "").Trim();
                        sceneConnections.Add(new string[] { scene.Name, sceneName });
                    }
                }
            }

            if (scene.IsStartScene)
            {
                sceneConnections.Add(new string[] { "start", scene.Name });
            }

            var node = CreateNode(scene);
            nodes.Add(scene.Name, node);
        }

        foreach (var connection in sceneConnections)
        {

            if ((nodes.ContainsKey(connection[0]) || connection[0] == "start") && nodes.ContainsKey(connection[1]))
            {
                var fromNode = startPoint;
                if (connection[0] != "start")
                {
                    fromNode = nodes[connection[0]];
                }

                var toNode = nodes[connection[1]];

                CreateConnection(fromNode, toNode);
            }
        }
    }

    void CreateConnection(GameObject from, GameObject to)
    {
        // Prevent duplicate connections in either direction
        foreach (ArrowBetweenUIElements connection in connectionList)
        {
            if ((connection.from.gameObject == from && connection.to.gameObject == to) ||
                (connection.from.gameObject == to && connection.to.gameObject == from))
            {
                Debug.Log("Connection already exists between " + from.name + " and " + to.name);
                connection.arrowsOnBothSides = true;
                return;
            }

        }

        ArrowBetweenUIElements newArrow = Instantiate(connectionPrefab, transform).GetComponent<ArrowBetweenUIElements>();
        newArrow.arrowsOnBothSides = false;
        newArrow.from = from.transform as RectTransform;
        newArrow.to = to.transform as RectTransform;
        connectionList.Add(newArrow);
    }

    GameObject CreateNode(Scene scene)
    {
        GameObject node = Instantiate(nodePrefab, transform);
        Vector3 offset = new Vector3(Random.Range(-offsetRange.x, offsetRange.x), Random.Range(-offsetRange.y, offsetRange.y), 0);
        node.transform.position = new Vector3(centerPosition.x, centerPosition.y, node.transform.localPosition.z) + offset;

        UIReorderableItem item = node.GetComponent<UIReorderableItem>();
        item.container = transform;

        Image image = node.GetComponent<Image>();

        StartCoroutine(textureManager.GetTexture(scene.Source, texture =>
        {
            if (texture != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.sprite = sprite;
            }
        }));
        return node;
    }
}
