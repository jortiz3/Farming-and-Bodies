using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

//rename to playerinventory

//create blank item
//do not allow click panel on blank item
//add item fills in empty slot
//	--array instead of list? list for chests/containers?

//add item methods
//remove item

public class InventoryScript : MonoBehaviour 
{
	//the parent for hotkey bar
	public static Transform hotkeyBarParent;
	//the transform for the right click menu
	public static GameObject UIClickPanel;

	public Item[] items;
	public GameObject[] equipped;
	public int maxNumOfItems = 30;
	public int money = 0;

	public static GameObject CurrentInvSlot;

	public void Initialize () 
	{
		hotkeyBarParent = GameObject.FindGameObjectWithTag("InvUIParent").transform;

		UIClickPanel = GameObject.FindGameObjectWithTag("InvUIClickPanel");
		UIClickPanel.SetActive (false);

		items = new Item[maxNumOfItems];
		equipped = new GameObject[4];
		UpdateInventoryUI();
	}

	public void UpdateInventoryUI()
	{
		UpdateUIInfoString();

		for (int i = 0; i < items.Length; i++)
		{
			UpdateUIElement(i);
		}
	}

	public void UpdateUIInfoString()
	{
		string tempString = "Monies: " + money;
		GameObject.Find("Inventory").transform.FindChild("Info").GetComponent<Text>().text = tempString;
	}

	private void UpdateUIElement(int i)
	{
		if (i > items.Length)
			return;

		GameObject tempObj = hotkeyBarParent.GetChild(i).gameObject;

		if (tempObj != null) {
			tempObj.name = items[i].name;

			Sprite tempSprite = Resources.Load<Sprite>("UI Prefabs/" + items[i].name);
			if (tempSprite != null)
				tempObj.transform.GetChild(0).GetComponent<Image>().sprite = tempSprite;
		}
	}

	public void AddMoney(int amount) {
		if (amount < 1)
			return;
		global.console.AddMessage(amount + " Money Gained");
		if (money < int.MaxValue - 100000)
			money += amount;
		else  {
			money = int.MaxValue - 100000;
			global.console.AddMessage("Max Money Achieved!");
		}
		UpdateUIInfoString ();
	}

	public bool HasItem(string itemName) {
		for (int i = 0; i < items.Length; i++) {
			if (items[i] != null && items[i].name.Equals(itemName))
				return true;
		}
		return false;
	}

	//redo
	public bool CanAddItem(Item item)
	{
		for (int i = 0; i < items.Length; i++) {
			if (items[i] == null) {
				return true;
			} else if (item.name.Equals(items[i].name) && item.stolenFrom.Equals(items[i].stolenFrom)) {
				return true;
			}
		}
		return false;
	}

	public bool AddItem (ItemScript iscript) {
		Item itemToAdd = iscript.GetItem ();
		if (!CanAddItem(itemToAdd))
			return false;
		return AddItem (itemToAdd);
	}

	public bool AddItem(int DatabaseID, int Quantity) {
		Item newItem = global.itemDatabase.GetItem(DatabaseID);
		newItem.quantity = Quantity;
		if (!CanAddItem(newItem))
			return false;
		return AddItem(newItem);
	}

	public bool AddItem(string itemName, int Quantity) {
		Item newItem = global.itemDatabase.GetItem(itemName);
		newItem.quantity = Quantity;
		return AddItem(newItem);
	}

	public bool AddItem (Item itemToAdd) {
		if (!CanAddItem(itemToAdd)) {
			global.console.AddMessage("Cannot pick up " + itemToAdd.name + ". You are already carrying " + maxNumOfItems + " items.");
			return false;
		}
		for(int index = 0; index < items.Length; index++) {
			//if the type of item is already in the list, add to the quantity, add to the weight and destroy the object outside of the inventory
			if (items[index].name.Equals(itemToAdd.name) && items[index].suffix.Equals(itemToAdd.suffix)
			    && items[index].stolenFrom.Equals(itemToAdd.stolenFrom)) {
				items[index].quantity += itemToAdd.quantity;

				for (int i = 1; i <= items[index].quantity; i++)
					global.gameManager.BroadcastActionCompleted("Get " + i + " " + items[index].name);

				if (items[index].stolenFrom != null && !items[index].stolenFrom.Equals("")) {
					for (int i = 1; i <= items[index].quantity; i++)
						global.gameManager.BroadcastActionCompleted("Steal " + i + " " + items[index].name + " from " + items[index].stolenFrom);
				}

				itemToAdd.quantity = 0;
				UpdateUIInfoString();
				UpdateUIElement(index);
				return true;
			}
		}

		Item newItem = new Item();
		newItem.CopyValues(itemToAdd);
		newItem.quantity = itemToAdd.quantity;
		itemToAdd.quantity = 0;

		for (int i = 1; i <= newItem.quantity; i++)
			global.gameManager.BroadcastActionCompleted("Get " + i + " " + newItem.name);

		//adds the item to the list
		//items.Add(newItem);
		UpdateUIInfoString();
		//UpdateUIElement(items.Count - 1);
		return true;
	}

	public void SetCurrentItemSlot(GameObject uiObj) {
		CurrentInvSlot = uiObj;
	}

