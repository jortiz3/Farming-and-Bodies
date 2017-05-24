using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class ItemDatabase : MonoBehaviour
{
	public Item[] items = new Item[0];
	
	public Item GetItem(int id)
	{
		try {
			Item temp = new Item();
			temp.CopyValues(items[id]);
			return temp;
		}
		catch {
			return null;
		}
	}

	//item_name
	public Item GetItem(string name)
	{
		Item temp = new Item ();
		string[] nameSuffix = name.Split (';');
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].name.Equals(nameSuffix[0]))
			{
				if (nameSuffix.Length < 2 || items[i].suffix.Equals(nameSuffix[1]))
				{
					temp.CopyValues(items[i]);
					return temp;
				}
			}
		}
		return null;
	}

	public Item GetItem(string name, string suffix)
	{
		Item temp = new Item ();
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].name.Equals(name) && items[i].suffix.Equals(suffix))
			{
				temp.CopyValues(items[i]);
				return temp;
			}
		}
		return null;
	}

	//item_name;item_suffix
	public int GetItemID(string name)
	{
		string[] nameSuffix = name.Split (';');
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].name.Equals(nameSuffix[0]))
			{
				if (nameSuffix.Length < 2 || items[i].suffix.Equals(nameSuffix[1]))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void Initialize()
	{
		for (int i = 0; i < items.Length; i++)
		{
			items[i].DatabaseID = i;
			items[i].SetPrefab(Resources.Load<GameObject>("Items/" + items[i].name));
		}
	}
}