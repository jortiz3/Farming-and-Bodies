using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//do a send message or something to update action completed

[Serializable]
public class Dialogue : MonoBehaviour
{
		[NonSerialized]
		private static CanvasGroup dialogueUI;
		[NonSerialized]
		private static Transform choiceScrollArea;

		public string nameOfPerson;
		[HideInInspector]
		public string nicknameOfPerson;
		//if this is false, the player cannot exit the dialogue without making a choice -- currently used for the intro
		public bool canFreelyExit = true;
		public bool frontCamView = true;
		public Greeting greeting;
		public Topic[] topics;
		private Topic currentTopic;
		private int currentTopicCounter;
		protected bool canInteract;
		[HideInInspector]
		public bool isInteracting;

		//keeps track of how many choices are active
		private int numOfActiveChoices;

		private static List<PlayerChoice> choicesWithRefreshActions;

		// Use this for initialization
		void Start ()
		{
				if (dialogueUI == null) {
						dialogueUI = GameObject.FindGameObjectWithTag ("DialogueUI").GetComponent<CanvasGroup> ();
						choiceScrollArea = dialogueUI.transform.FindChild ("Choice Scroll View").GetChild (0);
				}
				currentTopicCounter = -1;
				if (transform.parent != null) {
						greeting.SetRandomTopicGreeting (transform.parent.GetComponent<NPCScript> ());
				} else {
						greeting.SetRandomTopicGreeting (null);
				}
				currentTopic = greeting.topic;

				nicknameOfPerson = nameOfPerson;

				choicesWithRefreshActions = new List<PlayerChoice> ();
				for (int t = 0; t < topics.Length; t++) {
						for (int c = 0; c < topics [t].choices.Length; c++) {
								if (!topics [t].choices [c].actionToRefreshSelected.Equals (""))
										choicesWithRefreshActions.Add (topics [t].choices [c]);
						}
				}
		}

		void Update ()
		{
				if (canInteract && !Console.devConsole.activeSelf && Input.GetAxisRaw ("Action") == 1 &&
				  !isInteracting && Manager.currInteraction != null && Manager.currInteraction.Equals (gameObject)) {
						OpenDialogue ();
				}
				if (!global.uicanvas.currentState.Equals ("Dialogue"))
						isInteracting = false;
		}

		void OnTriggerEnter (Collider col)
		{
				if (col.gameObject.tag == "Player") {
						canInteract = true;
						global.gameManager.DisplayInteraction (gameObject, "Talk with " + nicknameOfPerson);
				}
		}

		void OnTriggerExit (Collider col)
		{
				if (col.gameObject.tag == "Player") {
						canInteract = false;

						if (global.uicanvas.currentState.Equals ("Dialogue") && global.uicanvas.currentDialogue == this) {
								Camera.main.GetComponent<PlayerCamera> ().RevertTarget ();
						}
						global.gameManager.RemoveInteraction (gameObject);
				}
		}

		public static void BroadcastActionRefresh (string action)
		{
				for (int c = 0; c < choicesWithRefreshActions.Count; c++) {
						if (choicesWithRefreshActions [c].actionToRefreshSelected.Equals (action)) {
								choicesWithRefreshActions [c].selected = false;
						}
				}
		}

		public void OpenDialogue ()
		{
				greeting.SetRandomTopicGreeting (transform.parent.GetComponent<NPCScript> ());
				OpenDialogue ("");
		}

		public void OpenDialogue (int topicIndex)
		{
				if (topicIndex < 0 || topicIndex >= topics.Length)
						return;

				currentTopicCounter = topicIndex;
				UpdateDialogueUI ();
				Camera.main.GetComponent<PlayerCamera> ().ChangeTargetToNPC (transform);
				global.uicanvas.ChangeState ("Dialogue");
				global.uicanvas.SetDialogue (this);
				global.gameManager.BroadcastActionCompleted ("Talk to " + nameOfPerson + " about " + currentTopic.name);
				isInteracting = true;
		}

		public void OpenDialogue (string topicName)
		{
				//if the topic is not found, current topic will be 0 if the dialogue was exited properly
				for (int i = 0; i < topics.Length; i++) {
						if (topics [i].name.Equals (topicName)) {
								currentTopicCounter = i;
								break;
						}
				}

				UpdateDialogueUI ();
				Camera.main.GetComponent<PlayerCamera> ().ChangeTargetToNPC (transform);
				global.uicanvas.ChangeState ("Dialogue");
				global.uicanvas.SetDialogue (this);
				global.gameManager.BroadcastActionCompleted ("Talk to " + nameOfPerson + " about " + currentTopic.name);
				isInteracting = true;
		}

