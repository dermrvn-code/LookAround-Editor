using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum Side
{
    Overview,
    Bundle
}

public class WorldListItem : MonoBehaviour
{
    int id;
    public int Id { get { return id; } }

    public Side side = Side.Overview;

    [SerializeField]
    RawImage startSceneImage;

    [SerializeField]
    TMP_Text worldNameText;

    [SerializeField]
    TMP_Text authorText;

    public string worldName;
    public string author;
    public string location;

    public bool isSelected = false;


    public Color selectedBackground;
    Color normalBackground;
    Image backgroundImage;
    Button button;

    [SerializeField]
    GameObject noImageIcon;

    [SerializeField]
    GameObject loaderIcon;

    [SerializeField]
    VideoPlayer videoPlayer;

    TextureManager textureManager;
    WorldBundleManager worldBundleManager;
    void Awake()
    {
        textureManager = FindObjectOfType<TextureManager>();
        worldBundleManager = FindObjectOfType<WorldBundleManager>();

        backgroundImage = GetComponent<Image>();
        normalBackground = backgroundImage.color;

        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                worldBundleManager.Select(id, isSelected);
            });
        }
    }


    void Update()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedBackground : normalBackground;
        }
    }


    public void Initialize(int id, string worldName, string author, string location, string filePath = null, Side side = Side.Overview)
    {
        this.id = id;
        this.worldName = worldName;
        this.author = author;
        this.side = side;
        this.location = location;

        if (worldNameText != null)
        {
            worldNameText.text = worldName;
        }

        if (authorText != null)
        {
            authorText.text = author;
        }

        if (startSceneImage != null)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                {
                    StartCoroutine(
                        textureManager.GetTexture(filePath, (texture) =>
                        {
                            if (startSceneImage != null && texture != null)
                            {
                                startSceneImage.texture = texture;
                            }
                            else
                            {
                                noImageIcon.SetActive(true);
                            }

                            loaderIcon.SetActive(false);
                        })
                    );
                }
                else if (extension == ".mp4" || extension == ".mov" || extension == ".avi")
                {
                    if (videoPlayer != null)
                    {
                        videoPlayer.enabled = true;
                        videoPlayer.url = filePath;
                        var renderTexture = new RenderTexture(1920, 1080, 0);
                        videoPlayer.targetTexture = renderTexture;
                        videoPlayer.Play();
                        startSceneImage.texture = renderTexture;
                        loaderIcon.SetActive(false);
                    }
                    else
                    {
                        noImageIcon.SetActive(true);
                        loaderIcon.SetActive(false);
                    }
                }
            }
            else
            {
                noImageIcon.SetActive(true);
                loaderIcon.SetActive(false);
            }
        }


    }
}
