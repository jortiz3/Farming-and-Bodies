using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Reflection;

public class NPCScript : MonoBehaviour 
{
	#region Variables
	//Animation stuff
	private Animator mechAnimation;
	public enum CharacterState 
	{
		Idle = 0,
		Walking = 1,
		Running = 2,
		Attacking = 3,
		Speaking = 4,
		Sleeping = 5,
	}
	[HideInInspector]
	public CharacterState _characterState;

	//Movement
	NavMesh nMesh;
	private NavMeshAgent nav;
	private float pauseTime;
	public float maxPauseTime;

	//Pointer to affect the state changes of the NPC
	[HideInInspector]
	public List<Action> taskList;
	public List<Action> routine;

	//this is used to determine if the npc hates the player or not
	[Range(-100, 100)]
	public int disposition;
	[Range(-100, 100)]
	public int friendlyThreshold = 50;
	[Range(-100, 100)]
	public int unfriendlyThreshold = -50;
	[Range(-100, 100)]
	public int hostileThreshold = -95;

	//the ragdoll for when the npc dies
	public GameObject ragdoll;

	//For Sound
	public AudioClip[] audioClip;
	public string[] cropsToPlant;

	//combat stuff
	[HideInInspector]
	public bool hostile;
	public static bool inCombat;

	public int maxHealth = 50;
	[HideInInspector]
	public int health;
	private EnemyFOV enemyFOV;
	
	private GameObject miniMapIcon;
	private GameObject recruitTarget;
	private bool recruited = false;
	#endregion
	void Start () {
		nav = GetComponent<NavMeshAgent>();

		gameObject.AddComponent("EnemyFOV");
		enemyFOV = GetComponent<EnemyFOV>();

		pauseTime = 0.0f;

		mechAnimation = GetComponent<Animator>();

		//sets the health to be full when the object spawns
		health = maxHealth;

		if (transform.parent.childCount > 1) {
			Transform temp = transform.parent.FindChild("Icon");
			if (temp != null)
				miniMapIcon = temp.gameObject;
		}

        NPCDatabase.RegisterNPC(this);

		//Adds methods to the list of actions
		taskList = new List<Action>();
		taskList.AddRange (routine);
	}

	void Update ()
	{
		if (transform.FindChild("Interaction") != null && transform.FindChild("Interaction").GetComponent<Dialogue>() != null
		    && transform.FindChild("Interaction").GetComponent<Dialogue>().isInteracting)
		{
			Vector3 lookPos = global.playerObject.transform.position - transform.position;
			lookPos.y = 0;
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5);
			if (nav != null)
				nav.Stop();
			_characterState = CharacterState.Speaking;
		}
		else if(taskList != null)
		{
			if (taskList.Count > 0 && taskList[0].GetFunction(this)(taskList[0].GetTarget()))
				taskList.RemoveAt(0);
			else
				taskList.AddRange(routine);
		}

		//This displays the NPC going to the last known position for the player
		/*if(enemyFOV.JustSawPlayer() && hostile)
		{
			InsertMethod(new Action("Walk", global.playerObject));
		}*/

		if (miniMapIcon != null)
			miniMapIcon.transform.position = transform.position + (Vector3.up * 50);

