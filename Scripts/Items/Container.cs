using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Container : MonoBehaviour
{
		public static GameObject UITemplate;

		public static Container displayedContainer; //used for buying/selling/organizing
		public static GameObject HighlightedGameObjectInUI;

		[SerializeField]
		protected Transform UIParent;

		protected int currency;
		protected Item[] items;
		[SerializeField]
		private int maxNumOfItems;

		[SerializeField]
		private string[] possibleContents;

		public int size { get { return items.Length; } }
		public int storedCurrency { get { return currency; } set { currency = value; } }

		void Start ()
		{
				Initialize ();
		}

		protected virtual void Initialize ()
		{
				if (UITemplate == null) {
						UITemplate = GameObject.FindGameObjectWithTag ("ContainerTemplate");
						UITemplate.SetActive (false);
				}

				if (UIParent == null) {
						UIParent = Instantiate (UITemplate).transform; //container template
						UIParent.name = "Container UI - " + gameObject.name; //rename template

						string containerTitle = "";

						if (transform.GetComponent<NPCScript> () != null)
								containerTitle = gameObject.name + "'s Inventory";
						else
								containerTitle = gameObject.name;

						UIParent.FindChild("Text").GetComponent<Text>().text = containerTitle; //change the displayed title

						UIParent = UIParent.GetChild (3); //set item parent

						while (UIParent.childCount < maxNumOfItems) { //make sure we have enough slots displayed
								Instantiate (UIParent.GetChild (0), UIParent);
						}
				}

				items = new Item[maxNumOfItems];

				GenerateContents ();

				UpdateContainer ();
		}

		private void GenerateContents() {
				if (possibleContents != null && possibleContents.Length > 0) {

						int randomVal;

						List<string> remainingPossibilites = new List<string> ();
						remainingPossibilites.AddRange (possibleContents);

						for (int i = 0; i < items.Length; i++) {
								randomVal = Random.Range (-remainingPossibilites.Count, remainingPossibilites.Count - 1);

								if (randomVal > 0) {
										items [i] = global.itemDatabase.GetItem (remainingPossibilites[randomVal]);
										remainingPossibilites.RemoveAt (randomVal);
								}
						}
				}
		}

		public virtual void UpdateContainer ()
		{
				GameObject currContainerSlot;
				Image spriteImage;
				Sprite currSprite;
				for (int i = 0; i < items.Length; i++) {
						currContainerSlot = UIParent.GetChild (i).GetChild (0).gameObject;
						spriteImage = currContainerSlot.GetComponent<Image> ();

						if (items [i] != null) {
								spriteImage.transform.parent.name = items [i].name;
								currContainerSlot.GetComponent<ItemScript> ().Quantity = items [i].quantity;
								currSprite = Resources.Load<Sprite> ("UI Prefabs/" + items [i].name);
						} else {
								spriteImage.transform.parent.name = "empty slot";
								currSprite = null;
						}


						spriteImage.sprite = currSprite;

						if (currSprite != null)
								spriteImage.color = Color.white;
						else
								spriteImage.color = Color.clear;
				}
		}

		public bool HasItem (Item item)
		{
				for (int i = 0; i < items.Length; i++) {
						if (items [i] != null) {
								if (items [i].Equals (item)) {
										return true;
								}
						}
				}
				return false;
		}

		public bool CanAddItem (Item item)
		{
				for (int i = 0; i < items.Length; i++) {
						if (items [i] == null) {
								return true;
						} else if (items [i].Equals (item)) {
								return true;
						}
				}
				return false;
		}

		public bool AddItem (Item item)
		{
				int emptyIndex = items.Length;
				for (int i = 0; i < items.Length; i++) {
						if (items [i] == null) {
								if (emptyIndex >= items.Length) {
										emptyIndex = i;
								}
						} else if (items [i].Equals (item)) {
								items [i].quantity += item.quantity;
								item.quantity = 0;

								for (int q = 0; q < items [i].quantity; q++) {
										global.gameManager.BroadcastActionCompleted("Get " + q + " " + items[i].name + items[i].suffix);
								}

								UpdateContainer ();
								return true;
						}
				}

				if (emptyIndex < items.Length) {
						items [emptyIndex] = item;
						UpdateContainer ();
						return true;
				}
				return false;
		}

		public bool RemoveItem (Item item)
		{
				for (int i = 0; i < items.Length; i++) {
						if (items [i] != null) {
								if (items [i].Equals (item)) {
										items [i] = null;
										return true;
								}
						}
				}
				return false;
		}

		public void DropItem (Item item)
		{
				if (RemoveItem (item)) {
						//instantiate item to world space
						global.gameManager.BroadcastMessage("Drop " + item.name + item.suffix);
						UpdateContainer ();
				}
		}

		public bool BuyItem (Item item)
		{
				int total = item.quantity * item.value;
				if (currency >= total) {
						if (AddItem (item)) { //if item successfully added
								currency -= total; //update currency
								global.gameManager.BroadcastMessage("Buy " + item.name + item.suffix);
								return true; //return that we did buy item
						}
				}
				return false; //unable to buy item
		}

		public void SellItem (Item item)
		{
				if (RemoveItem (item)) { //if we are able to remove the item
						currency += item.quantity * item.value;//update currency -- item should still exist; only array pointer should be null
						global.gameManager.BroadcastMessage("Sell " + item.name + item.suffix);
				}
		}

		public void UseItem(Item item)
		{
				for (int i = 0; i < items.Length; i++) {
						if (items [i] != null) {
								if (items [i].Equals (item)) {
										items [i].Use ();

										if (items [i].quantity <= 0) {
												items [i] = null;
										}

										UpdateContainer ();
										return;
								}
						}
				}
		}

		public Item GetItem(Item item) {
				Item temp = null;
				for (int i = 0; i < items.Length; i++) {
						if (items [i] != null) {
								if (items [i].Equals (item)) {
										temp = new Item ();
										temp.CopyValues(items [i]);
										break;
								}
						}
				}
				return temp;
		}

		public Item[] GetItems() {
				return (Item[])items.Clone ();
		}

		public void LoadItems(Item[] items) {
				this.items = (Item[])items.Clone();
				UpdateContainer ();
		}
}
