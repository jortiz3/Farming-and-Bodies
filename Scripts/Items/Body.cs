using UnityEngine;
using System.Collections;

public class Body : ItemScript {

	void Start()
	{

	}

	public override void OnPlayerTriggerEnter()
	{
		canInteract = true;
		global.gameManager.DisplayInteraction(gameObject, "Pick up " + transform.parent.parent.name);
	}
}
