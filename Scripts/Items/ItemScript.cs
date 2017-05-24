using UnityEngine;
using System;
using System.Collections;

public class ItemScript : MonoBehaviour 
{
	public static Transform ItemParentInHierarchy;

	public int Quantity;

	protected bool canInteract;
	protected float previousActionButtonState;

	void Start()
	{
		if (ItemParentInHierarchy == null)
			ItemParentInHierarchy = GameObject.FindGameObjectWithTag("ItemParent").transform;
	}

	void OnMouseDown()
	{
		if (canInteract && Manager.currInteraction != null && Manager.currInteraction.Equals(gameObject))
		{
			PickUp();
			global.gameManager.RemoveInteraction(gameObject);
		}
	}

	void LateUpdate()
	{
		if (canInteract && Input.GetAxisRaw("Action") == 1 && previousActionButtonState == 0 && Manager.currInteraction != null && Manager.currInteraction.Equals(gameObject))
		{
			PickUp();
			global.gameManager.RemoveInteraction(gameObject);
		}

		previousActionButtonState = Input.GetAxisRaw ("Action");
	}

	public virtual void PickUp()
	{
		if (global.playerInventory.AddItem(this))
		{
			global.gameManager.BroadcastActionCompleted("Pickup " + transform.parent.name);
			Destroy(transform.parent.gameObject);
		}
		else
			Debug.Log("You do not have enough space to carry this!");
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player")
			OnPlayerTriggerEnter();
	}

	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.tag == "Player")
			OnPlayerTriggerExit();
	}

	public virtual void OnPlayerTriggerEnter()
	{
		canInteract = true;
		global.gameManager.DisplayInteraction(gameObject, "Pick up " + transform.parent.name);
	}

	public virtual void OnPlayerTriggerExit()
	{
		canInteract = false;
		global.gameManager.RemoveInteraction(gameObject);
	}

	public virtual Item GetItem() {
		return global.itemDatabase.GetItem (transform.parent.name);
	}
}

[Serializable]
public class Item
{
	public enum ItemType { Weapon, Crop, Fertilizer, Armor, Other };
	private int id;
	public string name;
	public ItemType itemType;
	public int typeSpecificStat;
	public int skillEnhancement;
	public int skillEnhancementValue;
	public string description;
	public string suffix;
	public int levelreq = 1;
	public int value = 1;
	public int weight = 1;
	public int quantity = 1;
	[HideInInspector]
	public string stolenFrom;
	[NonSerialized]
	public GameObject prefab;

	public Item() {}
	
	public int DatabaseID
	{
		get { return id; }
		set { id = value; }
	}

	public void SetPrefab(GameObject Prefab)
	{
		prefab = Prefab;
	}

	public virtual void CopyValues(Item i)
	{
		id = i.DatabaseID;
		name = i.name;
		itemType = i.itemType;
		typeSpecificStat = i.typeSpecificStat;
		skillEnhancement = i.skillEnhancement;
		skillEnhancementValue = i.skillEnhancementValue;
		description = i.description;
		suffix = i.suffix;
		levelreq = i.levelreq;
		value = i.value;
		weight = i.weight;
		quantity = i.quantity;
		prefab = i.prefab;
		stolenFrom = i.stolenFrom;
	}

	public virtual bool Use()
	{
		switch (itemType)
		{
			case ItemType.Crop:
				int playerSkillLev = global.playerScript.Skills[1].CurrentLevel;
				//if the player meets the level requirement
				if (playerSkillLev >= levelreq) {
					Plot closestPlot = Plot.GetClosestEmptyPlotToPlayer();
					
					if (closestPlot != null) {
						GameObject temp = this.Instantiate(closestPlot.transform);//instantiate copy
						temp.transform.GetChild(0).GetComponent<Crop>().SetPlot(closestPlot);//tells the plot to plant the crop
						
						//reduces quantity by one
						quantity--;
						
						//updates any quests that require this item to be planted
						global.gameManager.BroadcastActionCompleted("Plant " + name);

						return true;
					}
					//if the closest plot is null, the player was too far away
					global.console.AddMessage("You must be at least 6 units away from a plot to plant this.");
				}
				else {
					//if player doesn't meet level req
					global.console.AddMessage("Your farming skill(" + playerSkillLev + ") needs to be level " + levelreq + " to plant this!");
				}
				break;
			case ItemType.Fertilizer:
				if (Plot.FertilizeClosestPlotToPlayer(this))
				{
					if (name.Equals("Body"))
						global.gameManager.BroadcastActionCompleted("Use Body");
					quantity--;
					return true;
				}
				else if (typeSpecificStat < 3)
					global.console.AddMessage("Unable to fertilize the nearest plot.");
				else
					global.console.AddMessage(name + " cannot be used at this time.");
				break;
			default:
				global.console.AddMessage(name + " cannot be used at this time.");
				break;
		}
		return false;
	}

	public GameObject Instantiate()
	{
		if (prefab == null)
			return null;
		GameObject temp = MonoBehaviour.Instantiate(prefab) as GameObject;
		temp.name = name;
		temp.transform.SetParent (ItemScript.ItemParentInHierarchy);
		return temp;
	}
	public GameObject Instantiate(Transform t)
	{
		if (prefab == null)
			return null;
		GameObject temp = MonoBehaviour.Instantiate(prefab, t.position, t.rotation) as GameObject;
		temp.name = name;
		temp.transform.SetParent (ItemScript.ItemParentInHierarchy);
		return temp;
	}
}
