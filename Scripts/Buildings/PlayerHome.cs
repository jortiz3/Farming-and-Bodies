using UnityEngine;
using System.Collections;

public class PlayerHome : MonoBehaviour {

	private bool canInteract;
	private bool hasInteracted;

	void Update()
	{
		if (canInteract && !hasInteracted && Input.GetAxisRaw("Action") == 1)
		{
			global.playerScript.Sleep();
			hasInteracted = true;
			global.gameManager.RemoveInteraction(gameObject);
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			global.gameManager.DisplayInteraction(gameObject, "Press the 'Action' Button to go to sleep");
			canInteract = true;
		}
	}
	
	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			global.gameManager.RemoveInteraction(gameObject);
			canInteract = false;
			hasInteracted = false;
		}
	}
}
