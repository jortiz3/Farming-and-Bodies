using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
		private float prevCharacterButtonState;
		private float prevPauseButtonState;
		private float prevQuestLogButtonState;
		private float prevEscapeButtonState;
		private bool prevF1ButtonState;
		public bool objectiveMarkersEnabled = true;
		public static GameObject currInteraction;
		private static string interactionString;

		public static bool LoadScreenOn = true;

		public string[] LoadScreenFileNames;

		void Start ()
		{
				if (!global.initialized)
						global.screenFadeTexture = GameObject.FindGameObjectWithTag ("ScreenFadeImage").GetComponent<Image> ();

				if (LoadScreenOn) {
						LoadScreenOn = false;
						global.screenFadeTexture.sprite = null;
						global.screenFadeTexture.color = Color.clear;
				}
		}

		void Update ()
		{
				if (LoadScreenOn) {
						global.screenFadeTexture.gameObject.SetActive (true);
						global.screenFadeTexture.sprite = Resources.Load<Sprite> ("Load Screens/" + (LoadScreenFileNames [Random.Range (0, LoadScreenFileNames.Length)]));
						global.screenFadeTexture.color = Color.white;
						return;
				}

				if (!global.initialized)
						return;

				if (global.playerObject == null) {
						//change this to a death ui at some point
						if (!global.uicanvas.currentState.Equals ("Pause"))
								global.uicanvas.ChangeState ("Pause");
						return;
				}

				if (global.screenIsFading) {
						Time.timeScale = 1;
						return;
				}

				if (!Console.devConsole.activeSelf) {
						if (Input.GetAxisRaw ("Character") == 1 && prevCharacterButtonState != 1) {
								ChangeUIState ("Character Info");
						} else if (Input.GetAxisRaw ("Pause") == 1 && prevPauseButtonState != 1) {
								ChangeUIState ("Pause");
						} else if (Input.GetAxisRaw ("Quest Log") == 1 && prevQuestLogButtonState != 1) {
								ChangeUIState ("Quest Log");
						} else if (Input.GetAxisRaw ("Cancel") == 1 && prevEscapeButtonState != 1) {
								if (global.uicanvas.currentState.Equals ("") || global.uicanvas.currentState.Equals ("Interaction") ||
								            global.uicanvas.currentState.Equals ("Tutorial"))
										ChangeUIState ("Pause");
								else if (global.uicanvas.currentState.Equals ("Dialogue")) {
										if (currInteraction == null ||
										currInteraction.GetComponent<Dialogue> () != null && currInteraction.GetComponent<Dialogue> ().canFreelyExit)
												ChangeUIState ("");
								} else
										ChangeUIState ("");
						}
				}

				if (Input.GetKeyDown (KeyCode.F1) && !prevF1ButtonState) {
						global.console.ToggleDevConsole ();
				}

				if (global.uicanvas.currentState.Equals ("Inventory")) {
						if (currInteraction != null) {
								if (currInteraction.GetComponent<Tutorial> () != null && currInteraction.GetComponent<Tutorial> ().DisplayText ())
										ChangeUIState ("Tutorial");
								else
										ChangeUIState ("Interaction");
						}
				}

				if (global.uicanvas.currentState.Equals ("") || global.uicanvas.currentState.Equals ("Dialogue") ||
				        global.uicanvas.currentState.Equals ("Interaction"))
						Time.timeScale = 1;
				else
						Time.timeScale = 0;

				prevF1ButtonState = Input.GetKeyDown (KeyCode.F1);
				prevEscapeButtonState = Input.GetAxisRaw ("Cancel");
				prevCharacterButtonState = Input.GetAxisRaw ("Character");
				prevPauseButtonState = Input.GetAxisRaw ("Pause");
				prevQuestLogButtonState = Input.GetAxisRaw ("Quest Log");
		}

		public void ChangeUIState (string state)
		{
				if (global.uicanvas.currentState.Equals ("Dialogue"))
						global.uicanvas.currentDialogue.ExitDialogue ();
				else if (global.uicanvas.currentState.Equals ("Tutorial"))
						currInteraction = null;

				global.uicanvas.ChangeState (state);
		}

		public void ChangeUIState (string state, string complimentaryState)
		{
				if (global.uicanvas.currentState.Equals ("Dialogue"))
						global.uicanvas.currentDialogue.ExitDialogue ();
				else if (global.uicanvas.currentState.Equals ("Tutorial"))
						currInteraction = null;

				global.uicanvas.ChangeState (state, complimentaryState);
		}

		public void BroadcastActionCompleted (string action)
		{
				QuestScript.UpdateQuestObjectives (global.playerScript, action);
				Dialogue.BroadcastActionRefresh (action);
		}

		public void DisplayInteraction (GameObject obj, string text)
		{
				if (global.screenIsFading)
						return;

				if (currInteraction == null) {
						if (obj.GetComponent<Tutorial> () != null && obj.GetComponent<Tutorial> ().DisplayText ())
								ChangeUIState ("Interaction", "Tutorial");
						else
								ChangeUIState ("Interaction");
				}

				if (global.uicanvas.currentState.Equals ("Tutorial"))
						ChangeUIState ("Interaction", "Tutorial");

				currInteraction = obj;
				global.uicanvas.transform.FindChild ("Interaction").FindChild ("Interaction Text").GetComponent<Text> ().text = text;
				RectTransform temp = global.uicanvas.transform.FindChild ("Interaction").GetComponent<RectTransform> ();
				temp.sizeDelta = new Vector2 (global.uicanvas.transform.FindChild ("Interaction").FindChild ("Interaction Text").GetComponent<Text> ().preferredWidth + 20, temp.sizeDelta.y);
		}

		public void RemoveInteraction (GameObject obj)
		{
				if (currInteraction == null || global.uicanvas.currentState.Equals ("Dialogue"))
						return;
				if (currInteraction.Equals (obj)) {
						if (global.uicanvas.currentState.Equals ("Interaction") || global.uicanvas.currentState.Equals ("Dialogue") ||
						 global.uicanvas.currentState.Equals ("Fading") || global.uicanvas.currentState.Equals ("Tutorial"))
								global.uicanvas.ChangeState ("");

						currInteraction = null;
				}
		}

		public void DisplayTutorial (GameObject obj)
		{
				currInteraction = obj;
				ChangeUIState ("Tutorial");
		}

		public void RemoveTutorial (GameObject obj)
		{
				if (currInteraction == null)
						return;
				if (currInteraction.Equals (obj)) {
						if (global.uicanvas.currentState.Equals ("Tutorial")) {
								global.uicanvas.ChangeState ("");
								currInteraction = null;
						} else
								global.uicanvas.HideGroup ("Tutorial");
				}
		}

		public string TriggerEvent (string name)
		{
				string[] temp = name.Split (' ');

				if (temp [0].Equals ("Give") || temp [0].Equals ("give")) {
						//"Give Quest <id>" or "Give Quest <quest_name>"
						if (temp [1].Equals ("Quest") || temp [1].Equals ("quest")) {
								try {
										global.playerScript.AddQuest (int.Parse (temp [2]));
								} catch {
										try {
												string questName = ReplaceCharWithSpaces (temp [2], '_');
												global.playerScript.AddQuest (questName);
										} catch {
												return "Could not parse. Command Format: 'Give Quest <id>' or 'Give Quest <quest_name>'";
										}
								}
						}
			//"Give Item <id> <qty>" or "Give Item <item_name;item_suffix> <qty>"
            else if (temp [1].Equals ("Item") || temp [1].Equals ("item")) {
								try {
										Item itemToAdd = global.itemDatabase.GetItem (int.Parse (temp [2]));
										itemToAdd.quantity = int.Parse (temp [3]);
										global.playerInventory.AddItem (itemToAdd);
								} catch {
										try {
												string itemName = ReplaceCharWithSpaces (temp [2], '_');
												Item itemToAdd = global.itemDatabase.GetItem (itemName);
												itemToAdd.quantity = int.Parse (temp [3]);
												global.playerInventory.AddItem (itemToAdd);
										} catch {
												return "Could not parse. Command Format: 'Give Item <id> <qty>' or 'Give Item <item_name;item_suffix> <qty>'";
										}
								}
						}
            //Give Money <qty>
            else if (temp [1].Equals ("Money") || temp [1].Equals ("money") ||
						               temp [1].Equals ("Gold") || temp [1].Equals ("gold")) {
								try {
										global.playerInventory.storedCurrency += int.Parse (temp [2]);
								} catch {
										return "Could not parse. Command Format: Give Money <qty>";
								}
						}
            //Give Bodies <qty>
            else if (temp [1].Equals ("Bodies") || temp [1].Equals ("bodies")) {
								try {
										Item itemToAdd = global.itemDatabase.GetItem("Body");
										itemToAdd.quantity = int.Parse (temp [2]);
										global.playerInventory.AddItem (itemToAdd);
								} catch {
										return "Could not parse. Command Format: Give Bodies <qty>";
								}
						}
						//give action <npc_name_or_player> <action_name>
				} else if (temp [0].Equals ("Advance") || temp [0].Equals ("advance")) {
						//Advance Quest <id> <position>
						if (temp [1].Equals ("Quest") || temp [1].Equals ("quest")) {
								try {
										if (temp.Length < 4)
												global.playerScript.AdvanceQuest (int.Parse (temp [2]));
										else
												global.playerScript.SetQuestPosition (int.Parse (temp [2]), int.Parse (temp [3]));
								} catch {
										try {
												string questName = ReplaceCharWithSpaces (temp [2], '_');
												if (temp.Length < 4)
														global.playerScript.AdvanceQuest (questName);
												else
														global.playerScript.SetQuestPosition (questName, int.Parse (temp [3]));
										} catch {
												return "Could not parse. Command Format: Advance Quest <id||quest_name> <toPosition:optional>";
										}
                    
								}
						}
            //"Advance <Skill Name> <exp amount>"
            else if (temp [1].Equals ("Farming") || temp [1].Equals ("farming")) {
								try {
										global.playerScript.AddExp (0, float.Parse (temp [2]));
								} catch {
										return "Could not parse '" + temp [2] + "' to an int.";
								}
						} else if (temp [1].Equals ("Speech") || temp [1].Equals ("speech")) {
								try {
										global.playerScript.AddExp (1, float.Parse (temp [2]));
								} catch {
										return "Could not parse '" + temp [2] + "' to a float.";
								}
						}
				} else if (temp [0].Equals ("Spawn") || temp [0].Equals ("spawn")) {
						//Spawn Item <id> <x,y,z>
						if (temp [1].Equals ("Item") || temp [1].Equals ("item")) {
								try {
										GameObject newItem = global.itemDatabase.GetItem (int.Parse (temp [2])).Instantiate ();
										string[] positionValues = temp [3].Split (',');
										newItem.transform.position = new Vector3 (float.Parse (positionValues [0]), float.Parse (positionValues [1]), float.Parse (positionValues [2]));
								} catch {
										return "Could not parse. Command Format: Spawn Item <id> <x,y,z>";
								}
						}
            //spawn npc <npc_name>
            else if (temp [1].Equals ("NPC") || temp [1].Equals ("npc")) {
								string objectName = ReplaceCharWithSpaces (temp [2], '_');
								try {
										GameObject npc = MonoBehaviour.Instantiate (Resources.Load<GameObject> ("NPC/" + objectName)) as GameObject;
										npc.name = objectName;
										npc.transform.SetParent (global.npcParentInHierarchy);
								} catch {
										return "Could not spawn NPC '" + objectName + "'.";
								}
						}
				} else if (temp [0].Equals ("Open") || temp [0].Equals ("open")) {
						//Open Shop <npcName>
						if (temp [1].Equals ("Shop") || temp [1].Equals ("shop")) {
								try {
										string objectName = ReplaceCharWithSpaces (temp [2], '_');
										GameObject.Find (objectName).transform.FindChild ("Interaction").GetComponent<ShopScript> ().OpenShopScreen ();
								} catch {
										return "Could not parse. Command Format: Open Shop <npc_name>";
								}
						}
            //Open Dialogue <npcName>
            else if (temp [1].Equals ("Dialogue") || temp [1].Equals ("dialogue")) {
								if (global.uicanvas.currentDialogue != null)
										global.uicanvas.currentDialogue.ExitDialogue ();

								Dialogue d;
								string objectName = ReplaceCharWithSpaces (temp [2], '_');
								try {
										d = GameObject.Find (objectName).GetComponent<Dialogue> ();

										if (temp.Length > 3) {
												try {
														d.OpenDialogue (int.Parse (temp [3]));
														return "";
												} catch {
														string topicName = ReplaceCharWithSpaces (temp [3], '_');
														d.OpenDialogue (topicName);
														return "";
												}
										}
										d.OpenDialogue ();
								} catch {
										try {
												d = GameObject.Find (objectName).transform.FindChild ("Interaction").GetComponent<Dialogue> ();


												if (temp.Length > 3) {
														try {
																d.OpenDialogue (int.Parse (temp [3]));
																return "";
														} catch {
																string topicName = ReplaceCharWithSpaces (temp [3], '_');
																d.OpenDialogue (topicName);
																return "";
														}
												}

												d.OpenDialogue ();
										} catch {
												try {
														d = GameObject.Find (objectName).transform.FindChild ("interaction").GetComponent<Dialogue> ();
							
							
														if (temp.Length > 3) {
																try {
																		d.OpenDialogue (int.Parse (temp [3]));
																		return "";
																} catch {
																		string topicName = ReplaceCharWithSpaces (temp [3], '_');
																		d.OpenDialogue (topicName);
																		return "";
																}
														}
							
														d.OpenDialogue ();
												} catch {
														return "Invalid Input. Appropriate format: Open Dialogue <npc_name> <topic_name(Optional Parameter)>";
												}
										}
								}
						}
				}
		//changedisposition <npc_name> <int value>
		else if (temp [0].Equals ("ChangeDisposition") || temp [0].Equals ("changedisposition")) {
						string npcName = ReplaceCharWithSpaces (temp [1], '_');
						try {
								GameObject.Find (npcName).GetComponent<NPCScript> ().ChangeDisposition (int.Parse (temp [2]));
						} catch {
								return "Invalid Input. Appropriate format: changedisposition <npc_name> <int value>" + npcName;
						}
				}
		//destroy <object_name>
        else if (temp [0].Equals ("Destroy") || temp [0].Equals ("destroy")) {
						string objectName = ReplaceCharWithSpaces (temp [1], '_');
						try {
								GameObject theObj = GameObject.Find (objectName);
								if (theObj.GetComponent<NPCScript> () != null)
										theObj.GetComponent<NPCScript> ().Destroy ();
								else
										Destroy (theObj, float.Parse (temp [2]));
						} catch {
								try {
										GameObject theObj = GameObject.Find (objectName);
										if (theObj.GetComponent<NPCScript> () != null)
												theObj.GetComponent<NPCScript> ().Destroy ();
										else
												Destroy (theObj);
								} catch {
										return "Invalid Input. Appropriate format: Destroy <object_name> <delay(optional)>";
								}
						}
				} else if (temp [0].Equals ("Kill") || temp [0].Equals ("kill")) {
						string objectName = ReplaceCharWithSpaces (temp [1], '_');
						try {
								GameObject theObj = GameObject.Find (objectName);
								if (theObj.GetComponent<NPCScript> () != null)
										theObj.GetComponent<NPCScript> ().Die ();
								else
										Destroy (theObj, float.Parse (temp [2]));
						} catch {
								try {
										GameObject theObj = GameObject.Find (objectName);
										if (theObj.GetComponent<NPCScript> () != null)
												theObj.GetComponent<NPCScript> ().Die ();
										else
												Destroy (theObj);
								} catch {
										return "Invalid Input. Appropriate format: Destroy <object_name> <delay(optional)>";
								}
						}
				} else if (temp [0].Equals ("ExitInteraction") || temp [0].Equals ("exitinteraction")) {
						currInteraction = null;
						ChangeUIState ("");
						global.uicanvas.currentDialogue.ExitDialogue ();
				}
        //CutScene <bool looping> <(moveToLocation)_(lookAtTransform)_(time)> <(moveToLocation)_(lookAtTransform)_(time)> ...
        else if (temp [0].Equals ("CutScene") || temp [0].Equals ("cutscene")) {
						if (temp.Length >= 3) {
								List<CameraDirection> directions = new List<CameraDirection> ();

								//the following variables and for loop create all of the directions for the cutscene
								string[] currdirection;
								string[] moveToVect;
								for (int i = 2; i < temp.Length; i++) {
										try {
												currdirection = temp [i].Split ('_');
												moveToVect = currdirection [0].Split (',');
												directions.Add (new CameraDirection (new Vector3 (float.Parse (moveToVect [0]), float.Parse (moveToVect [1]), float.Parse (moveToVect [2])),
														GameObject.Find (currdirection [1]).transform,
														int.Parse (currdirection [2])));
										} catch {
												return "Could not parse camera direction. Correct Format: moveToLocation_lookAtTransform_time";
										}
								}

								//if we actually wound up with some directions, then we proceed
								if (directions.Count > 0) {
										try {
												Camera.main.GetComponent<PlayerCamera> ().SetCutScene (new CutScene (directions, bool.Parse (temp [1])));
										} catch {
												return "CutScene Loop boolean could not be parsed: " + temp [1];
										}
								}
						} else {
								return "Not enough parameters. Command Format: CutScene <bool looping> <(moveToLocation)_(lookAtTransform)_(time)>";
						}
				}
		//NickName <object_name> <nick_name>
		else if (temp [0].Equals ("NickName") || temp [0].Equals ("nickname")) {
						try {
								string objectName = ReplaceCharWithSpaces (temp [1], '_');
								string nickName = ReplaceCharWithSpaces (temp [2], '_');
								Dialogue target = GameObject.Find (objectName).GetComponent<Dialogue> ();
								if (target == null)
										return "The object '" + objectName + "' could not be found in the scene.";
								target.nicknameOfPerson = nickName;
						} catch {
								return "Could not Parse. Command Format: NickName <object_name> <nick_name>";
						}
				}
        //SetActive <object_name> <bool>
        else if (temp [0].Equals ("SetActive") || temp [0].Equals ("setactive")) {
						try {
								bool active = bool.Parse (temp [2]);
								string objectName = ReplaceCharWithSpaces (temp [1], '_');
								GameObject target = GameObject.Find (objectName);
								target.SetActive (active);
						} catch {
								try {
										bool active = bool.Parse (temp [2]);
										string objectName = ReplaceCharWithSpaces (temp [1], '_');
										NPCDatabase.GetNPCFromDB (objectName).transform.gameObject.SetActive (active);
								} catch {
										return "Tip: SetActive can only make NPCs active from inactive. Format: SetActive <object_name> <bool>";
								}
						}
				}
		//setnpc <npc_name> <npc_database_variable>
		else if (temp [0].Equals ("SetNPC") || temp [0].Equals ("setnpc")) {
						string npcname = ReplaceCharWithSpaces (temp [1], '_');
						string variable = ReplaceCharWithSpaces (temp [2], '_');
						NPCDatabase.SetNPCNameSpecial (variable, npcname);
				}
		//setcamstatic <x,y,z>		or setcamstatic <bool>
		else if (temp [0].Equals ("SetCamStatic") || temp [0].Equals ("setcamstatic")) {
						try {
								if (!bool.Parse (temp [1]))
										Camera.main.GetComponent<PlayerCamera> ().DisableStaticCamera ();
						} catch {
								try {
										string[] moveToVectStrings = temp [1].Split (',');
										Camera.main.GetComponent<PlayerCamera> ().EnableStaticCamera (new Vector3 (float.Parse (moveToVectStrings [0]), float.Parse (moveToVectStrings [1]), float.Parse (moveToVectStrings [2])));
								} catch {
								}
						}
				}
        //place <object_name> <x,y,z>
        else if (temp [0].Equals ("Place") || temp [0].Equals ("place")) {
						string objectName = ReplaceCharWithSpaces (temp [1], '_');
						GameObject objectToPlace = GameObject.Find (objectName);
						if (objectToPlace == null)
								return "Object not found. Command Format:  Place <object_name> <x,y,z>";
						try {
								if (objectToPlace.GetComponent<UnityEngine.AI.NavMeshAgent> () != null)
										objectToPlace.GetComponent<UnityEngine.AI.NavMeshAgent> ().enabled = false;
								string[] positionValues = temp [2].Split (',');
								objectToPlace.transform.rotation = Quaternion.Euler (new Vector3 (0, 0, 0));
								objectToPlace.transform.localPosition = new Vector3 (float.Parse (positionValues [0]), float.Parse (positionValues [1]), float.Parse (positionValues [2]));
								if (objectToPlace.GetComponent<UnityEngine.AI.NavMeshAgent> () != null)
										objectToPlace.GetComponent<UnityEngine.AI.NavMeshAgent> ().enabled = true;
						} catch {
								try {
										string targetObjName = ReplaceCharWithSpaces (temp [2], '_');
										GameObject target = GameObject.Find (targetObjName);
										objectToPlace.transform.position = target.transform.position;
								} catch {
										return "Could not Parse. Command Format: Place <object_name> <x,y,z> or Place <object_name> <target_object_name>";
								}
						}
				}
		// npc <npc_name> <methodName> <x,y,z> or npc <npc_name> <methodName> <target_obj_name>
		else if (temp [0].Equals ("NPC") || temp [0].Equals ("npc")) {
						string npcName = ReplaceCharWithSpaces (temp [1], '_');
						try {
								NPCScript npc = GameObject.Find (npcName).GetComponent<NPCScript> ();
								try {
										string[] positionValues = temp [3].Split (',');
										Vector3 pos = new Vector3 (float.Parse (positionValues [0]), float.Parse (positionValues [1]), float.Parse (positionValues [2]));
										npc.InsertMethod (new Action (temp [2], pos));
								} catch {
										npc.InsertMethod (new Action (temp [2], GameObject.Find (temp [3])));
								}
						} catch {
								return "Could not Parse. Command Format: npc <npc_name> <methodName> <x,y,z> or npc <npc_name> <methodName> <target_obj_name>";
						}
				}
        //fade destroy-obj spawn-item-qty writeline-message afterfade
        else if (temp [0].Equals ("Fade") || temp [0].Equals ("fade")) {
						if (temp.Length > 1) {
								List<string> triggersOnFade = new List<string> ();
								List<string> triggersAfterFade = new List<string> ();
								string currentTrigger;
								bool afterFade = false;
								for (int i = 1; i < temp.Length; i++) {
										currentTrigger = ReplaceCharWithSpaces (temp [i], '`');
										if (currentTrigger.Equals ("then"))
												afterFade = true;
										else if (!afterFade)
												triggersOnFade.Add (currentTrigger);
										else
												triggersAfterFade.Add (currentTrigger);
								}
								global.playerScript.FadeToBlack (triggersOnFade.ToArray (), triggersAfterFade.ToArray ());
						} else
								global.playerScript.FadeToBlack (null, null);
				}
        //writeline <message>
        else if (temp [0].Equals ("WriteLine") || temp [0].Equals ("writeline")) {
						string message = ReplaceCharWithSpaces (temp [1], '_');
						global.console.AddMessage (message);
				}
		//getid <item_name;suffix> or getid <quest_name>
		else if (temp [0].Equals ("GetID") || temp [0].Equals ("getid")) {
						string objectName = ReplaceCharWithSpaces (temp [1], '_');

						int ID = global.questDatabase.GetQuestID (objectName);
						global.console.AddMessage ("Quest Database: " + ID);
						ID = global.itemDatabase.GetItemID (objectName);
						global.console.AddMessage ("Item Database: " + ID);
				} else if (!name.Equals ("")) {
						return "Unrecognized command: " + name;
				}
				return "";
		}

		public static string ReplaceCharWithSpaces (string s, char c)
		{
				string newString = "";
				string[] partsOfName = s.Split (c);
				for (int i = 0; i < partsOfName.Length; i++) {
						if (i > 0)
								newString += " ";
						newString += partsOfName [i];
				}
				return newString;
		}

}