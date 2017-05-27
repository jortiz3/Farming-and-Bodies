using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class global
{
		public static Player playerScript;
		public static GameObject playerObject;
		//public static PlayerInventory playerInventory;
		public static ItemDatabase itemDatabase;
		public static QuestScript questDatabase;
		public static MenuScript uicanvas;
		public static Manager gameManager;
		public static Transform npcParentInHierarchy;
		public static Image screenFadeTexture;
		public static Console console;

		public static bool screenIsFading = false;

		public static bool musicEnabled = true;
		//public static bool soundEnabled;

		public static float musicVolume = 0.5f;
		//public static float soundVolume;

		public static string previousScene;

		public static bool initialized;

		public static void Initialize ()
		{
				playerObject = GameObject.FindGameObjectWithTag ("Player");
				playerScript = playerObject.GetComponent<Player> ();
				playerInventory = playerObject.GetComponent<InventoryScript> ();

				npcParentInHierarchy = GameObject.Find ("NPCs").transform;

				uicanvas = GameObject.FindGameObjectWithTag ("UICanvas").GetComponent<MenuScript> ();
				itemDatabase = GameObject.FindGameObjectWithTag ("Database").GetComponent<ItemDatabase> ();
				gameManager = itemDatabase.GetComponent<Manager> ();
				questDatabase = GameObject.FindGameObjectWithTag ("Database").GetComponent<QuestScript> ();
				screenFadeTexture = GameObject.FindGameObjectWithTag ("ScreenFadeImage").GetComponent<Image> ();
				screenFadeTexture.gameObject.SetActive (false);
				console = GameObject.Find ("Console").GetComponent<Console> ();

				if (Camera.main.transform.childCount > 0) {
						if (Camera.main.transform.GetChild (0).GetComponent<AudioSource> () != null) {
								Camera.main.transform.GetChild (0).GetComponent<AudioSource> ().enabled = musicEnabled;
								Camera.main.transform.GetChild (0).GetComponent<AudioSource> ().volume = musicVolume;
						}
				}

				previousScene = SceneManager.GetActiveScene().name;

				itemDatabase.Initialize ();

				initialized = true;
		}

		public static void GetMusicVolume ()
		{
				if (Camera.main.transform.childCount > 0) {
						if (Camera.main.transform.GetChild (0).GetComponent<AudioSource> () != null) {
								musicVolume = Camera.main.transform.GetChild (0).GetComponent<AudioSource> ().volume;
						}
				}
		}
}
