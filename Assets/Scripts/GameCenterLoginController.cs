using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase.Auth;
using System;
using System.Threading.Tasks;

#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif

public class GameCenterLoginController : MonoBehaviour
{
    public event Action<LoginResultCode, string> OnLoginResult;

    public TMPro.TextMeshProUGUI infoText;
    private FirebaseUser user;
    private FirebaseAuth auth;

    private Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    public string uid { get { return user == null ? string.Empty : user.UserId; } }

    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
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

    public void AuthenticateToGameCenter()
    {
#if UNITY_IOS
        Social.localUser.Authenticate(success => {
          AddToInformation("Game Center Initialization Complete - Result: " + success);
        });

        if (Social.localUser.authenticated)
            SignInWithGameCenterAsync();
#else
        AddToInformation("Game Center is not supported on this platform.");
#endif
    }

    // Sign out the current user.
    public void LogOut()
    {
        AddToInformation("Signing out.");
        auth.SignOut();
    }

    public Task SignInWithGameCenterAsync()
    {
        var credentialTask = GameCenterAuthProvider.GetCredentialAsync();
        var continueTask = credentialTask.ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted)
                return null;

            if (task.Exception != null)
                AddToInformation("GC Credential Task - Exception: " + task.Exception.Message);

            var credential = task.Result;

            var loginTask = auth.SignInWithCredentialAsync(credential);
            return loginTask.ContinueWithOnMainThread(HandleSignInWithUser);
        });

        return continueTask;
    }

    void HandleSignInWithUser(Task<FirebaseUser> task)
    {
        if (LogTaskCompletion(task, "Sign-in"))
        {
            AddToInformation(String.Format("{0} signed in", task.Result.DisplayName));
        }
    }

    private void AddToInformation(string str)
    {
        infoText.text += "\n" + str;
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            AddToInformation(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            AddToInformation(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    authErrorCode = String.Format("AuthError.{0}: ",
                      ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                AddToInformation(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            AddToInformation(operation + " completed");
            complete = true;
        }
        return complete;
    }
}