		//called from outside this class; will trigger current topic exit events
		public void ExitDialogue ()
		{
				Camera.main.GetComponent<PlayerCamera> ().RevertTarget ();
				currentTopicCounter = -1;
				currentTopic.TriggerExitEvents ();
				currentTopic = greeting.topic;

				if (!global.uicanvas.currentState.Equals ("Shop"))
						global.uicanvas.ChangeState ("");
		}

		//if we exit via choice selection, current topic exit events were already run
		private void ExitDialogueViaSelection ()
		{
				Camera.main.GetComponent<PlayerCamera> ().RevertTarget ();
				currentTopicCounter = -1;
				currentTopic = greeting.topic;
		
				if (!global.uicanvas.currentState.Equals ("Shop"))
						global.uicanvas.ChangeState ("");
		}

		public void SelectPlayerChoice (GameObject UIButton)
		{
				if (UIButton.name.Equals ("Next") || UIButton.name.Equals ("Next Button")) {
						currentTopicCounter++;
						currentTopic.TriggerExitEvents ();
				} else {
						string[] temp = UIButton.name.Split (' ');
						try {
								//changes to the new topic based on the choice
								currentTopicCounter = currentTopic.Select (int.Parse (temp [1]));
						} catch {
								currentTopicCounter = -1;
						}
				}
				//no more topics after that last player choice (ie. "Goodbye.")
				if (currentTopicCounter < 0 || currentTopicCounter >= topics.Length) {
						ExitDialogueViaSelection ();
						return;
				}
				UpdateDialogueUI ();
		}

		public void UpdateDialogueUI ()
		{
				if (currentTopicCounter < 0)
						currentTopic = greeting.topic;
				else
						currentTopic = topics [currentTopicCounter];

				global.gameManager.BroadcastActionCompleted ("Talk to " + nameOfPerson + " about " + currentTopic.name);

				//updates the topic so that we may see what it is -- remove for release
				dialogueUI.transform.FindChild ("Topic").GetComponent<Text> ().text = currentTopic.GetTopicName ();
				//updates npc words
				dialogueUI.transform.FindChild ("NPC Words").GetComponent<Text> ().text = nicknameOfPerson + ": " + currentTopic.GetNPCWords ();

				//clear all previous player choices, and makes sure that we reset our number of active choices
				SetAllChoicesInactive ();


				//if the npc has more than one line to say
				if (currentTopic.multipleNPCLines) {
						//disable the background for player choices
						dialogueUI.transform.GetChild (0).gameObject.SetActive (false);
						//disable the scrollbar
						dialogueUI.transform.GetChild (3).gameObject.SetActive (false);
						//enable the next button
						dialogueUI.transform.GetChild (5).gameObject.SetActive (true);
				}
		//the player is presented with player choices
		else {
						//makes the background for player choices active
						dialogueUI.transform.GetChild (0).gameObject.SetActive (true);
						//sets the next button inactive
						dialogueUI.transform.GetChild (5).gameObject.SetActive (false);

						//updates the player choices
						for (int i = 0; i < currentTopic.choices.Length; i++) {
								if (i < currentTopic.choices.Length)
										SetChoiceActive (i);
								else
										SetChoiceInactive (i);
						}
				}

				//hides the scroll bar if there aren't enough visible choices
				if (numOfActiveChoices < 6)
						dialogueUI.transform.GetChild (3).gameObject.SetActive (false);
				else
						dialogueUI.transform.GetChild (3).gameObject.SetActive (true);
		
				UpdateChoicePositioning ();
		}

		private void SetAllChoicesInactive ()
		{
				for (int g = 0; g < choiceScrollArea.childCount; g++)
						choiceScrollArea.GetChild (g).gameObject.SetActive (false);
				numOfActiveChoices = 0;
		}

		private void SetChoiceActive (int choice)
		{
				//gets the button we want to set as active
				Transform button = choiceScrollArea.FindChild ("Choice " + choice);

				//if the button doesn't exist, create one!
				if (button == null) {
						//copy the first button
						button = Instantiate (choiceScrollArea.GetChild (0)) as Transform;
						//change the name
						button.name = "Choice " + choice;
						//make sure that it is put into the right place in the hierarchy
						button.SetParent (choiceScrollArea);
						//this makes sure that the positioning isn't weird -- which was happening
						button.localPosition = choiceScrollArea.GetChild (0).localPosition;

						UpdateChoicePositioning ();
				}

				//enables the button
				button.gameObject.SetActive (true);

				NPCScript thisNPC = transform.parent.GetComponent<NPCScript> ();

				currentTopic.choices [choice].SetText ();

				//disables the button if the choice isn't visible
				if (!currentTopic.choices [choice].isVisible (thisNPC)) {
						button.gameObject.SetActive (false);
						return;
				}

				//sets the text for the choice
				button.GetChild (0).GetComponent<Text> ().text = currentTopic.choices [choice].GetText ();

				Color textColor = Color.white;
				//if the choice has been selected before, we want to show a difference
				if (currentTopic.choices [choice].selected)
						textColor = Color.gray;

				button.GetChild (0).GetComponent<Text> ().color = textColor;
				//keeps track of how many choices are active at once -- this will be used when we set the positions of active buttons
				numOfActiveChoices++;
		}

