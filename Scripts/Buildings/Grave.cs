using UnityEngine;
using System.Collections;

public class Grave : MonoBehaviour
{

		private bool canInteract;

		private float prevActionButton;
	
		// Update is called once per frame
		void Update ()
		{
				if (canInteract && Input.GetAxisRaw ("Action") == 1 && prevActionButton != 1) {
						canInteract = false;
						global.playerInventory.UseItem (global.itemDatabase.GetItem ("Body"));
						global.gameManager.RemoveInteraction (gameObject);
				}

				prevActionButton = Input.GetAxisRaw ("Action");
		}

		void OnTriggerEnter (Collider col)
		{
				if (col.gameObject.tag == "Player" && global.playerInventory.HasItem (global.itemDatabase.GetItem ("Body"))) {
						global.gameManager.DisplayInteraction (gameObject, "Bury a Body");
						canInteract = true;
				}
		}

		void OnTriggerExit (Collider col)
		{
				if (col.gameObject.tag == "Player") {
						global.gameManager.RemoveInteraction (gameObject);
						canInteract = false;
				}
		}
}
