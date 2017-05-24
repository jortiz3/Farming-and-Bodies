using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuScript : MonoBehaviour {

	public string startState = "Main";
	[HideInInspector]
	public string currentState = "";
	[HideInInspector]
	public string complimentaryState = "";
	public SaveLoadScript saveLoadObject;

	[HideInInspector]
	public Dialogue currentDialogue;
	[HideInInspector]
	public ShopScript currentShop;

	private CanvasGroup[] groups;

	void Start() {
		groups = new CanvasGroup[transform.childCount];
		for(int i = 0; i < groups.Length; i++) {
			if (transform.GetChild(i).GetComponent<CanvasGroup>() != null)
				groups[i] = transform.GetChild(i).GetComponent<CanvasGroup>();
		}
		ChangeState(startState);
	}

	#region shop
	public void SetShop(ShopScript s) {
		currentShop = s;
		ChangeState ("Shop");
	}

	public void DisplayShopBuyScreen() {
		if (currentShop != null)
			currentShop.DisplayBuyScreen ();
	}

	public void DisplayShopSellScreen() {
		if (currentShop != null)
			currentShop.DisplaySellScreen ();
	}

	public void DisplayShopBuyBackScreen() {
		if (currentShop != null)
			currentShop.DisplayBuyBackScreen ();
	}

	public void BuyShopItem(GameObject uiObj) {
		if (currentShop != null)
			currentShop.BuyItem (uiObj);
	}

	public void SellItemToShop(GameObject uiObj) {
		if (currentShop != null)
			currentShop.SellItem (uiObj);
	}
	#endregion

	public void SetDialogue(Dialogue d) {
		currentDialogue = d;
	}

	public void SelectDialogueOption(GameObject UIObj) {
		currentDialogue.SelectPlayerChoice(UIObj);
	}

	public void SetCurrentItemSlot(GameObject uiObj) {
		global.playerInventory.SetCurrentItemSlot (uiObj);
		InventoryScript.UIClickPanel.transform.position = Input.mousePosition;
	}

	public void ClearCurrentItemSlot() {
		global.playerInventory.ClearCurrentItemSlot ();
	}

	public void DisplayCurrentItemSlotInfo(GameObject infoObj) {
		infoObj.SetActive (true);
		Item temp = global.playerInventory.GetItem(InventoryScript.CurrentInvSlot);
		infoObj.transform.Find ("Text").GetComponent<Text>().text = temp.name + "\nVal: " + temp.value + "\nQty: " + temp.quantity;
		infoObj.transform.position = InventoryScript.CurrentInvSlot.transform.position;
	}

	public void UseCurrentItemSlot(GameObject clickPanel) {
		if (global.playerInventory.UseCurrentItemSlot ())
			clickPanel.SetActive(false);
	}

	public void DropCurrentItemSlot() {
		global.playerInventory.DropCurrentItemSlot ();
	}

	public void Save(int index) {
		SaveLoadScript.SaveGame (index);
	}

	public void HideTutorialText() {
		if (Manager.currInteraction != null) {
			Tutorial temp = Manager.currInteraction.GetComponent<Tutorial>();
			if (temp != null)
				temp.HideText ();
			else
				HideGroup("Tutorial");
		}
	}

	public void ChangeState(string state) {
		if (currentState.Equals(state))
			currentState = "";
		else
			currentState = state;

		for (int i = 0; i < groups.Length; i++) {
			if (groups[i] != null) {
				if (groups[i].transform.name.Equals(currentState)) {
					groups[i].alpha = 1;
					groups[i].interactable = true;
					groups[i].blocksRaycasts = true;

					if (groups[i].GetComponent<MenuScript>() != null)
						groups[i].GetComponent<MenuScript>().RefreshState();
				}
				else {
					groups[i].alpha = 0;
					groups[i].interactable = false;
					groups[i].blocksRaycasts = false;
				}
			}
		}
	}

	public void ChangeState(string state, string complimentaryState)
	{
		if (currentState.Equals(state))
			currentState = "";
		else
			currentState = state;

		this.complimentaryState = complimentaryState;
		
		for (int i = 0; i < groups.Length; i++)
		{
			if (groups[i] != null)
			{
				if (groups[i].transform.name.Equals(currentState) || groups[i].transform.name.Equals(complimentaryState))
				{
					groups[i].alpha = 1;
					groups[i].interactable = true;
					groups[i].blocksRaycasts = true;
					
					if (groups[i].GetComponent<MenuScript>() != null)
						groups[i].GetComponent<MenuScript>().RefreshState();
				}
				else
				{
					groups[i].alpha = 0;
					groups[i].interactable = false;
					groups[i].blocksRaycasts = false;
				}
			}
		}
	}

	public void HideGroup(string canvasGroupName)
	{
		for (int i = 0; i < groups.Length; i++)
		{
			if (groups[i] != null)
			{
				if (groups[i].transform.name.Equals(canvasGroupName))
				{
					groups[i].alpha = 0;
					groups[i].interactable = false;
					groups[i].blocksRaycasts = false;
					return;
				}
			}
		}
	}

	public void RefreshState()
	{
		string temp = currentState;
		currentState = "";
		ChangeState(temp);
	}

	public void SaveGame(int index)
	{
		SaveLoadScript.SaveGame(index);
	}

	public void StartNewGame()
	{
		SaveLoadScript temp = Instantiate(saveLoadObject) as SaveLoadScript;
		temp.StartNewGame();
	}

	public void LoadGame(int index)
	{
		if (SaveLoadScript.SaveFileExists(index)) {
			SaveLoadScript temp = Instantiate(saveLoadObject) as SaveLoadScript;
			temp.StartLoadGame(index);
		}
	}

	public void ChangeToScene(string scene)
	{
		Application.LoadLevel(scene);
	}

	public void ExitApplication()
	{
		Application.Quit();
	}

	public void ToggleMusic()
	{
		global.musicEnabled = !global.musicEnabled;

		if (Camera.main.transform.childCount > 0)
		{
			if (Camera.main.transform.GetChild(0).audio != null)
			{
				Camera.main.transform.GetChild(0).audio.enabled = global.musicEnabled;
			}
		}
	}
}
