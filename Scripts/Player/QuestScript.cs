using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class QuestScript : MonoBehaviour
{

		public Quest[] questList;
		private GameObject UITemplate;
		private Transform templateParent;

		public static Quest activeQuest;

		void Awake ()
		{
				for (int i = 0; i < questList.Length; i++)
						questList [i].id = i;
		}

		void Start ()
		{
				if (SceneManager.GetActiveScene ().name.Equals ("MainMenu"))
						return;

				UITemplate = GameObject.FindGameObjectWithTag ("QuestUITemplate");
				UITemplate.SetActive (false);
				templateParent = GameObject.FindGameObjectWithTag ("QuestUIParent").transform;
		}

		public static void AddQuest (Quest quest)
		{
				Transform uiobj = null;
				if (!quest.completed)
						uiobj = global.questDatabase.templateParent.GetChild (0).FindChild (quest.name);
				if (uiobj == null) {
						AddUIForQuest (quest);
						RefreshQuestUI ();
				}
		}

		private static Transform AddUIForQuest (Quest quest)
		{
				GameObject newUIelement = Instantiate (global.questDatabase.UITemplate) as GameObject;
				newUIelement.SetActive (true);
				if (!quest.completed)
						newUIelement.transform.SetParent (global.questDatabase.templateParent.GetChild (0));
				else
						newUIelement.transform.SetParent (global.questDatabase.templateParent.GetChild (1));
				newUIelement.name = quest.name;
				newUIelement.GetComponent<Text> ().text = quest.name;
				try {
						newUIelement.transform.GetChild (0).GetComponent<Text> ().text = quest.objectives [quest.position].GetDescription ();
						global.console.AddMessage (quest.objectives [quest.position].GetDescription ());
						global.console.AddMessage ("Quest Added: " + quest.name);
				} catch {
						newUIelement.transform.GetChild (0).GetComponent<Text> ().text = "Quest Completed";
						newUIelement.transform.GetChild (0).GetComponent<Text> ().color = Color.green;
				}

				return newUIelement.transform;
		}

		private static void UpdateQuestInfo (Quest quest, Transform uiobj)
		{
				if (!quest.completed && !quest.failed) {
						uiobj.GetChild (0).GetComponent<Text> ().text = quest.objectives [quest.position].GetDescription ();
				} else {
						if (quest.completed) {
								uiobj.GetChild (0).GetComponent<Text> ().text = "Quest Complete";
								uiobj.GetChild (0).GetComponent<Text> ().color = Color.green;
						} else if (quest.failed) {
								uiobj.GetChild (0).GetComponent<Text> ().text = "Quest Failed";
								uiobj.GetChild (0).GetComponent<Text> ().color = Color.red;
						}
				}
		}

		//used by ui buttons
		public void RefreshUI ()
		{
				RefreshQuestUI ();
		}

		public static void RefreshQuestUI ()
		{
				float halfSize = global.questDatabase.templateParent.GetChild (0).GetChild (0).GetComponent<RectTransform> ().rect.max.y;
				float distBtwn = 110;

				int desiredActiveTabHeight = (int)(halfSize + distBtwn) * (global.questDatabase.templateParent.GetChild (0).childCount + 1);
				int desiredCompletedTabHeight = (int)(halfSize + distBtwn) * (global.questDatabase.templateParent.GetChild (1).childCount + 1);

				int currentDesiredHeight;

				bool onlyUpdateActive = false;
				if (global.questDatabase.templateParent.GetComponent<MenuScript> ().currentState.Equals ("Active")) {
						currentDesiredHeight = desiredActiveTabHeight;
						onlyUpdateActive = true;
				} else {
						currentDesiredHeight = desiredCompletedTabHeight;
				}

				// the size we want is bigger than the default size, then we adjust it
				if (currentDesiredHeight > global.questDatabase.templateParent.parent.GetComponent<RectTransform> ().rect.max.y) {
						int bottom = currentDesiredHeight
						             - (int)(global.questDatabase.templateParent.parent.GetComponent<RectTransform> ().rect.height);
						global.questDatabase.templateParent.GetComponent<RectTransform> ().offsetMin = new Vector2 (0, -bottom);
						global.questDatabase.templateParent.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, 0);
				} else {
						global.questDatabase.templateParent.GetComponent<RectTransform> ().offsetMin = new Vector2 (0, 0);
						global.questDatabase.templateParent.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, 0);
				}
		
				//parent of scroll area is scroll view, parent of scroll view is quest ui -- then we find the scroll bar under quest ui
				//then we make sure that it is at the top of the view
				global.questDatabase.templateParent.parent.parent.Find ("Scrollbar").GetComponent<Scrollbar> ().value = 1f;



				bool wasUpdated = false;
				//go through each of the player's quests
				for (int i = 0; i < global.playerScript.questList.Count; i++) {
						wasUpdated = false;
						if (global.playerScript.questList [i].completed && !onlyUpdateActive) {
								//if completed, check the completed tab
								for (int c = global.questDatabase.templateParent.GetChild (1).childCount - 1; c >= 0; c--) {
										//if we find it in the tab
										if (global.questDatabase.templateParent.GetChild (1).GetChild (c).name.Equals (global.playerScript.questList [i].name)) {
												//update text, color, etc
												UpdateQuestInfo (global.playerScript.questList [i], global.questDatabase.templateParent.GetChild (1).GetChild (c));
												//update position
												global.questDatabase.templateParent.GetChild (1).GetChild (c).GetComponent<RectTransform> ().localPosition = 
							new Vector3 (0f, (global.questDatabase.templateParent.GetComponent<RectTransform> ().rect.height / 2) - halfSize - (c * distBtwn), 0f);
												//flag as updated and break from the loop
												wasUpdated = true;
												break;
										}
								}

								//If it wasn't updated, that means it wasn't found in the tab. So, add it to the tab
								if (!wasUpdated) {
										AddUIForQuest (global.playerScript.questList [i]);
								}
						} else if (!global.playerScript.questList [i].completed) {
								//check the active quest tab
								for (int a = global.questDatabase.templateParent.GetChild (0).childCount - 1; a >= 1; a--) {
										if (global.playerScript.questList [i].name.Equals (global.questDatabase.templateParent.GetChild (0).GetChild (a).name)) {
												UpdateQuestInfo (global.playerScript.questList [i], global.questDatabase.templateParent.GetChild (0).GetChild (a));
												global.questDatabase.templateParent.GetChild (0).GetChild (a).GetComponent<RectTransform> ().localPosition = 
							new Vector3 (0f, (global.questDatabase.templateParent.GetComponent<RectTransform> ().rect.height / 2) - halfSize - ((a - 1) * distBtwn), 0f);
												wasUpdated = true;
												break;
										}
								}

								if (!wasUpdated) {
										AddUIForQuest (global.playerScript.questList [i]);
								}
						}
				}
		}

		public static void UpdateQuestObjectives (Player player, string ActionCompleted)
		{
				//this boolean is used to make sure that the current action only affects the first required occurrance in an objective per quest
				bool oneActionUpdated;
				//this loop goes through each of the quests in the list (q)
				for (int q = 0; q < player.questList.Count; q++) {
						oneActionUpdated = false;
						//this loop goes through the objectives required to complete the quest, starting with the current objective for the quest (o)
						for (int o = player.questList [q].position; o < player.questList [q].objectives.Length && !player.questList [q].completed && !player.questList [q].failed; o++) {
								//this loop goes through each of the actions required to complete the current objective (a)
								for (int a = 0; a < player.questList [q].objectives [o].ActionsNeeded.Length; a++) {
										//if the action completed matches an action in the current objective
										if (player.questList [q].objectives [o].ActionsNeeded [a].GetAction ().Equals (ActionCompleted)) {
												//if we are currently on the branch that is needed to complete this action
												if (player.questList [q].objectives [o].ActionsNeeded [a].RequiredBranch.Equals ("") ||
												    player.questList [q].objectives [o].ActionsNeeded [a].RequiredBranch.Equals (player.questList [q].currentBranch)) {
														//it was matched, so mark this action as completed -- but the current objective isn't necessarily fulfilled yet
														player.questList [q].objectives [o].ActionsNeeded [a].Complete ();
														//if the objective we are checking is in fact the current objective for the player and it is indeed completed
														if (o == player.questList [q].position && player.questList [q].objectives [o].isCompleted ()) {
																//advance the quest to the next objective according to the desired quest branch -- returns true if quest is completed
																if (player.questList [q].AdvanceToNextObjective (player.questList [q].objectives [o].ActionsNeeded [a])) {
																		CompleteQuest (player.questList [q]);
																} else if (activeQuest == player.questList [q]) {
																		player.questList [q].objectives [player.questList [q].position].PlaceAllMarkers ();
																}
														}
														//one action was updated, so we do not want to update the following objectives for the same action -- so we break from this loop
														oneActionUpdated = true;
														break;
												}
										}
								}
								//and break from this loop as well, and then check the next quest
								if (oneActionUpdated)
										break;
						}
				}
		
				RefreshQuestUI ();
		}

		public static void CompleteQuest (Quest q)
		{
				Transform uiobj = global.questDatabase.templateParent.GetChild (0).FindChild (q.name);
				uiobj.transform.SetParent (global.questDatabase.templateParent.GetChild (1));
				uiobj.GetChild (1).gameObject.SetActive (false);
				uiobj.GetChild (2).gameObject.SetActive (false);
		}

		//this method sets the selected quest as the active quest, which puts an objective marker on the minimap if the player has the setting on
		public void SetQuestToActive (GameObject QuestLogUI)
		{
				if (activeQuest != null)
						RemoveActiveQuest ();
				activeQuest = GetQuest (global.playerScript.questList.ToArray (), QuestLogUI.name);
				activeQuest.objectives [activeQuest.position].PlaceAllMarkers ();
		}

		public void RemoveActiveQuest ()
		{
				if (activeQuest == null)
						return;
				for (int i = 0; i < activeQuest.objectives.Length; i++)
						activeQuest.objectives [i].RemoveAllMarkers ();
				Transform temp = global.questDatabase.templateParent.GetChild (0).FindChild (activeQuest.name);
				if (temp != null) {
						temp.GetChild (1).gameObject.SetActive (true);
						temp.GetChild (2).gameObject.SetActive (false);
				}
		}

		//this method is called once an NPC dies, and it fails any quests that they are needed to complete.
		public static void CheckforRequiredNPC (List<Quest> list, string DeadNPC)
		{
				foreach (Quest q in list) {
						q.CheckForFailureDueToNPCDeath (DeadNPC);
				}
		}

		public int GetQuestID (string questName)
		{
				for (int i = 0; i < questList.Length; i++) {
						if (questList [i].name.Equals (questName)) {
								return questList [i].id;
						}
				}
				return -1;
		}

		public static Quest GetQuest (Quest[] list, string questName)
		{
				for (int i = 0; i < list.Length; i++) {
						if (list [i].name.Equals (questName)) {
								return list [i];
						}
				}
				return null;
		}
}

