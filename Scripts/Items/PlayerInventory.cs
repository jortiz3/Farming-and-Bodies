using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : Container
{
		public override void UpdateContainer ()
		{
				UIParent.parent.FindChild("Currency").GetComponent<Text>().text = currency + "\nGold";
				base.UpdateContainer ();
		}
}
