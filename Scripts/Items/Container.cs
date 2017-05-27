using UnityEngine;
using System.Collections;

public class Container : MonoBehaviour
{

		public static GameObject UITemplate;

		public static GameObject HighlightedItemInUI;

		protected Transform UIParent;

		protected int currency;
		[SerializeField]
		protected Item[] items;
		[SerializeField]
		private int maxNumOfItems;

		public int size { get { return items.Length; } }

		void Start ()
		{
				Initialize ();
		}

		protected virtual void Initialize ()
		{
				if (UITemplate == null)
						UITemplate = GameObject.FindGameObjectWithTag ("ContainerTemplate");

				UIParent = Instantiate (UITemplate).transform;
				while (UIParent.childCount < maxNumOfItems) {
						Instantiate (UIParent.GetChild (0), UIParent);
				}

				items = new Item[maxNumOfItems];
		}

		public virtual void UpdateContainerUI ()
		{
				//loops through children of uiparent to change values
				for (int i = 0; i < items.Length; i++) {
						if (items [i] != null) {
								//edit container slot
						}
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

								UpdateContainerUI ();
								return true;
						}
				}

				if (emptyIndex < items.Length) {
						items [emptyIndex] = item;
						UpdateContainerUI ();
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
						UpdateContainerUI ();
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

										UpdateContainerUI ();
										return;
								}
						}
				}
		}
}
