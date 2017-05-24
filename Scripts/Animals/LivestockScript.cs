using UnityEngine;
using System.Collections;

public class LivestockScript : MonoBehaviour
{
	//Animations
	public AnimationClip idleAnimation;
	//public AnimationClip walkAnimation;
	public float AnimationSpeed = 3.0f;
	private Animation anim;

	public bool hostile;
	//public Waypoints waypoint;

	//Movement
	public float moveSpeed = 1.0f;
	private Vector3 moveDirection;
	private Vector3 velocity;
	
	//for pause event
	public float pauseTime;
	private bool pauseFlag;
	//private bool playerInRange;
	//private bool isGrounded;
	
	// Use this for initialization
	void Start ()
	{
		//playerInRange = false;
		
		//For Moving between Waypoints
		//transform.position = waypoint.StartPosition ();
		//moveDirection = Vector3.zero;
		
//		pauseTime = 0.0f;
//		pauseFlag = false;
//		isGrounded = false;
		
		anim = GetComponent<Animation> ();
		Update ();
	}
	
	void Update ()
	{
//		moveDirection = waypoint.GetDirection (transform);
//		velocity = moveDirection * moveSpeed * Time.deltaTime;
		
//		if (!isGrounded)
//			velocity.y = -0.1f;
//		else
//			velocity.y = 0f;

		anim[ idleAnimation.name ].speed = AnimationSpeed;
		anim[ idleAnimation.name ].wrapMode = WrapMode.Loop;
		anim.CrossFade(idleAnimation.name);
		
//		if (global.playerObject != null && Vector3.Distance (transform.position, global.playerObject.transform.position) < 5)		
//			playerInRange = true;
//		else		
//			playerInRange = false;
//		
//		if (waypoint.ReachedWaypoint())
//			pauseFlag = true;
//		
//		if (!pauseFlag) 
//		{
//			if (moveDirection != Vector3.zero) 
//			{
//				transform.position += velocity;
//				transform.rotation = Quaternion.LookRotation (moveDirection);
//				//transform.rotation = Quaternion.Set(transform.rotation.x, transform.rotation.y, 90, transform.rotation.w);
//
//				anim[ walkAnimation.name ].speed = AnimationSpeed;
//				anim[ walkAnimation.name ].wrapMode = WrapMode.Loop;
//				anim.CrossFade(walkAnimation.name);
				
//				anim.SetBool ("IsWalk", true);
//				anim.SetBool ("IsIdle", false);
//				anim.SetBool ("IsAttack", false);
//			}
//		} 
//		else if (playerInRange && hostile) 
//		{
//			Vector3 targetPosition = global.playerObject.transform.position;
//			targetPosition.y = transform.position.y;
//			transform.LookAt (targetPosition);
//			if (Vector3.Distance (transform.position, global.playerObject.transform.position) < 2)
//				global.playerObject.GetComponent<Player> ().TakeDamage (1);
//			else 
//				transform.position += transform.forward * 2 * Time.deltaTime;
//
//			anim[ walkAnimation.name ].speed = AnimationSpeed;
//			anim[ walkAnimation.name ].wrapMode = WrapMode.Loop;
//			anim.CrossFade(walkAnimation.name);
			
//			anim.SetBool ("IsAttack", true);
//			anim.SetBool ("IsWalk", false);
//			anim.SetBool ("IsIdle", false);
//		} 
//		else
//		{
//			pauseTime += Time.fixedDeltaTime;		
//			if (pauseTime > waypoint.CurrentWaypoint ().waitTime) 
//			{
//				pauseFlag = false;
//				pauseTime = 0.0f;
//			}
//			anim[ idleAnimation.name ].speed = AnimationSpeed;
//			anim[ idleAnimation.name ].wrapMode = WrapMode.Loop;
//			anim.CrossFade(idleAnimation.name);

//			anim.SetBool ("IsIdle", true);
//			anim.SetBool ("IsAttack", false);
//			anim.SetBool ("IsWalk", false);
//		}
	}
	
	void OnCollisionEnter (Collision col)
	{
		//if (col.gameObject.GetComponent<Terrain> () != null)
			//isGrounded = true;
	}
	
	void OnCollisionExit (Collision col)
	{
		//if (col.gameObject.GetComponent<Terrain> () != null)
			//isGrounded = false;
	}
}
