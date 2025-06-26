using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    [SerializeField]
    TMP_Text dialogText;

    [SerializeField]
    Button okButton;

    [SerializeField]
    Button yesButton;

    [SerializeField]
    Button noButton;

    [SerializeField]
    RectTransform dialogPanel;

    Image backgroundImage;

    static Dialog instance;
    void Start()
    {
        instance = this;
        backgroundImage = GetComponent<Image>();

        noButton.onClick.AddListener(() =>
        {
            SetDisplay(false);
        });
        SetDisplay(false);
    }

    void Update()
    {
        if (!dialogPanel.gameObject.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (!RectTransformUtility.RectangleContainsScreenPoint(dialogPanel, mousePos, null))
            {
                SetDisplay(false);
            }
        }
    }

    void SetDisplay(bool isActive)
    {
        dialogPanel.gameObject.SetActive(isActive);
        backgroundImage.enabled = isActive;
    }

    void ClearButtons()
    {
        okButton.onClick.RemoveAllListeners();
        yesButton.onClick.RemoveAllListeners();
    }

    void _ShowDialogOkay(string text, UnityAction onOkClicked)
    {
        dialogText.text = text;

        ClearButtons();
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        okButton.gameObject.SetActive(true);

        okButton.onClick.AddListener(() =>
        {
            onOkClicked?.Invoke();
            SetDisplay(false);
        });

        SetDisplay(true);
    }

    void _ShowDialogConfirm(string text, UnityAction onConfirm)
    {
        dialogText.text = text;

        ClearButtons();
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        okButton.gameObject.SetActive(false);

        yesButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            SetDisplay(false);
        });

        SetDisplay(true);
    }


    public static void ShowDialogOkay(string text, UnityAction onOkClicked = null)
    {
        if (instance == null)
        {
            Debug.LogError("Dialog instance is not set. Make sure the Dialog script is attached to a GameObject in the scene.");
            return;
        }
        instance._ShowDialogOkay(text, onOkClicked);
    }

    public static void ShowDialogConfirm(string text, UnityAction onConfirm)
    {
        if (instance == null)
        {
            Debug.LogError("Dialog instance is not set. Make sure the Dialog script is attached to a GameObject in the scene.");
            return;
        }
        instance._ShowDialogConfirm(text, onConfirm);
    }



}