		UpdateState();
	}

	void PlaySound(int clip)
	{
		if(audioClip != null)
		{
			audio.clip = audioClip[clip];
			audio.Play();
		}
	}

	void UpdateState()
	{
		if(_characterState == CharacterState.Idle)
		{
			mechAnimation.SetBool ("IsIdle", true);
			mechAnimation.SetBool ("IsAttack", false);
			mechAnimation.SetBool ("IsWalk", false);
			mechAnimation.SetBool ("IsRun", false);
			mechAnimation.SetBool ("IsSpeak", false);
			mechAnimation.SetBool ("IsSleeping", false);
			mechAnimation.SetBool ("IsJump", false);
			if (nav != null)
				nav.speed = 0;
		}
		else if(_characterState == CharacterState.Walking)
		{
			mechAnimation.SetBool ("IsAttack", false);
			mechAnimation.SetBool ("IsWalk", true);
			mechAnimation.SetBool ("IsIdle", false);
			mechAnimation.SetBool ("IsRun", false);
			mechAnimation.SetBool ("IsSpeak", false);
			mechAnimation.SetBool ("IsSleeping", false);
			mechAnimation.SetBool ("IsJump", false);
			if (nav != null)
				nav.speed = 1;
		}
		else if(_characterState == CharacterState.Running)
		{
			mechAnimation.SetBool ("IsAttack", false);
			mechAnimation.SetBool ("IsWalk", false);
			mechAnimation.SetBool ("IsIdle", false);
			mechAnimation.SetBool ("IsRun", true);
			mechAnimation.SetBool ("IsSpeak", false);
			mechAnimation.SetBool ("IsSleeping", false);
			mechAnimation.SetBool ("IsJump", false);
			if (nav != null)
				nav.speed = 2;
		}
		else if(_characterState == CharacterState.Attacking)
		{
			mechAnimation.SetBool ("IsAttack", true);
			mechAnimation.SetBool ("IsWalk", false);
			mechAnimation.SetBool ("IsIdle", false);
			mechAnimation.SetBool ("IsRun", false);
			mechAnimation.SetBool ("IsSpeak", false);
			mechAnimation.SetBool ("IsSleeping", false);
			mechAnimation.SetBool ("IsJump", false);
			if (nav != null)
				nav.speed = 0;
		}
		else if(_characterState == CharacterState.Speaking)
		{
			mechAnimation.SetBool ("IsAttack", false);
			mechAnimation.SetBool ("IsWalk", false);
			mechAnimation.SetBool ("IsIdle", false);
			mechAnimation.SetBool ("IsRun", false);
			mechAnimation.SetBool ("IsSpeak", true);
			mechAnimation.SetBool ("IsSleeping", false);
			mechAnimation.SetBool ("IsJump", false);
			if (nav != null)
				nav.speed = 0;
		}
		else if(_characterState == CharacterState.Sleeping)
		{
			mechAnimation.SetBool ("IsAttack", false);
			mechAnimation.SetBool ("IsWalk", false);
			mechAnimation.SetBool ("IsIdle", false);
			mechAnimation.SetBool ("IsRun", false);
			mechAnimation.SetBool ("IsSpeak", false);
			mechAnimation.SetBool ("IsSleeping", true);
			mechAnimation.SetBool ("IsJump", false);
			if (nav != null)
				nav.speed = 0;
		}
	}

    public void Die()
    {
        Vector3 rdollpos = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
        GameObject body = Instantiate(ragdoll, rdollpos, transform.rotation) as GameObject;
        body.name = gameObject.name + "'s body";
        QuestScript.CheckforRequiredNPC(global.playerScript.questList, gameObject.name);
		NPCDatabase.RegisterDeadNPC (this);
		NPCDatabase.SetNPCNameSpecial("dead", gameObject.name);
        global.gameManager.RemoveInteraction(transform.FindChild("Interaction").gameObject);
		global.gameManager.BroadcastActionCompleted ("NPC Death");
		if (transform.parent != null && transform.parent.name.Contains("Container"))
			Destroy(transform.parent.gameObject);
		else
        	Destroy(gameObject);
    }

    public void Destroy()
    {
        global.gameManager.RemoveInteraction(transform.FindChild("Interaction").gameObject);
        Destroy(transform.parent.gameObject);
    }

	public bool Wait(Vector3 t)
	{
		_characterState = CharacterState.Idle;

		nav.Stop ();

		//lookat the target
		if (!transform.position.Equals(t))
		{
			Vector3 lookPos = t - transform.position;
			lookPos.y = 0;
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5);
		}

		pauseTime += Time.deltaTime;

		if(pauseTime > maxPauseTime)
		{
			pauseTime = 0.0f;
			return true;
		}
		else
			return false; 
	}

	public bool Walk(Vector3 t)
	{
		_characterState = CharacterState.Walking;

		nav.SetDestination(t);

		if(nav.remainingDistance < 1.5f)
		{
			nav.Stop();
			return true;
		}
		else 
			return false;
	}

	public bool Run(Vector3 t)
	{
		_characterState = CharacterState.Running;

		nav.SetDestination(t);

		if(nav.remainingDistance < 2.5f)
		{
			nav.Stop();
			return true;
		}
		else
			return false;
	}

	public bool Follow(Vector3 t)
	{
		nav.SetDestination(t);
		
		if (nav.remainingDistance > 6)
		{
			return Run (t);
		}
		else if (nav.remainingDistance > 3.5f)
		{
			return Walk(t);
		}
		else
		{
			return Wait(t);
		}
	}

	public bool Attack(Vector3 t)
	{
		Vector3 lookPos = t - transform.position;
		lookPos.y = 0;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5);
		
		t = global.playerObject.transform.position;
		_characterState = CharacterState.Attacking;

		if(mechAnimation.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
		{
			return true;
		}
		return false;
	}

	public bool Confront(Vector3 t)
	{
		if(!hostile)
			_characterState = CharacterState.Walking;
		else
			_characterState = CharacterState.Running;

		nav.SetDestination(t);

		if(Vector3.Distance(transform.position, t) < 3f)
		{
			nav.Stop();
			global.gameManager.TriggerEvent("Open Dialogue " + gameObject.name + " Confrontation");
			return true;
		}
		else 
			return false;
	}

	public bool Flank(Vector3 t)
	{
		Vector3 direction = t - transform.position;
		Vector3 side = Quaternion.AngleAxis(90, Vector3.up) * direction;
		NavMeshHit hit;

		//Keep the NPC from trying to go to a point outside of fence
		side.x = Mathf.Clamp(transform.position.x, -121f, 121f);
		direction.z = Mathf.Clamp(transform.position.z, -115f, 126f);

		if(UnityEngine.Random.Range(0, 9) > 4)
			side.x *= -1;

		//Detects if the point is located on the NavMesh
		bool temp = NavMesh.SamplePosition(side, out hit, .5f, 1);
		Debug.Log (temp);

		Debug.Log ("Side: " + side);
		Debug.Log ("Rear: " + direction);

		if(temp == true && side.x <= 121f && side.x >= -121f && direction.z <= 126f && direction.z >= -115f)
		{
			AddMethod(new Action("Run", side));
			AddMethod(new Action("Run", direction));
			AddMethod(new Action("Run", global.playerObject));
			AddMethod(new Action("Attack", global.playerObject));	
		}
		else
		{
			side.z -= 10f;
			AddMethod (new Action("Run", side));
			AddMethod(new Action("Run", direction));
			AddMethod(new Action("Run", global.playerObject));
			AddMethod(new Action("Attack", global.playerObject));
		}
		return true;
	}

	public bool Recruit(Vector3 t)
	{
		if(recruitTarget == null)
			recruitTarget = GetClosestNPC();

		if(Vector3.Distance(transform.position, recruitTarget.transform.position) > 6f && 
		   recruitTarget.GetComponent<NPCScript>().recruited == false)
		{
			AddMethod(new Action("Walk", recruitTarget));
		}

		if(Vector3.Distance(transform.position, recruitTarget.transform.position) < 6f && 
		   recruitTarget.GetComponent<NPCScript>().recruited == false)
		{
			recruitTarget.GetComponent<NPCScript>().ClearTaskList();
			recruitTarget.GetComponent<NPCScript>().InsertMethod(new Action("BeRecruited", transform.position));
			recruitTarget.GetComponent<NPCScript>().recruited = true;
			recruitTarget = null;
		}
		return true;
	}

	public bool BeRecruited(Vector3 t)
	{
		hostile = true;
		AddMethod(new Action("Walk", global.playerObject));
		AddMethod(new Action("Attack", global.playerObject));
		return true;
	}

	public bool Battle(Vector3 t)
	{			
		if(Vector3.Distance(transform.position, global.playerObject.transform.position) < 3f)
		{
			AddMethod(new Action("Attack", global.playerObject));
		}
		if(Vector3.Distance(transform.position, GetClosestNPC().transform.position) < 15f)
		{
			AddMethod(new Action("Recruit", new Vector3(0,0,0)));
		}
		else
		{
			AddMethod(new Action("Flank", global.playerObject));
		}
		return true;
	}

	public bool Sleep(Vector3 t)
	{	
		_characterState = CharacterState.Sleeping;
		return true;
	}

	public void ChangeDisposition(int value)
	{
		disposition += value;
		
		//keeps the disposition in the bounds
		if (disposition > 100)
			disposition = 100;
		else if (disposition < -100)
			disposition = -100;
		
		//sets the npc as hostile if they exceed the disposition threshold
		if (disposition < hostileThreshold)
			hostile = true;
		else
			hostile = false;
		
		string message;
		if (value > 49)
			message = gameObject.name + " LOVES what you just did";
		else if (value > 19)
			message = gameObject.name + " greatly approves";
		else if (value > 9)
			message = gameObject.name + " approves";
		else if (value > 0)
			message = gameObject.name + " slightly approves";
		else if (value > -10)
			message = gameObject.name + " slightly disapproves";
		else if (value > -20)
			message = gameObject.name + " disapproves";
		else if (value > -50)
			message = gameObject.name + " greatly disapproves";
		else
			message = gameObject.name + " HATES you for what you just did";
		
		global.console.AddMessage(message);
	}
	
	public bool isFriendly()
	{
		return disposition >= friendlyThreshold ? true : false;
	}
	public bool isUnfriendly()
	{
		return disposition < unfriendlyThreshold ? true : false;
	}

	public bool CurrentlySeesPlayer()
	{
		return enemyFOV.CanSeePlayer();
	}

	public void InsertMethod(Action insertAction)
	{
		taskList.Insert(0, insertAction);
	}

	public void AddMethod(Action addAction)
	{
		taskList.Add (addAction);
	}

	private void ClearTaskList()
	{
		taskList.Clear();
	}

	GameObject GetClosestNPC()
	{
		GameObject[] taggedNPC;
		taggedNPC = GameObject.FindGameObjectsWithTag("NPC");
		GameObject closestNPC = new GameObject();
		float distance = Mathf.Infinity;

		foreach(GameObject g in taggedNPC)
		{
			Vector3 difference = g.transform.position - transform.position;
			float currDist = difference.sqrMagnitude;
			if(currDist < distance && transform.name != g.name)
			{
				closestNPC = g;
				distance = currDist;
			}
		}
		return closestNPC;
	 }
}

