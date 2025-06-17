using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneTile : MonoBehaviour
{
    Scene scene;

    public TMP_Text sceneNameText;
    public Image image;
    public bool startScene = false;
    public GameObject icon;
    public Button mainButton;
    public Button editButton;

    SceneChanger sceneChanger;
    TextureManager textureManager;
    Animator animator;

    void Awake()
    {
        sceneChanger = FindObjectOfType<SceneChanger>();
        textureManager = FindObjectOfType<TextureManager>();
        animator = GetComponent<Animator>();

        icon.SetActive(startScene);
        if (mainButton != null)
        {
            mainButton.onClick.AddListener(OnClick);
        }
    }

    public void Setup(Scene scene, bool startScene = false)
    {
        this.scene = scene;
        SetStartScene(startScene);

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


    public void Open()
    {
        animator.SetBool("Open", true);
    }

    public void Close()
    {
        animator.SetBool("Open", false);
    }


    public void SetStartScene(bool isStart)
    {
        startScene = isStart;
        icon.SetActive(isStart);
        sceneNameText.fontStyle = isStart ? (FontStyles.Underline | FontStyles.Bold) : FontStyles.Bold;
    }
}
