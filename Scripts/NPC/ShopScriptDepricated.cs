using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/* To Do:
 * 
 * Make container class to do this
 */

public class ShopScriptDepricated : MonoBehaviour
{
		enum ShopScreen
		{
Buy,
				BuyBack}

		;

		//these two transforms store the corresponding parents in the hierarchy for later use
		private static Transform shopGroup;
		private static Transform shopScrollArea;

		[Tooltip ("The time in minutes until the stock is refreshed.")]
		public int timeUntilShopRefresh;
		//when this is false, the shop will refresh the first time it is opened
		private bool refreshCycleStarted;
		//this array stores the items that the shop has a chance to stock
		public StockItem[] possibleStock;
		//this array stores the current stock
		private List<Item> availableItems;
		//this array stores
		private List<Item> buyBackItems;

		private ShopScreen currentScreen;

		void Start ()
		{
				if (shopGroup == null) {
						shopGroup = GameObject.Find ("Shop").transform;
						shopScrollArea = shopGroup.FindChild ("Scroll View").GetChild (0);
				}

				availableItems = new List<Item> ();
				buyBackItems = new List<Item> ();
		}

		//current plan is to call this method from manager.TriggerEvent
		public void OpenShopScreen ()
		{
				if (!refreshCycleStarted) {
						StartCoroutine ("RefreshStock");
						refreshCycleStarted = true;
				}
				//global.uicanvas.SetShop (this);
				DisplayBuyScreen ();
		}

		IEnumerator RefreshStock ()
		{
				if (!global.uicanvas.currentState.Equals ("Shop")) {
						availableItems.Clear ();
						Item itemToAdd;
						foreach (StockItem s in possibleStock) {
								itemToAdd = s.GetItemByChance ();
								if (itemToAdd != null)
										availableItems.Add (itemToAdd);
						}
						buyBackItems.Clear ();
				}
				yield return new WaitForSeconds (timeUntilShopRefresh * 60);
				StartCoroutine ("RefreshStock");
		}

		public void DisplayBuyScreen ()
		{
				DestroyScrollAreaChildren ();
				//InstantiateScrollAreaChildren (availableItems, true);
				UpdateInfoText ();
				currentScreen = ShopScreen.Buy;
		}

		public void DisplaySellScreen ()
		{
				DestroyScrollAreaChildren ();
				//InstantiateScrollAreaChildren (global.playerInventory.items, false);
				UpdateInfoText ();
		}

		public void DisplayBuyBackScreen ()
		{
				DestroyScrollAreaChildren ();
				//InstantiateScrollAreaChildren (buyBackItems, true);
				UpdateInfoText ();
				currentScreen = ShopScreen.BuyBack;
		}

		private void InstantiateScrollAreaChildren (Item[] itemList, bool buttonsForBuying)
		{
				foreach (Item i in itemList) {
						Transform current = shopScrollArea.FindChild (i.name);
						//if the ui element isn't already there, we add one
						if (current == null) {
								if (buttonsForBuying)
										current = Instantiate (shopScrollArea.GetChild (0)) as Transform;
								else
										current = Instantiate (shopScrollArea.GetChild (1)) as Transform;
								current.gameObject.SetActive (true);
								//changes the button's name
								current.name = i.name;
								current.SetParent (shopScrollArea);
						}
						UpdateItemInfo (current, i);
				}
		
				UpdateUIPositioning ();
		}

		private void DestroyScrollAreaChildren ()
		{
				for (int i = shopScrollArea.childCount - 1; i > 1; i--)
						DestroyImmediate (shopScrollArea.GetChild (i).gameObject, false);
		}

		private void UpdateUIPositioning ()
		{
				//this is the height of each button rect
				int choiceButtonRectHeight = (int)shopScrollArea.GetChild (0).GetComponent<RectTransform> ().rect.size.y;
				int spaceBetweenButtons = 15;
				//this is the desired height for the scroll area
				int desiredScrollAreaHeight = (shopScrollArea.childCount - 2) * (choiceButtonRectHeight + spaceBetweenButtons);
		
				//this changes the size of the scroll area, so that if there are less/more, the scrollbar and area will be adjusted
				shopScrollArea.GetComponent<RectTransform> ().rect.Set (0, 0, shopScrollArea.GetComponent<RectTransform> ().rect.width, desiredScrollAreaHeight);
				shopGroup.transform.FindChild ("Scrollbar").GetComponent<Scrollbar> ().value = 1f;

				//this height is the oddly calculated height for rect transforms that will be used for the buttons
				int areaHeight = (int)(shopScrollArea.GetComponent<RectTransform> ().rect.height / 2) - (choiceButtonRectHeight / 2) - spaceBetweenButtons;
				for (int c = 2; c < shopScrollArea.childCount; c++) {
						shopScrollArea.GetChild (c).GetComponent<RectTransform> ().localPosition = 
				new Vector3 (shopScrollArea.GetChild (0).GetComponent<RectTransform> ().localPosition.x, 
								areaHeight - ((c - 2) * (choiceButtonRectHeight + spaceBetweenButtons)), 0);
				}
		}

