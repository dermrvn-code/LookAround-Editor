using UnityEngine;

public class Pairs : MonoBehaviour
{
    [System.Serializable]
    public class PrefabPair
    {
        public string value;
        public GameObject prefab;
    }

    [System.Serializable]
    public class SpritePair
    {
        public string value;
        public Sprite sprite;
    }

}

