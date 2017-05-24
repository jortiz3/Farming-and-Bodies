using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class Skill
{
	private string name;
	private float exp;
	private float expToLevel;
	private int currLevel;
	private int maxLevel;

	public string Name { get { return name; } }
	public bool IsAtMaxLevel { get { return currLevel >= maxLevel ? true : false; } }
	public int CurrentLevel { get { return currLevel; } }
	public float CurrentExperience { get { return exp; } }
	public float RequiredExperience { get { return expToLevel; } }
	public float SliderValue { get { return exp / expToLevel; } }

	public Skill(string Name, int MaximumLevelPossible)
	{
		name = Name;
		currLevel = 1;
		maxLevel = MaximumLevelPossible;

		exp = 0;
		expToLevel = 100;
	}

	public void AddExp(float amount)
	{
		exp += amount;

		while (exp >= expToLevel && !IsAtMaxLevel) {
			exp -= expToLevel;
			currLevel++;
			global.console.AddMessage(name + " level increased to " + currLevel + "!");
			global.gameManager.BroadcastActionCompleted(name + " " + currLevel);
		}

		//this keeps the value within 2 decimal places
		if (exp % 1 != 0) {
			int tempExp = (int)(exp * 100);
			exp = tempExp / 100.0f;
		}
	}

	public void SetLevelTo(int lv)
	{
		currLevel = lv;
		exp = 0;
	}
}
