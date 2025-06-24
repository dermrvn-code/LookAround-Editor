using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectFolder : MonoBehaviour
{

    public Button uploadButton;
    public Button clearButton;
    public TMP_Text folderPath;
    public TMP_Text label;

    public string labelText = "";
    string emptyPathPlaceholder;

    public UnityEvent<string> OnValueChanged = new UnityEvent<string>();

    public string value;

    void Awake()
    {
        label.text = labelText;

        uploadButton.onClick.AddListener(UploadImage);
        clearButton.onClick.AddListener(Clear);

        emptyPathPlaceholder = folderPath.text;
    }

    void UploadImage()
    {
        StandaloneFileBrowser.OpenFolderPanelAsync("Ordner auswählen", "", multiselect: false, (string[] paths) =>
        {
            if (paths.Length == 1)
            {
                string path = paths[0];
                value = path;
                OnValueChanged?.Invoke(path);
                DisplayPath(path);
            }
            else
            {
                Clear();
            }
        });
    }

    void DisplayPath(string path, int length = 2)
    {
        if (string.IsNullOrEmpty(path))
        {
            folderPath.text = emptyPathPlaceholder;
        }
        else
        {
            var parts = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (parts.Length > length)
            {
                folderPath.text = "...";
                for (int i = parts.Length - length; i < parts.Length; i++)
                {
                    folderPath.text += "/";
                    folderPath.text += parts[i];
                }
                folderPath.text += "/";
            }
            else
            {
                folderPath.text = path;
            }
        }
    }

    void Clear()
    {

        OnValueChanged?.Invoke(string.Empty);
    }

    public void Initialize(string path, string label = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            labelText = label;
            this.label.text = label;
        }

        value = path;
        DisplayPath(path);
    }
}