[Serializable]
public class Quest
{
		public string name;
		[HideInInspector]
		public string currentBranch;
		[HideInInspector]
		public int id;
		[HideInInspector]
		public int position;
		[HideInInspector]
		public bool completed;
		[HideInInspector]
		public bool failed;
		public Objective[] objectives = new Objective[0];
		public Reward[] rewards;

		public Quest ()
		{
		}

		//used to add quests to the player, without using the quests in the quest database. this allows the player to do the same quest multiple times.
		public void CopyQuest (Quest q)
		{
				name = q.name;
				currentBranch = q.currentBranch;
				id = q.id;
				position = 0;
				completed = false;
				failed = false;
				objectives = q.objectives;
				rewards = q.rewards;
		}

		public bool AdvanceToNextObjective (ObjectiveAction action)
		{
				if (action.JumpTo > -1)
						return JumpToNextObjective (action.JumpTo, action.ChangeBranchTo);
				else
						return AdvanceToNextObjective (action.ChangeBranchTo);
		}

		public bool JumpToNextObjective (int pos, string newBranch)
		{
				objectives [position].RemoveAllMarkers ();
				if (pos >= objectives.Length) {
						Complete ();
						return true;
				} else if (pos > -1) {
						if (!objectives [pos].isCompleted ()) {
								if (!newBranch.Equals (""))
										currentBranch = newBranch;
								position = pos;
								Refresh ();
								return false;
						}

						try {
								while (objectives [pos].isCompleted ())
										pos++;

								if (!newBranch.Equals (""))
										currentBranch = newBranch;
								position = pos;
								Refresh ();

						} catch {
								Complete ();
								return true;
						}
				}
				return false;
		}