		private void SetChoiceInactive (int choice)
		{
				Transform button = choiceScrollArea.FindChild ("Choice " + choice);
				if (button != null)
						button.gameObject.SetActive (false);
		}

		private void UpdateChoicePositioning ()
		{
				//this is the height of each button rect
				int choiceButtonRectHeight = (int)choiceScrollArea.GetChild (0).GetComponent<RectTransform> ().rect.size.y;
				int spaceBetweenButtons = 5;
				//this is the desired height for the scroll area
				int desiredScrollAreaHeight = (numOfActiveChoices) * (choiceButtonRectHeight + spaceBetweenButtons);

				if (desiredScrollAreaHeight > choiceScrollArea.parent.GetComponent<RectTransform> ().rect.height) {
						int bottom = (int)(desiredScrollAreaHeight - choiceScrollArea.parent.GetComponent<RectTransform> ().rect.height);
						choiceScrollArea.GetComponent<RectTransform> ().offsetMin = new Vector2 (0, -bottom);
						choiceScrollArea.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, 0);
				} else {
						choiceScrollArea.GetComponent<RectTransform> ().offsetMin = new Vector2 (0, 0);
						choiceScrollArea.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, 0);
				}
				dialogueUI.transform.GetChild (3).GetComponent<Scrollbar> ().value = 1f;

				//the local position for the current button
				Vector3 currLocalPos;
				//this height is the oddly calculated height for rect transforms that will be used for the buttons
				int areaHeight = (int)(choiceScrollArea.GetComponent<RectTransform> ().rect.height / 2) - (choiceButtonRectHeight / 2) - spaceBetweenButtons;
				for (int c = 0, buttonsUpdated = 0; c < choiceScrollArea.childCount && buttonsUpdated <= numOfActiveChoices; c++) {
						//if the current button is active, then we update the position so that all the active choices shuffle to the top of the view
						if (choiceScrollArea.GetChild (c).gameObject.activeSelf) {
								currLocalPos = choiceScrollArea.GetChild (c).GetComponent<RectTransform> ().localPosition;
								choiceScrollArea.GetChild (c).GetComponent<RectTransform> ().localPosition = 
					new Vector3 (currLocalPos.x, areaHeight - (buttonsUpdated * (choiceButtonRectHeight + spaceBetweenButtons)), 0);
								buttonsUpdated++;
						}
				}
		}
}

[Serializable]
public class Greeting
{
		public string[] NeutralGreetings;
		public string[] FriendlyGreetings;
		public string[] UnfriendlyGreetings;
		[Tooltip ("The text in npc words and name are overwritten by the Greeting class.")]
		public Topic topic;

		public void SetRandomTopicGreeting (NPCScript npc)
		{
				string[] currGreetings = NeutralGreetings;

				if (npc != null) {
						if (npc.isFriendly ()) {
								//this allows a chance to still show neutral greetings if the npc is friendly -- allows for more variation
								if (UnityEngine.Random.Range (0, 2) == 0)
										currGreetings = FriendlyGreetings;
						} else if (npc.isUnfriendly ())
								currGreetings = UnfriendlyGreetings;
				}

				if (currGreetings.Length > 0)
						topic.npcWords = currGreetings [UnityEngine.Random.Range (0, currGreetings.Length)];

				topic.name = "Greeting";
		}
}


[Serializable]
public class Topic
{
		public string name;
		public string npcWords;
		//use this if the npc text is too much to fit in one view -- the player will not get a chance to respond, but only to hit next/continue
		public bool multipleNPCLines;
		public string[] eventsFiredOnExit;
		public PlayerChoice[] choices;

		public string GetTopicName ()
		{
				string[] tempNPCWords = name.Split ('|');
				string currNPCWords = tempNPCWords [0];
				for (int i = 1; i < tempNPCWords.Length; i++) {
						//unique word -- npc name, quest item name, etc
						if (i % 2 == 1) {
								try {
										currNPCWords += NPCDatabase.GetNPCName (int.Parse (tempNPCWords [i]));
								} catch {
										currNPCWords += NPCDatabase.GetNPCNameSpecial (tempNPCWords [i]);
								}
						}
			//other text
			else {
								currNPCWords += tempNPCWords [i];
						}
				}
				return currNPCWords;
		}