[Serializable]
public class Action
{
	//only one target needs to be set, 
	//but having a vector3 target allows us to store a point without needing a gameobject in the scene
	public string methodName;
	public GameObject targetObj;
	public Vector3 targetVect;
	public delegate bool method(Vector3 t);
	public method function;

	//this constructor allows us to create an action using a vector3
	public Action(method Method,Vector3 Target)
	{
		targetVect = Target;
		function = Method;
	}

	//this is the constructor that we made together
	public Action(method Method, GameObject Target)
	{
		targetObj = Target;
		function = Method;
	}

	//The following two constructors allow us to add stuff through scripts
	public Action(string MethodName,Vector3 Target)
	{
		targetVect = Target;
		methodName = MethodName;
	}
	
	public Action(string MethodName, GameObject Target)
	{
		targetObj = Target;
		methodName = MethodName;
	}

	public method GetFunction(NPCScript npc)
	{
		if(function != null)
			return function;
		MethodInfo temp = typeof(NPCScript).GetMethod(methodName, new Type[] { Type.GetType("UnityEngine.Vector3, UnityEngine", true) });
		function = (method)Delegate.CreateDelegate(typeof (method), npc, temp);
		return function;
	}

	//gets the appropriate target -- the gameobject is used whenever one is provided
	public Vector3 GetTarget()
	{
		if (targetObj != null)
			return targetObj.transform.position;
		return targetVect;
	}
}
