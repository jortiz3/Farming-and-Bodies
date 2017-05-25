using UnityEngine;
using System.Collections;

public class Container : MonoBehaviour {

	public static GameObject UITemplate;

	protected Transform UIParent;

	[SerializeField]
	protected Item[] items;
	[SerializeField]
	private int maxNumOfItems;
	
	public int size { get { return items.Length; } }

	void Start() {
		if (UITemplate == null)
			UITemplate = GameObject.FindGameObjectWithTag("ContainerTemplate");

		UIParent = (Transform)Instantiate (UITemplate);
		Transform tempTransform;
		while (UIParent.childCount < maxNumOfItems) {
			tempTransform = (Transform)Instantiate(UIParent.GetChild(0));
			tempTransform.SetParent(UIParent);
		}

		items = new Item[maxNumOfItems];
	}

	public virtual void UpdateContainerUI() {
		//loops through children of uiparent to change values
	}

	public bool CanAddItem(Item item) {
		for (int i = 0; i < items.Length; i++) {
			if (items[i] == null) {
				return true;
			} else if (items[i].Equals(item)) {
				return true;
			}
		}
		return false;
	}

	public bool AddItem(Item item) {
		int emptyIndex = items.Length;
		for (int i = 0; i < items.Length; i++) {
			if (items[i] == null) {
				if (emptyIndex >= items.Length) {
					emptyIndex = i;
				}
			} else if (items[i].Equals(item)) {


				items[i].quantity += item.quantity;
				item.quantity = 0;
				UpdateContainerUI();
				return true;
			}
		}

		if (emptyIndex < items.Length) {
			items[emptyIndex] = item;
			UpdateContainerUI();
			return true;
		}
		return false;
	}

	public void DropItem(Item item) {
		for (int i = 0; i < items.Length; i++) {
			if (items[i] != null) {
				if (items[i].Equals(item)) {
					//instantiate object
					//remove from array
					UpdateContainerUI();
					return;
				}
			}
		}
	}
}
