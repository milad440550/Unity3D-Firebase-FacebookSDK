using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class User {
	public string email;
	public int points;
	public string birthday;
	public User() { }
	public User(Dictionary<string, object> userData)
	{
		this.email = userData[Consts.DBPath.email].ToString();
		this.points = int.Parse(userData[Consts.DBPath.points].ToString());
		this.birthday = userData[Consts.DBPath.birthday].ToString();
	}

	public User(string email, string birthday) {
		this.email = email;
		this.birthday = birthday;
	}
}