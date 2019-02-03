using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
public class DatabaseController : MonoBehaviour {
	public delegate void DataChanged(string path, Dictionary<string, object> data);
	public event DataChanged dataChanged;

	private DatabaseReference reference = null;
	public string currentUserID = string.Empty;
	private const string FirebaseURL = "https://DATABASE_URL.firebaseio.com/";

	void Start()
	{
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(FirebaseURL);
		reference = Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference;
	}
	public void AddUser(string id, string email, DateTime birthday, Action<User> callback)
	{
		this.currentUserID = id;
		User newUser = new User(email,birthday.ToShortDateString());
		string json = JsonUtility.ToJson(newUser);
		Debug.Log(json);
		reference.Child(Consts.DBPath.users).Child(id).SetRawJsonValueAsync(json);
		//Returning new User 
		callback(newUser);
	}
	public void FetchUserData(string userID, Action<User> callBack)
	{
		reference.Child(Consts.DBPath.users).Child(userID).GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted) 
			{
				Debug.LogError("Something went wrong while fetching user's data");
				return;
			}
			else if (task.IsCanceled)
			{
				Debug.LogError("Task is canceled while fetching user's data");
				return;
			}
			else if (task.IsCompleted) 
			{
			    this.currentUserID = userID;
				User userData = new User(ConvertSnapshotToDict(task.Result));
                callBack(userData);
			}
		});
	}
	/// <summary>This function will return user's data if it exist</summary>
	public void UserExist(string ID, Action<bool> callback)
	{
		string path = string.Format("{0}/{1}",Consts.DBPath.users,ID);
		reference.Child(path).GetValueAsync().ContinueWith (task => 
		{
			if (task.IsFaulted)
			{
				Debug.LogError("Faulted");
				callback(false);
			}
			else if(task.IsCanceled)
			{
				Debug.LogError("Task is canceled");
				callback(false);
			}
			else if (task.IsCompleted) 
			{
				if(task.Result.Exists)
				{
					callback(true);
				}
				else
				{
					callback(false);
				}
			}
		});
		
	}
	public void AddOrOverWriteData(string path, string newData)
	{
		reference.Child(path).SetValueAsync(newData);
	}
	public void ReadData(string path, Action<string> callback)
	{
		object data = null;
		reference.Child(path).GetValueAsync().ContinueWith(task =>
		{
			if (task.IsFaulted) 
			{
				Debug.LogError("Something went wrong while reading data");
				return;
			}
			else if (task.IsCanceled)
			{
				Debug.LogError("Task is canceled while reading data");
				return;
			}
			else if(task.IsCompleted)
			{	
				data = task.Result.Value;
				callback(data.ToString());
			}
		});
	}
	public void SetDataChange(string path)
	{
		reference.Child(path).ValueChanged += HandleValueChanged;
	}
	/// <summary>This function will add data in Database</summary>
	public void PushData(string path, Dictionary<string, object> newData)
	{
		reference.Child(path).UpdateChildrenAsync(newData);
	}
	public void DeleteData(string path)
	{
		reference.Child(path).RemoveValueAsync();
	}
	private void HandleValueChanged(object sender, ValueChangedEventArgs args) 
	{
		if (args.DatabaseError != null) 
		{
       		Debug.LogError(args.DatabaseError.Message);
        	return;
    	}
		Dictionary<string, object> newData = new Dictionary<string, object>();
		newData = args.Snapshot.Value as Dictionary<string, object>;
		
		//Raising dataChanged event and passing path of the data and the data itself
		dataChanged(args.Snapshot.Key, newData);	
	}
	//Converts Datasnapshot to dictionary and return it as a user data
	private Dictionary<string, object> ConvertSnapshotToDict(DataSnapshot snapshot)
	{
		Dictionary<string, object> userDict = new Dictionary<string, object>();
		userDict = snapshot.Value as Dictionary<string, object>;	
		return userDict;
	}
	void OnDestroy()
	{
		reference = null;
	}
}
