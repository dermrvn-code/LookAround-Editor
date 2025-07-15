using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;


public class ModelUploader : MonoBehaviour
{
    public Button uploadButton;
    public Button clearButton;
    public RawImage displayModel;
    public TMP_Text modelPath;
    public TMP_Text label;

    ModelManager modelManager;

    public string labelText = "";

    string emptyModelPath;
    public Animator animator;

    public UnityEvent<string, string> OnValueChanged = new UnityEvent<string, string>();

    public string value;

    public string modelName;

    void Awake()
    {
        modelName = $"model_preview_{Random.Range(0, 1000)}";
        modelManager = FindObjectOfType<ModelManager>();

        animator.SetBool("Open", false);
        label.text = labelText;

        uploadButton.onClick.AddListener(UploadModel);
        clearButton.onClick.AddListener(Clear);

        emptyModelPath = modelPath.text;
    }

    void UploadModel()
    {
        var extensions = new[] { new ExtensionFilter("GLTF Model", new string[] { "gltf" }) };
        StandaloneFileBrowser.OpenFilePanelAsync("Model hochladen", "", extensions, false, (string[] paths) =>
        {
            if (paths.Length == 1)
            {
                string path = paths[0];
                DisplayModel(path);
            }
            else
            {
                Clear();
            }
        });
    }

    void DisplayModel(string path, bool preview = true)
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
        modelPath.text = Path.GetFileName(path);

        if (preview)
        {
            ProcessIndicator.Show();
            modelManager.LoadModel(path, modelName, onLoaded: (GameObject obj, RenderTexture rt) =>
            {
                DisplayTexture(rt);
                OnValueChanged?.Invoke(path, modelName);
                value = path;
                ProcessIndicator.Hide();
            }, preview: true);
        }
        else
        {
            RenderTexture rt = modelManager.GetThumbnail(modelName);
            if (rt == null)
            {
                Debug.LogError("Thumbnail not found for model: " + modelName);
                return;
            }
            DisplayTexture(rt);
        }

    }

    void DisplayTexture(RenderTexture rt)
    {
        if (rt == null)
        {
            Debug.LogError("RenderTexture is null.");
            return;
        }

        displayModel.texture = rt;

        float width = displayModel.rectTransform.rect.width;
        float newWidth = width / rt.width + 0.5f;
        displayModel.uvRect = new Rect(-((newWidth - 1) / 2), 0, newWidth, 1);

        animator.SetBool("Open", true);
    }

    void Clear()
    {
        StartCoroutine(_Clear());
    }

    IEnumerator _Clear()
    {
        modelPath.text = emptyModelPath;
        animator.SetBool("Open", false);
        OnValueChanged?.Invoke(string.Empty, string.Empty);
        yield return new WaitForSeconds(1f);

        displayModel.texture = null;
        modelManager.RemovePreviewModel(modelName);
    }


    public void Initialize(string modelName, string label = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            labelText = label;
            this.label.text = label;
        }

        string path = modelManager.GetModelPath(modelName);
        this.modelName = modelName;
        value = path;
        StartCoroutine(_DisplayModel(path, false));
    }

    IEnumerator _DisplayModel(string path, bool preview = true)
    {
        yield return new WaitForSeconds(0.1f);
        DisplayModel(path, preview);
    }
}
