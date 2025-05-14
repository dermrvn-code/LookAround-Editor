using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneTile : MonoBehaviour
{
    Scene scene;

    public TMP_Text sceneNameText;
    public Image image;

    SceneChanger sceneChanger;
    TextureManager textureManager;

    void Awake()
    {
        sceneChanger = FindObjectOfType<SceneChanger>();
        textureManager = FindObjectOfType<TextureManager>();

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void Setup(Scene scene)
    {
        this.scene = scene;

        if (sceneNameText != null)
        {
            sceneNameText.text = this.scene.Name;
        }

        if (!string.IsNullOrEmpty(this.scene.Source))
        {
            StartCoroutine(textureManager.GetTexture(this.scene.Source, texture =>
            {
                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    image.sprite = sprite;
                }
            }));
        }
    }

    void OnClick()
    {
        sceneChanger.SwitchSceneAnimation(scene);
    }
}
