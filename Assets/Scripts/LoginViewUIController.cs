using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoginViewUIController : MonoBehaviour
{
    [SerializeField] private LoginProviderType loginProvider;

    [SerializeField] private Button facebookSignInButton;
    [SerializeField] private Button signOutButton;
    [SerializeField] private FacebookLoginController facebookLoginController;

    [SerializeField] private GameObject loadingView;

    [SerializeField] private TMPro.TextMeshProUGUI infoText;
    private void Start()
    {
        facebookSignInButton.onClick.AddListener(OnLoginFacebook);
        signOutButton.onClick.AddListener(OnLogOut);
        facebookLoginController.OnLoginResult += OnFasebookLoginResult;
    }

    private void OnDestroy()
    {
        facebookSignInButton.onClick.RemoveListener(OnLoginFacebook);
        signOutButton.onClick.RemoveListener(OnLogOut);
        facebookLoginController.OnLoginResult -= OnFasebookLoginResult;
    }
    
    public void OnLoginFacebook()
	{
        if (PlayerDataHolder.loginProvider != LoginProviderType.None)
            return;

        ShowLoadingView();

        Log($"OnLoginFacebook");
        facebookLoginController.SignInWithFacebook();         
    }


    private void OnFasebookLoginResult(LoginResultCode code, string message)
    {
        Log($"OnFasebookLoginResult");
        switch (code)
        {
            case LoginResultCode.Success:
                {
                    loginProvider = LoginProviderType.Facebook;
                    ConnectWithUID(facebookLoginController.uid);
                    break;
                }
            case LoginResultCode.Canceled:
                {
                    Log(message);
                    HideLoadingView();

                    break;
                }
            case LoginResultCode.Failed:
                {
                    Log(message);
                    HideLoadingView();

                    break;
                }
            default:
                break;
        }

    }

    private void ConnectWithUID(string uid)
    {
        if (string.IsNullOrEmpty(uid))
        {
			Log($"Error uid: {string.IsNullOrEmpty(uid)}");
            return;
        }

        Log($"ConnectWithUID {uid}");

        PlayerDataHolder.uid = uid;
        PlayerDataHolder.loginProvider = loginProvider;

        HideLoadingView();
    }

    public void OnLogOut()
	{
        if (loginProvider == LoginProviderType.None)
            return;

        Log($"OnLogOut");

        switch (loginProvider)
		{
			case LoginProviderType.Facebook:
                {
                    facebookLoginController.LogOut();
                    break;
                }
			case LoginProviderType.Email:
			case LoginProviderType.Guest:
            case LoginProviderType.GameCenter:
            case LoginProviderType.None:
			default:
				break;
		}

		loginProvider = LoginProviderType.None;

        StartCoroutine(Restart());
    }

    private IEnumerator Restart()
	{
        ShowLoadingView();
        yield return new WaitForSeconds(1);

        PlayerDataHolder.uid = string.Empty;
        PlayerDataHolder.loginProvider = LoginProviderType.None;

        SceneManager.LoadScene(0);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Log("In focus");
        }
        else
        {
            Log("Not In focus");
        }
    }

    public void ShowLoadingView()
	{
        loadingView.SetActive(true);
    }

    public void HideLoadingView()
    {
        loadingView.SetActive(false);
    }

    private void Log(string str)
    {
        infoText.text += "\n" + DateTime.UtcNow.ToShortTimeString() + "  " + str;
    }
}
