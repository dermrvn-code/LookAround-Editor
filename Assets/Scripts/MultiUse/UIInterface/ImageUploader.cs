using System.Collections;
using System.Collections.Generic;
using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ImageUploader : MonoBehaviour
{

    public Button uploadButton;
    public Button clearButton;
    public Image displayImage;
    public TMP_Text imagePath;
    public TMP_Text label;

    public string labelText = "";

    string emptyImagePath;
    public Animator animator;

    public UnityEvent<string> OnValueChanged = new UnityEvent<string>();

    public string value;

    void Awake()
    {
        animator.SetBool("Open", false);
        label.text = labelText;

        uploadButton.onClick.AddListener(UploadImage);
        clearButton.onClick.AddListener(Clear);

        emptyImagePath = imagePath.text;
    }

    void UploadImage()
    {
        StandaloneFileBrowser.OpenFilePanelAsync("Bild hochladen", "", new[] { new ExtensionFilter("Image Files", "jpg", "png") }, false, (string[] paths) =>
        {
            if (paths.Length == 1)
            {
                string path = paths[0];
                value = path;
                DisplayImage(path);
            }
            else
            {
                Clear();
            }
        });
    }

    void DisplayImage(string path)
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
        imagePath.text = Path.GetFileName(path);
        Sprite sprite = LoadSpriteFromFile(path);

        if (sprite != null)
        {
            displayImage.sprite = sprite;
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
        imagePath.text = emptyImagePath;
        animator.SetBool("Open", false);
        OnValueChanged?.Invoke(string.Empty);
        yield return new WaitForSeconds(1f);

        displayImage.sprite = null;
    }

    // Loads a sprite from a file path
    Sprite LoadSpriteFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
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
        StartCoroutine(_DisplayImage(path));
    }

    IEnumerator _DisplayImage(string path)
    {
        yield return new WaitForSeconds(0.1f);
        DisplayImage(path);
    }
}
