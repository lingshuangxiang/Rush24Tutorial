using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpPanel : MonoBehaviour
{
    public Text Title;
    public Text Description;
    public Button ConfirmButton;
    public Button CancelButton;

    public void Show(string title, string description, Action onConfirm = null, Action onCancel = null)
    {
        gameObject.SetActive(true);
        Title.text = title;
        Description.text = description;
        ConfirmButton.onClick.RemoveAllListeners();
        ConfirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            gameObject.SetActive(false);
            ConfirmButton.onClick.RemoveAllListeners();

        });
        CancelButton.onClick.RemoveAllListeners();
        CancelButton.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            gameObject.SetActive(false);
            CancelButton.onClick.RemoveAllListeners();

        });
    }
}
