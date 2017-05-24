using UnityEngine;
using System.Collections;

public class Plot : MonoBehaviour
{
	//the owner of the plot
	public NPCScript owner;

	//this is the property that will be affected if a body were used for fertilization
	public Property propertyAffected;

	//the currently planted crop
	private Crop plantedCrop;

	//the current fertilizer
	private Item fertilizer;
	private GameObject currFertilizerLight;

	private bool canInteract;
	private float prevActionButton;

	void Start () {
		gameObject.tag = "Plot";
	}

	void Update () {
		//if the player can interact with the plot, and the ui is prompting the player about this particular plot
		if (canInteract && Manager.currInteraction != null && Manager.currInteraction.Equals(gameObject)) {
			//if the player isn't in any menus
			if (global.uicanvas.currentState.Equals("") || global.uicanvas.currentState.Equals("Interaction")) {
				//if the player presses the action button
				if (Input.GetAxisRaw ("Action") == 1 && prevActionButton != 1) {
					//open inventory screen
					global.gameManager.ChangeUIState("Inventory");
				}
			}
		}
		
		prevActionButton = Input.GetAxisRaw ("Action");
	}
	
	void OnTriggerEnter(Collider col)
	{
		if (owner == null) {
			if (col.gameObject.tag == "Player") {
				if (plantedCrop == null) {
					canInteract = true;
					global.gameManager.DisplayInteraction(gameObject, "Interact with Plot");
				}
			}
		}
		else if (!col.isTrigger && col.name.Equals(owner.name)) {
			OwnerTryToPlant();
		}
	}
	void OnTriggerExit(Collider col)
	{
		if (owner == null) {
			if (col.gameObject.tag == "Player") {
				canInteract = false;
				global.gameManager.RemoveInteraction(gameObject);
			}
		}
	}
	
	public void OwnerTryToPlant() {
		if (plantedCrop == null) {
			//if the owner has some crops to plant
			if (owner.cropsToPlant.Length > 0) {
				//if we get lucky
				if (Random.Range(0, 100) >= 90) {
					Item tempItem = global.itemDatabase.GetItem(owner.cropsToPlant[Random.Range(0, owner.cropsToPlant.Length)]);
					GameObject temp = tempItem.Instantiate(transform);
					temp.transform.GetChild(0).GetComponent<Crop>().SetPlot(this);
					temp.transform.SetParent(GameObject.Find("Items").transform);
				}
			}
		}
	}

	public bool IsOccupied() {
		if (plantedCrop != null)
			return true;
		return false;
	}

	public void SetNextPropertyToDie() {
		if (propertyAffected != null)
			propertyAffected.SetNextToDie();
	}

	public void Plant(Crop c) {
		plantedCrop = c;
	}

	public bool Fertilize(Item i) {
		if (i.itemType == Item.ItemType.Fertilizer) {
			if (fertilizer == null) {
				global.gameManager.BroadcastActionCompleted("Fertilize Plot");
				if (i.typeSpecificStat < 3) {
					GameObject temp = Instantiate(Resources.Load("Items/Fertilizer Light")) as GameObject;
					temp.transform.SetParent(transform);
					temp.transform.localPosition = new Vector3(0, 0, 0);
					currFertilizerLight = temp;
					fertilizer = i;
					return true;
				}
				else if (propertyAffected != null) {
					GameObject temp = MonoBehaviour.Instantiate(Resources.Load("Items/Special Fertilizer Light")) as GameObject;
					temp.transform.SetParent(transform);
					temp.transform.localPosition = new Vector3(0, 0, 0);
					global.gameManager.BroadcastActionCompleted("Special Fertilize Plot");
					currFertilizerLight = temp;
					fertilizer = i;
					return true;
				}
			}
		}
		return false;
	}

	//returns true if it was stolen
	public bool Harvest(int cropLevelReq, bool fullyGrown) {
		bool stolen = false;
		//if there isn't an owner, then the player isn't stealing
		if (owner == null)
			global.gameManager.BroadcastActionCompleted("Harvest " + plantedCrop.name);
		else {
			if(owner.CurrentlySeesPlayer())
				owner.InsertMethod(new Action("Confront", global.playerObject));
			stolen = true;
		}

		if (fullyGrown && !stolen) {
			//the experience to reward the player with
			int exp = 2 * cropLevelReq;
			//random bonus qty from [1, 1 + <farming skill / 3>)
			int bonusQuantity = Random.Range(1, 1 + (global.playerScript.Skills[0].CurrentLevel / 3));
			if (fertilizer != null) {
				//gives more exp if the plot was fertilized
				exp += cropLevelReq;
				//yields more crop if the plot was fertilized
				bonusQuantity *= fertilizer.typeSpecificStat;
				//if the fertilizer was special or was a body, set our next target to die
				if (fertilizer.typeSpecificStat > 2)
					SetNextPropertyToDie();
				//removes the fertilizer
				fertilizer = null;
				Destroy(currFertilizerLight);
			}
			plantedCrop.Quantity += bonusQuantity;

			//gives player experience
			global.playerScript.AddExp(0, exp * (cropLevelReq / (float)global.playerScript.Skills[0].CurrentLevel));
		}

		plantedCrop = null;
		return stolen;
	}

	//gets the nearest empty plot from the player that is owned by the player
	public static Plot GetClosestEmptyPlotToPlayer() {
		Plot closestPlot = null;
		float closestDistance = float.MaxValue;
		float currentDistance = 0f;
		//finds the plots in the scene
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Plot")) {
			currentDistance = Vector3.Distance(global.playerObject.transform.position, g.transform.position);
			//get the closest plot
			if (currentDistance < 6 && currentDistance < closestDistance) {
				Plot p = g.GetComponent<Plot>();
				//plot is owned by the player and doesn't have something planted there already
				if (!p.IsOccupied() && p.owner == null) {
					closestDistance = currentDistance;
					closestPlot = p;
				}
			}
		}
		return closestPlot;
	}

	public static bool FertilizeClosestPlotToPlayer(Item Fertilizer) {
		Plot closest = GetClosestEmptyPlotToPlayer ();
		if (closest != null)
			return closest.Fertilize(Fertilizer);
		return false;
	}
}
