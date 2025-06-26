using System.Collections;
using System.Collections.Generic;
using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class MediaUploader : MonoBehaviour
{

    public Button uploadButton;
    public Button clearButton;
    public RawImage displayMedia;
    public TMP_Text mediaPath;
    public TMP_Text label;

    [SerializeField]
    Texture videoTexture;

    VideoPlayer videoPlayer;

    public string labelText = "";

    string emptyMediaPath;
    public Animator animator;

    public UnityEvent<string> OnValueChanged = new UnityEvent<string>();

    public string value;

    void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GameObject.Find("UIVideoPlayer")?.GetComponent<VideoPlayer>();
        }
        animator.SetBool("Open", false);
        label.text = labelText;

        uploadButton.onClick.AddListener(UploadMedia);
        clearButton.onClick.AddListener(Clear);

        emptyMediaPath = mediaPath.text;
    }

    void UploadMedia()
    {
        var extensions = new[] { new ExtensionFilter("Bilder und Videos", "jpg", "png", "jpeg", "mp4", "mov") };
        StandaloneFileBrowser.OpenFilePanelAsync("Medium hochladen", "", extensions, false, (string[] paths) =>
        {
            if (paths.Length == 1)
            {
                string path = paths[0];
                value = path;
                DisplayMedia(path);
            }
            else
            {
                Clear();
            }
        });
    }

    void DisplayMedia(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Clear();
            return;
        }

        if (!File.Exists(path))
        {
            Clear();
            return;
        }
        mediaPath.text = Path.GetFileName(path);

        if (path.EndsWith(".mp4") || path.EndsWith(".mov"))
        {
            if (videoPlayer != null)
            {
                DisplayVideo(path);
            }
            else
            {
                Debug.LogWarning("VideoPlayer not found in the scene. Please add a VideoPlayer component to a GameObject named 'UIVideoPlayer'.");
                Clear();
            }
        }
        else
        {
            DisplayImage(path);
        }
    }

    void DisplayImage(string path)
    {
        Texture tex = LoadImageTextureFromFile(path);

        if (tex != null)
        {
            displayMedia.texture = tex;
            animator.SetBool("Open", true);
            OnValueChanged?.Invoke(path);
        }
    }

    void DisplayVideo(string path)
    {
        if (videoPlayer != null)
        {
            videoPlayer.url = path;
            videoPlayer.Play();
            displayMedia.texture = videoTexture;
            animator.SetBool("Open", true);
            OnValueChanged?.Invoke(path);
        }
    }

    void Clear()
    {
        StartCoroutine(_Clear());
    }

    IEnumerator _Clear()
    {
        mediaPath.text = emptyMediaPath;
        animator.SetBool("Open", false);
        OnValueChanged?.Invoke(string.Empty);
        yield return new WaitForSeconds(1f);

        displayMedia.texture = null;
    }

    // Loads a sprite from a file path
    Texture2D LoadImageTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            return tex;
        }
        return null;
    }



    public void Initialize(string path, string label = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            labelText = label;
            this.label.text = label;
        }

        value = path;
        StartCoroutine(_DisplayMedia(path));
    }

    IEnumerator _DisplayMedia(string path)
    {
        yield return new WaitForSeconds(0.1f);
        DisplayMedia(path);
    }
}