		public string GetNPCWords ()
		{
				string[] tempNPCWords = npcWords.Split ('|');
				string currNPCWords = tempNPCWords [0];
				for (int i = 1; i < tempNPCWords.Length; i++) {
						//unique word -- npc name, quest item name, etc
						if (i % 2 == 1) {
								try {
										currNPCWords += NPCDatabase.GetNPCName (int.Parse (tempNPCWords [i]));
								} catch {
										currNPCWords += NPCDatabase.GetNPCNameSpecial (tempNPCWords [i]);
								}
						}
			//other text
			else {
								currNPCWords += tempNPCWords [i];
						}
				}
				return currNPCWords;
		}

		public void TriggerExitEvents ()
		{
				try {
						for (int i = 0; i < eventsFiredOnExit.Length; i++)
								global.gameManager.TriggerEvent (eventsFiredOnExit [i]);
				} catch {
				}
		}

		public int Select (int choiceToSelect)
		{
				TriggerExitEvents ();
				return choices [choiceToSelect].Select ();
		}
}

//a way to check current num of items in player inventory
//change required action to refresh 'selected' based on that action
[Serializable]
public class PlayerChoice
{
		public string text = "What should the player say?";
		public string[] eventsFiredWhenChosen;
		[Range (-1, 500), Tooltip ("-1 to exit dialogue, >=0 to go to desired topic")]
		public int nextTopic = -1;
		public string actionToRefreshSelected;
		[Tooltip ("Status Requirements: Enter the key for the special variable in the NPC database")]
		public StatusRequirement[] npcStatusRequirements;
		[Tooltip ("Item Requirements: The player needs to have all of these items with the specified minimum quantity")]
		public ItemRequirement[] reqItems;
		[Tooltip ("Quest Requirements: All requirements needed in order for this choice to be visible")]
		public QuestRequirement[] reqQuest;
		public int reqSpeechLevel = 0;
		public float speechExpReward = 0f;
		[Range (-100, 100), Tooltip ("NPC disposition has to be greater than or equal to this value")]
		public int dispositionGreaterThan = -100;
		[Range (-100, 100), Tooltip ("NPC disposition has to be less than or equal to this value")]
		public int dispositionLessThan = 100;
		public bool repeatable = true;
		public bool selected;
		private string currText;

		public int Select ()
		{
				if (!selected)
						global.playerScript.AddExp (1, speechExpReward);

				selected = true;
				try {
						for (int i = 0; i < eventsFiredWhenChosen.Length; i++)
								global.gameManager.TriggerEvent (eventsFiredWhenChosen [i]);
				} catch {
						Debug.Log ("Failed to trigger event.");
				}
				for (int i = 0; i < reqItems.Length; i++) {
						if (!reqItems [i].invertReqResult)
								reqItems [i].Enforce ();
				}
				return nextTopic;
		}

		public void SetText ()
		{
				string[] tempText = text.Split ('|');
				currText = tempText [0];
				string textToADD;
				for (int i = 1; i < tempText.Length; i++) {
						//unique word -- npc name, quest item name, etc
						if (i % 2 == 1) {
								try {
										textToADD = NPCDatabase.GetNPCName (int.Parse (tempText [i]));
										currText += textToADD;
								} catch {
										textToADD = NPCDatabase.GetNPCNameSpecial (tempText [i]);
										currText += textToADD;
								}
								if (textToADD.Equals ("null")) {
										currText = "null";
										return;
								}
						}
			//other text
			else {
								currText += tempText [i];
						}
				}
		}

		public string GetText ()
		{
				return currText;
		}

