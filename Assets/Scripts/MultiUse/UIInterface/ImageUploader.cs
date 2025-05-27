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
    Animator animator;

    public UnityEvent<string> OnValueChanged = new UnityEvent<string>();

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;
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
                if (string.IsNullOrEmpty(path))
                {
                    Clear();
                    return;
                }

                imagePath.text = Path.GetFileName(path);
                displayImage.sprite = LoadSpriteFromFile(path);

                OnValueChanged?.Invoke(path);

                animator.enabled = true;
                animator.SetBool("Open", true);
            }
            else
            {
                Clear();
            }
        });
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
}