		public bool AdvanceToNextObjective (string newBranch)
		{
				if (!newBranch.Equals (""))
						currentBranch = newBranch;

				//this removes the minimap markers
				objectives [position].RemoveAllMarkers ();

				try {
						//this while loop gets us the next objective in this particular branch of the quest
						//it throws an exception once it passes the end of the array -- which means there is not a next objective
						do {
								position++;
						} while (!objectives [position].Branch.Equals (currentBranch));
				} catch {
						Complete ();
						return true;
				}

				//if the new objective was previously completed, go to the next one
				if (objectives [position].isCompleted ())
						return AdvanceToNextObjective (currentBranch);
				else
						Refresh ();
				return false;
		}

		private void Refresh ()
		{
				//display the new description in the console
				global.console.AddMessage (objectives [position].GetDescription ());
		}

		private void Complete ()
		{
				completed = true;
				QuestScript.CompleteQuest (this);
				//tell the player that they've completed the quest
				global.console.AddMessage ("Quest Completed: " + name);
				//this loop gets the rewards for that branch
				for (int i = 0; i < rewards.Length; i++) {
						if (rewards [i].branchForReward.Equals (currentBranch) || rewards [i].branchForReward.Equals ("default")) {
								rewards [i].GiveRewards ();
						}
				}
		}

