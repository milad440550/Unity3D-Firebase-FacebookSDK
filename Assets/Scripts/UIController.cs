using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour {

	[Header("Forgot panel panel")]
	public InputField forgotPasswordText;
	public Text forgotPasswordResult;
	[Space(5)]
	[Header("Sign Up dropdowns panel")]
	public Dropdown dayDropDown;
	public Dropdown monthDropDown;
	public Dropdown yearDropDown;
	public GameObject loadingPanel;

	[Space(5)]
	public AuthPanel signInPanel;
	public AuthPanel signUpPanel;
	public MainMenuPanel userPanel;

	[System.Serializable]
	public class AuthPanel
	{
		public GameObject panel;
		public InputField email;
		public InputField password;
		public Text result;
	}

	[System.Serializable]
	public class MainMenuPanel
	{
		public GameObject panel;
		public Text userEmailText;
		public Text pointsText;	
		public Text birthDayText;	
		
	}
	
}
