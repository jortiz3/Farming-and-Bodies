using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

//make Tutorial an overlay, remove complimentary state

public class MenuScript : MonoBehaviour
{

		public string startState = "Main";
		[HideInInspector]
		public string currentState = "";
		[HideInInspector]
		public string complimentaryState = "";

		[Tooltip ("Only hidden when a focus menu is displayed.")]
		public string[] overlays;

		[Tooltip ("No overlays may be displayed when one of these menus are displayed.")]
		public string[] focusMenus;

		public SaveLoadScript saveLoadObject;

		[HideInInspector]
		public Dialogue currentDialogue;

		private CanvasGroup[] groups;

		void Start ()
		{
				groups = new CanvasGroup[transform.childCount];
				for (int i = 0; i < groups.Length; i++) {
						if (transform.GetChild (i).GetComponent<CanvasGroup> () != null)
								groups [i] = transform.GetChild (i).GetComponent<CanvasGroup> ();
				}
				ChangeState (startState);
		}

		public void SetDialogue (Dialogue d)
		{
				currentDialogue = d;
		}

		public void SelectDialogueOption (GameObject UIObj)
		{
				currentDialogue.SelectPlayerChoice (UIObj);
		}

		public void SetCurrentItemSlot (GameObject uiObj)
		{
				Item HighlightedItemInUI = uiObj.GetComponent<ItemScript> ().GetItem ();

				if (HighlightedItemInUI != null)
						Container.HighlightedGameObjectInUI = uiObj;
		}

		public void DisplayCurrentItemSlotInfo (GameObject infoObj)
		{
				if (Container.HighlightedGameObjectInUI != null) {
						Item UIItem = Container.HighlightedGameObjectInUI.GetComponent<ItemScript> ().GetItem ();
						infoObj.SetActive (true);
						infoObj.transform.Find ("Text").GetComponent<Text> ().text = UIItem.name + "\nVal: " + UIItem.value + "\nQty: " + UIItem.quantity;
						infoObj.transform.position = Container.HighlightedGameObjectInUI.transform.position;
				}
		}

		public void DisplayClickPanel (GameObject clickPanel)
		{
				if (Container.HighlightedGameObjectInUI != null) {
						clickPanel.SetActive (true);
						clickPanel.transform.position = Container.HighlightedGameObjectInUI.transform.position;
				}
		}

		public void UseCurrentItemSlot (GameObject clickPanel)
		{
				Item currItem = Container.HighlightedGameObjectInUI.GetComponent<ItemScript> ().GetItem ();
				global.playerInventory.UseItem (currItem);
				if (global.playerInventory.HasItem(currItem))
						Container.HighlightedGameObjectInUI = null;
				clickPanel.SetActive (false);
		}

		public void DropCurrentItemSlot ()
		{
				global.playerInventory.DropItem (Container.HighlightedGameObjectInUI.GetComponent<ItemScript> ().GetItem ());
				Container.HighlightedGameObjectInUI = null;
		}

		public void Save (int index)
		{
				SaveLoadScript.SaveGame (index);
		}

		public void HideTutorialText ()
		{
				if (Manager.currInteraction != null) {
						Tutorial temp = Manager.currInteraction.GetComponent<Tutorial> ();
						if (temp != null)
								temp.HideText ();
						else
								HideGroup ("Tutorial");
				}
		}

		public void ChangeState (string state)
		{
				if (currentState.Equals (state))
						currentState = "";
				else
						currentState = state;

				bool currStateIsFocus = false;

				for (int f = 0; f < focusMenus.Length; f++) {
						if (currentState.Equals (focusMenus [f])) {
								currStateIsFocus = true;
								break;
						}
				}

				string currGroupName = "";
				bool currGroupIsOverlay;
				for (int i = 0; i < groups.Length; i++) {
						if (groups [i] != null) {
								currGroupName = groups [i].transform.name;

								currGroupIsOverlay = false;
								for (int o = 0; o < overlays.Length; o++) {
										if (currGroupName.Equals (overlays [o])) {
												currGroupIsOverlay = true;
												break;
										}
								}

								if (currGroupName.Equals (currentState) || (!currStateIsFocus && currGroupIsOverlay)) {
										groups [i].alpha = 1;
										groups [i].interactable = true;
										groups [i].blocksRaycasts = true;

										if (groups [i].GetComponent<MenuScript> () != null)
												groups [i].GetComponent<MenuScript> ().RefreshState ();
								} else {
										groups [i].alpha = 0;
										groups [i].interactable = false;
										groups [i].blocksRaycasts = false;
								}
						}
				}
		}

		public void ChangeState (string state, string complimentaryState)
		{
				if (currentState.Equals (state))
						currentState = "";
				else
						currentState = state;

				this.complimentaryState = complimentaryState;
		
				for (int i = 0; i < groups.Length; i++) {
						if (groups [i] != null) {
								if (groups [i].transform.name.Equals (currentState) || groups [i].transform.name.Equals (complimentaryState)) {
										groups [i].alpha = 1;
										groups [i].interactable = true;
										groups [i].blocksRaycasts = true;
					
										if (groups [i].GetComponent<MenuScript> () != null)
												groups [i].GetComponent<MenuScript> ().RefreshState ();
								} else {
										groups [i].alpha = 0;
										groups [i].interactable = false;
										groups [i].blocksRaycasts = false;
								}
						}
				}
		}

		public void HideGroup (string canvasGroupName)
		{
				for (int i = 0; i < groups.Length; i++) {
						if (groups [i] != null) {
								if (groups [i].transform.name.Equals (canvasGroupName)) {
										groups [i].alpha = 0;
										groups [i].interactable = false;
										groups [i].blocksRaycasts = false;
										return;
								}
						}
				}
		}

		public void RefreshState ()
		{
				string temp = currentState;
				currentState = "";
				ChangeState (temp);
		}

		public void SaveGame (int index)
		{
				SaveLoadScript.SaveGame (index);
		}

		public void StartNewGame ()
		{
				SaveLoadScript temp = Instantiate (saveLoadObject) as SaveLoadScript;
				temp.StartNewGame ();
		}

		public void LoadGame (int index)
		{
				if (SaveLoadScript.SaveFileExists (index)) {
						SaveLoadScript temp = Instantiate (saveLoadObject) as SaveLoadScript;
						temp.StartLoadGame (index);
				}
		}

		public void ChangeToScene (string scene)
		{
				SceneManager.LoadScene (scene);
		}

		public void ExitApplication ()
		{
				Application.Quit ();
		}

		public void ToggleMusic ()
		{
				global.musicEnabled = !global.musicEnabled;

				if (Camera.main.transform.childCount > 0) {
						if (Camera.main.transform.GetChild (0).GetComponent<AudioSource> () != null) {
								Camera.main.transform.GetChild (0).GetComponent<AudioSource> ().enabled = global.musicEnabled;
						}
				}
		}
}