		//returns true if the quest is failed due to an npc dying
		public bool CheckForFailureDueToNPCDeath (string npcName)
		{
				//this variable stores the number of actions affected by the npc's death
				int actionsAffected;
				//this loop goes through the upcoming objectives in the quest
				for (int o = position; o < objectives.Length; o++) {
						actionsAffected = 0;
						//this loop goes through all of the actions needed
						for (int a = 0; a < objectives [o].ActionsNeeded.Length; a++) {
								//if the current action requires the npc that died, add it to our count of affected actions
								if (objectives [o].ActionsNeeded [a].RequiredNPC.Equals (npcName)) {
										actionsAffected++;

										//if we need to have 3 actions to complete the objective, we have 4 possible actions, but 2 are affected.. 
										//we are left with 4-2 < 3. quest failed
										if (objectives [o].ActionsNeeded.Length - actionsAffected < objectives [o].numOfActionsReqToComplete) {
												failed = true;
												return true;
										}
								}
						}
				}
				return false;
		}
}

[Serializable]
public class Objective
{
		public string Description;
		//this string will be used to make branching quests -- ie. talk to so-and-so or kill bob. kill bob takes you down a diff path
		public string Branch;

		//this bool determines whether we need each action or only one of them to go to the next objective
		[Range (1, 25)]
		public int numOfActionsReqToComplete = 1;
		public ObjectiveAction[] ActionsNeeded;

		private static GameObject prefabMarker;

		public string GetDescription ()
		{
				if (Description != null) {
						string[] tempDecription = Description.Split ('|');
						string currDescription = tempDecription [0];
						for (int i = 1; i < tempDecription.Length; i++) {
								//unique word -- npc name, quest item name, etc
								if (i % 2 == 1) {
										try {
												currDescription += NPCDatabase.GetNPCName (int.Parse (tempDecription [i]));
										} catch {
												currDescription += NPCDatabase.GetNPCNameSpecial (tempDecription [i]);
										}
								}
				//other text
				else {
										currDescription += tempDecription [i];
								}
						}
						Description = currDescription;
				}
				return Description;
		}

		public bool isCompleted ()
		{
				int numOfCompletedActions = 0;
				for (int i = 0; i < ActionsNeeded.Length; i++) {
						if (ActionsNeeded [i].Completed)
								numOfCompletedActions++;
				}

				//this is just in case we set the requirement higher than the actual number of actions
				if (numOfActionsReqToComplete > ActionsNeeded.Length) {
						if (numOfCompletedActions >= ActionsNeeded.Length)
								return true;
				}

				return numOfCompletedActions >= numOfActionsReqToComplete ? true : false;
		}

		public void PlaceAllMarkers ()
		{
				if (prefabMarker == null)
						prefabMarker = Resources.Load<GameObject> ("UI Prefabs/ObjectiveMarker");
		
				for (int i = 0; i < ActionsNeeded.Length; i++) {
						if (!ActionsNeeded [i].Completed)
								ActionsNeeded [i].PlaceMarker (prefabMarker);
				}
		}

