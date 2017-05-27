using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadScript : MonoBehaviour
{

		void Start ()
		{
				DontDestroyOnLoad (gameObject);
		}

		public void StartNewGame ()
		{
				StartCoroutine (NewGameRoutine ());
		}

		IEnumerator NewGameRoutine ()
		{
				Manager.LoadScreenOn = true;
				global.GetMusicVolume ();
				global.initialized = false;
				SceneManager.LoadScene ("Tutorial");
				yield return new WaitForSeconds (0.3f);
				global.gameManager.TriggerEvent ("Open Dialogue Intro");
				global.gameManager.TriggerEvent ("CutScene true -28,20,-130_Player_2 -35,20,-100_Player_2 -45,20,-130_Player_2");
				Destroy (gameObject);
		}

		public void StartLoadGame (int index)
		{
				StartCoroutine (LoadGameRoutine (index));
		}

		IEnumerator LoadGameRoutine (int index)
		{
				Manager.LoadScreenOn = true;
				if (SaveFileExists (index)) {
						global.GetMusicVolume ();
						SaveFile save = LoadGame (index);
						global.initialized = false;
						SceneManager.LoadScene (save.SceneName);
						yield return new WaitForSeconds (0.3f);
						save.Load ();
						global.uicanvas.ChangeState ("");
						Destroy (gameObject);
				} else {
						StartCoroutine (NewGameRoutine ());
				}
		}

		public void LoadScene (string sceneName, string[] eventsToFire)
		{
				StartCoroutine (LoadSceneRoutine (sceneName, eventsToFire));
		}

		IEnumerator LoadSceneRoutine (string sceneName, string[] eventsToFire)
		{
				Manager.LoadScreenOn = true;
				global.GetMusicVolume ();
				SceneManager.LoadScene (sceneName);
				yield return new WaitForSeconds (1.5f);
				for (int i = 0; eventsToFire != null && i < eventsToFire.Length; i++)
						global.gameManager.TriggerEvent (eventsToFire [i]);
				Destroy (gameObject, 1.5f);
		}

		public static void SaveGame (int fileIndex)
		{
				BinaryFormatter bf = new BinaryFormatter ();
				FileStream file = File.Create (Application.persistentDataPath + "/save" + fileIndex + ".dat");
				SaveFile save = new SaveFile ();
				bf.Serialize (file, save);
				file.Close ();
				global.console.AddMessage ("Save Successful");
		}

		public static SaveFile LoadGame (int fileIndex)
		{
				BinaryFormatter bf = new BinaryFormatter ();
				FileStream file;
		
				if (SaveFileExists (fileIndex)) {
						file = File.Open (Application.persistentDataPath + "/save" + fileIndex + ".dat", FileMode.Open);
			
						SaveFile data = (SaveFile)bf.Deserialize (file);
						file.Close ();

						return data;
				}
				return null;
		}

		public static bool SaveFileExists (int fileIndex)
		{
				if (File.Exists (Application.persistentDataPath + "/save" + fileIndex + ".dat"))
						return true;
				return false;
		}
}

[Serializable]
public class SaveFile
{
		private string scene;
		private SavablePlayer player;
		private SavableNPC[] npcs;
		private SavableDeadNPCs deadnpcs;
		private SavableSceneItem[] itemsInScene;

		public string SceneName { get { return scene; } }

		public SaveFile ()
		{
				scene = SceneManager.GetActiveScene().name;
				player = new SavablePlayer ();
				NPCDatabase.SetAllNPCsActive ();
				NPCScript[] npcsInScene = Transform.FindObjectsOfType (typeof(NPCScript)) as NPCScript[];
				npcs = new SavableNPC[npcsInScene.Length];
				for (int i = 0; i < npcs.Length; i++)
						npcs [i] = new SavableNPC (npcsInScene [i]);
				deadnpcs = new SavableDeadNPCs ();

				Transform itemParent = GameObject.FindGameObjectWithTag ("ItemParent").transform;
				itemsInScene = new SavableSceneItem[itemParent.childCount];
				for (int i = 0; i < itemsInScene.Length; i++) {
						itemsInScene [i] = new SavableSceneItem (itemParent.GetChild (i));
				}
		}

		public void Load ()
		{
				player.Load ();
				deadnpcs.Load ();
				for (int i = 0; i < npcs.Length; i++)
						npcs [i].Load ();

				Transform itemParent = GameObject.FindGameObjectWithTag ("ItemParent").transform;
				foreach (Transform t in itemParent)
						Transform.Destroy (t.gameObject);
				for (int i = 0; i < itemsInScene.Length; i++) {
						itemsInScene [i].Load ();
				}

		}
}

[Serializable]
public class SavableDeadNPCs
{
		private string[] names;

		public SavableDeadNPCs ()
		{
				names = NPCDatabase.GetDeadNames ();
		}

		public void Load ()
		{
				if (names == null)
						return;
				for (int i = 0; i < names.Length; i++) {
						GameObject deadNPC = GameObject.Find (names [i]);
						if (deadNPC != null) {
								if (deadNPC.GetComponent<NPCScript> () != null) {
										deadNPC.GetComponent<NPCScript> ().Die ();
								}
						}
				}
		}
}

