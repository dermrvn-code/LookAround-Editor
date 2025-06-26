using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SceneTile : MonoBehaviour
{
    Scene scene;

    public TMP_Text sceneNameText;
    public RawImage image;
    public bool startScene = false;
    public GameObject icon;
    public Button mainButton;
    public Button editButton;
    public VideoPlayer videoPlayer;

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
            if (image != null)
            {
                if (scene.Type == Scene.MediaType.Photo)
                {
                    StartCoroutine(textureManager.GetTexture(this.scene.Source, texture =>
                    {
                        if (texture != null)
                        {
                            image.texture = texture;
                        }
                    }));
                }
                else if (scene.Type == Scene.MediaType.Video)
                {
                    if (videoPlayer != null)
                    {
                        videoPlayer.enabled = true;
                        videoPlayer.url = this.scene.Source;
                        var renderTexture = new RenderTexture(1920, 1080, 0);
                        videoPlayer.targetTexture = renderTexture;
                        videoPlayer.Play();
                        image.texture = renderTexture;
                    }
                }
            }
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
