using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : Container
{
		protected override void Initialize ()
		{
				//load the hotkey bar instead of copying template
				UIParent = GameObject.FindGameObjectWithTag("InvUIParent").transform;
				base.Initialize ();
		}

		public override void UpdateContainerUI ()
		{
				//load the sprite
				//set sprite gameobject name to item name
				for (int i = 0; i < items.Length; i++) {
						if (items [i] != null) {
								//edit container slot
						}
				}
		}
}
