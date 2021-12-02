using System;
using UnityEngine;
using UnityEngine.UI;

public class LoginResultDialogueUIController : MonoBehaviour
{
    [SerializeField] private GameObject view;

    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;
    [SerializeField] private Button dialogueAcceptButton;
    [SerializeField] private Button dialogueCancelButton;

    private Action acceptCallback;
    private Action cancelCallback;

    private void Start()
    {
        dialogueAcceptButton.onClick.AddListener(OnAcceptClick);
        dialogueCancelButton.onClick.AddListener(OnCancelClick);
    }

    private void OnDestroy()
    {
        dialogueAcceptButton.onClick.RemoveListener(OnAcceptClick);
        dialogueCancelButton.onClick.RemoveListener(OnCancelClick);
    }

    public void ShowDialogue(string message, Action acceptCallback = null, Action cancelCallback = null)
    {
        dialogueText.text = message;

        this.acceptCallback = acceptCallback;
        this.cancelCallback = cancelCallback;

        dialogueAcceptButton.gameObject.SetActive(true);
        
        bool activateCancelButton = acceptCallback != null;
        dialogueCancelButton.gameObject.SetActive(activateCancelButton);

        view.SetActive(true);
    }

    public void HideDialogue()
	{
        view.SetActive(false);
        
        acceptCallback = null;
        cancelCallback = null;

        dialogueAcceptButton.gameObject.SetActive(false);
        dialogueCancelButton.gameObject.SetActive(false);
    }

    private void OnAcceptClick()
    {
        acceptCallback?.Invoke();
        HideDialogue();
    }

    private void OnCancelClick()
    {
        cancelCallback?.Invoke();
        HideDialogue();
    }

}