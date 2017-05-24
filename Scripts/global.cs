using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public static class global
{
	public static Player playerScript;
	public static GameObject playerObject;
	public static InventoryScript playerInventory;
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

	public static float musicVolume = 1f;
	//public static float soundVolume;

	public static string previousScene;

	public static bool initialized;

	public static void Initialize()
	{
		playerObject = GameObject.FindGameObjectWithTag("Player");
		playerScript = playerObject.GetComponent<Player>();
		playerInventory = playerObject.GetComponent<InventoryScript>();

		npcParentInHierarchy = GameObject.Find ("NPCs").transform;

		uicanvas = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<MenuScript>();
		itemDatabase = GameObject.FindGameObjectWithTag("Database").GetComponent<ItemDatabase>();
		gameManager = itemDatabase.GetComponent<Manager>();
		questDatabase = GameObject.FindGameObjectWithTag("Database").GetComponent<QuestScript>();
		screenFadeTexture = GameObject.FindGameObjectWithTag("ScreenFadeImage").GetComponent<Image>();
		screenFadeTexture.gameObject.SetActive(false);
		console = GameObject.Find("Console").GetComponent<Console>();

		if (Camera.main.transform.childCount > 0)
		{
			if (Camera.main.transform.GetChild(0).audio != null)
			{
				Camera.main.transform.GetChild(0).audio.enabled = musicEnabled;
				Camera.main.transform.GetChild(0).audio.volume = musicVolume;
			}
		}

		previousScene = Application.loadedLevelName;

		itemDatabase.Initialize();

		initialized = true;
	}

	public static void GetMusicVolume()
	{
		if (Camera.main.transform.childCount > 0)
		{
			if (Camera.main.transform.GetChild(0).audio != null)
			{
				musicVolume = Camera.main.transform.GetChild(0).audio.volume;
			}
		}
	}
}
