using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public string PlayerName = "Zed";

	private CharacterController controller;

	[HideInInspector]
	public Skill[] Skills;
	
	public List<Quest> questList = new List<Quest>();

	public static bool atleastOnePersonKilled;

	private Animator anim;

	//The max speed when walking
	public float walkSpeed = 2.0f;
	//The max speed when running
	public float runSpeed = 12.0f;
	//The current movement speed
	private float moveSpeed = 0.0f;
	//The speed at which the character turns
	public float rotateSpeed = 50.0f;

	private float currVelocityY;

	public float walkMaxAnimationSpeed = 0.75f;
	public float runMaxAnimationSpeed = 1.0f;

	private Vector3 moveDirection = Vector3.zero;

	private bool jumpBool;
	
	void Start ()
	{
		if (global.previousScene != null)
		{
			GameObject temp = GameObject.FindGameObjectWithTag("Player");
			if (temp != null)
			{
				global.Initialize();
				if (global.previousScene.Equals("Tutorial"))
					temp.GetComponent<Player>().TutorialReset();
				Camera.main.GetComponent<PlayerCamera>().player = temp.transform;
				Camera.main.GetComponent<PlayerCamera>().RevertTarget();
				temp.transform.position = transform.position;
				temp.transform.rotation = transform.rotation;
				Destroy(gameObject);
				return;
			}
		}

		gameObject.name = "Player";
		gameObject.tag = "Player";

		global.Initialize();

		Skills = new Skill[]{ new Skill("Farming", 10), new Skill("Speech", 10) };

		atleastOnePersonKilled = false;

		UpdateCharacterInfo();

		moveDirection = transform.TransformDirection(Vector3.forward);
        anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController>();

		DontDestroyOnLoad (gameObject);
	}

	void Update()
	{
		if (Console.devConsole.activeSelf || global.uicanvas.currentState.Equals ("Dialogue") || global.uicanvas.currentState.Equals ("Tutorial") ||
		    global.uicanvas.currentState.Equals ("Quest Log") || global.uicanvas.currentState.Equals ("Pause") || 
		    global.uicanvas.currentState.Equals ("Character Info") || global.uicanvas.currentState.Equals ("Shop") || global.screenIsFading)
		{
            anim.SetBool("IsAttack", false);
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsRun", false);
            anim.SetBool("IsJump", false);
			anim.SetTrigger("IsIdle");
			return;
		}

		AnimatorStateInfo currAnimState = anim.GetCurrentAnimatorStateInfo (0);

        if (Input.GetAxisRaw("Sprint") != 0 && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
        {
			if (!currAnimState.IsName("Walk"))
				anim.SetTrigger("IsWalk");
        }
        else if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
			if (!currAnimState.IsName("Run"))
				anim.SetTrigger("IsRun");
        }
		else {
			if (!currAnimState.IsName("Idle"))
				anim.SetTrigger("IsIdle");
		}

		if (Input.GetButtonDown("Jump") && jumpBool == false)
		{
			anim.SetTrigger("IsJump");
			currVelocityY = 0.5f;
			jumpBool = true;
		}

		#region movement
		Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward = forward.normalized;
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		
		float upDown = Input.GetAxisRaw("Vertical");
		float leftRight = Input.GetAxisRaw("Horizontal");
		
		// Target direction relative to the camera
		Vector3 groundDirection= leftRight * right + upDown * forward;

		if (groundDirection != Vector3.zero)
		{
			float groundSpeed= Mathf.Min(groundDirection.magnitude, 1.0f);
			if(currAnimState.IsName("Run"))
				groundSpeed *= runSpeed;
			else if(currAnimState.IsName("Walk"))
				groundSpeed *= walkSpeed;
			else if (currAnimState.IsName("Idle"))
			{
				groundDirection = Vector3.zero;
				groundSpeed = 0;
			}

			moveSpeed = Mathf.Lerp(moveSpeed, groundSpeed, 2 * Time.deltaTime);
			moveDirection = Vector3.RotateTowards(moveDirection, groundDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);										
			moveDirection = moveDirection.normalized;
		}
		else if (global.uicanvas.currentState.Equals("Dialogue"))
		{
			Vector3 lookPos = global.uicanvas.currentDialogue.transform.position - transform.position;
			lookPos.y = 0;
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5);
		}
		else
		{
			moveSpeed = 0;
			moveDirection = Vector3.zero;
		}

		// Calculate actual motion
		Vector3 movement = moveDirection * moveSpeed;
		movement *= Time.deltaTime;
		if (!controller.isGrounded) {
			currVelocityY -= 2f * Time.deltaTime;
			if (currVelocityY < -20f)
				currVelocityY = -20f;
			movement.y += currVelocityY;
		} else {
			jumpBool = false;
		}
		// Move the controller
		controller.Move(movement);
		
		if(moveDirection != Vector3.zero)
			transform.rotation = Quaternion.LookRotation(moveDirection);
		#endregion

		//this keeps the player from falling infinitely if they manage to fall off the map
		if(transform.position.y < -1500)
		{
			transform.position = Vector3.up * 2;
		}
	}

	public void TutorialReset()
	{
		Skills = new Skill[]{ new Skill("Farming", 10), new Skill("Speech", 10) };

		//gameObject.GetComponent<InventoryScript> ().Initialize ();
	}

	private void UpdateCharacterInfo()
	{
		Transform temp = global.uicanvas.transform.FindChild("Character Info");
		Transform currSkillTransform;
		for (int s = 0; s < Skills.Length; s++) {
			currSkillTransform = temp.Find("Skill " + s);
			if (!Skills[s].IsAtMaxLevel) {
				currSkillTransform.GetComponent<Text>().text = Skills[s].Name + "\tLv: " + Skills[s].CurrentLevel + "\nExp: " + Skills[s].CurrentExperience + "/" + Skills[s].RequiredExperience;
				currSkillTransform.GetChild(0).GetComponent<Slider>().value = Skills[s].SliderValue;
			} else {
				currSkillTransform.GetComponent<Text>().text = Skills[s].Name + "\tLv: " + Skills[s].CurrentLevel + "    <color=yellow><b>.:!MAX!:.</b></color>";
				currSkillTransform.GetChild(0).GetComponent<Slider>().value = 1;
			}
		}
	}

	public void AddExp(int skill, float amount)
	{
		if (amount <= 0 || skill < 0 || skill >= Skills.Length)
			return;
		//this keeps the value within 2 decimal places
		if (amount % 1 != 0) {
			int newAmount = (int)(amount * 100);
			amount = newAmount / 100.0f;
		}
		Skills [skill].AddExp (amount);
		global.console.AddMessage(amount + " " + Skills[skill].Name + " Experience Gained");
		UpdateCharacterInfo();
	}

	public void AddQuest(int questID)
	{
		if (questID < 0 || questID >= global.questDatabase.questList.Length)
			return;
		Quest temp = new Quest();
		temp.CopyQuest(global.questDatabase.questList[questID]);
		questList.Add(temp);
		QuestScript.AddQuest(temp);
	}

	public void AddQuest(string name)
	{
		Quest temp = QuestScript.GetQuest (global.questDatabase.questList, name);
		if (temp != null)
		{
			questList.Add (temp);
			QuestScript.AddQuest(temp);
		}
	}

	public void SetQuestPosition(int QuestID, int NewPosition)
	{
		if (QuestID < 0)
			return;
		
		for (int i = 0; i < questList.Count; i++)
		{
			if (questList[i].id == QuestID)
			{
				questList[i].JumpToNextObjective(NewPosition, "");
				QuestScript.RefreshQuestUI();
				return;
			}
		}
	}

	public void SetQuestPosition(string QuestName, int NewPosition)
	{
		SetQuestPosition (global.questDatabase.GetQuestID (QuestName), NewPosition);
	}

	public void AdvanceQuest(int QuestID)
	{
		if (QuestID < 0)
			return;
		
		for (int i = 0; i < questList.Count; i++)
		{
			if (questList[i].id == QuestID)
			{
				questList[i].AdvanceToNextObjective("");
				QuestScript.RefreshQuestUI();
				return;
			}
		}
	}

	public void AdvanceQuest(string QuestName)
	{
		AdvanceQuest (global.questDatabase.GetQuestID(QuestName));
	}

	public bool HasQuest(string questName)
	{
		for (int i = 0; i < questList.Count; i++)
		{
			if (questList[i].name.Equals(questName))
				return true;
		}
		return false;
	}

	public int GetQuestPosition(string questName)
	{
		for (int i = 0; i < questList.Count; i++)
		{
			if (questList[i].name.Equals(questName))
				return questList[i].position;
		}
		return -1;
	}

	public bool GetQuestCompleted(string questName)
	{
		for (int i = 0; i < questList.Count; i++)
		{
			if (questList[i].name.Equals(questName))
				return questList[i].completed;
		}
		return false;
	}

	public string GetQuestBranch(string questName)
	{
		for (int i = 0; i < questList.Count; i++)
		{
			if (questList[i].name.Equals(questName))
				return questList[i].currentBranch;
		}
		return "Not Found";
	}

	public void Sleep()
	{
		global.gameManager.BroadcastActionCompleted("Sleep");
		if (Property.KillNextProperty())
		{
			global.gameManager.BroadcastActionCompleted("NPC Killed");
			if (!atleastOnePersonKilled)
			{
				FadeToBlack (new string[] {"Destroy Daughter_Container", "spawn npc Sick_Daughter_Container"},
				new string[] {"open dialogue First_Death", "cutscene true 34,-503,-43_Player_1 34,-503,-35_Player_1 42,-503,-35_Player_1 42,-503,-43_Player_1"});
				atleastOnePersonKilled = true;
			}
			else
				FadeToBlack (new string[] {"writeline You_awake_the_next_day"}, null);
		}
		else
		{
			FadeToBlack (new string[] {"writeline You_awake_the_next_day"}, null);
		}
	}

	public void FadeToBlack(string[] eventsToTriggerOnFade, string[] eventsToTriggerAfterFade)
	{
		StartCoroutine(FadeScreen(eventsToTriggerOnFade, eventsToTriggerAfterFade));
	}

	IEnumerator FadeScreen(string[] eventsToTriggerOnFade, string[] eventsToTriggerAfterFade)
	{
		global.screenIsFading = true;
		global.screenFadeTexture.gameObject.SetActive(true);
		//fades to black
		while (global.screenFadeTexture.color.a < 0.95f)
		{
			global.screenFadeTexture.color = Color.Lerp(global.screenFadeTexture.color, Color.black, 2f * Time.deltaTime);
			yield return null;
		}
		global.screenFadeTexture.color = Color.black;

		yield return new WaitForSeconds (0.5f);
		if (eventsToTriggerOnFade != null)
		{
			try {
				for (int i = 0; i < eventsToTriggerOnFade.Length; i++)
					global.gameManager.TriggerEvent(eventsToTriggerOnFade[i]);
			} catch {}
		}
		yield return new WaitForSeconds (2f);

		while (global.screenFadeTexture.color.a > 0.05f)
		{
			global.screenFadeTexture.color = Color.Lerp(global.screenFadeTexture.color, Color.clear, 2f * Time.deltaTime);
			yield return null;
		}
		global.screenFadeTexture.color = Color.clear;
		global.screenFadeTexture.gameObject.SetActive(false);
		global.screenIsFading = false;

		if (eventsToTriggerAfterFade != null)
		{
			try {
				for (int i = 0; i < eventsToTriggerAfterFade.Length; i++)
					global.gameManager.TriggerEvent(eventsToTriggerAfterFade[i]);
			} catch {}
		}
	}
}