using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : Container
{
		public override void UpdateContainer ()
		{
				UIParent.parent.FindChild("Currency").GetComponent<Text>().text = currency + "\nGold";

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
}