	public void ClearCurrentItemSlot() {
		CurrentInvSlot = null;
	}

	public void UseItem(GameObject uiObj) {
		int index = FindIndexUsingSlotObject(uiObj);
		if (index < 0) {
			Debug.Log("Unable to find item in inventory to use.");
			return;
		}
		if (items[index].Use()) {
			//updates the current ui element
			UpdateUIElement(index);
			
			//removes the item from inventory and updates ui
			if(items[index].quantity <= 0)
				RemoveItem(uiObj, false);

			UpdateUIInfoString();
		}
	}

	public bool UseCurrentItemSlot() {
		int index = FindIndexUsingSlotObject(CurrentInvSlot);
		if (index < 0) {
			return true;
		}
		bool closePanel = true;
		if (items[index].Use()) {
			//updates the current ui element
			UpdateUIElement(index);
			
			//removes the item from inventory and updates ui
			if(items[index].quantity <= 0) {
				RemoveItem(CurrentInvSlot, false);
			} else {
				closePanel = false;
			}
			
			UpdateUIInfoString();
		}
		return closePanel;
	}


	public void RemoveItem(string itemName, int quantity) {
		for (int i = 0; i < items.Length; i++) {
			if (items[i].name.Equals(itemName)) {
				items[i].quantity--;
				UpdateUIInfoString();
				if (items[i].quantity <= 0) {
					GameObject temp = new GameObject ();
					temp.name = itemName;
					RemoveItem (temp, false);
					return;
				}
				else
					global.gameManager.BroadcastActionCompleted("Get " + items[i].quantity + " " + items[i].name);
				UpdateUIElement(i);
				return;
			}
		}
	}

	public void RemoveItem(GameObject ItemSlotObject) {
		RemoveItem(ItemSlotObject, true);
	}

	public void DropCurrentItemSlot() {
		RemoveItem (CurrentInvSlot);
	}

	public void RemoveItem(GameObject ItemSlotObject, bool spawnPrefab) {
		//index of the item to remove
		int index = FindIndexUsingSlotObject(ItemSlotObject);

		try {
			RemoveItem (items [index], spawnPrefab);
		}
		catch {
			Destroy(ItemSlotObject);
		}
	}

	public void RemoveItem(Item item, bool spawnPrefab)
	{
		try {
			//destroys the ui element attached to the item
			Destroy(hotkeyBarParent.FindChild(item.name).gameObject);
		}
		//ui element was already destroyed
		catch {}

		//removes the item if it is currently equipped
		if (equipped[0] != null && equipped [0].name.Equals (item.name))
			Destroy(equipped[0]);

		if (spawnPrefab) {
			//drops the prefab in front of the player
			GameObject temp = item.Instantiate();
			temp.transform.position = global.playerObject.transform.position + global.playerObject.transform.forward * 3 + global.playerObject.transform.up * 2;
		}

		for (int i = 0; i < items.Length; i++) {
			if (items[i] != null) {
				if (items[i].name.Equals(item.name)) {
					items[i] = null;
					UpdateUIElement(i);
					break;
				}
			}
		}

		UpdateInventoryUI();
	}

	public Item SellItem(GameObject ItemSlotObject, int quantity) {
		int index = FindIndexUsingSlotObject(ItemSlotObject);
		money += quantity * items[index].value;
		items[index].quantity -= quantity;
		if (items[index].quantity <= 0) {
			RemoveItem(ItemSlotObject, false);
			return null;
		}
		Item temp = new Item ();
		temp.CopyValues(items [index]);
		return temp;
	}

	public bool BuyItem(Item itemToBuy) {
		if(money >= itemToBuy.value)
		{
			if (AddItem(itemToBuy))
			{
				money -= itemToBuy.value;
				global.gameManager.BroadcastMessage("Buy " + itemToBuy.name + itemToBuy.suffix);
				return true;
			}
		}
		return false;
	}

	public bool PickUpBodies(int amount) {
		if (amount < 1)
			return true;
		if (AddItem ("Body", amount))
		{
			global.gameManager.BroadcastActionCompleted("Pickup Body");
			UpdateUIInfoString();
			return true;
		}
		return false;
	}

	public void UseBodies(int amount) {
		if (amount < 1)
			return;
		RemoveItem ("Body", amount);
		global.gameManager.BroadcastActionCompleted("Use Body");
		UpdateUIInfoString();
	}

	//finds the index of an item by using the ui object
	public int FindIndexUsingSlotObject(GameObject ItemSlotObject) {
		if (ItemSlotObject == null)
			return -1;
		return FindIndex (ItemSlotObject.name);
	}

	private int FindIndex(string name) {
		//finds the item in the list
		for (int index = 0; index < items.Length; index++)
		{
			if (name.Equals(items[index].name))
			{
				return index;
			}
		}
		return -1;
	}

	public Item GetItemAtIndex(int index)
	{
		return items[index];
	}

	public Item GetItem(GameObject ItemSlotObject)
	{
		return GetItemAtIndex(FindIndexUsingSlotObject(ItemSlotObject));
	}

	public Item GetItem(string name)
	{
		return GetItemAtIndex (FindIndex (name));
	}
}