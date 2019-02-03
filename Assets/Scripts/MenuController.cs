using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MenuController : MonoBehaviour {

	[SerializeField] private AuthenticationController authController;
	[SerializeField] private DatabaseController databaseController;
	[SerializeField] private UIController uiController;

	List<string> monthsList = new List<string>(){"Month"};
	List<string> yearsList = new List<string>(){"Year"};
	List<string> daysList = new List<string>(){"Day"};

	private User currentUser;
	public User CurrentUser {
		get
		{
			return currentUser;
		}
		set
		{
			currentUser = value;
			UnityMainThreadDispatcher.Instance().Enqueue(UpdateUserUI());
		}
	}
	private int points;
	public int Points 
	{
		get
		{
			return points;
		}
		set
		{
            points = value;
			uiController.userPanel.pointsText.text = "Points: " + points.ToString();
        }
	}
	private string userEmail;
	public string UserEmail 
	{
		get
		{
			return userEmail;
		}
		set
		{
            userEmail = value;
			uiController.userPanel.userEmailText.text = "Email: " + userEmail.ToString();
        }
	}
	private string birthday;
	public string Birthday 
	{
		get
		{
			return birthday;
		}
		set
		{
            birthday = value;
			uiController.userPanel.birthDayText.text = "Birthday: " + birthday.ToString();
        }
	}


	void Start()
	{
		PopulateList();
	}
	
	#region Authentication
	public void SignUpWithEmail()
	{
		//Clearing result text if there was any message before
		UnityMainThreadDispatcher.Instance().Enqueue(ShowSignUpResults(string.Empty));
		
		string email = uiController.signUpPanel.email.text;
		string password = uiController.signUpPanel.password.text;
		if(!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
		{	 
			int day, month, year;
			try {
				day = int.Parse(daysList[uiController.dayDropDown.value]);
				month = int.Parse(monthsList[uiController.monthDropDown.value]);
				year = int.Parse(yearsList[uiController.yearDropDown.value]);
			}
			catch {
				string errorText = "Please Fill In Your Birthday!";
				UnityMainThreadDispatcher.Instance().Enqueue(ShowSignUpResults(errorText));
				return;
			}
			DateTime birthday = new DateTime(year,month,day);

			//Showing loading panel
			UnityMainThreadDispatcher.Instance().Enqueue(ShowLoadingPanel(true));        
			authController.SignUpWithEmail(email, password, (userID, _resultText, taskCompleted)  =>
			{
				UnityMainThreadDispatcher.Instance().Enqueue(ShowSignUpResults(_resultText));
				if (taskCompleted)
				{
					//Adding user in Database after signing up 
					databaseController.AddUser(userID, email, birthday, userData =>
					{
						//Subscribing to the SetValueChange event, if value changes in DB it
						SetUserSettings(userData);
					});
				}
				UnityMainThreadDispatcher.Instance().Enqueue(ShowLoadingPanel(false));        
			});
		}
		else {
			string errorText = "Please Fill In Email and Password Fields!";
			UnityMainThreadDispatcher.Instance().Enqueue(ShowSignUpResults(errorText));
		}
	}
	public void SignInWithEmail()
	{
		//Clearing result text if there was any message before
		UnityMainThreadDispatcher.Instance().Enqueue(ShowSignInResults(string.Empty));
		
		string email = uiController.signInPanel.email.text;
		string password = uiController.signInPanel.password.text;

		if(!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
		{	 
			//Showing Loading Panel
			UnityMainThreadDispatcher.Instance().Enqueue(ShowLoadingPanel(true));        
			
			authController.SignInEmailUser(email, password, (userID, result) =>
			{
				UnityMainThreadDispatcher.Instance().Enqueue(ShowSignInResults(result));
				//If user signed in successfully
				if(!string.IsNullOrEmpty(userID))
				{	
                    databaseController.FetchUserData(userID, userData =>
					{
						SetUserSettings(userData);
					});
				}
				UnityMainThreadDispatcher.Instance().Enqueue(ShowLoadingPanel(false));
			});
		}
		else {
			string errorText = "Please Fill In Email and Password Fields!";
			UnityMainThreadDispatcher.Instance().Enqueue(ShowSignInResults(errorText));
		}
	}
	#endregion
	private void CheckForDataChange(string path, Dictionary<string, object> data)
	{
		if(path == databaseController.currentUserID)
		{
			User userData = new User(data);
			CurrentUser = userData;
		}
	}
	private void SetUserSettings(User userData)
	{
		//This function should be dispatched from Main thread 
		UnityMainThreadDispatcher.Instance().Enqueue(ShowLoadingPanel(false));
		UnityMainThreadDispatcher.Instance().Enqueue(ShowMenuPanel());
        SubscribeToValueChange();
    }
	private IEnumerator ShowSignUpResults(string result)
	{
		uiController.signUpPanel.result.text = result;
		yield return null;
	}
	private IEnumerator ShowSignInResults(string result)
	{
		uiController.signInPanel.result.text = result;
		yield return null;
	}
	IEnumerator UpdateUserUI()
	{
		UserEmail = CurrentUser.email;
		Birthday = CurrentUser.birthday;
		Points = CurrentUser.points;
		yield return null;
	}
	private IEnumerator ShowMenuPanel()
	{
		uiController.signInPanel.panel.SetActive(false);
		uiController.signUpPanel.panel.SetActive(false);
		uiController.userPanel.panel.SetActive(true);
		yield return null;
	}
	private IEnumerator ShowLoadingPanel(bool active)
	{
		uiController.loadingPanel.SetActive(active);
		yield return null;
	}

	private void SubscribeToValueChange()
	{
        string path = string.Format("{0}/{1}",Consts.DBPath.users, databaseController.currentUserID);	
		//Subscribing to Database to raise event when this path's value changes
        databaseController.SetDataChange(path);
        databaseController.dataChanged += CheckForDataChange;
    }
	void OnDestroy()
	{
		if(CurrentUser != null)
			databaseController.dataChanged -= CheckForDataChange;
	}

	void PopulateList()
	{
		//Adding Days to the drop down
		int totalDays = 31;
		for (int day = 1; day <= totalDays; day++)
		{
			daysList.Add(day.ToString());
		}
		uiController.dayDropDown.AddOptions(daysList);

		//Adding Months to the drop down
		int totalMonths = 12;
		for (int month = 1; month <= totalMonths; month++)
		{
			monthsList.Add(month.ToString());	
		}
		uiController.monthDropDown.AddOptions(monthsList);

		//Adding Years to the drop down
		int minYear = 1970;
		int totalYears = 2019;
		for (int year = minYear; year <= totalYears; year++)
		{
			yearsList.Add(year.ToString());
		}
		uiController.yearDropDown.AddOptions(yearsList);
	}

	private void SignIn(string userID, string result, bool taskCompleted)
	{
		UnityMainThreadDispatcher.Instance().Enqueue(ShowSignInResults(result));
		//If user signed in successfully
		if(!string.IsNullOrEmpty(userID))
		{	
			databaseController.FetchUserData(userID, userData =>
			{
				SetUserSettings(userData);
			});
		}
		UnityMainThreadDispatcher.Instance().Enqueue(ShowLoadingPanel(false));
	}
	public void ForgetPassword()
	{
		string email = uiController.forgotPasswordText.text;
		if (!string.IsNullOrEmpty(email))
		{
			authController.ForgotPassword(email, result =>
			{
				UnityMainThreadDispatcher.Instance().Enqueue(ShowForgotPassResult(result));
			});
		}
		else
		{
			string result = "Please enter your email!";
			UnityMainThreadDispatcher.Instance().Enqueue(ShowForgotPassResult(result));
		}
	
	}
	private IEnumerator ShowForgotPassResult(string message)
	{
		uiController.forgotPasswordResult.text = message;
		yield return null;
	}
	void OnEnable()
	{
		AuthenticationController.signInCompleted += SignIn;
	}
	void OnDisable()
	{
		AuthenticationController.signInCompleted -= SignIn;
	}
}