		public void RemoveMarkerForAction (int a)
		{
				if (a < 0 || a > ActionsNeeded.Length)
						return;

				ActionsNeeded [a].RemoveMarker ();
		}

		public void RemoveAllMarkers ()
		{
				for (int a = 0; a < ActionsNeeded.Length; a++) {
						ActionsNeeded [a].RemoveMarker ();
				}
		}
}

[Serializable]
public class ObjectiveAction
{
		public string Action;

		public string ObjectiveObjectName;
		[NonSerialized]
		public Vector2 Location;

		[HideInInspector]
		public bool Completed;
		[Tooltip ("Enter -1 to ignore; If the objective it jumps to has been completed, will go to next objective in branch automatically.")]
		public int JumpTo = -1;
		public string ChangeBranchTo;

		public string RequiredBranch;
		public string RequiredNPC;

		public string[] EventsFiredOnComplete;

		[NonSerialized]
		private GameObject currentMarker;

		public string GetAction ()
		{
				string[] tempAction = Action.Split ('|');
				string currAction = tempAction [0];
				for (int i = 1; i < tempAction.Length; i++) {
						//unique word -- npc name, quest item name, etc
						if (i % 2 == 1) {
								try {
										currAction += NPCDatabase.GetNPCName (int.Parse (tempAction [i]));
								} catch {
										currAction += NPCDatabase.GetNPCNameSpecial (tempAction [i]);
								}
						}
			//other text
			else {
								currAction += tempAction [i];
						}
				}
				return currAction;
		}

		public void PlaceMarker (GameObject prefab)
		{
				try {
						string name = "";
						if (ObjectiveObjectName.Contains ("|")) {
								name = ObjectiveObjectName.Substring (1, ObjectiveObjectName.Length - 2);
								name = NPCDatabase.GetNPCNameSpecial (name);
						} else
								name = ObjectiveObjectName;

						currentMarker = GameObject.Instantiate (prefab, GameObject.Find (name).transform.position, prefab.transform.rotation) as GameObject;
						currentMarker.name = "ObjectiveMarker";
						currentMarker.transform.SetParent (GameObject.Find (name).transform);
						currentMarker.transform.localPosition = Vector3.zero + new Vector3 (0, 50, 0);
						currentMarker.layer = 8;
				} catch {
						if (Location.Equals (Vector2.zero))
								return;
						try {
								currentMarker = GameObject.Instantiate (prefab, new Vector3 (Location.x, 50, Location.y), prefab.transform.rotation) as GameObject;
								currentMarker.name = "ObjectiveMarker";
								currentMarker.layer = 8;
						} catch {
						}
				}
		}

		public void RemoveMarker ()
		{
				try {
						GameObject.Destroy (currentMarker);
				} catch {
				}
		}

		private void FireEvents ()
		{
				try {
						for (int i = 0; i < EventsFiredOnComplete.Length; i++)
								global.gameManager.TriggerEvent (EventsFiredOnComplete [i]);
				} catch {
						Debug.Log ("Failed to trigger event.");
				}
		}

		public void Complete ()
		{
				Completed = true;
				FireEvents ();
				RemoveMarker ();
		}
}

[Serializable]
public class Reward
{
		public string branchForReward;
		public int[] itemIDs;
		public int money;
		public int bodies; //remove at some point
		public float[] experience;
		public string followUpQuestName;
		public string[] eventsToFire;

		public void GiveRewards ()
		{
				global.playerInventory.storedCurrency += money;

				for (int b = 0; b < bodies; b++)
						global.playerInventory.AddItem (global.itemDatabase.GetItem ("Body"));
				
				global.playerScript.AddQuest (followUpQuestName);

				for (int e = 0; e < experience.Length; e++) {
						global.playerScript.AddExp (e, experience [e]);
				}

				try {
						Item tempItem;
						for (int i = 0; i < itemIDs.Length; i++) {
								tempItem = global.itemDatabase.GetItem (itemIDs [i]);
								global.playerInventory.AddItem (tempItem);
				
						}
				} catch {
				}

				try {
						for (int i = 0; i < eventsToFire.Length; i++)
								global.gameManager.TriggerEvent (eventsToFire [i]);
				} catch {
				}
		}
}