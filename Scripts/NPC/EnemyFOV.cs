using UnityEngine;
using System.Collections;

public class EnemyFOV : MonoBehaviour 
{
	public float fieldOfViewAngle = 110f;		//The angle of the collider that is used for the fov
	public float visibilityRange = 10f;			//Distance that the NPC can see

	[HideInInspector]
	public Vector3 personalLastSighting;		//So that the guard knows where the player was spotted and so that other guards 
												//know of a sighting
	[HideInInspector]
	public bool playerJustInSight;
	private Animator anim;

	#region View Detection
	public bool CanSeePlayer()	//Used to detect when player is in the NPCs field of view
	{
		RaycastHit hit;
		Vector3 direction = global.playerObject.transform.position - transform.position;

		if(Vector3.Angle(direction, transform.forward) <= (fieldOfViewAngle / 2))
		{
			if(Physics.Raycast(transform.position + transform.up, direction, out hit, visibilityRange))
			{
				playerJustInSight = false;
				return true;
			}
		}
		return false;
	}

	public bool JustSawPlayer()
	{
		return playerJustInSight;
	}

	void OnTriggerExit(Collider other)
	{
		if(other.gameObject == global.playerObject)
		{
			playerJustInSight = true;
		}
		else
			playerJustInSight = false;
	}
	#endregion

}
