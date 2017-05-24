using UnityEngine;
using System.Collections;

public class InteractableObject : Dialogue {

	public QuestRequirement[] qReqToInteract;
	public ItemRequirement[] iReqToInteract;
	
	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player" && MeetsReqs())
		{
			canInteract = true;
			global.gameManager.DisplayInteraction(gameObject, "Interact with " + transform.parent.name);
		}
	}

	private bool MeetsReqs()
	{
		bool hasAllOfQuestReqs = true;
		for (int i = 0; i < qReqToInteract.Length; i++)
		{
			if (!qReqToInteract[i].isSatisfied())
			{
				hasAllOfQuestReqs = false;
				break;
			}
		}

		if (!hasAllOfQuestReqs)
			return false;

		bool hasRequiredItems = true;
		for (int i = 0; i < iReqToInteract.Length; i++)
		{
			if (!iReqToInteract[i].isSatisfied())
			{
				if (!iReqToInteract[i].invertReqResult)
				{
					hasRequiredItems = false;
					break;
				}
			}
			else if (iReqToInteract[i].invertReqResult)
			{
				hasRequiredItems = false;
				break;
			}
		}
		
		if (!hasRequiredItems)
			return false;

		return true;
	}
}