		public bool isVisible (NPCScript npc)
		{
				if (text.Equals ("null") || currText.Equals ("null"))
						return false;

				//if the player doesn't have a high enough coercion level
				if (global.playerScript.Skills [1].CurrentLevel < reqSpeechLevel)
						return false;

				//the npc doesn't like the player enough
				if (npc != null && (npc.disposition < dispositionGreaterThan || npc.disposition > dispositionLessThan))
						return false;

				bool hasAllOfQuestReqs = true;
				for (int i = 0; i < reqQuest.Length; i++) {
						if (!reqQuest [i].isSatisfied ()) {
								hasAllOfQuestReqs = false;
								break;
						}
				}
				//if the player doesn't have all of the quests or the required action hasn't been completed
				if (!hasAllOfQuestReqs && reqQuest.Length > 0)
						return false;

				bool hasRequiredStatuses = true;
				for (int i = 0; i < npcStatusRequirements.Length; i++) {
						bool haveStatus = NPCDatabase.GetNPCNameSpecial (npcStatusRequirements [i].status).Equals (npc.name);
						//if the status being empty is suffice
						if (npcStatusRequirements [i].unheldSuffice) {
								//if we don't hold the status, and the status is currently held, return false
								if (!haveStatus && !NPCDatabase.GetNPCNameSpecial (npcStatusRequirements [i].status).Equals ("null")) {
										hasRequiredStatuses = false;
										break;
								}
						}
			//if having the status is mandatory
			else {
								//if we don't have the status, return false
								if (!haveStatus) {
										hasRequiredStatuses = false;
										break;
								}
						}
				}

				if (!hasRequiredStatuses)
						return false;


				bool hasRequiredItems = true;
				for (int i = 0; i < reqItems.Length; i++) {
						if (!reqItems [i].isSatisfied ()) {
								if (!reqItems [i].invertReqResult) {
										hasRequiredItems = false;
										break;
								}
						} else if (reqItems [i].invertReqResult) {
								hasRequiredItems = false;
								break;
						}
				}
				if (!hasRequiredItems)
						return false;

				//if the option isn't repeatable, and it has been selected once before
				if (!repeatable && selected)
						return false;
				return true;
		}
}

[Serializable]
public class QuestRequirement
{
		public string questName;
		[Tooltip ("If there is no required branch, leave blank or place '~'")]
		public string questBranch;
		[Tooltip ("-1 for completed quest, -2 for having quest, -3 for not having quest. >0 for quest position.")]
		public int questPosition;
		[Tooltip ("Mark true if dialogue option should only exist at the exact position for the quest.")]
		public bool exactPosition;

		public bool isSatisfied ()
		{
				bool hasQuest = global.playerScript.HasQuest (questName);
				// -3 for when the player doesn't have the quest
				if (questPosition == -3)
						return !hasQuest;
		//-2 for just having the quest
		else if (questPosition == -2)
						return hasQuest;
		
				string branch = global.playerScript.GetQuestBranch (questName);
				//for the following checks, we need to be on the right branch
				if (questBranch.Equals ("") || questBranch.Equals ("~") || questBranch.Equals (branch)) {
						//-1 for when the quest is completed
						bool questcomplete = global.playerScript.GetQuestCompleted (questName);
						if (questPosition == -1)
								return questcomplete;
			
						int pos = global.playerScript.GetQuestPosition (questName);
						//if the quest is not complete
						if (!questcomplete) {
								//if the exact position is required
								if (exactPosition) {
										if (pos == questPosition)
												return true;
								}
				//else if we are at that position or past it
				else if (pos >= questPosition) {
										return true;
								}
				
						}
				}
				//nothing was satisfied, so we don't meet req
				return false;
		}
}

[Serializable]
public class ItemRequirement
{
		[Tooltip ("The name of  the item")]
		public string name;
		[Range (0, 5000)]
		public int quantity;
		[Tooltip ("Does the player need to have this exact quantity?")]
		public bool exactQuantity;
		[Tooltip ("Should this quantity be removed on selection?")]
		public bool removeQuantityOnSelect;
		[Tooltip ("Item requirement now returns this value if the player doesn't meet the requirement. (if invert, return true)")]
		public bool invertReqResult;

		public bool isSatisfied ()
		{
				if (name.Equals ("Money") || name.Equals ("money") || name.Equals ("gold") || name.Equals ("Gold")) {
						if (global.playerInventory.storedCurrency < quantity)
								return false;
						return true;
				}

				if (!global.playerInventory.HasItem (global.itemDatabase.GetItem(name)))
						return false;

				int itemqty = global.playerInventory.GetItem (global.itemDatabase.GetItem(name)).quantity;
				if (!exactQuantity && itemqty < quantity)
						return false;
				else if (exactQuantity && itemqty != quantity)
						return false;

				return true;
		}

		public void Enforce ()
		{
				if (removeQuantityOnSelect && isSatisfied ()) {
						if (name.Equals ("Money") || name.Equals ("money") || name.Equals ("gold") || name.Equals ("Gold")) {
								global.playerInventory.storedCurrency -= quantity;
								return;
						}
						global.playerInventory.RemoveItem (global.itemDatabase.GetItem(name));
				}
		}
}

[Serializable]
public class StatusRequirement
{
		public string status;
		[Tooltip ("Set true if having the status as empty is suffice")]
		public bool unheldSuffice;
}