using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase;
using Firebase.Unity.Editor;
using UnityEngine.UI;
public class AuthenticationController : MonoBehaviour {
	private Firebase.Auth.FirebaseAuth auth;
	public delegate void SignedIn(string userID, string result, bool taskCompleted);
	public static event SignedIn signInCompleted;
	void Start()
	{	
		Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
		var dependencyStatus = task.Result;
		if (dependencyStatus == Firebase.DependencyStatus.Available) {
			InitializeFirebase();
		} 
		else {
			// Firebase Unity SDK is not safe to use here.
			Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
		}
        });
	}

	private void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }
	#region Authentications		
	public void SignUpWithEmail(string email, string password, Action<string, string, bool> callBack)
	{
		Firebase.Auth.FirebaseUser newUser = null;
		string result = string.Empty;
		auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
			if (task.IsCanceled) {
				result = ("Create User With Email And Password Async was canceled.");
				callBack("",result,false);
				return;
			}
			else if (task.IsFaulted) {
				result = ("Create User With Email encountered an error: " + task.Exception);
				callBack("", result, false);
				return;
			}
			else if (task.IsCompleted) {
				// Firebase user has been created.
				newUser = task.Result;
				SendVerificationEmail();
				result = string.Format("User created successfully: {0} ({1})",
					newUser.DisplayName, newUser.UserId);
				//returning userID after successfully signing up
				callBack(newUser.UserId, result, true);
			}
		});
	}
	public void SignInEmailUser(string email, string password, Action<string, string> callBack)
	{
		Firebase.Auth.FirebaseUser newUser = null;
		string result = string.Empty;
		auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => 
		{
			if (task.IsCanceled) 
			{
				result = "Sign In With Email And Password Async was canceled.";
				callBack("", result);
				return;
			}
			else if (task.IsFaulted) {
				result = "Sign In With Email And Password Async encountered an error: " + task.Exception.Message;
				callBack("", result);
				return;
			}
			else if(task.IsCompleted) {
				newUser = task.Result;
				result = string.Format("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
				callBack(newUser.UserId, result);
				signInCompleted(newUser.UserId,result, true);
			}
		});
	}
	public void SignInAnonymously(Action<string, string> callBack)
	{
		string result = string.Empty;
		auth.SignInAnonymouslyAsync().ContinueWith(task => {
		if (task.IsCanceled) {
			result = ("Sign In Anonymously Async was canceled.");
			return;
		}
		else if (task.IsFaulted) {
			result = ("Sign In Anonymously Async encountered an error: " + task.Exception);
			return;
		}
		else if (task.IsCompleted) {
			Firebase.Auth.FirebaseUser _newUser = task.Result;
			result = string.Format("User signed in successfully: {0} ({1})", _newUser.DisplayName, _newUser.UserId); 
			callBack(_newUser.UserId, result);
		}

		});
	}
	#endregion
	private void SendVerificationEmail()
	{
		auth.CurrentUser.SendEmailVerificationAsync();
	}
	public void ForgotPassword(string emailAddress, Action<string> result)
	{	
		auth.SendPasswordResetEmailAsync(emailAddress).ContinueWith(task => {
			if (task.IsCanceled) {
				result("Send Password Reset Email Async was canceled.");
				return;
			}
			if (task.IsFaulted) {
				result("Send Password Reset Email Async encountered an error: " + task.Exception);
				return;
			}
			result("Password reset email sent successfully.");
		});
	}

}