[Serializable]
public class SavableNPC
{
		private string name;
		private int disposition;
		private int friendlyThreshold;
		private int unfriendlyThreshold;
		private int hostileThreshold;
		private int maxHealth;
		private int health;
		private bool hostile;
		private SavableVector3 position;
		private SavableDialogue dialogue;

		public SavableNPC (NPCScript npc)
		{
				this.name = npc.transform.parent.name;
				this.disposition = npc.disposition;
				this.friendlyThreshold = npc.friendlyThreshold;
				this.unfriendlyThreshold = npc.unfriendlyThreshold;
				this.hostileThreshold = npc.hostileThreshold;
				this.maxHealth = npc.maxHealth;
				this.health = npc.health;
				this.hostile = npc.hostile;
				position = new SavableVector3 (npc.transform.position);

				Transform interaction = npc.transform.FindChild ("Interaction");
				if (interaction == null)
						interaction = npc.transform.FindChild ("interaction");
				if (interaction != null) {
						Dialogue d = interaction.GetComponent<Dialogue> ();
						if (d != null)
								dialogue = new SavableDialogue (d);
				}
		}

		public void Load ()
		{
				NPCScript npc;
				GameObject obj = GameObject.Find (name);
				if (obj == null) {
						global.gameManager.TriggerEvent ("spawn npc " + name.Replace (' ', '_'));
						obj = GameObject.Find (name);
				}
				if (obj == null) {
						Debug.Log ("Unable to load info for (NPC)" + name + ".");
						return;
				}

				npc = obj.GetComponent<NPCScript> ();

				if (npc != null) {
						npc.disposition = disposition;
						npc.friendlyThreshold = friendlyThreshold;
						npc.unfriendlyThreshold = unfriendlyThreshold;
						npc.hostileThreshold = hostileThreshold;
						npc.maxHealth = maxHealth;
						npc.health = health;
						npc.hostile = hostile;
						npc.transform.position = position.toVector3 ();
						if (dialogue != null)
								dialogue.Load (npc);
				}
		}
}

[Serializable]
public class SavableDialogue
{
		private Greeting greeting;
		private Topic[] topics;

		public SavableDialogue (Dialogue d)
		{
				greeting = d.greeting;
				topics = d.topics;
		}

		public void Load (NPCScript npc)
		{
				Dialogue d = null;
				Transform interaction = npc.transform.FindChild ("Interaction");
				if (interaction == null)
						interaction = npc.transform.FindChild ("interaction");
				if (interaction != null) {
						d = interaction.GetComponent<Dialogue> ();
				}
				if (d != null) {
						d.greeting = greeting;
						d.topics = topics;
				}
		}
}

[Serializable]
class SavablePlayer
{
		private string name;
		private Skill[] skills;
		private List<Quest> quests;
		private bool onePersonHasBeenKilled;
		private SavableVector3 position;
		private SavableInventory inventory;

		public SavablePlayer ()
		{
				name = global.playerScript.PlayerName;
				skills = global.playerScript.Skills;
				quests = global.playerScript.questList;
				onePersonHasBeenKilled = Player.atleastOnePersonKilled;
				position = new SavableVector3 (global.playerScript.transform.position);
				inventory = new SavableInventory (global.playerInventory.GetItems());
		}

		public void Load ()
		{
				if (global.playerScript == null) {
						Debug.Log ("'global.playerScript' is null. Unable to load info.");
						return;
				}
				global.playerScript.PlayerName = name;
				global.playerScript.Skills = skills;
				global.playerScript.questList = quests;
				Player.atleastOnePersonKilled = onePersonHasBeenKilled;
				global.playerScript.transform.position = position.toVector3 ();
				inventory.Load ();

				QuestScript.RefreshQuestUI ();
		}
}

[Serializable]
public class SavableInventory
{
		private Item[] items;
		private int money;

		public SavableInventory (Item[] inventory)
		{
				items = new Item[inventory.Length];
				for (int i = 0; i < items.Length; i++)
						items [i] = inventory [i];
				money = global.playerInventory.storedCurrency;
		}

		public void Load ()
		{
				if (global.playerInventory == null) {
						Debug.Log ("'global.playerInventory' is null. Unable to load info.");
						return;
				}

				global.playerInventory.LoadItems(items);
				global.playerInventory.storedCurrency = money;

				global.playerInventory.UpdateContainerUI ();
		}
}

[Serializable]
public class SavableItem
{
		private int id;
		private int qty;

		public SavableItem (Item i)
		{
				id = i.DatabaseID;
				qty = i.quantity;
		}

		public Item Get ()
		{
				Item temp = global.itemDatabase.GetItem (id);
				temp.quantity = qty;
				return temp;
		}
}

[Serializable]
public class SavableSceneItem
{
		private int id;
		private SavableVector3 position;

		public SavableSceneItem (Transform itemTransform)
		{
				id = global.itemDatabase.GetItemID (itemTransform.name);
				position = new SavableVector3 (itemTransform.position);
		}

		public void Load ()
		{
				if (id < 0)
						return;
				GameObject item = global.itemDatabase.GetItem (id).Instantiate ();
				item.transform.position = position.toVector3 ();
		}
}

[Serializable]
public class SavableVector3
{
		private float x;
		private float y;
		private float z;

		public SavableVector3 (Vector3 v)
		{
				x = v.x;
				y = v.y;
				z = v.z;
		}

		public Vector3 toVector3 ()
		{
				return new Vector3 (x, y, z);
		}
}