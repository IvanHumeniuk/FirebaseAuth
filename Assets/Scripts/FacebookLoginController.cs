using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using UnityEngine.UI;
using System;
using Firebase.Extensions;
using Firebase.Auth;

public class FacebookLoginController : MonoBehaviour
{
    public event Action<LoginResultCode, string> OnLoginResult;

    public TMPro.TextMeshProUGUI infoText;
    private FirebaseUser user;
    private FirebaseAuth auth;


    private Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    public string uid { get { return user == null ? string.Empty : user.UserId; } }

	private void Start()
	{
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance; 
            }
            else
            {
                AddToInformation(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

	private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FBlogin();
        }
        else
        {
            AddToInformation("Failed to Initialize the Facebook SDK");
            OnLoginResult?.Invoke(LoginResultCode.Failed, "Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            //Time.timeScale = 0;
            AddToInformation($"Time.timeScale = 0");
        }
        else
        {
            // Resume the game - we're getting focus again
            //Time.timeScale = 1;
            AddToInformation($"Time.timeScale = 1");
        }
    }

    public void SignInWithFacebook()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FBlogin();
        }   
    }

    private void FBlogin()
	{
        FB.ActivateApp();

        List<string> permissions = new List<string>();
        permissions.Add("public_profile");

        FB.LogInWithReadPermissions(permissions, AuthCallback);
    }

    public void LogOut()
    {
        if (FB.IsLoggedIn)
            FB.LogOut();
    }

    private void AuthCallback(ILoginResult result)
    {
        AddToInformation("AuthCallback");

        if (FB.IsLoggedIn)
        {
            if(result.Cancelled)
			{
                AddToInformation("User cancelled login");
                OnLoginResult?.Invoke(LoginResultCode.Canceled, "User cancelled login");
                return;
            }

            string token = result.AccessToken.TokenString;
            AddToInformation("result: " + result.RawResult);
   
           Credential credential = FacebookAuthProvider.GetCredential(token);

            auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    AddToInformation("ERROR SignInWithCredentialAsync was canceled.");
                    OnLoginResult?.Invoke(LoginResultCode.Canceled, "SignInWithCredentialAsync was canceled.");

                    return;
                }
                if (task.IsFaulted)
                {
                    AddToInformation("ERROR SignInWithCredentialAsync encountered an error: " + task.Exception);
                    OnLoginResult?.Invoke(LoginResultCode.Failed, "SignInWithCredentialAsync encountered an error: " + task.Exception);

                    return;
                }

                user = task.Result;
                AddToInformation($"User signed in successfully: {user.DisplayName} ({user.UserId})");
                OnLoginResult?.Invoke(LoginResultCode.Success, "User signed in successfully");
            });
        }
        else
        {
            AddToInformation("User cancelled login");
            OnLoginResult?.Invoke(LoginResultCode.Canceled, "User cancelled login");
        }
    }
	private void AddToInformation(string str)
    {
        infoText.text += "\n" + str;
    }
}