		private void UpdateItemInfo (Transform current, Item i)
		{
				string tempString;
				//sets up the string of info for each item
				if (i.itemType == Item.ItemType.Weapon)
						tempString = i.name + i.suffix + "  Lv: " + i.levelreq + "  Dmg: " + i.typeSpecificStat + "  Qty: " + i.quantity + "  Value: " + i.value;
				else if (i.itemType == Item.ItemType.Armor)
						tempString = i.name + i.suffix + "  Lv: " + i.levelreq + "  Amr: " + i.typeSpecificStat + "  Qty: " + i.quantity + "  Value: " + i.value;
				else if (i.itemType == Item.ItemType.Crop)
						tempString = i.name + i.suffix + "  Lv: " + i.levelreq + "  Qty: " + i.quantity + "  Value: " + i.value;
				else
						tempString = i.name + i.suffix + "  Qty: " + i.quantity + "  Value: " + i.value;
		
				current.FindChild ("Item Info").GetComponent<Text> ().text = tempString;
		}

		private void UpdateInfoText ()
		{
				shopGroup.FindChild ("Info").GetComponent<Text> ().text = "Your Money: " + global.playerInventory.storedCurrency;
		}

		public void BuyItem (GameObject uiObj)
		{
				switch (currentScreen) {
				case ShopScreen.Buy:
						BuyItem (uiObj, availableItems);
						break;
				case ShopScreen.BuyBack:
						BuyItem (uiObj, buyBackItems);
						break;
				}
				UpdateInfoText ();
		}

		//for some reason quantity does not change after buying item when it reaches one
		private void BuyItem (GameObject uiObj, List<Item> listToBuyFrom)
		{
				for (int i = 0; i < listToBuyFrom.Count; i++) {
						if (listToBuyFrom [i].name.Equals (uiObj.name)) {
								//if the player is able to buy the item, then do stuff
								if (global.playerInventory.BuyItem (listToBuyFrom [i])) {
										if (listToBuyFrom [i].quantity <= 0) {
												listToBuyFrom.RemoveAt (i);
												GameObject.DestroyImmediate (uiObj.gameObject, false);
												UpdateUIPositioning ();
										} else {
												UpdateItemInfo (uiObj.transform, listToBuyFrom [i]);
										}
								} else {
										global.console.AddMessage ("You don't have enough money to buy that.");
								}
								return;
						}
				}
				//if we make it this far, the item wasn't in stock
				GameObject.Destroy (uiObj.transform.parent.gameObject);
				UpdateUIPositioning ();
		}

		public void SellItem (GameObject uiObj)
		{
				//sells one of the item -- returns null if the item ran out
				/*Item temp = global.playerInventory.SellItem (global.itemDatabase.GetItem(uiObj.name));
				if (temp == null) {
						for (int i = 0; i < buyBackItems.Count; i++) {
								if (buyBackItems [i].name.Equals (uiObj.name)) {
										buyBackItems [i].quantity++;
										break;
								}
						}

						//if player has sold the last of that item, remove it from the view
						GameObject.DestroyImmediate (uiObj.gameObject, false);
				} else {
						//this lets us know if the quantity was modified, or the item was newly added to the buyback list
						bool itemExistedPrior = false;
						for (int i = 0; i < buyBackItems.Count; i++) {
								if (buyBackItems [i].name.Equals (temp.name) && buyBackItems [i].suffix.Equals (temp.suffix)) {
										buyBackItems [i].quantity++;
										itemExistedPrior = true;
										break;
								}
						}
						if (!itemExistedPrior) {
								buyBackItems.Add (temp);
						}

						UpdateItemInfo (uiObj.transform, temp);
				}

				UpdateUIPositioning ();
				UpdateInfoText ();*/
		}
}

[Serializable]
public class StockItem
{
		public string itemName;
		public string itemSuffix;
		[Range (0, 100)]
		public int chanceToStock;
		[Range (1, 100)]
		public int maxQuantityToStock;

		//returns the item if the chances are met
		public Item GetItemByChance ()
		{
				if (UnityEngine.Random.Range (0, 100) < chanceToStock) {
						Item temp;
						if (!itemSuffix.Equals (""))
								temp = global.itemDatabase.GetItem (itemName, itemSuffix);
						else
								temp = global.itemDatabase.GetItem (itemName);

						if (temp != null)
								temp.quantity = UnityEngine.Random.Range (1, maxQuantityToStock);
						return temp;
				}
				return null;
		}
}